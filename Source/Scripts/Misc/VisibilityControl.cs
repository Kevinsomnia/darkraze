using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class VisibilityControl : MonoBehaviour
{
    public LayerMask layerMask = -1;

    [HideInInspector] public bool isVisible = false;

    private float oldTime;

    void Update()
    {
        if (GeneralVariables.mainPlayerCamera != null && Mathf.Round(Time.time * 10f) / 10f != oldTime)
        {
            isVisible = !Physics.Linecast(GetComponent<Renderer>().bounds.center, GeneralVariables.mainPlayerCamera.transform.position, layerMask.value);
            oldTime = Mathf.Round(Time.time * 10f) / 10f;
        }
    }
}