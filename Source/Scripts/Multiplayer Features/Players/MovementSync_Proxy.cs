using UnityEngine;
using System.Collections;

public class MovementSync_Proxy : Topan.TopanMonoBehaviour {
    public SkinnedMeshRenderer skinRenderer;
    public AudioSource equipmentRattleSource;
    public Vector3 crouchOffset = new Vector3(0f, -1f, 0f);
    public float posSmoothing = 10f;
    public Material dissolveMaterial;

    [HideInInspector]
    public Vector3 velocity;
    [HideInInspector]
    public bool jumpRattleEquip;
    [HideInInspector]
    public bool isGrounded;
    [HideInInspector]
    public bool isSprinting;
    [HideInInspector]
    public bool isCrouching;
    [HideInInspector]
    public bool isWalking;

    //Easier access
    [HideInInspector]
    public int playerID;
    [HideInInspector]
    public int playerTeam;

    private CharacterController controller;
    private Vector3 oldPos;
    private Vector3 target = Vector3.zero;
    private Quaternion targetRot = Quaternion.identity;
    private Transform tr;
    private float rattleTimer;
    private float targetY = 0f;
    private float currentY = 0f;

    private Animator animator;
    private ProxyAnimator pa;
    private TimeScaleSound rattleTSS;
    private WeaponHandler_Proxy whP;

    void Start() {
        GetComponent<BaseStats>().hasLimbs = true;
        pa = GetComponent<ProxyAnimator>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        rattleTSS = equipmentRattleSource.GetComponent<TimeScaleSound>();
        isGrounded = false;
    }

    void NetworkStart() {
        if(topanNetworkView != null && topanNetworkView.owner == null) {
            topanNetworkView.Deallocate();
            return;
        }

        if(topanNetworkView != null) {
            topanNetworkView.owner.SetLocalData("currentView", topanNetworkView);
        }
        
        tr = transform;
        GeneralVariables.Networking.playerInstances[topanNetworkView.owner.id] = tr;
        oldPos = tr.position;
        target = tr.position;
        gameObject.name = ((CombatantInfo)topanNetworkView.owner.GetInitialData("dat")).username;
        playerID = topanNetworkView.owner.id;

        playerTeam = (int)((byte)topanNetworkView.owner.GetPlayerData("team"));
        foreach(Material mat in skinRenderer.materials) {
            Color col = mat.color;

            if(playerTeam == 0) {
                col.r *= 1.15f;
            }
            else {
                col.b *= 1.15f;
            }

            mat.color = col;
        }

        if(GeneralVariables.gameModeHasTeams && playerTeam == (byte)Topan.Network.player.GetPlayerData("team")) {
            GeneralVariables.uiController.teamMarkerSystem.AddPlayerMarker(topanNetworkView.owner.id);
        }

        whP = GetComponent<WeaponHandler_Proxy>();
    }

    void Topan_Deallocating() {
        GetComponent<BaseStats>().curHealth = 0;
        GetComponent<RagdollHandler>().SetRagdoll(true);
        Destroy(animator);
        whP.DestroyWeapons();
        whP.DeleteInactiveExplosives(10f);

        MonoBehaviour[] toDestroy = GetComponents<MonoBehaviour>();
        foreach(MonoBehaviour mb in toDestroy) {
            if(mb == whP) {
                Destroy(mb, 10.5f);
                continue;
            }

            Destroy(mb);
        }

        skinRenderer.gameObject.AddComponent<DissolveEffect>().Dissolve(new Material(dissolveMaterial), GameSettings.settingsController.ragdollDestroyTimer, 0.18f, new Color(1f, 0.3f, 0f), DissolveEffect.DissolveDirection.DissolveOut, true);
    }

    void Update() {
        currentY = Mathf.Lerp(currentY, targetY, Time.deltaTime * 10f);
        animator.SetFloat("Aim", currentY);

        tr.position = Vector3.Lerp(tr.position, target + ((isCrouching) ? crouchOffset : Vector3.zero), Time.deltaTime * posSmoothing);
        tr.rotation = Quaternion.Slerp(tr.rotation, targetRot, Time.deltaTime * posSmoothing);
        velocity = (tr.position - oldPos);
        oldPos = tr.position;

        Vector3 xzVelo = velocity;
        xzVelo.y = 0f;
        float xzMagn = velocity.magnitude;
        if(isSprinting && xzMagn > 0.045f) {
            rattleTSS.pitchMod = 1f;
            equipmentRattleSource.volume = Mathf.Lerp(equipmentRattleSource.volume, 0.151f, Time.deltaTime * 9f);
        }
        else {
            float velocityFactor = Mathf.Clamp01(xzMagn / 0.06f);
            if(jumpRattleEquip) {
                rattleTimer += Time.deltaTime;

                if(xzMagn < 0.045f) {
                    rattleTSS.pitchMod = 1f;
                    equipmentRattleSource.volume = Mathf.Lerp(equipmentRattleSource.volume, 0.0552f, Time.deltaTime * 9f);
                }

                if(rattleTimer >= 0.35f) {
                    rattleTimer = 0f;
                    jumpRattleEquip = false;
                }
            }
            else {
                if(isGrounded) {
                    if(xzMagn >= 0.045f) {
                        rattleTSS.pitchMod = (isCrouching || isWalking) ? 0.8f : 0.96f;
                        equipmentRattleSource.volume = Mathf.Lerp(equipmentRattleSource.volume, 0.092f * velocityFactor, Time.deltaTime * 9f);
                    }
                    else {
                        equipmentRattleSource.volume = Mathf.Lerp(equipmentRattleSource.volume, 0f, Time.deltaTime * 9f);
                    }
                }
                else {
                    equipmentRattleSource.volume = Mathf.Lerp(equipmentRattleSource.volume, 0f, Time.deltaTime * 9f);
                }
            }
        }
    }

    [RPC]
    public void PlayerGrounded(bool grounded) {
        isGrounded = grounded;
        if(!grounded) {
            jumpRattleEquip = true;
        }
    }

    [RPC]
    public void PlayerCrouching(bool crouching) {
        isCrouching = crouching;
        pa.ToggleCrouch(crouching);
    }

    [RPC]
    public void PlayerWalking(bool walking) {
        isWalking = walking;
    }

    [RPC]
    public void PlayerSprinting(bool sprinting) {
        isSprinting = sprinting;
    }

    [RPC]
    public void SyncTransform(Vector3 t, float xRot, float lookY) {
        target = t;

        Vector3 euler = tr.rotation.eulerAngles;
        euler.y = xRot;
        targetRot = Quaternion.Euler(euler);

        targetY = lookY;
    }
}