using UnityEngine;
using System.Collections;

public class RagdollHandler : MonoBehaviour
{
    public float forceFactor = 1f;

    private MovementSync_Proxy msP;
    private Rigidbody[] ragdollRigidbodies;

    void Start()
    {
        msP = GetComponent<MovementSync_Proxy>();
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        SetRagdoll(false);
    }

    public void SetRagdoll(bool e)
    {
        foreach (Rigidbody limb in ragdollRigidbodies)
        {
            limb.isKinematic = !e;

            if (e)
            {
                limb.AddForce((Time.deltaTime > 0f) ? (msP.velocity / Time.deltaTime) * forceFactor : Vector3.zero, ForceMode.Impulse);
            }
        }
    }
}