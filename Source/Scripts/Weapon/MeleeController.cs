using UnityEngine;
using System.Collections;

public class MeleeController : MonoBehaviour
{
    public string weaponName = "Knife";
    public float meleeRange = 1.5f;
    public float meleeDelay = 0.2f;
    public int meleeMinDamage = 100;
    public int meleeMaxDamage = 100;
    public float meleeForce = 1.25f;
    public float meleeCooldown = 0.9f;
    public AudioClip meleeSwingSound;
    public Texture2D iconTexture;
    public Vector2 iconSize = new Vector2(90f, 25f);
    public Vector2 iconOffset = new Vector2(0f, 0f);
    public LayerMask layersToMelee = -1;

    private bool friendlyFire;
    private bool isMeleeState;
    private RaycastHit hit;
    private WeaponManager wm;
    private DynamicMovement dm;
    private AntiClipSystem acs;
    private Transform camTr;
    private UILabel nameLabel;
    private UISlider ammoBar;
    private UILabel curAmmoLabel;
    private UILabel ammoLeftLabel;

    void Start()
    {
        camTr = GeneralVariables.mainPlayerCamera.transform;
        isMeleeState = false;

        PlayerReference pr = GeneralVariables.playerRef;
        dm = pr.dm;
        acs = pr.acs;
        wm = pr.wm;

        UIController uic = GeneralVariables.uiController;
        nameLabel = uic.weaponName;
        ammoBar = uic.ammoBar;
        curAmmoLabel = uic.curAmmoDisplay;
        ammoLeftLabel = uic.ammoLeftDisplay;

        friendlyFire = NetworkingGeneral.friendlyFire;
    }

    void Update()
    {
        MeleeGUI();

        if (!acs.clipping && cInput.GetButtonDown("Fire Weapon") && !isMeleeState)
        {
            dm.DoMeleeAnimation(); //Obviously placeholder.
            GetComponent<AudioSource>().PlayOneShot(meleeSwingSound);
            StartCoroutine(MeleeAction());
        }
    }

    private IEnumerator MeleeAction()
    {
        isMeleeState = true;
        yield return new WaitForSeconds(meleeDelay);

        if (Physics.Raycast(camTr.position, camTr.forward, out hit, meleeRange, layersToMelee.value))
        {
            BaseStats bs = hit.collider.GetComponent<BaseStats>();
            Limb lb = hit.collider.GetComponent<Limb>();

            if (bs != null)
            {
                bool showHitMarker = false;
                if (wm != null && bs.curHealth > 0)
                {
                    showHitMarker = true;
                }

                bool canDamage = true;
                bs.headshot = false;

                if (Topan.Network.isConnected)
                {
                    Topan.NetworkView damageView = bs.GetComponent<Topan.NetworkView>();
                    if (damageView != null)
                    {
                        BotVitals hitBot = bs.GetComponent<BotVitals>();

                        if (GeneralVariables.gameModeHasTeams)
                        {
                            byte targetTeam = /*(hitBot) ? BotManager.allBotPlayers[hitBot.bm.myIndex].team : */(byte)damageView.owner.GetPlayerData("team", (byte)0);

                            if (targetTeam == (byte)Topan.Network.player.GetPlayerData("team", (byte)0))
                            {
                                if (!friendlyFire)
                                {
                                    canDamage = false;
                                }

                                showHitMarker = false;
                            }
                        }
                        else
                        {
                        }

                        if (canDamage)
                        {
                            if (Topan.Network.isServer && (damageView.ownerID == 0 || hitBot))
                            {
                                bs.ApplyDamageNetwork((byte)Mathf.Clamp(Random.Range(meleeMinDamage, meleeMaxDamage + 1), 0, 255), (byte)Topan.Network.player.id, (byte)0, (byte)4);
                            }
                            else
                            {
                                damageView.RPC(Topan.RPCMode.Owner, "ApplyDamageNetwork", (byte)Mathf.Clamp(Random.Range(meleeMinDamage, meleeMaxDamage + 1), 0, 255), (byte)Topan.Network.player.id, (byte)0, (byte)4);
                            }
                        }
                    }
                }
                else
                {
                    bs.headshot = false;
                    bs.ApplyDamageMain(Random.Range(meleeMinDamage, meleeMaxDamage + 1), true);
                }

                if (showHitMarker)
                {
                    wm.HitTarget(bs.curHealth <= 0);
                }
            }
            else if (lb != null && lb.rootStats != null)
            {
                bs = lb.rootStats;
                int finalDmg = Mathf.RoundToInt(Random.Range(meleeMinDamage, meleeMaxDamage + 1) * Mathf.Clamp01(lb.realDmgMult));

                bool showHitMarker = false;
                if (wm != null && bs.curHealth > 0)
                {
                    showHitMarker = true;
                }

                bool canDamage = true;

                if (Topan.Network.isConnected)
                {
                    Topan.NetworkView damageView = bs.GetComponent<Topan.NetworkView>();
                    if (damageView != null)
                    {
                        BotVitals hitBot = bs.GetComponent<BotVitals>();

                        if (GeneralVariables.gameModeHasTeams)
                        {
                            byte targetTeam =/* (hitBot) ? BotManager.allBotPlayers[hitBot.bm.myIndex].team : */(byte)damageView.owner.GetPlayerData("team", (byte)0);

                            if (targetTeam == (byte)Topan.Network.player.GetPlayerData("team", (byte)0))
                            {
                                if (!friendlyFire)
                                {
                                    canDamage = false;
                                }

                                showHitMarker = false;
                            }
                        }
                        else
                        {
                        }

                        if (canDamage)
                        {
                            if (Topan.Network.isServer && (damageView.ownerID == 0 || hitBot))
                            {
                                bs.ApplyDamageNetwork((byte)Mathf.Clamp(finalDmg, 0, 255), (byte)Topan.Network.player.id, (byte)0, (byte)lb.limbType);
                            }
                            else
                            {
                                damageView.RPC(Topan.RPCMode.Owner, "ApplyDamageNetwork", (byte)Mathf.Clamp(finalDmg, 0, 255), (byte)Topan.Network.player.id, (byte)0, (byte)lb.limbType);
                            }
                        }
                    }
                }
                else
                {
                    bs.headshot = false;
                    bs.ApplyDamageMain(finalDmg, true);
                }

                if (showHitMarker)
                {
                    wm.HitTarget(bs.curHealth <= 0);
                }
            }

            Rigidbody rigid = hit.rigidbody;
            if (rigid != null && !rigid.isKinematic)
            {
                rigid.AddForce(camTr.forward * meleeForce, ForceMode.Impulse);
            }

            int particleIndex = 0;
            string surfTag = hit.collider.tag;
            if (surfTag == "Dirt")
            {
                particleIndex = 1;
            }
            else if (surfTag == "Metal")
            {
                particleIndex = 2;
            }
            else if (surfTag == "Wood")
            {
                particleIndex = 3;
            }
            else if (surfTag == "Flesh" || surfTag == "Player - Flesh")
            {
                particleIndex = 4;
            }
            else if (surfTag == "Water")
            {
                particleIndex = 5;
            }

            PoolManager.Instance.RequestParticleEmit(particleIndex, hit.point + (hit.normal * 0.06f), Quaternion.LookRotation(hit.normal));
        }

        yield return new WaitForSeconds(meleeCooldown);
        isMeleeState = false;
    }

    private void MeleeGUI()
    {
        nameLabel.text = weaponName;
        ammoBar.value = Mathf.MoveTowards(ammoBar.value, 1f, Time.deltaTime * 2f);
        curAmmoLabel.text = "---";
        ammoLeftLabel.text = "---";
    }
}