using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Topan.CustomTypes;

public class GrenadeController : MonoBehaviour {
	public string grenadeName = "M67";
	public Texture2D grenadeIcon;
	public Transform throwPos;
	public Rigidbody grenadePrefab;
	public AudioClip pullPinSound;
	public AudioClip throwSound;
	public MeshRenderer displayMesh;
    public float throwThreshold = 0.35f; //Pretty much minimum delay before you throw.
	public float cookingThreshold = 0f; //0 = cook for an infinite amount of time.
	public float throwStrength = 23f;
	public float tossStrength = 11f;
    public bool isDetonatable = false; //RMB to explode the thrown explosives (e.g. C4)
	public float baseDelay = 0.3f; //After detonating, the initial amount of time before it triggers first explosion.
    public float detonationDelay = 0.2f; //Time between the detonations.

    //Access these through debug inspector.
    public GameObject[] toDelete; //PrepareForMultiplayer()
    	
	public int grenadeID = -1;
	
	private PlayerMovement pm;
    private WeaponManager wm;
	private GrenadeManager gm;
	private GrenadeAmmoManager gam;
	private AntiClipSystem acs;
	
	[HideInInspector] public bool recharging = false;
    [HideInInspector] public Vector3 thirdPersonPosition;
    [HideInInspector] public Quaternion thirdPersonRotation;
    [HideInInspector] public float targetStrength;
    [HideInInspector] public bool isQuickThrow = false;
    [HideInInspector] public int curAmount;
    [HideInInspector] public int maxAmount;
	
	public bool cannotSwitch {
		get {
			return (recharging || pulledPin);
		}
	}

    public bool couldSwitchDetonate {
        get {
            return (isDetonatable && (curAmount > 0 || gm.detonationList.Count > 0));
        }
    }

    public int slotNumber {
        get {
            if(isSlotTwo) {
                return 1;
            }

            return 0;
        }
    }
	
	private Rigidbody grenadeInstance;

	private int switchToWeapon = -1;
	private bool gettingGrenade;
	private bool pulledPin = false;
	private bool canThrow = true;
	private bool isSlotOne = false;
	private bool isSlotTwo = false;

	private bool canDetonate;
	private float pullPinTime;
    private float lerpText;
		
	private UILabel weaponName;
	private UILabel curAmmo;
	private UILabel ammoLeft;
    private UITexture grenadeSlotIcon;
	private UISlider ammoBar;
	
	public void Initialize() {
		UIController uic = GeneralVariables.uiController;
		weaponName = uic.weaponName;
		curAmmo = uic.curAmmoDisplay;
		ammoLeft = uic.ammoLeftDisplay;
		ammoBar = uic.ammoBar;
		
        PlayerReference pr = GeneralVariables.playerRef;
        gm = transform.parent.GetComponent<GrenadeManager>();
		pm = pr.GetComponent<PlayerMovement>();
        gam = pr.gam;
		wm = pr.wm;
		acs = pr.acs;

        if(isDetonatable) {
			canDetonate = true;
        }
		
		OnSelect();
		switchToWeapon = -1;
		canThrow = true;
	}

    public void PrepareForMultiplayer(bool destroyThis = false) {
        foreach(GameObject go in toDelete) {
            Destroy(go);
        }

        SprintAnimOverride sao = GetComponent<SprintAnimOverride>();
        if(sao != null) {
            Destroy(sao);
        }

        gameObject.AddComponent<GrenadeHandler_Proxy>();

        if(destroyThis) {
            Destroy(this);
        }
    }
	
	void Update() {
		GetGrenadeValues();
		GrenadeGUI();

		if(gettingGrenade && switchToWeapon <= -1 && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2))) {
			if(Input.GetKeyDown(KeyCode.Alpha1)) {
				switchToWeapon = 0;
			}
			else if(Input.GetKeyDown(KeyCode.Alpha2)) {
				switchToWeapon = 1;
			}

			gm.dm.ExtendDrawTime(0.3f);
		}
		
		if(!canThrow && !recharging && !gm.dm.animationIsPlaying) {
            if(!isQuickThrow) {
                StartCoroutine(RechargeGrenade());
            }
		}

        if(isDetonatable && canDetonate && Input.GetMouseButtonDown(1)) {
            DetonateExplosives();
            StartCoroutine(CheckExplosiveCount());
        }
			
		if(!RestrictionManager.restricted && !RestrictionManager.mpMatchRestrict && !pm.sprinting) {
			if(canThrow && !recharging && !acs.clipping && curAmount > 0) {
				if(!pulledPin) {
					if(cInput.GetButtonDown("Fire Weapon")) {
						PullPin();
						targetStrength = throwStrength;
					}
					else if(cInput.GetButtonDown("Aim") && !isDetonatable) {
                        PullPin();
                        targetStrength = tossStrength;
					}
				}
				else {
					if(grenadeInstance == null && !recharging) {
						StartCoroutine(RechargeGrenade());
					}

                    if((!cInput.GetButton("Fire Weapon") && !cInput.GetButton("Throw Grenade") && (Time.time - pullPinTime) >= throwThreshold) || (cookingThreshold > 0f && (Time.time - pullPinTime) >= throwThreshold + cookingThreshold)) {
						Throw(targetStrength);
					}
				}
			}
		}
	}
	
	public void PullPin() {
		displayMesh.enabled = false;
		grenadeInstance = (Rigidbody)Instantiate(grenadePrefab, displayMesh.transform.position, displayMesh.transform.rotation);
		grenadeInstance.transform.parent = transform;
		grenadeInstance.transform.position = displayMesh.transform.position;
        grenadeInstance.isKinematic = true;

        if(grenadeInstance.GetComponent<Collider>() != null) {
            grenadeInstance.GetComponent<Collider>().enabled = false;
        }

        int newSyncID = Random.Range(0, 9999999);
        GrenadeScript gScript = grenadeInstance.GetComponent<GrenadeScript>();
        PlasticExplosive pExpl = grenadeInstance.GetComponent<PlasticExplosive>();

        if(Topan.Network.isConnected && wm.rootNetView != null) {
            wm.rootNetView.RPC(Topan.RPCMode.Others, "PullPinGrenade", newSyncID);
        }

        if(isDetonatable) {
			if(pExpl != null) {
				pExpl.enabled = false;
                pExpl.myID = newSyncID;
			}
		}
		else {
            if(gScript != null) {
                gScript.PulledPin();
                gScript.myID = newSyncID;
                gScript.databaseID = grenadeID;
            }			
        }

        GetComponent<AudioSource>().PlayOneShot(pullPinSound);

		pullPinTime = Time.time;
		pulledPin = true;
	}
	
	public void Throw(float strength) {
		if(grenadeInstance != null) {
            gam.ChangeGrenadeAmount(grenadeID, -1);
			GetComponent<AudioSource>().PlayOneShot(throwSound);            
			
			grenadeInstance.transform.parent = null;
            grenadeInstance.isKinematic = false;

            if(grenadeInstance.GetComponent<Collider>() != null) {
                grenadeInstance.GetComponent<Collider>().enabled = true;
            }

            Vector3 dir = throwPos.transform.forward + (Vector3)(Random.insideUnitCircle * 0.025f);
            Vector3 velo = (pm.controller.velocity * 0.5f) + ((dir + (Vector3.up * 0.1f)) * strength);
			grenadeInstance.velocity = velo;
			grenadeInstance.angularVelocity = new Vector3(1f, 0.69f, 0.86f) * 4.5f;

            if(Topan.Network.isConnected && wm.rootNetView != null) {
				wm.rootNetView.RPC(Topan.RPCMode.Others, "ThrowGrenade", velo, grenadeInstance.transform.position);
            }

            PlasticExplosive pExpl = grenadeInstance.GetComponent<PlasticExplosive>();
            if(isDetonatable && pExpl != null) {
                gm.detonationList.Add(pExpl);
				pExpl.enabled = true;
            }
						
			canThrow = false;			
			pulledPin = false;
			targetStrength = 0f;

            GetGrenadeValues();

			grenadeInstance = null;
		}
	}
	
	private IEnumerator RechargeGrenade() {
		GetGrenadeValues();
		
		if(curAmount <= 0 && !isDetonatable) {
			gm.SwitchToAvailableGrenade();
			yield break;
		}
		
		recharging = gettingGrenade = true;
		canThrow = false;
		canDetonate = false;
		gm.dm.Draw(0.7f);

		float timer = 0f;
		while(timer < gm.dm.currentDrawDelay) {
			timer += Time.deltaTime;
			yield return null;
		}

		canDetonate = true;
		canThrow = true;
		displayMesh.enabled = true;
		pulledPin = false;

		if(switchToWeapon > -1 && curAmount > 0) {
			wm.FindWeaponToUse(true, switchToWeapon);
			switchToWeapon = -1;
			recharging = false;
		}
		
		gettingGrenade = false;
		yield return new WaitForSeconds(0.32f);
		recharging = false;
	}

    private void DetonateExplosives() {
		GetComponent<AudioSource>().PlayOneShot(pullPinSound); //Detonation sound.

        for(int i = 0; i < gm.detonationList.Count; i++) {
            gm.detonationList[i].Detonate(baseDelay + (i * detonationDelay));
        }

        gm.detonationList.Clear();

        if(Topan.Network.isConnected && wm.rootNetView != null) {
            wm.rootNetView.RPC(Topan.RPCMode.Others, "DetonateExplosives");
        }
    }

    private IEnumerator CheckExplosiveCount() {
		canDetonate = false;
        yield return new WaitForSeconds(0.5f);
		canDetonate = true;

        //No more explosives when detonated
        if(curAmount <= 0) {
            gm.SwitchToAvailableGrenade();
        }
    }
	
	public void OnSelect() {
		isSlotOne = (grenadeID == gam.grenadeTypeOne);
		isSlotTwo = (grenadeID == gam.grenadeTypeTwo);
		
		if(isSlotOne) {
			grenadeSlotIcon = GeneralVariables.uiController.grenadeOneIcon;
		}
		else if(isSlotTwo) {
			grenadeSlotIcon = GeneralVariables.uiController.grenadeTwoIcon;
		}
		
		grenadeSlotIcon.mainTexture = grenadeIcon;
        lerpText = (float)wm.displayCurAmmo;

		GetGrenadeValues();
		if(curAmount > 0) {
			canThrow = true;
			displayMesh.enabled = true;
		}
	}
	
	private void OnDestroy() {
		if(isSlotOne) {
			gam.typeOneGrenades = 0;
		}
		else if(isSlotTwo) {
			gam.typeTwoGrenades = 0;
		}
	}
	
	private void GetGrenadeValues() {
		if(isSlotOne) {
			curAmount = gam.typeOneGrenades;
			maxAmount = gam.typeOneMaxGrenades;
		}
		else if(isSlotTwo) {
			curAmount = gam.typeTwoGrenades;
			maxAmount = gam.typeTwoMaxGrenades;
		}
	}
	
	private void GrenadeGUI() {
        if(!wm.pe.hasEMP) {
            weaponName.text = grenadeName;
            lerpText = Mathf.Lerp(lerpText, curAmount, Time.unscaledDeltaTime * 14f);
            wm.dca = lerpText;
            curAmmo.text = wm.displayCurAmmo.ToString();
            curAmmo.color = curAmmo.defaultColor;
            ammoLeft.text = "---";
        }

        ammoBar.value = Mathf.Lerp(ammoBar.value, (float)curAmount / (float)maxAmount, Time.deltaTime * 8f);
        grenadeSlotIcon.mainTexture = grenadeIcon;
	}
}