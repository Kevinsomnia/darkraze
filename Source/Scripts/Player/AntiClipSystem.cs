using UnityEngine;
using System.Collections;

public class AntiClipSystem : MonoBehaviour
{
    public LayerMask layers;
    public float rayLength = 1.0f;
    public float distThreshold = 0.375f;
    public float distMultiplier = 0.25f;
    public float aimDistMultiplier = 0.2f;
    public GameObject antiClipObj;
    public float smoothing = 8f;
    public float clipMultiplier = 1.5f;
    public AnimationCurve overshootCurve; //domain and range should be from 0 to 1.
    public float overshootMultiplier = 10f;

    [HideInInspector] public bool clipping;

    private Vector3 defaultPos;
    private Vector3 defaultRot;
    private WeaponManager wm;
    private DynamicMovement dm;
    private PlayerMovement pm;
    private AntiClipVariables acv;
    private AimController ac;

    private Vector3 mainRot;
    private Vector3 antiClipPos;
    private Vector3 rotateWeaponTr;
    private Vector3 drawOffset;

    private float switchRotX;
    private float switchRotY;
    private float switchRotZ;

    private float startTime;
    private Transform tr;
    private Transform acoTr;
    private Transform lastWepTrans;

    void Awake()
    {
        tr = transform;
        acoTr = antiClipObj.transform;

        PlayerReference pr = GeneralVariables.playerRef;
        wm = pr.wm;
        pm = pr.GetComponent<PlayerMovement>();
        dm = pr.dm;
        ac = pr.ac;

        switchRotX = dm.drawRot.x;
        switchRotY = -dm.drawRot.y;
        switchRotZ = dm.drawRot.z;
    }

    void Start()
    {
        defaultPos = acoTr.localPosition;
        defaultRot = acoTr.localEulerAngles;
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time - startTime < 0.15f)
        {
            switchRotX = dm.drawRot.x;
            switchRotY = -dm.drawRot.y;
            switchRotZ = dm.drawRot.z;
        }

        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (wm.currentWeaponTransform != null && lastWepTrans != wm.currentWeaponTransform)
        {
            acv = wm.currentWeaponTransform.GetComponent<AntiClipVariables>();
            lastWepTrans = wm.currentWeaponTransform;
        }

        RaycastHit hit1;
        RaycastHit hit2;
        Physics.Raycast(tr.position, Vector3.up, out hit2, 0f);
        antiClipPos = Vector3.zero;
        float lerpRotation = 0f;
        ac.antiClipVector = Vector3.Lerp(ac.antiClipVector, Vector3.zero, Time.deltaTime * 6.5f);
        if (Physics.Raycast(tr.position, tr.forward, out hit1, rayLength, layers.value) || Physics.Raycast(tr.position - (tr.right * 0.15f), tr.forward, out hit2, rayLength * 0.95f, layers.value))
        {
            float distFromObj = Mathf.Max(hit1.distance, hit2.distance);
            if (rayLength - distFromObj < distThreshold)
            {
                clipping = false;
                float rDepth = rayLength - distFromObj;
                antiClipPos = -Vector3.forward * rDepth * distMultiplier;
                ac.antiClipVector = Vector3.Lerp(ac.antiClipVector, -Vector3.forward * rDepth * aimDistMultiplier, Time.deltaTime * 6.5f);
                mainRot = DarkRef.LerpTowards(mainRot, defaultRot, Time.deltaTime * smoothing, Time.deltaTime * 280f, 0.25f);
            }
            else
            {
                clipping = true;
                lerpRotation = Mathf.Clamp((Mathf.Max(0.55f - distFromObj) / 0.27f) + 0.7f, 0.7f, 1f);
                if (wm.currentWeaponTransform != null && acv != null && !pm.sprinting && !pm.onLadder)
                {
                    mainRot = DarkRef.LerpTowards(mainRot, Vector3.Lerp(defaultRot, acv.antiClipRot, lerpRotation), Time.deltaTime * smoothing, Time.deltaTime * 280f, 0.25f);
                }
            }
        }
        else
        {
            clipping = false;
            mainRot = Vector3.Lerp(mainRot, defaultRot, Time.deltaTime * smoothing);
        }

        rotateWeaponTr = DarkRef.LerpTowards(rotateWeaponTr, (dm.rotateWeaponTransform || pm.onLadder) ? Vector3.zero : dm.curSprintRot, Time.deltaTime * ((ac.isAiming) ? 8.1f : 6.9f) * ((pm.wasSprinting) ? 0.8f : 1f), Time.deltaTime * dm.sprintRot.magnitude * 2.5f, 0.18f);
        switchRotX = Mathf.Lerp(switchRotX, dm.switchRot.x, Time.deltaTime * 7.3f);
        switchRotY = Mathf.Lerp(switchRotY, dm.switchRot.y, Time.deltaTime * 6.6f);
        switchRotZ = Mathf.Lerp(switchRotZ, dm.switchRot.z, Time.deltaTime * 6.7f);

        Vector3 realSwitchRot = new Vector3(switchRotX - (overshootCurve.Evaluate((1f - Mathf.Clamp01(Mathf.Abs(switchRotY) / Mathf.Abs(Mathf.Max(0.001f, dm.drawRot.y))))) * overshootMultiplier), switchRotY, switchRotZ);

        drawOffset = Vector3.Lerp(drawOffset, Vector3.up * dm.drawOffset, Time.deltaTime * 6.6f);
        acoTr.localPosition = drawOffset + Vector3.Lerp(acoTr.localPosition, antiClipPos + ((acv != null) ? Vector3.Lerp(defaultPos, acv.antiClipPos, lerpRotation) : defaultPos), Time.deltaTime * 6.5f);
        acoTr.localRotation = Quaternion.Euler(mainRot + realSwitchRot + rotateWeaponTr);
    }
}