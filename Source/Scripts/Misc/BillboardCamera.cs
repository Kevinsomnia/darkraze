using UnityEngine;
using System.Collections;

public class BillboardCamera : MonoBehaviour
{
    private Camera cam;

    void Awake()
    {
        cam = GeneralVariables.mainPlayerCamera;

        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    void Update()
    {
        if (cam == null)
        {
            cam = GeneralVariables.mainPlayerCamera;

            if (GeneralVariables.mainPlayerCamera == null)
            {
                cam = Camera.main;
            }

            return;
        }

        Vector3 dir = (cam.transform.position - transform.position);
        transform.rotation = Quaternion.LookRotation(dir);
    }
}