using UnityEngine;
using System.Collections;

public class RotateTowardsPlayer : MonoBehaviour {
	Transform myTransform;
	
	void Start(){
		myTransform = transform;	
	}
	
	
	void Update () {
		if (Camera.main == null){
			return;	
		}
		Vector3 rot = Quaternion.Slerp( myTransform.rotation, Quaternion.LookRotation( myTransform.position - Camera.main.transform.position ), Time.deltaTime * 10f).eulerAngles;
		rot.x = 0f;
		rot.z = 0f;
		myTransform.rotation = Quaternion.Euler(rot);
	}
}
