using UnityEngine;
using System.Collections;

public class NetworkRigidbody_Owner : Topan.TopanMonoBehaviour
{
    private Rigidbody rigid;
    private Vector3 lastPosition = Vector3.zero;
    private Quaternion lastRotation = Quaternion.identity;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(rigid.position, lastPosition) > 0.15f || Quaternion.Angle(lastRotation, rigid.rotation) > 2f)
        {
            topanNetworkView.UnreliableRPC(Topan.RPCMode.Others, "SyncTransform", rigid.position, rigid.velocity, rigid.rotation.eulerAngles);
            lastPosition = rigid.position;
            lastRotation = rigid.rotation;
        }
    }

    [RPC]
    void AddRigidbodyForce(Vector3 force)
    {
        rigid.AddForce(force);
    }
}