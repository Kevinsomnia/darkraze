using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 3.0f;
    public float height = 3.0f;
    public float damping = 5.0f;
    public float rotationDamping = 10.0f;
    public bool followBehind = true;
    public bool fixedUpdate = false;

    void Update()
    {
        if (!fixedUpdate)
            Follow();
    }

    void FixedUpdate()
    {
        if (fixedUpdate)
            Follow();
    }

    private void Follow()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = (followBehind) ? target.TransformPoint(0f, height, -distance) : target.TransformPoint(0f, height, distance);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * damping);

        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, target.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationDamping);
    }
}