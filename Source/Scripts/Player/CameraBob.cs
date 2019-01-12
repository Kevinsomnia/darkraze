using UnityEngine;
using System.Collections;

public class CameraBob : MonoBehaviour
{
    public float bobSpeed = 10f;
    public Vector2 bobAmount = Vector2.one * 0.4f;
    public Vector2 sprintBobAmount = Vector2.one * 0.75f;
    public Vector2 ladderBobAmount = Vector2.one * 0.5f;
    public float tiltFactor = 1f;
    public AnimationCurve bobCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.25f, 2f), new Keyframe(0.5f, 1f)); //Important! Domain from 0 to 0.5! LOOPED!

    [HideInInspector] public float translateChangeX;
    [HideInInspector] public float translateChangeY;
    [HideInInspector] public float limpY;
    [HideInInspector] public float limpZ;

    private Vector3 defaultPos;
    private Vector3 defaultRot;
    private Vector2 bobbingAmount;
    private float bobbingSpeed;
    private float wavesliceLimp;
    private float totalAxes;
    private float timer;
    private float tiltAngleBob;
    private float normalizeCurve;

    private PlayerMovement pm;
    private AimController ac;

    void Start()
    {
        defaultPos = transform.localPosition;
        defaultRot = transform.localEulerAngles;

        PlayerReference pr = GeneralVariables.playerRef;
        pm = pr.GetComponent<PlayerMovement>();
        ac = pr.ac;
    }

    void Update()
    {
        float limpMultiplier = Mathf.Max(0f, 1f - pm.fDmgSpeedMult);
        limpY = translateChangeX * limpMultiplier * 10f;
        limpZ = translateChangeX * limpMultiplier * -5f;

        transform.localPosition = defaultPos + new Vector3(translateChangeX * 0.024f, translateChangeY * 0.024f, 0f);
        transform.localRotation = Quaternion.Euler(defaultRot + new Vector3(-translateChangeY + limpY, translateChangeX * 0.85f, -(tiltAngleBob * ((pm.sprinting) ? 0.55f : 0.4f)) + limpZ));

        if (RestrictionManager.restricted)
        {
            translateChangeX = 0f;
            translateChangeY = 0f;
            return;
        }

        float moveSpeedFactor = pm.controllerVeloMagn / pm.movement.runSpeed;
        if (pm.isMoving || pm.onLadder)
        {
            if (pm.sprinting)
            {
                bobbingSpeed = bobSpeed * moveSpeedFactor * 0.8f;
                bobbingAmount = Vector2.Lerp(bobbingAmount, sprintBobAmount, Time.deltaTime * 10f);
            }
            else if (pm.onLadder)
            {
                float climbSpeedFactor = pm.controllerVeloMagn / pm.movement.ladderClimbMagnitude;
                bobbingSpeed = bobSpeed * climbSpeedFactor * 0.75f;
                bobbingAmount = Vector2.Lerp(bobbingAmount, ladderBobAmount, Time.deltaTime * 10f);
            }
            else
            {
                bobbingSpeed = bobSpeed * moveSpeedFactor;
                bobbingAmount = Vector2.Lerp(bobbingAmount, bobAmount * ((ac.isAiming) ? 0.5f : 1f), Time.deltaTime * 10f);
            }
        }
        else
        {
            bobbingSpeed = 0f;
            bobbingAmount = Vector2.zero;
        }

        float horizontalMove = cInput.GetAxis("Horizontal Move");
        float verticalMove = cInput.GetAxis("Vertical Move");

        if (Mathf.Abs(horizontalMove) <= 0.01f && Mathf.Abs(verticalMove) <= 0.01f)
        {
            timer = 0f;
            tiltAngleBob = Mathf.Lerp(tiltAngleBob, 0f, Time.deltaTime * 3f);
        }
        else
        {
            timer += bobbingSpeed * Time.deltaTime;

            if (timer >= Mathf.PI * 2f)
            {
                timer -= Mathf.PI * 2f;
            }

            tiltAngleBob = Mathf.Lerp(tiltAngleBob, Mathf.Sin(timer) * tiltFactor, Time.deltaTime * 12f);
        }

        if (pm.grounded || pm.onLadder)
        {
            totalAxes = Mathf.Clamp01(Mathf.Abs(horizontalMove) + Mathf.Abs(verticalMove));
            translateChangeX = (Mathf.Sin(timer) * bobbingAmount.x * totalAxes);
            translateChangeY = (Mathf.Cos(timer * 2f) * bobCurve.Evaluate(timer / (Mathf.PI * 2f)) * bobbingAmount.y * totalAxes);
        }
        else
        {
            if (pm.wasSprinting)
            {
                translateChangeX = 0f;
                translateChangeY = 0f;
            }
        }
    }
}