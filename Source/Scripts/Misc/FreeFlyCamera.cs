using UnityEngine;
using System.Collections;

public class FreeFlyCamera : MonoBehaviour
{
    public float flySpeedSlow = 5f;
    public float flySpeed = 15f;
    public float flySpeedFast = 30f;
    public float rotateSpeedFactor = 1f;
    public bool ignoreTimeScale = false;
    public bool isDevFTCam = false;

    private float curFlySpeed;
    private Transform tr;
    private Vector3 oldPos;
    private Vector3 newPos;
    private Vector3 directionVector;

    private float lookX;
    private float lookY;

    void Start()
    {
        tr = transform;
        oldPos = tr.position;
        newPos = tr.position;

        lookX = tr.eulerAngles.y;
        lookY = tr.eulerAngles.x;
    }

    void Update()
    {
        if (isDevFTCam)
        {
            if (RestrictionManager.devConsole || RestrictionManager.pauseMenu)
            {
                return;
            }
        }

        oldPos = tr.position;

        float inputX = 0f;
        if (cInput.GetButton("Strafe Left"))
        {
            inputX = -1f;
        }
        else if (cInput.GetButton("Strafe Right"))
        {
            inputX = 1f;
        }

        float inputY = 0f;
        if (cInput.GetButton("Backward"))
        {
            inputY = -1f;
        }
        else if (cInput.GetButton("Forward"))
        {
            inputY = 1f;
        }

        float mouseX = cInput.GetAxisRaw("Horizontal Look");
        float mouseY = cInput.GetAxisRaw("Vertical Look");

        float delta = (ignoreTimeScale) ? Time.unscaledDeltaTime : Time.deltaTime;

        float verticalMove = 0f;
        if (cInput.GetButton("Jump"))
        {
            verticalMove = 1f;
        }
        else if (cInput.GetButton("Crouch"))
        {
            verticalMove = -1f;
        }

        if (cInput.GetButton("Run"))
        {
            curFlySpeed = flySpeedFast;
        }
        else if (cInput.GetButton("Walk"))
        {
            curFlySpeed = flySpeedSlow;
        }
        else
        {
            curFlySpeed = flySpeed;
        }

        bool isMoving = ((Mathf.Abs(inputX) + Mathf.Abs(inputY)) > 0f);
        if (isMoving || verticalMove != 0f)
        {
            directionVector = tr.TransformDirection(new Vector3(inputX, 0f, inputY)) + new Vector3(0f, verticalMove, 0f);
            newPos += directionVector * curFlySpeed * delta;
        }

        lookX += mouseX * GameSettings.settingsController.sensitivityX * rotateSpeedFactor;
        lookY -= mouseY * GameSettings.settingsController.sensitivityY * rotateSpeedFactor;
        lookY = Mathf.Clamp(lookY, -90f, 90f);

        tr.position = newPos;
        tr.localRotation = Quaternion.Euler(lookY, lookX, 0f);
    }
}