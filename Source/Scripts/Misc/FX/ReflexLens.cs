using UnityEngine;
using System.Collections;

public class ReflexLens : MonoBehaviour
{
    public Transform reflexDot; //rotation 0, 270, 0
    public float parallaxFactor = 1f;
    public float notAimFactor = 0.5f;

    private Vector3 defaultPos;
    private DynamicMovement dm;
    private AimController ac;

    void Awake()
    {
        InitializeVariables();
    }

    public void InitializeVariables()
    {
        PlayerReference pr = GeneralVariables.playerRef;
        if (pr != null)
        {
            dm = pr.dm;
            ac = pr.ac;
        }
        else
        {
            this.enabled = false;
            return;
        }

        defaultPos = reflexDot.localPosition;
    }

    void Update()
    {
        float aimFactor = (ac.isAiming) ? 1f : notAimFactor;
        reflexDot.localPosition = defaultPos - (dm.parallaxOffset * parallaxFactor * aimFactor);
    }
}