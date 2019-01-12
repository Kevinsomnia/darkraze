using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {
	public float moveSmoothing = 5f;
	public float positionThreshold = 0.03f; //For performance.
    public UICamera toDisable; //fast speed accidentally clik;
    public DistortionEffect distortionFx; //optional distortion camera effect;
    public float distortIntensity = 1f;
	public bool ignoreTimeScale = false;
    public bool deselectUI = true;
	
	private Transform tr;
	private Vector3 targetPos;
    private float travelDistance;

	void Awake() {
		tr = transform;
		targetPos = tr.localPosition;
        travelDistance = 0f;
	}
	
	void Update() {
        float dist = (targetPos - tr.localPosition).magnitude;
		if(dist >= positionThreshold) {
			tr.localPosition = Vector3.Lerp(tr.localPosition, targetPos, ((ignoreTimeScale) ? Time.unscaledDeltaTime : Time.deltaTime) * moveSmoothing);
		}

        if(distortionFx != null) {
            if(travelDistance > 0f) {
                distortionFx.baseIntensity = Mathf.Sin(Mathf.Clamp01((travelDistance - dist) / travelDistance) * Mathf.PI) * distortIntensity;
                distortionFx.enabled = (distortionFx.baseIntensity > 0.005f);
            }
            else {
                distortionFx.enabled = false;
            }
        }

        if(toDisable != null) {
            toDisable.enabled = ((targetPos - tr.localPosition).sqrMagnitude <= 120f);
        }
	}
	
	public void TargetPos(Vector3 pos) {
		targetPos = pos;
        travelDistance = (targetPos - tr.localPosition).magnitude;

        if(deselectUI) {
            UICamera.selectedObject = null;
        }
	}
}