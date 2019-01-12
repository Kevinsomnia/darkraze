using UnityEngine;
using System.Collections;

public class VariableScope : MonoBehaviour {
	public Camera scopeCamera;
	public float baseFOV = 30f;
	public float[] magnificationSteps = new float[3]{2f, 4f, 8f};
	public bool useScrollWheel = true; //False will use MMB click.
	public float scrollSensitivity = 20f;
	public TextMesh zoomText;

	private AimController ac;
	private PlayerLook pl;
	private AimController aControl {
		get {
			if(ac == null && GeneralVariables.playerRef != null) {
				ac = GeneralVariables.playerRef.ac;
				pl = GeneralVariables.playerRef.GetComponent<PlayerLook>();
			}

			return ac;
		}
	}

	private float scrollAmount = 0f;
	private float mMag = 0f;
	private int magIndex = 0;
	private float camHeight;

	void Start() {
		if(magnificationSteps.Length <= 0 || aControl == null || transform.root.gameObject != GeneralVariables.player) {
			Destroy(scopeCamera.gameObject);
			this.enabled = false;
			return;
		}

		magIndex = 0;
		mMag = magnificationSteps[magIndex];
		camHeight = 100f * Mathf.Tan(baseFOV * 0.5f * Mathf.Deg2Rad);
	}

	void Update() {
        if(transform.root.gameObject != GeneralVariables.player) {
            return;
        }

		if(aControl != null && aControl.isAiming) {
			float scrollInput = Input.GetAxis("Mouse ScrollWheel");
			if(useScrollWheel && Mathf.Abs(scrollInput) >= 0.01f) {
				scrollAmount += scrollInput * scrollSensitivity * 2f;
			}

			scrollAmount = Mathf.Clamp(scrollAmount, -20f, 20f);
			if(Mathf.Abs(scrollAmount) >= 10f || (!useScrollWheel && Input.GetMouseButtonDown(2))) {
				int dir = ((scrollAmount > 0f) ? 1 : -1);
				magIndex += ((useScrollWheel) ? dir : 1);

				if(useScrollWheel) {
					magIndex = Mathf.Clamp(magIndex, 0, magnificationSteps.Length - 1);
				}
				else {
					if(magIndex >= magnificationSteps.Length) {
						magIndex = 0;
					}
					else if(magIndex < 0) {
						magIndex = magnificationSteps.Length - 1;
					}
				}

				scrollAmount = 0f;
			}

			pl.magnificationFactor = 1f / mMag;
		}
		else {
			pl.magnificationFactor = 1f;
		}

		mMag = Mathf.Lerp(mMag, magnificationSteps[magIndex], Time.deltaTime * 12f);
		if(scopeCamera != null) {
			scopeCamera.fieldOfView = 2f * Mathf.Tan(camHeight * 0.5f / (50f * mMag)) * Mathf.Rad2Deg;
		}

		if(zoomText != null) {
			zoomText.text = "x" + mMag.ToString("F1");
		}
	}
}