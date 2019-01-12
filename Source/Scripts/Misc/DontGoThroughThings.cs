using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class DontGoThroughThings : MonoBehaviour
{
    public LayerMask layersToAffect = -1;

    private Rigidbody body;
    private Vector3 oldPos;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        oldPos = body.position;
    }

    void FixedUpdate()
    {
        Vector3 movementDir = body.position - oldPos;
        float dist = movementDir.magnitude;

        if (dist > 0.005f)
        {
            RaycastHit hit;
            if (Physics.Raycast(oldPos, movementDir, out hit, dist, layersToAffect.value))
            {
                body.position = hit.point;
            }
        }

        oldPos = body.position;
    }
}