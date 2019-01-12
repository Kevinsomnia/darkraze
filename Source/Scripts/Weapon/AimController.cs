using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class AimController : MonoBehaviour
{
    public float aimSpeed = 3.1f;
    public float aimFovBoost = 6f;
    public float fovInterpolateSpeed = 10f;

    public float intensity = 4;
    public float blur = 2;
    public float effectSmoothing = 5;

    private PlayerMovement pm;
    private WeaponManager wm;
    private PlayerVitals pv;
    private AntiClipSystem acs;
    private VignettingC vc;
    private GunController gc;
    private CameraBob cb;
    private DynamicMovement dm;
    private PlayerLook pl;
    private ImpactAnimation ia;
    private WeaponDepthOfField wdof;

    [HideInInspector] public bool aimEffect;
    [HideInInspector] public bool isAiming;
    [HideInInspector] public float shakeIntensity;
    [HideInInspector] public Vector3 flinchPos;
    [HideInInspector] public Vector3 flinchRot;
    [HideInInspector] public Vector3 antiClipVector;
    [HideInInspector] public Vector3 shootShake;
    [HideInInspector] public Vector3 lerpPos;

    public float aimTransition
    {
        get
        {
            if (gc == null)
            {
                return 0f;
            }

            return Mathf.Round((lerpPos.magnitude / gc.aimPos.magnitude) * 100f) / 100f;
        }
    }

    public bool isTransitioning
    {
        get
        {
            return !(aimTransition <= 0f || aimTransition >= 1f);
        }
    }

    public float zoomFactor
    {
        get
        {
            return settingsFOV / GetComponent<Camera>().fieldOfView;
        }
    }

    private Transform _tr;
    private Transform cachedTr
    {
        get
        {
            if (_tr == null)
            {
                _tr = transform;
            }

            return _tr;
        }
    }

    private bool hasStartAiming;
    private float tilt;
    private float lastAim;
    private float lastUnAim;
    private float aimBlurValue;
    private float velocity;
    private float fallAnim;
    private float settingsFOV;

    [HideInInspector] public float lastRecoilTime;
    [HideInInspector] public float rX;
    [HideInInspector] public float rY;

    [HideInInspector] public float idleX;
    [HideInInspector] public float idleY;
    [HideInInspector] public float idleZ;

    private float noisePos;
    private float lastShake;
    private float shakeX;
    private float shakeY;

    //DoF stuff
    private float dofBlurAmount;
    private float dofBlurDistance;
    private float dofBlurAperture;
    private float dofBlurFocalSize;
    private float dofBlendBlur;

    void Awake()
    {
        GeneralVariables.mainPlayerCamera = GetComponent<Camera>();
    }

    void Start()
    {
        PlayerReference pRef = GeneralVariables.playerRef;

        pm = GeneralVariables.player.GetComponent<PlayerMovement>();
        wm = pRef.wm;
        dm = pRef.dm;
        pl = pm.GetComponent<PlayerLook>();
        pv = pm.GetComponent<PlayerVitals>();
        vc = GetComponent<VignettingC>();
        wdof = GetComponent<WeaponDepthOfField>();
        cb = pRef.cb;
        acs = pRef.acs;
        ia = pRef.ia;

        GetComponent<Camera>().fieldOfView = (float)GameSettings.settingsController.FOV;

        cachedTr.localPosition = Vector3.zero;
        lastUnAim = -0.25f;
        lastAim = -0.25f;
        lastShake = -100f;
    }

    void Update()
    {
        GameSettings sManager = GameSettings.settingsController;

        noisePos += Time.deltaTime * 0.58f;
        float perlinX = (Mathf.PerlinNoise(noisePos, 0f) - 0.5f);
        float perlinY = (Mathf.PerlinNoise(0f, noisePos) - 0.5f);
        idleX = Mathf.Lerp(idleX, perlinX * ((isAiming) ? 0.36f : 0.55f), Time.deltaTime * 12f);
        idleY = Mathf.Lerp(idleY, perlinY * ((isAiming) ? 0.36f : 0.55f), Time.deltaTime * 12f);
        idleZ = Mathf.Lerp(idleZ, (Mathf.PerlinNoise(noisePos * 0.4f, noisePos * 0.3f) - 0.5f) * ((isAiming) ? 0.3f : 0.45f), Time.deltaTime * 12f);

        shakeX = Mathf.PerlinNoise(Mathf.PingPong(Time.time * 19.5f, 750f), -501f) - 0.5f;
        shakeY = Mathf.PerlinNoise(32f, Mathf.PingPong(Time.time * 19.5f, 650f)) - 0.5f;

        gc = wm.currentGC;
        float spdFactor = 1f;
        if (!RestrictionManager.restricted)
        {
            if (gc != null && !wm.prepareToSwitch && !dm.animationIsPlaying && !pm.onLadder && !dm.terminalVelocity)
            {
                bool aimCheck = (!pm.sprinting && !pm.wasSprinting && !gc.reloading && !acs.clipping && !hasStartAiming && (Time.time - lastUnAim) > 0.25f);
                bool aimSettings = (sManager.aimToggle == "Hold (Press)");
                bool aimMethod = (aimSettings) ? cInput.GetButton("Aim") : cInput.GetButtonDown("Aim");
                if (aimMethod && aimCheck)
                {
                    isAiming = true;
                    hasStartAiming = true;
                    pv.jumpRattleEquip = true;
                    lastAim = Time.time;
                }

                bool unaimBool = (aimSettings ? !cInput.GetButton("Aim") : cInput.GetButtonDown("Aim")) && (hasStartAiming && (Time.time - lastAim) > 0.25f);
                if ((unaimBool && !pl.isBreathing) || gc.reloading)
                {
                    isAiming = false;
                    hasStartAiming = false;
                    lastUnAim = Time.time;
                }
                if (unaimBool)
                {
                    pv.jumpRattleEquip = true;
                }

                spdFactor = gc.aimSpeedFactor;
            }
            else
            {
                isAiming = false;
                hasStartAiming = false;
                aimEffect = false;
            }
        }

        bool wepEnableDof = false;
        if (gc != null && gc.dofBlurAmount > 0f)
        {
            wepEnableDof = true;
            dofBlurAmount = gc.dofBlurAmount;
            dofBlurDistance = gc.dofBlurDistance;
            dofBlurAperture = gc.dofBlurAperture;
            dofBlurFocalSize = gc.dofBlurFocalSize;
            dofBlendBlur = gc.dofBlendBlur;
        }

        if (aimEffect)
        {
            vc.sniperBlur = Mathf.Lerp(vc.sniperBlur, blur, Time.deltaTime * effectSmoothing);
            vc.sniperIntensity = Mathf.Lerp(vc.sniperIntensity, intensity, Time.deltaTime * effectSmoothing);
        }
        else
        {
            vc.sniperBlur = Mathf.Lerp(vc.sniperBlur, 0f, Time.deltaTime * effectSmoothing);
            vc.sniperIntensity = Mathf.Lerp(vc.sniperIntensity, 0f, Time.deltaTime * effectSmoothing);
        }

        fallAnim = Mathf.Lerp(fallAnim, (ia.impactY > 0f) ? ia.impactY : 0f, Time.deltaTime * 3f);
        float upwardKickAnim = (dm.kickPos.z < -0.0001f && !dm.isEmptyClick) ? (dm.kickPosReal.z / dm.kickPos.z) * 0.65f * pl.camAnimation : 0f;
        tilt = Mathf.Lerp(tilt, Mathf.Clamp(pl.xVelocity * 0.6f * 0.01f, -3f, 3f), Time.deltaTime * 4f); //Mouse
        shootShake = new Vector3((Mathf.PerlinNoise(noisePos * 25f, 0f) - 0.5f), (Mathf.PerlinNoise(0f, noisePos * 25f) - 0.5f), 0f) * (0.2f - Mathf.Clamp(Time.time - lastShake, 0f, 0.2f)) * 5f;
        cachedTr.localRotation = Quaternion.Euler((pl.shakePos * 160f) + (shootShake * 0.75f) + flinchRot + new Vector3(-fallAnim * 36f + idleX - Mathf.Pow(upwardKickAnim, 0.6f), idleY - (upwardKickAnim * ((Mathf.PerlinNoise(Mathf.PingPong(Time.time * 12f, 50f), 0f)) - 0.5f) * 2.5f), tilt + (idleZ * 2f) + (fallAnim * 12f) + (pl.recoilZ * 1.3f) - (cb.translateChangeX * 0.9f)) + new Vector3(shakeX, shakeY, 0f) * shakeIntensity * 0.6f * (Mathf.Clamp(ia.shakeTime - Time.time, 0f, 0.5f)));

        settingsFOV = (float)sManager.FOV;

        if ((isAiming || pl.isBreathing) && !acs.clipping)
        {
            lerpPos = DarkRef.LerpTowards(lerpPos, gc.aimPos, Time.deltaTime * 10f, Time.deltaTime * aimSpeed * 0.956f * spdFactor, 0.5f);
            GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, settingsFOV - (aimFovBoost * (1f + (((settingsFOV - 60) / 30f) * 2.75f))) - ((gc != null) ? gc.addZoomFOV : 0f), Time.deltaTime * spdFactor * fovInterpolateSpeed);
            pv.aimEffect = Mathf.Lerp(pv.aimEffect, 2.5f, Time.deltaTime * 8f);

            DarkRef.ToggleComponent(wdof, sManager.wDepthOfField == 1 && wepEnableDof);
            wdof.maxBlurSize = dofBlurAmount;
            wdof.focalLength = dofBlurDistance;
            wdof.aperture = dofBlurAperture;
            wdof.focalSize = dofBlurFocalSize;
            wdof.foregroundOverlap = dofBlendBlur;
        }
        else
        {
            lerpPos = DarkRef.LerpTowards(lerpPos, Vector3.zero, Time.deltaTime * 10f, Time.deltaTime * aimSpeed * spdFactor, 0.5f);
            GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, settingsFOV, Time.deltaTime * spdFactor * fovInterpolateSpeed);
            pv.aimEffect = Mathf.Lerp(pv.aimEffect, 0f, Time.deltaTime * 8f);

            DarkRef.ToggleComponent(wdof, false);
        }

        cachedTr.localPosition = (pl.shakePos * 0.3f) + (flinchPos * 0.75f) + antiClipVector;
    }

    public void ShootShake(float intensity)
    {
        shakeIntensity = intensity;
        lastShake = Time.time;
    }
}