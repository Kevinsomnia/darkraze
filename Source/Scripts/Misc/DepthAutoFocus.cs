using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DepthOfField34))]
public class DepthAutoFocus : MonoBehaviour {
    private DepthOfField34 dofScript;

    void Awake() {
        dofScript = GetComponent<DepthOfField34>();
    }

	void Update() {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit)) {
            dofScript.focalPoint = Mathf.Lerp(dofScript.focalPoint, hit.distance, Time.deltaTime * 8f);
        }
        else {
            dofScript.focalPoint = Mathf.Lerp(dofScript.focalPoint, GetComponent<Camera>().farClipPlane, Time.deltaTime * 8f);
        }
	}
}