using UnityEngine;
using System.Collections;

public class RainEffect : MonoBehaviour {
	public PlayerMovement pMove;
    public PlayerLook pLook;
    public float maximumEmission = 30f;

    private ParticleSystem rainFX;
	private bool checkRay;

    void Awake() {
        UIController uic = GeneralVariables.uiController;
        rainFX = uic.rainFX;
		rainFX.enableEmission = false;
    }

    void Update() {
		if((Time.frameCount % 2) == 0) {
			checkRay = Physics.Raycast(transform.position, Vector3.up);
		}

        if((pLook.yRot > -10f || pMove.xyVelocity >= 0.5f) && !checkRay) {
            rainFX.emissionRate = maximumEmission * Mathf.Clamp01(0.15f + (pLook.yRot / (pLook.maximumY * 0.75f)));
        }
        else {
            rainFX.emissionRate = 0f;
        }
    }

	public void RainEnabled(bool e) {
		rainFX.enableEmission = true;
	}
}