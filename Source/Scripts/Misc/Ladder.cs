using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class Ladder : MonoBehaviour
{
    public Transform bottomPoint;
    public Transform topPoint;
    public Vector3 climbDirection;
    public GameObject faceDirection;

    [HideInInspector] public float faceDirectionAngle;
    [HideInInspector] public Vector3 topSpot;
    [HideInInspector] public Collider col;

    void Start()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;

        if (bottomPoint != null && topPoint != null)
        {
            climbDirection = (topPoint.position - bottomPoint.position).normalized;
        }

        if (faceDirection != null)
        {
            faceDirectionAngle = faceDirection.transform.eulerAngles.y;
        }

        topSpot = col.bounds.center;
        topSpot.y = col.bounds.max.y;
    }
}