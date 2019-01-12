using UnityEngine;
using System.Collections;
using Topan.CustomTypes;

public class BotWeapons : Topan.TopanMonoBehaviour {
    public Transform weaponsParent;
    public int startingWeapon = 3;

    [Tooltip("Look dot product must be greater than this in order to shoot")]
    public float lookThreshold = 0.8f;

    [HideInInspector] public Vector3 aimRot;

    //private BotMovement bm;
    private BotVitals bv;
    private Animator animator;
    private GunController currentGun;
    private GunVisuals currentVisuals;
    private float muzzleBrightness;
    private bool isSwitchingWeapons = false;
    private bool isReloadingWeapon = false;
    private float lastShootTime;
    private float curSpread;
    private float lookDot;
    private float lightTime;
    private Quaternion randomRot = Quaternion.identity;

    public void NetworkStart() {
        //bm = GetComponent<BotMovement>();
        bv = GetComponent<BotVitals>();
        animator = GetComponent<Animator>();
        lastShootTime = -100f;

        int startWep = startingWeapon;
        if(topanNetworkView.initialData.ContainsKey("sw")) {
            startWep = (byte)topanNetworkView.GetInitialData("sw");
        }

        StartCoroutine(SelectWeaponRoutine(startWep, 0));
    }

    void Update() {
        if(Topan.Network.isServer) {
            if(!bv.isDead && currentGun != null && GeneralVariables.Networking.matchStarted && !GeneralVariables.Networking.finishedGame) {
                if(Time.time - lastShootTime >= Mathf.Min(0.5f, (1f + Time.deltaTime) / (currentGun.firstRPM / 60f))) {
                    curSpread = Mathf.MoveTowards(curSpread, currentGun.baseSpreadAmount, Time.deltaTime * currentGun.recoverSpeed);
                }
                else {
                    curSpread = Mathf.MoveTowards(curSpread, currentGun.baseSpreadAmount, Time.deltaTime * currentGun.recoverSpeed * 0.1f);
                }

                aimRot = transform.eulerAngles;
                /*
                if(bm.currentTarget != null && bm.canSeeTarget && Time.time - lastShootTime >= 1f / (currentGun.firstRPM / 60f)) {
                    BaseStats targetBS = bm.currentTarget.GetComponent<BaseStats>();

                    if(targetBS != null && targetBS.curHealth > 0) {
                        if(currentGun.currentAmmo > 0) {
                            aimRot.x -= bm.lookAngle;

                            Vector3 targetPosRelative = bm.currentTarget.position;
                            targetPosRelative.y = transform.position.y;
                            lookDot = Vector3.Dot((targetPosRelative - transform.position).normalized, transform.forward);

                            if(lookDot > lookThreshold) {
                                ShootWeapon(aimRot.x, aimRot.y);
                            }
                        }
                        else if(currentGun.ammoLeft > 0) {
                            BotReloadWeapon();
                        }
                    }
                }*/
            }
        }
    }

    private IEnumerator SelectWeaponRoutine(int id, int drawType) {
        isSwitchingWeapons = true;

        animator.SetBool("Draw", true);
        bool stopDraw = false;
        while(!stopDraw && animator != null) {
            if(animator.IsInTransition(2)) {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(2);

                if(state.IsName("Torso.Hide")) {
                    animator.SetBool("Draw", false);
                    stopDraw = true;
                }
            }

            yield return null;
        }

        foreach(Transform child in weaponsParent) {
            Destroy(child.gameObject);
        }

        if(drawType == 0) {
            DrawWeapon(id);
        }

        bool gotDraw = false;
        while(!gotDraw && animator != null) {
            if(animator.IsInTransition(2)) {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(2);

                if(state.IsName("Torso.Draw")) {
                    gotDraw = true;
                }
            }

            yield return null;
        }

        isSwitchingWeapons = false;
    }

    private void DrawWeapon(int weaponID) {
        GunController toInstantiate = WeaponDatabase.GetWeaponByID(weaponID);
        currentGun = (GunController)Instantiate(toInstantiate);
        currentGun.transform.parent = weaponsParent;
        currentGun.transform.localPosition = toInstantiate.thirdPersonPosition;
        currentGun.transform.localRotation = toInstantiate.thirdPersonRotation;
        currentGun.PrepareForBot();

        currentVisuals = currentGun.GetComponent<GunVisuals>();
        muzzleBrightness = currentGun.muzzleLight.intensity;

        curSpread = currentGun.baseSpreadAmount;
    }

    [RPC]
    public void ShootWeapon(float xRot, float yRot) {
        if(currentGun == null) {
            return;
        }

        for(int i = 0; i < ((Topan.Network.isServer) ? currentGun.bulletsPerShot : 1); i++) {
            Vector2 randomTargetPoint = Random.insideUnitCircle * curSpread;
            Vector3 randomDir = new Vector3(randomTargetPoint.x, randomTargetPoint.y, 0f);

            if(Topan.Network.isServer) {
                topanNetworkView.RPC(Topan.RPCMode.Others, "ShootWeapon", (TopanFloat)ApplyRotation(xRot + randomDir.x), (TopanFloat)ApplyRotation(yRot + randomDir.y));
                randomRot = Quaternion.Euler(new Vector3(xRot, yRot, 0f) + randomDir);
            }
            else {
                randomRot = Quaternion.Euler(new Vector3(xRot, yRot, 0f) * 360f);
            }

            GameObject proj = PoolManager.Instance.RequestInstantiate(currentGun.bulletInfo.poolIndex, currentGun.firePos.position, randomRot, false);

            /*
            if(currentGun.bulletInfo.bulletType == BulletInfo.BulletType.Bullet) {
                Bullet projBul = proj.GetComponent<Bullet>();
                projBul.BulletInfo(currentGun.bulletInfo, currentGun.weaponID, !Topan.Network.isServer, false, bm.myIndex);
                projBul.InstantiateStart();
            }
            else if(currentGun.bulletInfo.bulletType == BulletInfo.BulletType.Rocket) {
                Rocket projRoc = proj.GetComponent<Rocket>();
                projRoc.RocketInfo(currentGun.bulletInfo, currentGun.weaponID, !Topan.Network.isServer, false, bm.myIndex);
                projRoc.InstantiateStart();
            }*/
        }

        currentGun.firePos.GetComponent<AudioSource>().PlayOneShot(currentGun.fireSound);

        if(currentGun.currentAmmo > 0) {
            currentGun.currentAmmo--;
        }

        curSpread += currentGun.spreadSpeed;

        StartCoroutine(MuzzleControl());
        lastShootTime = Time.time;
    }

    private IEnumerator MuzzleControl() {
        if(Random.value < currentGun.muzzleProbability) {
            currentVisuals.muzzleFlash.Emit(1);
            currentVisuals.muzzleGlow.Emit(1);
            currentVisuals.muzzleSpark.Emit(1);

            currentGun.muzzleLight.enabled = true;
            currentGun.muzzleLight.range = Random.Range(3f, 4f);

            lightTime = 0f;
            float randomIntensity = Random.Range(0.9f, 1.1f);
            while(lightTime < 2f && currentGun.muzzleLight != null) {
                lightTime += Time.deltaTime * 25f;
                currentGun.muzzleLight.intensity = Mathf.PingPong(Mathf.Clamp(lightTime, 0f, 2f), 1f) * muzzleBrightness * randomIntensity;
                yield return 0;
            }
        }

        if(currentVisuals.muzzleSmoke != null) {
            currentVisuals.muzzleSmoke.Emit(1);
        }
    }

    [RPC]
    public void BotReloadWeapon() {
        if(currentGun == null || isReloadingWeapon) {
            return;
        }

        if(Topan.Network.isServer) {
            topanNetworkView.RPC(Topan.RPCMode.Others, "BotReloadWeapon");
        }

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine() {
        if(currentGun == null) {
            yield break;
        }

        isReloadingWeapon = true;
        currentGun.firePos.GetComponent<AudioSource>().PlayOneShot(currentGun.reloadSoundEmpty, 0.5f);

        float timer = 0f;
        while(timer < currentGun.reloadLengthEmpty) {
            if(currentGun == null) {
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if(currentGun.ammoLeft > 0) {
            int diff = Mathf.Clamp(currentGun.clipSize - currentGun.currentAmmo, 0, currentGun.ammoLeft);
            currentGun.currentAmmo += diff;
            currentGun.ammoLeft -= diff;
        }

        isReloadingWeapon = false;
        curSpread = currentGun.baseSpreadAmount;
    }

    public void StopReloadSound() {
        currentGun.firePos.GetComponent<AudioSource>().enabled = false;
    }

    private float ApplyRotation(float rot) {
        return Mathf.Clamp01((rot % 360f) / 360f);
    }
}