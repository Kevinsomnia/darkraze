using UnityEngine;
using System.Collections;

public class GUISway : MonoBehaviour
{
    public bool isHUD = true; //Mainly for player HUD, uncheck for natural and general sway.
    public Vector2 swayAmount = new Vector2(0.1f, 0.1f);
    public float bobAmount = 1f;
    public float sprintBobAmount = 1.5f;
    public float recoilIntensity = 1f;
    public float recoilClamp = 3f;
    public float shakeIntensity = 45f;
    public float zoomAmount = 2f;
    public float velocityRattle = 0.005f;

    [HideInInspector] public Vector3 focusRot;

    private float x;
    private float y;
    private float transX;
    private float transY;
    private float bobbingAmount;
    private Vector3 extraEffectRot;
    private Vector3 defaultRot;

    private Transform tr;
    private Vector3 defaultPos;
    private Vector3 smoothedRot;
    private PlayerMovement pm;
    private PlayerLook pl;
    private ImpactAnimation ia;
    private DynamicMovement dm;
    private AimController ac;
    private CameraBob cb;
    private bool initialized;

    void Start()
    {
        tr = transform;
        defaultPos = tr.localPosition;
        defaultRot = tr.localRotation.eulerAngles;

        if (!isHUD)
        {
            return;
        }

        InitializeVariables();
    }

    public void InitializeVariables()
    {
        PlayerReference pr = GeneralVariables.playerRef;
        if (pr != null)
        {
            pm = pr.GetComponent<PlayerMovement>();
            pl = pr.GetComponent<PlayerLook>();
            dm = pr.dm;
            ia = pr.ia;
            ac = pr.ac;
            cb = pr.cb;

            initialized = true;
        }
    }

    void Update()
    {
        if (isHUD && !initialized)
        {
            return;
        }

        if (!RestrictionManager.restricted)
        {
            bool isValidHUD = (isHUD && GeneralVariables.player != null && pm.controller != null);
            float inputX = (isValidHUD ? pl.xVelocity : cInput.GetAxis("Horizontal Look"));
            float inputY = (isValidHUD ? -pl.yVelocity : -cInput.GetAxis("Vertical Look"));

            x = Mathf.Clamp(inputX * swayAmount.x * 0.018f, Mathf.Abs(swayAmount.x) * -25f, Mathf.Abs(swayAmount.x) * 25f);
            y = Mathf.Clamp(inputY * swayAmount.y * 0.018f, Mathf.Abs(swayAmount.y) * -25f, Mathf.Abs(swayAmount.y) * 25f);

            if (isHUD && pm != null)
            {
                bobbingAmount = (pm.sprinting) ? sprintBobAmount : bobAmount;
            }
        }
        else
        {
            x = 0f;
            y = 0f;
        }

        if (isHUD)
        {
            float aimFactor = (ac != null && ac.isAiming) ? 2f : 1f;
            tr.localPosition = defaultPos + Vector3.forward * ((!dm.isEmptyClick) ? Mathf.Clamp(-dm.kickPosReal.z * 220f * aimFactor * recoilIntensity, -recoilClamp, recoilClamp) : 0f);

            GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, (ac != null && ac.isAiming) ? (60f - (zoomAmount * ac.zoomFactor)) : 60f, Time.deltaTime * 10f);
            transX = (cb.translateChangeX * bobbingAmount);
            transY = (cb.translateChangeY * bobbingAmount) + (ia.currentPos.y * 5f * ((ia.currentPos.y < 0f) ? 0.55f : 1f));

            extraEffectRot = (pl.shakePos * shakeIntensity) + (ac.shootShake * 0.3f) + (dm.rattleVector * velocityRattle);
        }
        else
        {
            extraEffectRot = Vector3.zero;
            bobbingAmount = 0f;
            transX = 0f;
            transY = 0f;
        }

        smoothedRot = Vector3.Lerp(smoothedRot, new Vector3(y + transY, x - transX, 0f), Time.deltaTime * 10.5f);
        tr.localRotation = Quaternion.Euler(defaultRot + smoothedRot + extraEffectRot + focusRot);
    }
}