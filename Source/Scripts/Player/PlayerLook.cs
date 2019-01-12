using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerLook : MonoBehaviour
{
    public float minimumY = -70;
    public float maximumY = 80;
    public Vector2 mouseSmoothingRange = new Vector2(15f, 35f);
    public Transform head;
    public float useBreathSpeed = 12f;
    public float rechargeBreath = 15f;
    public AudioSource breathSource;
    public AudioClip startBreath;
    public AudioClip endBreath;

    private float ladderLook;
    private Vector3 defHeadRot;

    private float idleAnim;
    private float timer;
    private bool shaking;

    [HideInInspector] public float xVelocity;
    [HideInInspector] public float yVelocity;

    [HideInInspector] public bool isBreathing;
    [HideInInspector] public Vector3 shakePos;
    [HideInInspector] public float aimMouseModifier;
    [HideInInspector] public float sineVal;
    [HideInInspector] public float xRot;
    [HideInInspector] public float yRot;
    [HideInInspector] public float recoilX;
    [HideInInspector] public float recoilY;
    [HideInInspector] public float recoilZ;
    [HideInInspector] public float inputX;
    [HideInInspector] public float inputY;
    [HideInInspector] public float sniperSwayFactor;
    [HideInInspector] public float magnificationFactor;
    [HideInInspector] public float ladderClampAnim;
    [HideInInspector] public float camAnimation = 1f;

    private Transform tr;
    private GameSettings settingsController;
    private PlayerMovement pm;
    private WeightController wc;
    private WeaponManager wm;
    private AimController ac;

    private float ladderLookReturn;
    private float autoReturnY;
    private float lastRecoil;
    private float returnSpeed;

    private bool canBreath;
    private float availBreath;
    private float deepBreathSway;
    private float lastBreath;
    private float breathTime;

    private Vector3 flinch;
    private Vector3 realFlinch;
    private float targetZRot;
    private float flinchSpeed;
    private float timeSinceLastFlinch;

    private float xRecoil = 0f;
    private float yRecoil = 0f;
    private float xRecoilSmooth = 0f;
    private float yRecoilSmooth = 0f;

    private float drawX;
    private float drawY;

    void Start()
    {
        tr = transform;
        settingsController = GameSettings.settingsController;
        pm = GetComponent<PlayerMovement>();

        PlayerReference pr = GeneralVariables.playerRef;
        ac = pr.ac;
        wc = pr.wc;
        wm = pr.wm;

        defHeadRot = head.localEulerAngles;
        xRot = tr.localEulerAngles.y;
        aimMouseModifier = 1f;
        magnificationFactor = 1f;
        availBreath = 100f;
        deepBreathSway = 1f;
        canBreath = true;
    }

    void Update()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        float mouseSmoothing = (settingsController.mouseSmoothing <= 0f) ? 120f : Mathf.Lerp(mouseSmoothingRange.y, mouseSmoothingRange.x, settingsController.mouseSmoothing);

        float swayFactor = 0f;
        if (wm.currentGC != null && ac.isAiming)
        {
            swayFactor = wm.currentGC.aimSwayFactor * ((pm.crouching) ? 0.8f : 1f);

            if (!isBreathing && Time.time - lastBreath >= 1f && cInput.GetButtonDown("Run") && swayFactor > 0f && availBreath > 0f && canBreath)
            {
                AudioSource.PlayClipAtPoint(startBreath, tr.position, 0.15f);
                isBreathing = true;
                breathTime = Time.time;
            }
            else if (isBreathing && Time.time - breathTime >= 1.1f && !cInput.GetButton("Run") || swayFactor <= 0f || availBreath <= 0f)
            {
                if (swayFactor > 0f)
                {
                    if (availBreath <= 0f && canBreath)
                    {
                        deepBreathSway = 3.5f;
                        StartCoroutine(TakeBreath());
                    }

                    if (availBreath > 0f)
                    {
                        deepBreathSway = 2.5f;
                        AudioSource.PlayClipAtPoint(endBreath, tr.position, 0.14f + ((100f - availBreath) * 0.0043f));
                    }
                }

                isBreathing = false;
                lastBreath = Time.time;
            }
        }
        else
        {
            isBreathing = false;
        }

        if (isBreathing)
        {
            swayFactor *= Mathf.Clamp01(0.2f + (pm.controllerVeloMagn * 0.08f));
            availBreath -= Time.deltaTime * useBreathSpeed;
        }
        else
        {
            availBreath += Time.deltaTime * rechargeBreath;
        }

        availBreath = Mathf.Clamp(availBreath, 0f, 100f);

        deepBreathSway = Mathf.MoveTowards(deepBreathSway, 1f, Time.deltaTime);
        xRot += (Mathf.PerlinNoise(Mathf.PingPong(Time.time * 0.6f, 100f), 0f) - 0.5f) * 0.1f * (swayFactor + (pm.xyVelocity * 0.03f)) * deepBreathSway;
        yRot += (Mathf.PerlinNoise(0f, Mathf.PingPong(Time.time * 0.52f, 100f)) - 0.5f) * 0.1f * (swayFactor + (pm.xyVelocity * 0.03f)) * deepBreathSway;

        float oldXRecoil = xRecoilSmooth;
        float oldYRecoil = yRecoilSmooth;
        xRecoilSmooth = Mathf.Lerp(xRecoilSmooth, xRecoil, Time.deltaTime * 21f);
        yRecoilSmooth = Mathf.Lerp(yRecoilSmooth, yRecoil, Time.deltaTime * 21f);

        xRot += (xRecoilSmooth - oldXRecoil);
        yRot += (yRecoilSmooth - oldYRecoil);

        float sensitivityX = settingsController.sensitivityX;
        float sensitivityY = settingsController.sensitivityY;

        if (!RestrictionManager.restricted)
        {
            inputX = cInput.GetAxisRaw("Horizontal Look");
            inputY = cInput.GetAxisRaw("Vertical Look");
        }
        else
        {
            inputX = Mathf.Lerp(inputX, 0f, Time.deltaTime * 8f);
            inputY = Mathf.Lerp(inputX, 0f, Time.deltaTime * 8f);
        }

        bool isIdle = (Mathf.Abs(cInput.GetAxis("Horizontal Move")) + Mathf.Abs(cInput.GetAxis("Vertical Move")) + Mathf.Abs(inputX) + Mathf.Abs(inputY) <= 0.01f);

        if (RestrictionManager.restricted || isIdle)
        {
            timer += Time.deltaTime;

            sineVal = Mathf.Sin(timer * 1.5f);
            idleAnim = (ac.isAiming) ? sineVal * 0.1f : sineVal * 0.5f;
        }

        float oldX = xRot;
        float oldY = yRot;

        if (pm.onLadder)
        {
            xRot = pm.ladderFaceRot;
            ladderLook += inputX * sensitivityX * Time.timeScale;
        }
        else
        {
            xRot += inputX * sensitivityX * aimMouseModifier * magnificationFactor * Time.timeScale;
            ladderLook = Mathf.Lerp(ladderLook, 0f, Time.deltaTime * 1.5f);
        }

        float yInput = inputY * sensitivityY * aimMouseModifier * magnificationFactor * Time.timeScale;
        yRot += yInput;

        if (autoReturnY > 0f && yInput < 0f)
        {
            autoReturnY += yInput;
            autoReturnY = Mathf.Clamp(autoReturnY, 0f, 50f);
        }

        ladderLook = Mathf.Clamp(ladderLook, -60f, 60f);
        float ladderRestraint = (pm.onLadder) ? 0.8f : 1f;
        yRot = Mathf.Clamp(yRot, (minimumY * ladderRestraint) + Mathf.Abs(ladderLook * 0.18f), maximumY - ladderClampAnim);

        xVelocity = (xRot - oldX) / Mathf.Max(0.001f, Time.deltaTime);
        yVelocity = (yRot - oldY) / Mathf.Max(0.001f, Time.deltaTime);

        if (realFlinch.sqrMagnitude >= flinch.sqrMagnitude * 0.75f)
        {
            flinch = Vector3.zero;
            flinchSpeed = 9f;
        }
        else
        {
            flinchSpeed = 25f;
        }

        if (autoReturnY > 0f && Time.time - lastRecoil >= 0.1f)
        {
            float oldAutoY = autoReturnY;
            autoReturnY = Mathf.MoveTowards(autoReturnY, 0f, Time.deltaTime * returnSpeed);
            yRot += (autoReturnY - oldAutoY);
        }

        realFlinch = Vector3.Lerp(realFlinch, flinch, Time.deltaTime * flinchSpeed);

        if (!shaking)
        {
            shakePos = Vector3.Lerp(shakePos, Vector3.zero, Time.deltaTime * 4.25f);
        }

        ladderLookReturn = Mathf.Lerp(ladderLookReturn, (pm.onLadder) ? ladderLook : 0f, (pm.onLadder) ? 1f : Time.deltaTime * 6f);
        recoilZ = Mathf.Lerp(recoilZ, targetZRot, Time.deltaTime * 12f);
        head.localRotation = Quaternion.Slerp(head.localRotation, Quaternion.Euler(defHeadRot.x - yRot - recoilY - drawY - idleAnim - (realFlinch.y * (0.4f + (Mathf.PerlinNoise(0.04f, Mathf.PingPong(Time.time * 25f, 100f)) * 0.4f))), ladderLookReturn, recoilZ * 0.5f), Time.deltaTime * mouseSmoothing);
        targetZRot = Mathf.MoveTowards(targetZRot, 0f, Time.deltaTime * 2.2f);

        float mSmoothing = (pm.onLadder) ? 7f : mouseSmoothing;
        tr.localRotation = Quaternion.Slerp(tr.localRotation, Quaternion.Euler(0f, xRot + recoilX + drawX + (realFlinch.x * (0.6f + (Mathf.PerlinNoise(0.21f, Mathf.PingPong(Time.time * 25f, 100f)) * 0.4f))), 0f), Time.deltaTime * mSmoothing);

        float aimMod = (ac.isAiming) ? 0.7f : 1f;
        ac.flinchPos = new Vector3(realFlinch.x, realFlinch.y, 0f) * 0.002f * aimMod;
        ac.flinchRot = new Vector3(-realFlinch.y * 0.3f, realFlinch.x * 0.3f, (realFlinch.z * (0.55f + ((Mathf.PerlinNoise(0.02f, Mathf.PingPong(Time.time * 20f, 100f)) * 1.1f) - 0.55f))));

        if (!ac.isAiming)
        {
            magnificationFactor = 1f;
        }
    }

    public void Recoil(float recoilAmount, float upKickAmount, float sideKickAmount, Vector2 inf, float tiltAmount, float camAnim, float autoReturnSpeed)
    {
        recoilX = Random.value * recoilAmount;
        recoilY = Random.value * recoilAmount;
        targetZRot = Random.Range(-1f, 1f) * tiltAmount;
        camAnimation = camAnim;

        xRecoil += (sideKickAmount * Random.Range(0.3f, 1f) * ((Random.value < 0.5f) ? -1f : 1f)) + (inf.x * Random.value * Random.value * 4f);

        if (yRot < maximumY)
        {
            float upK = upKickAmount * (1f - (Mathf.Clamp01((yRot - 45f) / (maximumY - 45f)) * 0.2f));
            yRecoil += upK + inf.y;

            if (autoReturnSpeed > 0f)
            {
                autoReturnY += upK;
                returnSpeed = autoReturnSpeed;
            }
        }

        ac.ShootShake(camAnim);
        lastRecoil = Time.time;
    }

    public void Flinch(float amount)
    {
        float intensity = 0.08f + Mathf.Sqrt(amount);
        flinch = new Vector3(((Random.value < 0.5f) ? -intensity : intensity) * Random.Range(0.35f, 1.0f), ((Random.value < 0.5f) ? -intensity : intensity) * Random.Range(0.35f, 1.0f), ((Random.value < 0.5f) ? -intensity * 1.5f : intensity * 1.5f) * Random.Range(0.45f, 1.0f));
    }

    public void ShakeCamera(float duration = 0.4f, float speed = 0.6f, float intensity = 2.5f)
    {
        StopCoroutine("Shake");
        StartCoroutine(Shake(duration, speed, intensity * 0.03f));
    }

    private IEnumerator Shake(float duration, float speed, float intensity)
    {
        float timer = 0f;
        float shake = Random.value * 300f;
        shaking = true;

        while (timer < duration)
        {
            timer += Time.deltaTime * speed;
            float completion = Mathf.Clamp01(timer / duration);

            float damper = 1f - Mathf.Clamp01((2f * completion) - 1f);
            shake += (3.2f * completion);

            float x = Mathf.PerlinNoise(shake, 0f) * Random.Range(0.6f, 1.0f) * ((Random.value < 0.5f) ? -1f : 1f) * intensity;
            float y = Mathf.PerlinNoise(0f, shake) * Random.Range(0.6f, 1.0f) * ((Random.value < 0.5f) ? -1f : 1f) * intensity;

            shakePos = Vector3.Lerp(shakePos, new Vector3(x, y, (x + y) * 0.5f) * damper, Time.deltaTime * 4f);

            yield return null;
        }

        shaking = false;
    }

    private IEnumerator TakeBreath()
    {
        canBreath = false;
        AudioSource.PlayClipAtPoint(endBreath, tr.position, 0.425f);
        while (availBreath < 40f)
        {
            yield return null;
        }

        canBreath = true;
        deepBreathSway = 1f;
    }

    public void CameraDrawAnimation(float duration)
    {
        StopCoroutine("CameraDrawAnim");
        StartCoroutine(CameraDrawAnim(duration));
    }

    private IEnumerator CameraDrawAnim(float dur)
    {
        float time = 0f;
        float randomOffsetX = Random.value * 35f;
        float randomOffsetY = Random.value * 20f;
        while (time < dur)
        {
            time += Time.deltaTime;

            float dampingVal = Mathf.PingPong(time * 2f, dur);
            drawX = (Mathf.PerlinNoise(randomOffsetX, time * 1.5f) - 0.5f) * 1.5f * dampingVal;
            drawY = ((Mathf.PerlinNoise(time * 1.5f, randomOffsetY) * 0.5f) + 0.8f) * -1f * dampingVal;
            yield return null;
        }
    }
}