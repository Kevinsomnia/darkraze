using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour {
    public Vector3 endPoint = Vector3.forward;
    public float lerpSpeed = 0.05f;

	private Transform tr;
    private Vector3 defPos;
    private float lerp;

    void Start() {
        tr = transform;
		defPos = tr.localPosition;
    }
	
	public void Update() {
        lerp = Mathf.PingPong(Time.time * lerpSpeed, 1f);
		tr.localPosition = Vector3.Lerp(defPos, (defPos + endPoint), lerp);
	}
}