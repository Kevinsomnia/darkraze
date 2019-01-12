using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GravityAttractor : MonoBehaviour {
    public float attractRadius = 10f;
    public float attractForce = 25f;

    private float checkTime = 0f;
    private List<Rigidbody> affectedRigids = new List<Rigidbody>();

	void Update() {
        if(Time.timeScale <= 0f) {
            return;
        }

        if(Time.time - checkTime >= 0.1f) {
            CheckRigidbodies();
            checkTime = Time.time;
        }

        foreach(Rigidbody rigid in affectedRigids) {
            if(rigid == null || (rigid != null && rigid.isKinematic)) {
                continue;
            }

            rigid.AddForce((transform.position - rigid.position).normalized * attractForce * (attractRadius - (transform.position - rigid.position).magnitude));
        }
	}

    public void CheckRigidbodies() {
        affectedRigids.Clear();

        Collider[] colsInRange = Physics.OverlapSphere(transform.position, attractRadius);
        foreach(Collider col in colsInRange) {
            if(col.GetComponent<Rigidbody>() != null && !col.GetComponent<Rigidbody>().isKinematic) {
                affectedRigids.Add(col.GetComponent<Rigidbody>());
            }
        }
    }
}
