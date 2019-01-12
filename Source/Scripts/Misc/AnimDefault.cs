using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AnimDefault : MonoBehaviour {
	public float returnSpeed = 8.0f;
	public Vector3 returnPos;
	public Quaternion returnRot;
	public bool copyTransform;
	
	void Update() {
		if(copyTransform) {
			returnPos = transform.localPosition;
			returnRot = transform.localRotation;
			copyTransform = false;
		}
		
		if(transform.localPosition != returnPos) {
			transform.localPosition = Vector3.Lerp(transform.localPosition, returnPos, Time.deltaTime * returnSpeed);
		}
		if(transform.localRotation != returnRot) {
			transform.localRotation = Quaternion.Slerp(transform.localRotation, returnRot, Time.deltaTime * returnSpeed);
		}
	}
}