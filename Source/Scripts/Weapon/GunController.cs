using UnityEngine;
using System.Collections;
using Topan.CustomTypes;

[System.Serializable]
public class BulletInfo {
	public enum BulletType {Bullet = 0, Rocket = 1}
	public BulletType bulletType = BulletType.Bullet;

    public int poolIndex = 0;
    [UnityEngine.Serialization.FormerlySerializedAs("maxDamage")]
	public int damage = 15;
    public float explosionRadius = 8f;
    public float force = 2f;
    public float muzzleVelocity = 350f;
    public float gravityFactor = 0.5f;
    public AnimationCurve damageFalloff = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(50f, 1f), new Keyframe(400f, 0.5f));
    public int ricochetLength = 0;
    public float ricochetMaxAngle = 0f;
    public float penetrationDistance = 0.5f;
    public float penetrationDamageReduction = 0.3f;
    public float penetrationSpeedReduction = 0.15f;
}

[System.Serializable]
public class GunController : Topan.TopanMonoBehaviour {
	public enum FireMode {None, SemiAuto, BurstFire, FullAuto}
	public enum ReloadMethod {Magazine, Singular}
	public ReloadMethod reloadMethod = ReloadMethod.Magazine;
	
	public Transform leftHandTransform;
	public Transform firePos;
	public Texture2D iconTexture;
	public Vector2 iconScale = new Vector2(100, 50);
    public Vector2 iconOffset = new Vector2(0f, 0f);
	public WeaponSlot weaponSlot = WeaponSlot.Primary;
	public FireMode firstMode = FireMode.FullAuto;
	public FireMode secondMode = FireMode.None;
	public FireMode thirdMode = FireMode.None;
	public AudioClip switchSound;
	public string gunName = "Machine Gun";
	public BulletInfo bulletInfo;
	public float firstRPM = 600f;
	public float secondRPM = 300f;
	public float thirdRPM = 450f;
	public AudioClip fireSound;
	public AudioClip emptySound;
	public AudioClip reloadEnd;
	public AudioClip reloadSound;
    public AudioClip reloadSoundEmpty;
	public bool reloadOnMouseClick = false;
	public bool countsAsOneBullet = true;
	public int bulletsPerShot = 1;
	public int bulletsPerBurst = 3;
	public float burstInterval = 0.1f;
	public float burstCooldown = 0.2f;
	public int currentAmmo = 30;
	public int clipSize = 30;
	public float reloadLength = 2.1f;
    public float reloadLengthEmpty = 2.8f;
	public float aimBobFactor = 0.3f;

    public int extraRecoilThreshold = 5;
    public float extraRecoilAmount = 0.05f;
    public float maxExtraRecoil = 0.5f;
	
	public float reloadDelay = 0.5f;
	public float reloadInterval = 0.8f;
	public int reloadAmount = 1;

    public float muzzleProbability = 0.85f;
	public ParticleEmitter muzzleFlash;
	public Light muzzleLight;
    public float muzzleSpeed = 25f;
    
	public ParticleEmitter shootParticle;
	public bool sniperAimEffect = false;
	public float baseSpreadAmount = 0.5f;
	public float maxSpreadAmount = 5f;
    public float movementSpreadAmount = 0.4f;
	public float spreadSpeed = 0.2f;
	public float spreadAimFactor = 0.5f;
	public float recoverSpeed = 5.0f;
	public float aimSpreadModifier = 0.5f;

    //Offset, not additive
    public float recoilAmount = 1.3f;

	public float upKickAmount = 0.7f;
    public Vector2 kickInfluence = Vector2.zero;
	public float autoReturn = 0f;
    public float sideKickAmount = 0.3f;
	public float kickBackAmount = 1.5f;
    public float kickCameraTilt = 1.0f;
    public float kickGunTilt = 1.0f;
    public float camShakeAnim = 1.0f;
	public float playerSpeedAim = 0.8f;
	public float mouseSensitivityAim = 0.5f;
	public Vector3 aimPos;
    public float aimSpeedFactor = 1f;
	public float addZoomFOV = 0f;
	public string reloadAnim = "";
	public bool ejectionEnabled = true;
	public Transform ejectionPos;
	public int bulletShellIndex = 2;
	public float ejectionDelay = 0f;
    public Vector3 ejectionMinForce = new Vector3(1.8f, 0.78f, 0.25f);
    public Vector3 ejectionMaxForce = new Vector3(2.1f, 1f, 0.45f);
    public float ejectionRotation = 10f;
	public bool includeChamberRound = true;
	public float aimUpkickModifier = 0.4f; //For up-kick and side-kick.
    public float crouchUpkickModifier = 0.8f; //For up-kick and side-kick.
	public float crouchWalkModifier = 0.8f;
	public float kickbackAimFactor = 0.5f;
	public float kickSpeedFactor = 1f;
	public float aimSwayFactor = 0f; //Snip3r lel
	
	public bool crosshairsEnabled = true;
	
	public AudioClip flashlightClick;
	public Light flashlight;
	public float weaponWeight = 2.5f;
	public int weaponID = -1;
	
	public GameObject[] activateOnUse;
	public GameObject[] deactivateOnUse;
	
	public Vector3 firstPersonPosition = Vector3.zero;
	public Quaternion firstPersonRotation = Quaternion.identity;
	public Vector3 thirdPersonPosition = Vector3.zero;
	public Quaternion thirdPersonRotation = Quaternion.identity;

    public float dofBlurAmount = 1.6f; //0 = disable
    public float dofBlurDistance = 0.5f;
    public float dofBlurAperture = 9.5f;
    public float dofBlurFocalSize = 1f;
    public float dofBlendBlur = 10f;

	[HideInInspector] public GunVisuals gunVisuals;
	[HideInInspector] public bool reloading;
	[HideInInspector] public bool flashlightOn;
	[HideInInspector] public int ammoLeft = 270;
	[HideInInspector] public int ammoLeftCap = 270;
    [HideInInspector] public float percent;
    [HideInInspector] public float timeSinceLastFire;
	[HideInInspector] public float muzzlePercent;
	[HideInInspector] public FireMode currentFireMode;
	[HideInInspector] public bool bulletInChamber;
    [HideInInspector] public float spreadReal;
	
    private bool ownedByBot = false;
	private bool canSwitchModes = true;
	private bool switching;

	private bool rDelayDone;
	private bool doneReloading;
	private bool forceStopReload;
	private bool queueStopReload;
	private bool bursting;
	private bool mpReferenceOnly;
    private bool startCounting;
	private Vector3 defReloadSize;
	private int ammoDif;
    private float muzzleBrightness;
    private float initializeTime;
    private float shootImpulseGUI;
	private float reloadImpulseGUI;
	private float timer;
	private float lastClick;
	private float startReload;
    private float endReload;
	private float bsna;
	private float asm;
    private float curReloadDur;
	private float reloadSinAlpha;
	private float rDelayTimer;
	private float rIntervalTimer;
    private float lightTime;
	private int fireModeIndex;
	private int maxIndex;
    private int fireCount;
	private PistolAnim pa;
	private PlayerMovement pm;
	private WeaponManager wm;
	private DynamicMovement dm;
	private PlayerLook pl;
	private TimeScaleSound tss;
	private AimController ac;
	private AntiClipSystem acs;

	private UILabel curAmmoDisplay;
	private UILabel ammoLeftDisplay;
	private UISlider ammoBar;
	
	private GameObject player;
	private CrosshairGUI crosshair;
	
	public void RunStart() {
		if(mpReferenceOnly) {
			return;
		}

		PlayerReference pr = GeneralVariables.playerRef;
		pl = pr.GetComponent<PlayerLook>();
		pm = pr.GetComponent<PlayerMovement>();
		wm = pr.wm;
		dm = pr.dm;
		ac = pr.ac;
		acs = pr.acs;
		tss = firePos.GetComponent<TimeScaleSound>();

		UIController uiController = GeneralVariables.uiController;
		curAmmoDisplay = uiController.curAmmoDisplay;
		ammoLeftDisplay = uiController.ammoLeftDisplay;
		ammoBar = uiController.ammoBar;
		crosshair = uiController.crosshairs;

        if(muzzleLight != null) {
            muzzleBrightness = muzzleLight.intensity;
        }

        initializeTime = Time.time;
        PoolManager.Instance.Initialize();
		gunVisuals = GetComponent<GunVisuals>();
        shootImpulseGUI = 1f;
		reloadImpulseGUI = 1f;
		
		pa = GetComponent<PistolAnim>();
		maxIndex = 2;
		SwitchFiringMethod();
		flashlightOn = false;
        startCounting = false;
        fireCount = 0;
        asm = 1f;
        spreadReal = baseSpreadAmount;
        bsna = baseSpreadAmount;
	}

    public void OnWeaponDraw() {
        AntiHackSystem.ProtectInt("currentAmmo", currentAmmo);
        AntiHackSystem.ProtectInt("clipSize", clipSize);
        AntiHackSystem.ProtectInt("ammoLeft", ammoLeft);
        AntiHackSystem.ProtectInt("ammoLeftCap", ammoLeftCap);
    }
	
	public void StripFunctions(bool setupForMultiplayer) {
		if(pa != null) {
			Destroy(pa);
		}
		
		AntiClipVariables acv = GetComponent<AntiClipVariables>();
		if(acv != null) {
			Destroy(acv);
		}
		
		GunVisuals gv = GetComponent<GunVisuals>();
		if(gv != null) {
			foreach(GameObject go in gv.activateOnUse) {
				Destroy(go);
			}
			
			if(!setupForMultiplayer) {
				foreach(GameObject go in gv.deactivateOnUse) {
					go.SetActive(true);
				}
				
				if(!Topan.Network.isConnected) {
					Destroy(gv);
				}
			}
		}

		if(!setupForMultiplayer) {
			Destroy(this);
		}

		mpReferenceOnly = setupForMultiplayer;
	}

    public void PrepareForBot() {
        ownedByBot = true;

        AntiClipVariables acv = GetComponent<AntiClipVariables>();
        if(acv != null) {
            Destroy(acv);
        }

        GunVisuals gv = GetComponent<GunVisuals>();
        if(gv != null) {
            foreach(GameObject go in gv.activateOnUse) {
                Destroy(go);
            }
        }

        PistolAnim pa = GetComponent<PistolAnim>();
        if(pa != null) {
            Destroy(pa);
        }
    }
	
	public void MakePickup(Vector3 force) {
		Rigidbody body = gameObject.AddComponent<Rigidbody>();
		body.mass = 3f;
		
		UsableObject uo = gameObject.AddComponent<UsableObject>();
		uo.weaponPickup = new UsableObject.WeaponPickup();
		uo.weaponPickup.enabled = true;
		uo.weaponPickup.ammoAmount = AntiHackSystem.RetrieveInt("currentAmmo");
        uo.weaponPickup.reserveAmmo = AntiHackSystem.RetrieveInt("ammoLeft");
		uo.weaponPickup.chamberedBullet = bulletInChamber;
		uo.weaponPickup.weaponID = weaponID;
		transform.parent = null;
		
		body.AddForce(force * 7f, ForceMode.Impulse);
		body.AddTorque(Random.rotation.eulerAngles * 3f, ForceMode.Impulse);

		uo.objectName = gunName;
		StripFunctions(false);
	}
	
	void Update() {
		if(mpReferenceOnly || ownedByBot) {
			return;
		}
		
        int retrievedAmmo = AntiHackSystem.RetrieveInt("currentAmmo");
		CheckAmmo();
		WeaponGUI();

        float curTime = Time.time;
		if(curTime - timeSinceLastFire >= Mathf.Min(0.5f, (1f + Time.deltaTime) / GetFireRate())) {
			bsna = Mathf.MoveTowards(bsna, baseSpreadAmount, Time.deltaTime * recoverSpeed);
		}
        else {
			bsna = Mathf.MoveTowards(bsna, baseSpreadAmount, Time.deltaTime * recoverSpeed * 0.1f);
        }

        bsna = Mathf.Clamp(bsna, baseSpreadAmount, maxSpreadAmount);

        if(timer > 0f) {
            timer -= Time.deltaTime * GetFireRate();
        }

        if(curTime - initializeTime < 0.5f) {
            return;
        }

        if(cInput.GetButton("Fire Weapon") && currentAmmo > 0) {
            if(!startCounting) {
                startCounting = true;
            }
        }
        else {
            if(startCounting) {
                fireCount = 0;
                startCounting = false;
            }
        }

        float cwm = (pm.crouching || pm.walking) ? crouchWalkModifier : 1f;
        float veloFactor = Mathf.Clamp01(pm.controllerVeloMagn / pm.movement.runSpeed) * movementSpreadAmount * asm;
        float airFactor = ((pm.grounded) ? 1f : (1f + (Mathf.Clamp01(Mathf.Max(0.1f, Mathf.Abs(pm.controller.velocity.y) - 2.5f) * 0.15f) * (ac.isAiming ? 32f : 1f))));
        spreadReal = (bsna * cwm * asm * airFactor) + veloFactor;

        if(!RestrictionManager.restricted && !RestrictionManager.mpMatchRestrict && timer <= 0f && retrievedAmmo > 0 && !reloading && !pm.sprinting && !switching && !acs.clipping && !pm.onLadder && !dm.animationIsPlaying && !dm.terminalVelocity && Time.time - endReload >= 0.15f && !ac.isTransitioning) {
            if(cInput.GetButtonDown("Fire Weapon") && currentFireMode == FireMode.SemiAuto) {
                Shoot(cwm, veloFactor, airFactor);
            }

            if(cInput.GetButton("Fire Weapon")) {
                if(currentFireMode == FireMode.FullAuto) {
                    Shoot(cwm, veloFactor, airFactor);
                }
                else if(currentFireMode == FireMode.BurstFire && !bursting) {
                    StartCoroutine(Burst(cwm, veloFactor, airFactor));
                }
            }
        }
					
		if(reloading) {	
			if(reloadMethod == ReloadMethod.Magazine) {
				if(curTime - startReload >= curReloadDur) {
					Reload();
				}
			}
			else if(reloadMethod == ReloadMethod.Singular) {
				if(cInput.GetButtonDown("Reload") || cInput.GetButtonDown("Fire Weapon")) {
					forceStopReload = true;
				}
				
				if(rDelayDone) {
					rIntervalTimer += Time.deltaTime;
					
					if(rIntervalTimer >= reloadInterval) {
						InsertBullet();
						rIntervalTimer -= reloadInterval;
					}
				}
				else {
					rDelayTimer += Time.deltaTime;
					if(rDelayTimer >= reloadDelay) {
						rIntervalTimer = reloadInterval;
						rDelayTimer = 0f;
						rDelayDone = true;
					}
				}
			}
		}
		
		if(ac.isAiming && !pm.onLadder) {
			asm = aimSpreadModifier;
			pl.aimMouseModifier = mouseSensitivityAim;
			pm.speedAimMod = playerSpeedAim;
			if(sniperAimEffect) {
				ac.aimEffect = true;
			}
		}
		else {
			asm = 1f;
			pl.aimMouseModifier = 1f;
			pm.speedAimMod = 1f;
			if(sniperAimEffect) {
				ac.aimEffect = false;
			}
		}
		
		if(!pm.onLadder && !RestrictionManager.restricted) {
			if(cInput.GetButtonDown("Fire Mode") && secondMode != FireMode.None && canSwitchModes && !switching) {
				SwitchFireModes();
			}

            if(!reloading && (Time.time - timeSinceLastFire > 0.2f) && AntiHackSystem.RetrieveInt("ammoLeft") > 0 && !pm.sprinting && ((cInput.GetButtonDown("Reload") && ammoDif > 0) || (retrievedAmmo <= 0 && reloadOnMouseClick && cInput.GetButtonDown("Fire Weapon")))) {                
                if(reloadMethod == ReloadMethod.Magazine) {
					StartMagazineReload();
				}
				else if(reloadMethod == ReloadMethod.Singular && !reloading) {
					reloading = true;
				}
			}
		}
	}
	
	private void Shoot(float cwm, float vf, float af) {
		for(int i = 0; i < bulletsPerShot; i++) {
			Vector2 randomTargetPoint = Random.insideUnitCircle * ((bsna * cwm * asm * af) + vf);
            Vector3 randomDir = new Vector3(randomTargetPoint.x, randomTargetPoint.y, 0f);
            Quaternion randomRot = Quaternion.Euler(firePos.eulerAngles) * Quaternion.Euler(randomDir);
			GameObject proj = PoolManager.Instance.RequestInstantiate(bulletInfo.poolIndex, firePos.position, randomRot, false);

			if(bulletInfo.bulletType == BulletInfo.BulletType.Bullet) {
				Bullet projBul = proj.GetComponent<Bullet>();
				projBul.BulletInfo(bulletInfo, weaponID, false, true, -1);
				projBul.noWhizSound = true;
				projBul.InstantiateStart();
			}
			else if(bulletInfo.bulletType == BulletInfo.BulletType.Rocket) {
				Rocket projRoc = proj.GetComponent<Rocket>();
				projRoc.RocketInfo(bulletInfo, weaponID, false, true, -1);
				projRoc.InstantiateStart();
			}
            
			if(Topan.Network.isConnected && wm.rootNetView) {
                Vector3 fwdRot = Quaternion.LookRotation(randomRot * Vector3.forward).eulerAngles;
                wm.rootNetView.UnreliableRPC(Topan.RPCMode.Others, "NetworkShoot", (TopanFloat)(fwdRot.x / 360f), (TopanFloat)(fwdRot.y / 360f), firePos.position);
			}
		}
		
		bsna += spreadSpeed * spreadAimFactor;

        if(countsAsOneBullet) {
            AntiHackSystem.ProtectInt("currentAmmo", currentAmmo - 1);
        }
        else {
            AntiHackSystem.ProtectInt("currentAmmo", currentAmmo - bulletsPerShot);
        }

        fireCount++;

        StopCoroutine("MuzzleControl");
		StartCoroutine(MuzzleControl());
		
		if(ejectionEnabled) {
			if(ejectionDelay > 0f) {
				StartCoroutine(EjectShell());
			}
			else {
				Rigidbody shell = PoolManager.Instance.RequestInstantiate(bulletShellIndex, ejectionPos.position, ejectionPos.rotation).GetComponent<Rigidbody>();
			    shell.velocity = (pm.controller.velocity * 0.7f) + pm.transform.TransformDirection(DarkRef.RandomVector3(ejectionMinForce, ejectionMaxForce));
			    shell.angularVelocity = Random.rotation.eulerAngles * ejectionRotation;
            }
		}
		
		tss.pitchMod = Random.Range(0.95f, 1f);
		tss.GetComponent<AudioSource>().PlayOneShot(fireSound);
				
		if(pa != null) {
			pa.startAnimation = true;
		}
		
        float aimMod = (ac.isAiming) ? aimUpkickModifier : 1f;
        float crouchMod = (pm.crouching) ? crouchUpkickModifier : 1f;
        float extraMod = (fireCount > extraRecoilThreshold) ? 1f + Mathf.Clamp((fireCount - extraRecoilThreshold) * extraRecoilAmount, 0f, maxExtraRecoil) : 1f;
        pl.Recoil(recoilAmount * ((ac.isAiming) ? 0.56f : 1.0f), upKickAmount * aimMod * crouchMod * extraMod, sideKickAmount * aimMod * crouchMod * extraMod, kickInfluence, kickCameraTilt, camShakeAnim, autoReturn);
		dm.Kickback(kickBackAmount * 0.04f, kickSpeedFactor, kickGunTilt);

        shootImpulseGUI += 0.1f;
		ammoBar.value += 0.01f;
		crosshair.JoltAnimation(spreadSpeed);
		
		timeSinceLastFire = Time.time;
		
		if(currentFireMode != FireMode.BurstFire)
			timer += 1f;
	}
	
	private IEnumerator Burst(float cwm, float vf, float af) {
		bursting = true;
		float burstTimer = 0f;
		int fireCount = 0;
		while(fireCount < bulletsPerBurst && AntiHackSystem.RetrieveInt("currentAmmo") > 0) {
			burstTimer += Time.deltaTime;
			if(burstTimer >= burstInterval) {
				Shoot(cwm, vf, af);
				fireCount++;
				burstTimer -= burstInterval;
			}
			yield return null;
		}
		yield return new WaitForSeconds(burstCooldown);
		bursting = false;
	}

    private IEnumerator MuzzleControl() {
        if(shootParticle != null) {
            shootParticle.Emit();
        }

        if(muzzleFlash != null && muzzleLight != null && Random.value < muzzleProbability) {
            muzzleFlash.Emit();
            muzzleLight.enabled = true;
            muzzleLight.range = Random.Range(3f, 4f);

            lightTime = 0f;
            muzzlePercent = 0f;
            float randomIntensity = Random.Range(0.9f, 1.1f) * muzzleBrightness;
            while(lightTime < 2f) {
                lightTime += Time.deltaTime * muzzleSpeed;
                muzzlePercent = Mathf.PingPong(Mathf.Clamp(lightTime, 0f, 2f), 1f) * randomIntensity;
                muzzleLight.intensity = muzzlePercent;
                yield return 0;
            }

            muzzlePercent = 0f;
        }
    }
	
	private IEnumerator EjectShell() {
		yield return new WaitForSeconds(ejectionDelay);
		
        Rigidbody shell = PoolManager.Instance.RequestInstantiate(bulletShellIndex, ejectionPos.position, ejectionPos.rotation).GetComponent<Rigidbody>();
		shell.velocity = (pm.controller.velocity * 0.7f) + pm.transform.TransformDirection(DarkRef.RandomVector3(ejectionMinForce, ejectionMaxForce));
		shell.angularVelocity = Random.rotation.eulerAngles * ejectionRotation;
	}
	
	private void StartMagazineReload() {
		if(reloading) {
			return;
		}
		
		startReload = Time.time;
		reloading = true;
		tss.pitchMod = 1f;

        bool magIsEmpty = (AntiHackSystem.RetrieveInt("currentAmmo") <= 0);
		tss.GetComponent<AudioSource>().PlayOneShot((magIsEmpty && reloadSoundEmpty != null) ? reloadSoundEmpty : reloadSound, 0.5f);
        curReloadDur = (magIsEmpty && reloadSoundEmpty != null) ? reloadLengthEmpty : reloadLength;
		
		if(Topan.Network.isConnected && wm.rootNetView) {
			wm.rootNetView.RPC(Topan.RPCMode.Others, "NetworkReload", magIsEmpty);
		}
				
		//Assign a new variable called AnimationContainer instead of using parents...
		/*
		if(reloadAnim != "" && !transform.parent.parent.parent.animation.isPlaying) {
			transform.parent.parent.parent.animation.Play(reloadAnim);
		}
		*/
	}
	
	private void Reload() {
        int retrievedAmmoLeft = AntiHackSystem.RetrieveInt("ammoLeft");
		bulletInChamber = (AntiHackSystem.RetrieveInt("currentAmmo") > 0 && includeChamberRound);
		CheckAmmo();

        AntiHackSystem.ProtectInt("currentAmmo", currentAmmo + ((ammoDif > retrievedAmmoLeft) ? retrievedAmmoLeft : (ammoDif + ((bulletInChamber) ? 1 : 0))));

        if(ammoDif > retrievedAmmoLeft) {
			AntiHackSystem.ProtectInt("ammoLeft", 0);
		}
		else {
            AntiHackSystem.ProtectInt("ammoLeft", ammoLeft - ammoDif);
		}

		reloadImpulseGUI += 0.1f;
		bsna = baseSpreadAmount;
        endReload = Time.time;
		reloading = false;
	}

    public void CancelReload() {
        firePos.GetComponent<AudioSource>().Stop();
        reloading = false;
    }
	
	private void InsertBullet() {
        int retrievedAmmo = AntiHackSystem.RetrieveInt("currentAmmo");
        int retrievedAmmoLeft = AntiHackSystem.RetrieveInt("ammoLeft");
        int retrievedClipSize = AntiHackSystem.RetrieveInt("clipSize");
        if(retrievedAmmo >= retrievedClipSize || retrievedAmmoLeft <= 0 || queueStopReload) {
			if(!doneReloading) {
				StartCoroutine(StopSingularReload());
			}

			return;
		}

        if(retrievedAmmo < retrievedClipSize && retrievedAmmoLeft > 0) {
			if(ammoDif < reloadAmount) {
                AntiHackSystem.ProtectInt("currentAmmo", currentAmmo + ammoDif);
                AntiHackSystem.ProtectInt("ammoLeft", ammoLeft - ammoDif);
			}
			else if(reloadAmount >= retrievedAmmoLeft) {
                AntiHackSystem.ProtectInt("currentAmmo", currentAmmo + ammoLeft);
                AntiHackSystem.ProtectInt("ammoLeft", 0);
			}
			else {
				AntiHackSystem.ProtectInt("currentAmmo", currentAmmo + reloadAmount);
                AntiHackSystem.ProtectInt("ammoLeft", ammoLeft - reloadAmount);
			}

            tss.pitchMod = Random.Range(0.95f, 1f);
			tss.GetComponent<AudioSource>().PlayOneShot(reloadSound, 0.5f);
			queueStopReload = forceStopReload;
			//Reload insert shell animation play.
		}

		reloadImpulseGUI += 0.1f;
	}
			
	private IEnumerator StopSingularReload() {
		rDelayDone = false;
		rIntervalTimer = 0f;
		doneReloading = true;
		
		if(reloadEnd) {
			firePos.GetComponent<AudioSource>().PlayOneShot(reloadEnd, 0.7f);
			yield return new WaitForSeconds(reloadEnd.length);
		}
		
		doneReloading = false;
		reloading = false;
		forceStopReload = false;
		queueStopReload = false;
	}
	
	private void CheckAmmo() {
        int retrievedAmmo = AntiHackSystem.RetrieveInt("currentAmmo");
        int retrievedAmmoLeft = AntiHackSystem.RetrieveInt("ammoLeft");
        int retrievedAmmoLeftCap = AntiHackSystem.RetrieveInt("ammoLeftCap");

        ammoLeft = Mathf.Clamp(retrievedAmmoLeft, 0, retrievedAmmoLeftCap);

        if(!reloadOnMouseClick && cInput.GetButtonDown("Fire Weapon") && (Time.time - lastClick) > 0.3f && retrievedAmmo <= 0 && !reloading && !RestrictionManager.restricted) {
            if(Topan.Network.isConnected && wm.rootNetView) {
				wm.rootNetView.UnreliableRPC(Topan.RPCMode.Others, "NetworkEmptyClick");
			}

			firePos.GetComponent<AudioSource>().PlayOneShot(emptySound);
            dm.EmptyAnimation();
			lastClick = Time.time;
		}

        int newMaxSize = AntiHackSystem.RetrieveInt("clipSize") + ((bulletInChamber) ? 1 : 0);

        currentAmmo = Mathf.Clamp(retrievedAmmo, 0, newMaxSize);
        ammoDif = newMaxSize - retrievedAmmo;
        percent = (float)retrievedAmmo / (float)newMaxSize;
	}
	
	private void SwitchFireModes() {
		if(!reloading && !pm.sprinting) {
			StartCoroutine(SwitchMode());
		}
	}
	
	private void SwitchFiringMethod() {
		if(secondMode == FireMode.None && thirdMode == FireMode.None) {
			maxIndex = 0;
		}
		else if(thirdMode == FireMode.None) {
			maxIndex = 1;
		}
		
		if(fireModeIndex > maxIndex) {
			fireModeIndex = 0;
		}
		
		if(fireModeIndex == 0) {
			currentFireMode = firstMode;
		}
		if(fireModeIndex == 1 && secondMode != FireMode.None) {
			currentFireMode = secondMode;
		}
		if(fireModeIndex == 2 && thirdMode != FireMode.None) {
			currentFireMode = thirdMode;
		}
	}
	
	private float GetFireRate() {		
		if(fireModeIndex == 0) {
			return (firstRPM / 60f);
		}
		else if(fireModeIndex == 1) {
			return (secondRPM / 60f);
		}
		else if(fireModeIndex == 2) {
			return (thirdRPM / 60f);
		}
		
		return 0f;
	}
	
	private IEnumerator SwitchMode() {
		switching = true;
		canSwitchModes = false;
		firePos.GetComponent<AudioSource>().PlayOneShot(switchSound, 0.5f);
		yield return new WaitForSeconds(0.25f);
		fireModeIndex++;
		switching = false;
		canSwitchModes = true;
		SwitchFiringMethod();
		wm.CheckText();
	}
	
	private void WeaponGUI() {
        shootImpulseGUI = Mathf.Clamp(Mathf.Lerp(shootImpulseGUI, 1f, Time.unscaledDeltaTime * 5f), 1f, 1.11f);
		reloadImpulseGUI = Mathf.Clamp(Mathf.Lerp(reloadImpulseGUI, 1f, Time.unscaledDeltaTime * 5f), 1f, 1.11f);
		reloadSinAlpha = (reloading) ? Mathf.Abs(Mathf.Sin(Time.time * 5f)) * 0.4f : 0f;

        curAmmoDisplay.color = (percent <= 0.33f && !wm.pe.hasEMP) ? new Color(1f, 0.3f, 0.1f, 1f - reloadSinAlpha) : new Color(1f, 1f, 1f, 1f - reloadSinAlpha);

        curAmmoDisplay.cachedTrans.localScale = Vector3.one * shootImpulseGUI;
		curAmmoDisplay.text = wm.displayCurAmmo.ToString();
		ammoLeftDisplay.cachedTrans.localScale = Vector3.one * reloadImpulseGUI;
		ammoLeftDisplay.text = wm.displayAmmoLeft.ToString();

        if(!wm.pe.hasEMP) {
            ammoBar.value = DarkRef.LerpTowards(ammoBar.value, percent, Time.deltaTime * 8f, Time.deltaTime * 2f, 0.5f);
        }
	}
	
	public void ToggleFlashlight() {
		if(gunVisuals && gunVisuals.flashlight == null) {
			return;
		}

		flashlightOn = !flashlightOn;
		flashlight.GetComponent<AudioSource>().PlayOneShot(flashlightClick);
		flashlight.enabled = flashlightOn;
		
		if(Topan.Network.isConnected) {
			wm.rootNetView.RPC(Topan.RPCMode.OthersBuffered, "SetFlashlight", flashlightOn);
		}
	}

    public int GetMinimumRange() {
        int minRange = 0;
        for(int i = 0; i < bulletInfo.damageFalloff.length; i++) {
            Keyframe key = bulletInfo.damageFalloff[i];
            if(key.value >= 1f && key.time > minRange) {
                minRange = (int)key.time;
            }
        }

        return minRange;
    }

    public int GetMaximumRange() {
        return (int)bulletInfo.damageFalloff[bulletInfo.damageFalloff.length - 1].time;
    }
}