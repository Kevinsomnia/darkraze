using UnityEngine;
using System.Collections;

public class DynamicMovement : MonoBehaviour
{
    [HideInInspector] public PlayerMovement pm;
    public float swayAmount = 0.4f;
    public float bobSpeed = 10.5f;
    public Vector2 bobAmount = Vector2.one * 1.5f;
    public AnimationCurve bobCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.25f, 2f), new Keyframe(0.5f, 1f)); //Important! Domain from 0 to 0.5! LOOPED!
    public float crouchFactor = 0.7f;
    public float smoothing = 10;
    public float tiltAmount = 15;
    public float tiltSmoothing = 7;
    public float downWalkTilt = 0.75f;
    public float maxTilt = 30;
    public float aimTiltFactor = 0.2f;
    public Vector3 drawRot = new Vector3(-20, -40, 0);
    public Vector3 sprintRot = new Vector3(15, -43, 22);
    public float sprintBobSpeed = 1;
    public float sprintBobAmount = 0.03f;
    public float moveTiltAmount = 3f;
    public float weaponTiltDirSwayFactor = 1f; //directional rotation of weapon based on sway.
    public float kickbackTilt = 5f; //Just BF3-inspired kickback tilt animation.

    public Transform switchObj;
    public float horizVertKick = 0.01f;

    [HideInInspector] public bool animationIsPlaying;
    [HideInInspector] public bool rotateWeaponTransform;
    [HideInInspector] public bool isEmptyClick;
    [HideInInspector] public bool terminalVelocity;
    [HideInInspector] public SprintAnimOverride sao;
    [HideInInspector] public float timer;
    [HideInInspector] public float moveXTilt;
    [HideInInspector] public Vector3 parallaxOffset;
    [HideInInspector] public Vector3 switchRot;
    [HideInInspector] public Vector3 curSprintRot;
    [HideInInspector] public Vector3 sprintRotOverride;
    [HideInInspector] public Vector3 kickPos;
    [HideInInspector] public Vector3 kickPosReal;
    [HideInInspector] public float clampValue;
    [HideInInspector] public float currentDrawDelay;
    [HideInInspector] public float drawOffset;
    [HideInInspector] public Vector3 rattleVector;
    [HideInInspector] public float sinSprint;
    [HideInInspector] public float cosSprint;
    [HideInInspector] public Vector3 defaultPos;
    [HideInInspector] public float xPos;
    [HideInInspector] public float yPos;

    private Vector3 defaultArmRot;
    private float factorX;
    private float factorY;
    private float tiltFactor;
    private Vector2 ba;
    private float bobbingSpeed;
    private Vector3 translateChange;
    private float totalAxes;

    private float moveSpeedFactor;
    private float idleAnim;

    private PlayerLook pl;
    private CameraBob cb;
    private WeaponManager wm;
    private ImpactAnimation ia;
    private AntiClipSystem acs;

    private bool kicking;
    private float kickTilt;
    private float lastKickTime;
    private float smoothKickFactor;

    private float zTimer;
    private Vector3 sprintOffset;
    private Vector3 lerpPos;
    private Vector3 lerpRot;
    private Vector3 lerpWepRot;
    private Vector3 smoothKick;
    private Vector3 sprintWeaponTr;
    private Vector3 randSwaySprint;
    private float drawTilt;
    private float dTiltTarg;
    private float kickLerp;
    private float kickSpeedFactor;
    private float reloadRot;
    private float meleeAnim;
    private float meleeTarget;
    private float idleAnimPos;
    private float downwardTilt;
    private float slopeTilt;

    private float switchWepRotTarget;
    private float switchWepRot;

    private float asf;
    private float apf;

    private float dirSwayX;
    private float dirSwayY;

    private Transform root;
    private Transform tr;
    private AimController ac;

    void Start()
    {
        tr = transform;
        root = tr.root;
        defaultPos = tr.localPosition;
        defaultArmRot = tr.localEulerAngles;
        ba = bobAmount;

        PlayerReference pr = GeneralVariables.playerRef;
        if (pr != null)
        {
            pm = pr.GetComponent<PlayerMovement>();
            ac = pr.ac;
            cb = pr.cb;
            ia = pr.ia;
            wm = pr.wm;
            acs = pr.acs;
            pl = pr.GetComponent<PlayerLook>();
        }

        lastKickTime = -0.1f;
        smoothKickFactor = 0.01f;
        kickSpeedFactor = 1f;
        animationIsPlaying = false;
    }

    void Update()
    {
        if (RestrictionManager.restricted || DarkRef.IsIdle())
        {
            idleAnim = pl.sineVal * ((ac.isAiming) ? 0.15f : 0.55f);
        }
        else
        {
            idleAnim = 0f;
        }

        float atf = 1f;
        if (ac.isAiming)
        {
            asf = 0.28f; apf = 0.4f; atf = aimTiltFactor;
        }
        else
        {
            asf = 1f; apf = 1f;
        }

        float xMove = cInput.GetAxis("Horizontal Move");
        float yMove = cInput.GetAxis("Vertical Move");

        if (!RestrictionManager.restricted)
        {
            factorX = Mathf.Clamp((cInput.GetAxis("Horizontal Look") / Mathf.Max(0.01f, Time.deltaTime)) * swayAmount * asf * 0.18f, -swayAmount * 12.5f, swayAmount * 12.5f) * pl.magnificationFactor;
            factorY = Mathf.Clamp((cInput.GetAxis("Vertical Look") / Mathf.Max(0.01f, Time.deltaTime)) * swayAmount * asf * 0.14f * Mathf.Clamp01(Mathf.Abs(pl.yVelocity * 3f)), -swayAmount * 10f, swayAmount * 10f) * pl.magnificationFactor;
            tiltFactor = Mathf.Clamp(cInput.GetAxis("Horizontal Look") * tiltAmount * atf * 0.015f, -maxTilt * 1.2f, maxTilt * 1.2f) * pl.magnificationFactor;
        }
        else
        {
            factorX = 0f;
            factorY = 0f;
            tiltFactor = 0f;
        }

        Vector3 adjustedAimCurve = ac.lerpPos + (Vector3.up * 0.01f * Mathf.Sin(ac.aimTransition * Mathf.PI));
        Vector3 controllerVelocity = pm.controller.velocity;
        float cVeloMagnitude = pm.controllerVeloMagn;

        if (Time.timeScale > 0f)
        {
            float delta = Time.deltaTime;
            xPos = Mathf.Lerp(xPos, factorX * 0.006f * ((ac.isAiming) ? 0.65f : 1f), delta * ((ac.isAiming) ? 3f : 2.5f));
            yPos = Mathf.Lerp(yPos, factorY * 0.006f * ((ac.isAiming) ? 0.65f : 1f), delta * ((ac.isAiming) ? 3f : 2.5f));
            idleAnimPos = Mathf.Lerp(idleAnimPos, idleAnim * 0.0019f * ((ac.isAiming) ? 0.6f : 1f), delta * 5f);
            float idleZPos = Mathf.PerlinNoise(Mathf.PingPong(Time.time * 0.22f, 100f), Mathf.PingPong(Time.time * 0.3f, 50f)) - 0.5f;

            float overrideSmoothing = ((sao != null) ? sao.offsetSmoothing : 5f) * ((ac.isAiming) ? 1.35f : 1f);
            Vector3 overrideOffset = (sao != null) ? sao.offset : Vector3.forward * 0.07f;
            sprintOffset = Vector3.Lerp(sprintOffset, ((pm.sprinting && cVeloMagnitude > 1f) || terminalVelocity) ? overrideOffset : Vector3.zero, delta * overrideSmoothing);
            slopeTilt = Mathf.Lerp(slopeTilt, (pm.grounded && !pm.isSlipping) ? Mathf.Clamp(-pm.controller.velocity.y, -3.5f, 3.5f) : 0f, delta * 4.5f);

            Vector2 noiseStationary = (Mathf.Abs(xMove) + Mathf.Abs(yMove) <= 0f) ? new Vector2(Mathf.PerlinNoise(315f, Time.time * 0.45f) - 0.5f, Mathf.PerlinNoise(Time.time * 0.35f, 152f) - 0.5f) * 0.005f * ((ac.isAiming) ? 0.35f : 1f) : Vector2.zero;

            Vector3 sprintLimpVector = new Vector3(sinSprint + (translateChange.x * 0.0018f * apf), -cosSprint + (translateChange.y * 0.0017f * apf) - (cb.limpY * 0.0012f), translateChange.z + (idleZPos * 0.015f * ((ac.isAiming) ? 0.25f : 1f)));
            float aimingJumpFactor = ((ac.isAiming) ? 1.12f : 3.25f);
            lerpPos = Vector3.Lerp(lerpPos, (Vector3.up * ia.jumpCurrentPos.y * 0.07f * ((ac.isAiming) ? 0.18f : 1f)) + new Vector3(noiseStationary.x, noiseStationary.y, 0f) + (Vector3.up * ia.currentPos.y * 0.08f * ((ia.currentPos.y > 0f) ? aimingJumpFactor : 1f)) + sprintLimpVector - (Vector3.up * reloadRot * 0.0005f) + ((ac.isAiming) ? Vector3.zero : (-Vector3.forward * (0.025f - (0.025f * ((75 - GameSettings.settingsController.FOV) / 15f))))), delta * 11f);

            meleeAnim = Mathf.Lerp(meleeAnim, meleeTarget, delta * 11f);
            if (meleeAnim >= meleeTarget * 0.8f)
            {
                meleeTarget = 0f;
            }

            float fallSpd = Mathf.Clamp(Mathf.Abs(pm.controller.velocity.y), 0f, pm.movement.maxFallSpeed * 0.9f);
            float rattleX = Mathf.PerlinNoise(1.6f, Mathf.PingPong(Time.time * 19f, 190f)) * fallSpd;
            float rattleY = Mathf.PerlinNoise(Mathf.PingPong(Time.time * 18f, 180f), -1.6f) * fallSpd;
            rattleVector = new Vector3(rattleX, rattleY, (rattleX - rattleY) * 0.3f) * fallSpd * 0.05f;

            smoothKick = Vector3.Lerp(smoothKick, (Time.time - lastKickTime < 0.1f) ? -Vector3.forward * smoothKickFactor * ((ac.isAiming) ? 0.4f : 1f) : Vector3.zero, delta * 5.5f);
            Vector3 swayMeleePos = new Vector3(xPos + meleeAnim, yPos + idleAnimPos - meleeAnim, meleeAnim * 1.1f);

            float x = Mathf.PerlinNoise(Mathf.PingPong(Time.time * 1.1f, 100f), 0f) - 0.5f;
            float y = Mathf.PerlinNoise(0f, Mathf.PingPong(Time.time * 1.1f, 80f)) - 0.5f;

            if (terminalVelocity)
            {
                randSwaySprint = Vector3.Lerp(randSwaySprint, new Vector3(x, y, 0f) * 0.032f, delta * 5.5f);
            }
            else if (pm.sprinting)
            {
                randSwaySprint = Vector3.Lerp(randSwaySprint, new Vector3(x, y * 0.7f, 0f) * 0.035f, delta * 6.5f);
            }
            else
            {
                randSwaySprint = Vector3.Lerp(randSwaySprint, Vector3.zero, delta * 8.5f);
            }

            float aimFactor = (ac.isAiming) ? 0.5f : 1f;
            tr.localPosition = smoothKick + kickPosReal + defaultPos + sprintOffset + swayMeleePos + lerpPos + randSwaySprint + (rattleVector * 0.00003f) + (-Vector3.forward * Mathf.Abs(slopeTilt) * 0.005f * aimFactor) - adjustedAimCurve;

            kickTilt = Mathf.MoveTowards(kickTilt, 0f, Time.deltaTime * 3f * ((wm.currentGC != null && wm.currentGC.currentAmmo <= 0) ? 3f : 1f));

            switchWepRot = Mathf.Lerp(switchWepRot, switchWepRotTarget + (Mathf.PingPong(ac.aimTransition * 2f, 1f) * 8f), delta * 8.5f);

            //Temp anim
            reloadRot = Mathf.Lerp(reloadRot, (wm.currentGC != null && wm.currentGC.reloading) ? 7f : 0f, delta * 7.5f);

            lerpRot = Vector3.Lerp(lerpRot, defaultArmRot + ((!ac.isAiming) ? new Vector3((ac.idleX - ac.idleZ) * 2f, ac.idleZ * 2.5f, (ac.idleX + ac.idleY) * 1.5f) : Vector3.zero) + new Vector3(0f, 0f, tiltFactor + (translateChange.x * apf)), delta * tiltSmoothing);
            tr.localRotation = Quaternion.Euler(lerpRot + new Vector3(reloadRot, 0f, switchWepRot - (reloadRot * 0.52f) + (kickTilt * Mathf.Clamp01(kickLerp) * Mathf.PerlinNoise(Mathf.PingPong(Time.time * 25f, 250f), -0.1f))) + (randSwaySprint * 55f));

            float tiltTarget = (pm.grounded) ? (-root.InverseTransformDirection(controllerVelocity).normalized.x * moveTiltAmount) : 0f;
            moveXTilt = Mathf.Lerp(moveXTilt, tiltTarget, delta * 4.4f);

            Transform curWeaponTr = wm.currentWeaponTransform;
            downwardTilt = Mathf.Lerp(downwardTilt, ((cInput.GetButton("Fire Weapon") && !RestrictionManager.restricted) || ac.isAiming) ? 0f : -Mathf.Abs(translateChange.x * downWalkTilt * ((ac.isAiming) ? 0.65f : 1f)), Time.deltaTime * 10f);
            if (curWeaponTr != null)
            {
                dirSwayX = Mathf.Lerp(dirSwayX, factorX * ((ac.isAiming) ? 0.75f : 1f), delta * 8.5f);
                dirSwayY = Mathf.Lerp(dirSwayY, factorY * ((ac.isAiming) ? 0.75f : 1f) * 0.81f, delta * 8.5f);

                drawTilt = Mathf.Lerp(drawTilt, dTiltTarg, Time.deltaTime * 6f);
                sprintWeaponTr = Vector3.Lerp(sprintWeaponTr, (rotateWeaponTransform) ? curSprintRot : Vector3.zero, delta * ((ac.isAiming) ? 9.5f : 7f) * ((pm.wasSprinting) ? 0.8f : 1f));
                Vector2 sinRotFactor = (sao != null) ? sao.sprintRotFactor : Vector2.one;
                lerpWepRot = Vector3.Lerp(lerpWepRot, new Vector3((moveXTilt * atf) + (translateChange.x * 0.5f) + cb.limpZ, factorX + (translateChange.x * 0.75f) + (sinSprint * 350f * sinRotFactor.x) + (ia.jumpCurrentPos.y * 6f) + (ia.currentPos.y * 4f * ((ia.currentPos.y > 0f) ? 5f : 1f)), factorY + (translateChange.y * 0.7f) + idleAnimPos + (kickPosReal.magnitude * 60f) + (sinSprint * 50f * sinRotFactor.y) - (cb.limpY * 0.5f)), delta * smoothing);
                curWeaponTr.localRotation = Quaternion.Euler(lerpWepRot + sprintWeaponTr + new Vector3(drawTilt, -90f - (meleeAnim * 1000f) + (dirSwayX * weaponTiltDirSwayFactor), downwardTilt + (ia.jumpCurrentPos.y * 7f) + (Mathf.Abs(slopeTilt) * -0.8f * aimFactor) + (ia.currentPos.y * 60f * ((ia.currentPos.y > 0f) ? 1f : 0f)) + (dirSwayY * weaponTiltDirSwayFactor)));
            }

            if (kicking)
            {
                kickLerp += delta * 28.5f * kickSpeedFactor;
                if (kickLerp > 1f)
                {
                    kickLerp -= (kickLerp - 1f) * 1.2f;
                    kicking = false;
                }
            }
            else if (kickLerp > 0f)
            {
                kickLerp -= delta * 28.5f * kickSpeedFactor * ((wm.currentGC != null && wm.currentGC.currentAmmo > 0 && cInput.GetButton("Fire Weapon")) ? 1f : 0.6f);
            }
        }

        kickPosReal = Vector3.Lerp(Vector3.zero, kickPos, Mathf.Clamp01(kickLerp));
        moveSpeedFactor = Mathf.Clamp01(cVeloMagnitude / pm.movement.runSpeed);

        if (pm.grounded)
        {
            if (!pm.sprinting && !animationIsPlaying)
            {
                float cFactor = ((pm.crouching || pm.walking) && !ac.isAiming) ? crouchFactor : 1f;

                if (wm.currentGC != null && ac.isAiming && !acs.clipping)
                {
                    bobAmount = ba * wm.currentGC.aimBobFactor;
                }
                else
                {
                    bobAmount = ba;
                }

                bobAmount = ((wm.currentGC != null && ac.isAiming && !acs.clipping) ? (ba * wm.currentGC.aimBobFactor) : ba) * cFactor;
                sinSprint = 0f;
                cosSprint = 0f;

                switchRot = (pm.onLadder && pl.yRot < 10f) ? new Vector3(drawRot.x, -drawRot.y, 0f) : Vector3.zero;
                curSprintRot = Vector3.zero;
                bobbingSpeed = (pm.isMoving) ? bobSpeed * moveSpeedFactor : 0f;
            }

            if (pm.sprinting && !pm.onLadder && cVeloMagnitude > 1f)
            {
                if (sao != null)
                {
                    rotateWeaponTransform = sao.rotateWeaponTransform;
                    curSprintRot = sao.sprintRot;
                }
                else
                {
                    rotateWeaponTransform = false;
                    curSprintRot = sprintRot;
                }

                sinSprint = Mathf.Sin(timer * 8f) * sprintBobAmount * ((sao != null) ? sao.sprintBobAmount.x : 1f);
                cosSprint = Mathf.Cos(timer * 16f) * sprintBobAmount * 0.8f * ((sao != null) ? sao.sprintBobAmount.y : 1f);
                bobbingSpeed = 0f;
                bobAmount = Vector2.zero;
            }
        }
        else if (!animationIsPlaying)
        {
            if (!terminalVelocity)
            {
                curSprintRot = Vector3.zero;
                sinSprint = 0f;
            }
            else if (terminalVelocity)
            {
                if (sao != null)
                {
                    rotateWeaponTransform = sao.rotateWeaponTransform;
                    curSprintRot = sao.sprintRot;
                }
                else
                {
                    rotateWeaponTransform = false;
                    curSprintRot = sprintRot;
                }

                sinSprint = 0f;
            }

            switchRot = (pm.onLadder) ? new Vector3(drawRot.x, -drawRot.y, 0f) : Vector3.zero;
            bobbingSpeed = 0f;
            bobAmount = Vector2.zero;
        }

        parallaxOffset = (tr.localPosition - defaultPos) + adjustedAimCurve;

        if (Mathf.Abs(xMove) + Mathf.Abs(yMove) <= 0.015f)
        {
            timer = 0f;
            zTimer = 0f;
        }
        else
        {
            if (pm.grounded && pm.isMoving && !RestrictionManager.restricted)
            {
                float increment = ((pm.sprinting) ? (sprintBobSpeed * Mathf.Clamp01(cVeloMagnitude / pm.movement.sprintSpeed)) : bobbingSpeed) * Time.deltaTime;
                timer += increment * ((pm.sprinting && !pm.onLadder && cVeloMagnitude > 2f && sao != null) ? sao.animationSpeed : 1f);
                zTimer += increment * 0.5f;
            }

            if (timer >= Mathf.PI * 2f)
            {
                timer -= Mathf.PI * 2f;
            }
            if (zTimer >= Mathf.PI * 2f)
            {
                zTimer -= Mathf.PI * 2f;
            }
        }

        if (pm.grounded && !RestrictionManager.restricted)
        {
            float sprintReload = (pm.sprintReloadBoost > 1f) ? 1.6f : 1f;
            totalAxes = Mathf.Clamp01(Mathf.Abs(xMove) + Mathf.Abs(yMove));
            translateChange.x = (Mathf.Sin(timer) * bobAmount.x * totalAxes * sprintReload) + ((Mathf.PerlinNoise(25f, Mathf.PingPong(Time.time * 0.85f, 100f)) - 0.5f) * 3f * totalAxes);
            translateChange.y = (Mathf.Cos(timer * 2f) * bobCurve.Evaluate(timer / (Mathf.PI * 2f)) * bobAmount.y * totalAxes * sprintReload) + ((Mathf.PerlinNoise(Mathf.PingPong(Time.time * 0.85f, 100f), 15f) - 0.5f) * 3f * totalAxes);
            translateChange.z = Mathf.Sin(zTimer) * 0.004f * totalAxes * ((ac.isAiming) ? 0.4f : 1f);
        }
        else
        {
            translateChange = Vector3.zero;
        }
    }

    //Obviously placeholder.
    public void DoMeleeAnimation()
    {
        meleeTarget = 0.1f;
    }

    public void Draw(float drawTime)
    {
        StopCoroutine("DrawAnimation");
        StartCoroutine(DrawAnimation(drawTime));
    }

    private IEnumerator DrawAnimation(float drawTime)
    {
        currentDrawDelay = drawTime;
        animationIsPlaying = true;
        switchRot = new Vector3(drawRot.x, -drawRot.y, drawRot.z);
        switchWepRotTarget = 22f;
        drawOffset = -0.005f;
        drawTilt = 0f;
        dTiltTarg = 16f;

        pl.CameraDrawAnimation(drawTime * 1.45f);

        float coolDown = 0f;
        while (coolDown < currentDrawDelay)
        {
            coolDown += Time.deltaTime;
            yield return null;
        }

        drawTilt = 16f;
        dTiltTarg = 0f;
        switchRot = Vector3.zero;
        drawOffset = 0f;
        switchWepRot = 14f;
        switchWepRotTarget = 0f;
        currentDrawDelay = 0f;
        yield return new WaitForSeconds(0.3f);
        animationIsPlaying = false;
    }

    public void ExtendDrawTime(float time)
    {
        if (!animationIsPlaying)
        {
            return;
        }

        currentDrawDelay += Mathf.Max(0f, time);
    }

    public void Kickback(float amount, float kickSpdFactor, float kickTilt)
    {
        float aimFactor = (ac.isAiming) ? wm.currentGC.kickbackAimFactor : 1f;
        smoothKickFactor = amount * 1.08f;
        kickPos = new Vector3(Random.Range(-horizVertKick, horizVertKick), Random.Range(-horizVertKick, horizVertKick), -amount * 0.56f) * aimFactor;
        kickTilt = ((ac.isAiming && wm.currentGC != null && wm.currentGC.currentAmmo > 0) ? 0f : Random.Range(-kickbackTilt, kickbackTilt) * kickTilt);
        kickSpeedFactor = kickSpdFactor;
        lastKickTime = Time.time;
        kicking = true;
        isEmptyClick = false;
    }

    public void EmptyAnimation()
    {
        float aimFactor = (ac.isAiming) ? 0.7f : 1f;
        kickPos = Vector3.forward * -0.0022f * aimFactor;
        kickSpeedFactor = 0.8f;
        kicking = true;
        isEmptyClick = true;
    }
}