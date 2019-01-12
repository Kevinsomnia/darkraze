using UnityEngine;
using System.Collections;

public class NetworkRigidbody_Proxy : Topan.TopanMonoBehaviour
{
    private Rigidbody rigid;
    private Vector3 targetPos = Vector3.zero;
    private Quaternion targetRot = Quaternion.identity;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rigid.position = Vector3.Lerp(rigid.position, targetPos, Time.deltaTime * 2f);
        rigid.rotation = Quaternion.Lerp(rigid.rotation, targetRot, Time.deltaTime * 2f);
    }

    [RPC]
    void SyncTransform(Vector3 pos, Vector3 rot, Vector3 velocity)
    {
        targetPos = pos;
        targetRot = Quaternion.Euler(rot);
        rigid.velocity = velocity;
    }
}
