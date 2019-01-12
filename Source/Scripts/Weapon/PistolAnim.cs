using UnityEngine;
using System.Collections;

public class PistolAnim : MonoBehaviour {
	public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.1f, 5f), new Keyframe(0.2f, 0f));
	public Transform leftForearm;
	public Transform leftWrist;
	public Transform rightForearm;
	public Transform rightWrist;
    public float animationIntensity = 1f;
    public float aimFactor = 0.5f;
    public float extraSmoothing = 27.5f;

    [HideInInspector] public bool startAnimation;
	
    private AimController ac;

	private Vector3 leftForearmDefaultRot;
	private Vector3 rightForearmDefaultRot;
	private Vector3 leftWristDefaultRot;
	private Vector3 rightWristDefaultRot;
	
	private float animVal;
    private float animTime;
	
	void Start() {
        ac = GeneralVariables.playerRef.ac;

		leftForearmDefaultRot = leftForearm.localEulerAngles;
		rightForearmDefaultRot = rightForearm.localEulerAngles;
		leftWristDefaultRot = leftWrist.localEulerAngles;
		rightWristDefaultRot = rightWrist.localEulerAngles;

        animTime = 0f;
        animVal = 0f;
        startAnimation = false;
	}
	
	void Update() {
		if(Time.timeScale <= 0f) {
			return;
		}

        animVal = Mathf.Lerp(animVal, animationCurve.Evaluate(animTime) * animationIntensity * ((ac.isAiming) ? aimFactor : 1f), Time.deltaTime * extraSmoothing);

        if(startAnimation) {
            animTime += Time.deltaTime;

            if(animTime >= animationCurve.keys[animationCurve.keys.Length - 1].time) {
                startAnimation = false;
                animTime = 0f;
            }
        }
				
		leftForearm.localRotation = Quaternion.Euler(leftForearmDefaultRot + new Vector3(animVal * 0.39f, -animVal * 0.9f, -animVal));
		leftWrist.localRotation = Quaternion.Euler(leftWristDefaultRot + new Vector3(animVal, -animVal * 0.42f, -animVal * 0.81f));
		rightForearm.localRotation = Quaternion.Euler(rightForearmDefaultRot + new Vector3(-animVal * 0.024f, -animVal * 0.67f, animVal));
		rightWrist.localRotation = Quaternion.Euler(rightWristDefaultRot + new Vector3(animVal * 0.61f, -animVal, animVal * 1.01f));
	}
}