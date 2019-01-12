using UnityEngine;
using System.Collections;

public class ShadowController : MonoBehaviour
{
    public PlayerMovement movementScript;
    public float transitionTime = 0.25f;
    public string idleAnim = "Idle";
    public string runAnim = "Run";
    public string runLeftAnim = "RunLeft";
    public string runRightAnim = "RunRight";
    public string runBackAnim = "";
    public string crouchAnim = "Crouch";
    public string crouchForwardAnim = "CrouchForward";
    public string crouchLeftAnim = "CrouchLeft";
    public string crouchRightAnim = "CrouchRight";
    public string jumpAnim = "Jump";

    private Animation anim;

    void Start()
    {
        anim = GetComponent<Animation>();
    }

    void Update()
    {
        if (movementScript.sprinting)
        {
            anim[runAnim].speed = movementScript.xyVelocity * 0.18f;
            anim.CrossFade(runAnim, transitionTime);
        }
        else
        {
            Vector3 moveDir = movementScript.moveDirection.normalized;

            if (movementScript.crouching)
            {
                float animVeloSpeed = movementScript.xyVelocity * 0.5f;

                if (moveDir.z > 0.05f)
                {
                    anim[crouchForwardAnim].speed = animVeloSpeed;
                    anim.CrossFade(crouchForwardAnim, transitionTime);
                }
                else if (moveDir.z < -0.05f)
                {
                    anim[crouchForwardAnim].speed = -animVeloSpeed;
                    anim.CrossFade(crouchForwardAnim, transitionTime);
                }
                else if (moveDir.x < -0.05f)
                {
                    anim[crouchLeftAnim].speed = animVeloSpeed;
                    anim.CrossFade(crouchLeftAnim, transitionTime);
                }
                else if (moveDir.x > 0.05f)
                {
                    anim[crouchRightAnim].speed = animVeloSpeed;
                    anim.CrossFade(crouchRightAnim, transitionTime);
                }
            }
            else
            {
                float animVeloSpeed = movementScript.xyVelocity * transitionTime;

                if (moveDir.z > 0.05f)
                {
                    anim[runAnim].speed = animVeloSpeed;
                    anim.CrossFade(runAnim, transitionTime);
                }
                else if (moveDir.z < -0.05f)
                {
                    if (runBackAnim != "")
                    {
                        anim[runBackAnim].speed = animVeloSpeed;
                        anim.CrossFade(runBackAnim, transitionTime);
                    }
                    else
                    {
                        anim[runAnim].speed = -animVeloSpeed;
                        anim.CrossFade(runAnim, transitionTime);
                    }
                }
                else if (moveDir.x < -0.05f)
                {
                    anim[runLeftAnim].speed = animVeloSpeed;
                    anim.CrossFade(runLeftAnim, transitionTime);
                }
                else if (moveDir.x > 0.05f)
                {
                    anim[runRightAnim].speed = animVeloSpeed;
                    anim.CrossFade(runRightAnim, transitionTime);
                }
            }
        }

        if (movementScript.xyVelocity <= 0.5f)
        {
            if (movementScript.crouching)
            {
                anim.CrossFade(crouchAnim, transitionTime);
            }
            else
            {
                anim.CrossFade(idleAnim, transitionTime);
            }
        }
    }
}