using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponHandler_Proxy : Topan.TopanMonoBehaviour
{
    public Transform weaponsParent;
    public AudioClip drawSound;
    public VisibilityControl renderV;
    public GameObject meleeObject;
    public Collider[] ignoreGrenades;

    [HideInInspector] public GunController currentGC;
    [HideInInspector] public GunVisuals currentVisuals;
    [HideInInspector] public GrenadeHandler_Proxy currentGHP;
    [HideInInspector] public List<PlasticExplosive> detonationList = new List<PlasticExplosive>();

    private MovementSync_Proxy msP;
    private Animator animator;
    private bool changingWeapons = false;
    private float muzzleBrightness;
    private float targetFireRate;
    private float lastFireTime;

    private float baseDelay;
    private float detonationDelay;
    private float lightTime;

    private AudioClip pullPinSound;
    private AudioClip throwSound;

    void Awake()
    {
        lastFireTime = -100f;
    }

    void NetworkStart()
    {
        msP = GetComponent<MovementSync_Proxy>();
        animator = GetComponent<Animator>();
    }

    [RPC]
    void NetworkShoot(float x, float y, Vector3 pos)
    {
        if (currentGC == null)
        {
            Debug.Log("Proxy gun is null");
            return;
        }

        if (Time.time - lastFireTime >= targetFireRate * 0.2f)
        {
            currentGC.firePos.GetComponent<AudioSource>().PlayOneShot(currentGC.fireSound);
            lastFireTime = Time.time;
        }

        GameObject poolProjectile = PoolManager.Instance.RequestInstantiate(currentGC.bulletInfo.poolIndex, pos, Quaternion.Euler(x * 360f, y * 360f, 0f), false);

        if (currentGC.bulletInfo.bulletType == BulletInfo.BulletType.Bullet)
        {
            Bullet bTracer = poolProjectile.GetComponent<Bullet>();
            bTracer.BulletInfo(currentGC.bulletInfo, -1, true);
            bTracer.InstantiateStart();
        }
        else if (currentGC.bulletInfo.bulletType == BulletInfo.BulletType.Rocket)
        {
            Rocket vRocket = poolProjectile.GetComponent<Rocket>();
            vRocket.RocketInfo(currentGC.bulletInfo, -1, true);
            vRocket.InstantiateStart();
        }

        if (renderV.isVisible && currentGC.ejectionEnabled && currentGC.ejectionPos != null)
        {
            Rigidbody bShell = PoolManager.Instance.RequestInstantiate(currentGC.bulletShellIndex, currentGC.ejectionPos.position, currentGC.ejectionPos.rotation).GetComponent<Rigidbody>();
            Vector3 randomForce = DarkRef.RandomVector3(currentGC.ejectionMinForce, currentGC.ejectionMaxForce);
            bShell.velocity = msP.velocity + transform.TransformDirection(randomForce);
            bShell.angularVelocity = Random.rotation.eulerAngles * currentGC.ejectionRotation;
        }

        if (currentVisuals.muzzleFlash != null && currentVisuals.muzzleGlow != null && currentVisuals.muzzleSpark != null)
        {
            StartCoroutine(MuzzleControl());
        }
    }

    [RPC]
    void NetworkReload(bool isEmpty)
    {
        if (currentGC == null)
        {
            Debug.Log("Proxy gun is null");
            return;
        }

        currentGC.firePos.GetComponent<AudioSource>().PlayOneShot((isEmpty && currentGC.reloadSoundEmpty != null) ? currentGC.reloadSoundEmpty : currentGC.reloadSound, 0.3f);
        //third person reload animations, etc...
    }

    private IEnumerator MuzzleControl()
    {
        if (Random.value < currentGC.muzzleProbability)
        {
            currentVisuals.muzzleFlash.Emit(1);
            currentVisuals.muzzleGlow.Emit(1);
            currentVisuals.muzzleSpark.Emit(1);

            currentGC.muzzleLight.enabled = true;
            currentGC.muzzleLight.range = Random.Range(3f, 4f);

            lightTime = 0f;
            float randomIntensity = Random.Range(0.9f, 1.1f);
            while (lightTime < 2f && currentGC.muzzleLight != null)
            {
                lightTime += Time.deltaTime * 25f;
                currentGC.muzzleLight.intensity = Mathf.PingPong(Mathf.Clamp(lightTime, 0f, 2f), 1f) * muzzleBrightness * randomIntensity;
                yield return 0;
            }

            currentGC.muzzleLight.intensity = 0f;
        }

        if (currentVisuals.muzzleSmoke != null)
        {
            currentVisuals.muzzleSmoke.Emit(1);
        }
    }

    private IEnumerator SelectWeaponRoutine(int id, int drawType)
    {
        changingWeapons = true;

        animator.SetBool("Draw", true);
        bool setDrawToFalse = false;
        while (!setDrawToFalse && animator != null)
        {
            if (animator.IsInTransition(2))
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(2);

                if (state.IsName("Torso.Hide"))
                {
                    animator.SetBool("Draw", false);
                    setDrawToFalse = true;
                }
            }

            yield return null;
        }

        foreach (Transform child in weaponsParent)
        {
            Destroy(child.gameObject);
        }

        if (drawType == 0)
        {
            DrawWeapon(id);
        }
        else if (drawType == 1)
        {
            DrawGrenade(id);
        }
        else if (drawType == 2)
        {
            DrawSpecial(id);
        }

        bool gotDraw = false;
        while (!gotDraw && animator != null)
        {
            if (animator.IsInTransition(2))
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(2);

                if (state.IsName("Torso.Draw"))
                {
                    gotDraw = true;
                }
            }
            yield return null;
        }

        changingWeapons = false;
    }

    private void DrawWeapon(int weaponID)
    {
        meleeObject.SetActive(false);

        GunController toInstantiate = WeaponDatabase.GetWeaponByID(weaponID);
        currentGC = (GunController)Instantiate(toInstantiate);
        currentGC.transform.parent = weaponsParent;
        currentGC.transform.localPosition = toInstantiate.thirdPersonPosition;
        currentGC.transform.localRotation = toInstantiate.thirdPersonRotation;
        currentGC.StripFunctions(true);
        currentVisuals = currentGC.GetComponent<GunVisuals>();
        muzzleBrightness = currentGC.muzzleLight.intensity;
        GetComponent<AudioSource>().PlayOneShot(drawSound, 0.7f);

        targetFireRate = 60f / Mathf.Max(currentGC.firstRPM, (currentGC.secondMode != GunController.FireMode.None) ? currentGC.secondRPM : 0f, (currentGC.thirdMode != GunController.FireMode.None) ? currentGC.thirdRPM : 0f);
        currentGHP = null;
        pullPinSound = null;
        throwSound = null;

        if (currentVisuals)
        {
            foreach (GameObject go in currentVisuals.activateOnUse)
            {
                go.SetActive(false);
            }

            currentGC.firePos.GetComponent<AudioSource>().volume = 1f;
        }
    }

    private void DrawGrenade(int grenadeID)
    {
        meleeObject.SetActive(false);

        GrenadeController toInstantiate = GrenadeDatabase.GetGrenadeByID(grenadeID);
        GrenadeController grenInstance = (GrenadeController)Instantiate(toInstantiate);
        grenInstance.transform.parent = weaponsParent;
        grenInstance.transform.localPosition = toInstantiate.thirdPersonPosition;
        grenInstance.transform.localRotation = toInstantiate.thirdPersonRotation;
        grenInstance.PrepareForMultiplayer();
        GetComponent<AudioSource>().PlayOneShot(drawSound, 0.7f);

        pullPinSound = toInstantiate.pullPinSound;
        throwSound = toInstantiate.throwSound;
        currentGHP = grenInstance.GetComponent<GrenadeHandler_Proxy>();
        currentGHP.whp = this;
        currentGHP.grenadePrefab = grenInstance.grenadePrefab;
        currentGHP.displayMesh = grenInstance.displayMesh;
        currentGHP.throwStr = grenInstance.throwStrength;
        currentGHP.tossStr = grenInstance.tossStrength;
        currentGHP.ignoreColliders = ignoreGrenades;
        baseDelay = grenInstance.baseDelay;
        detonationDelay = grenInstance.detonationDelay;
        Destroy(grenInstance);

        currentGC = null;
        currentVisuals = null;
    }

    private void DrawSpecial(int specialID)
    {
        if (specialID == 0)
        {
            //Drawing Hands
        }
        else if (specialID == 1)
        {
            meleeObject.SetActive(true);
        }
    }

    public void DestroyWeapons()
    {
        for (int i = 0; i < weaponsParent.childCount; i++)
        {
            Destroy(weaponsParent.GetChild(i).gameObject);
        }
    }

    #region Explosive RPCs
    [RPC]
    void NetworkSelectGrenade(byte grenadeID)
    {
        StopCoroutine("SelectWeaponRoutine");
        StartCoroutine(SelectWeaponRoutine((int)grenadeID, 1));
    }

    [RPC]
    void PullPinGrenade(int grenadeID)
    {
        if (currentGHP == null)
        {
            Debug.Log("Grenade handler is null");
            return;
        }

        currentGHP.DoPullPin(pullPinSound, grenadeID);
    }

    [RPC]
    void ThrowGrenade(Vector3 direction, Vector3 position)
    {
        if (currentGHP == null)
        {
            Debug.Log("Grenade handler is null");
            return;
        }

        currentGHP.DoThrow(throwSound, direction, position);
    }

    [RPC]
    void DetonateExplosives()
    {
        GetComponent<AudioSource>().PlayOneShot(pullPinSound); //Detonation sound.

        for (int i = 0; i < detonationList.Count; i++)
        {
            detonationList[i].Detonate(baseDelay + (i * detonationDelay));
        }

        detonationList.Clear();
    }
    #endregion

    [RPC]
    void RefreshWeapon(byte weaponID)
    {
        StopCoroutine("SelectWeaponRoutine");
        StartCoroutine(SelectWeaponRoutine((int)weaponID, 0));
    }

    [RPC]
    void SetSpecialActive(byte specialID)
    {
        StopCoroutine("SelectWeaponRoutine");
        StartCoroutine(SelectWeaponRoutine((int)specialID, 2));
    }

    [RPC]
    void SetFlashlight(bool on)
    {
        if (currentVisuals != null && currentVisuals.flashlight != null)
        {
            StartCoroutine(FlashlightRoutine(on));
        }
    }

    private IEnumerator FlashlightRoutine(bool on)
    {
        while (changingWeapons)
        {
            yield return null;
        }

        currentVisuals.flashlight.enabled = on;
    }

    [RPC]
    void NetworkEmptyClick()
    {
        if (currentGC == null)
        {
            Debug.Log("Proxy gun is null");
            return;
        }

        currentGC.firePos.GetComponent<AudioSource>().PlayOneShot(currentGC.emptySound, 0.4f);
    }

    public void DeleteInactiveExplosives(float delay)
    {
        for (int i = 0; i < detonationList.Count; i++)
        {
            detonationList[i].RemoveInstance(delay);
        }
    }
}