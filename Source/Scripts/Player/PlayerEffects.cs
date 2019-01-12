using UnityEngine;
using System.Collections;

public class PlayerEffects : MonoBehaviour {
    public delegate void OnFinishEMP_Phase();

	public AudioSource audioSource;
	public float minVolumeFallSpeed = 5f;
	public float maxVolumeFallSpeed = 20f;
    public float velocityMultiplier = 1.5f;
    public SunShafts sunShafts;
    public float lookDotThreshold = 0.4f;
    public AudioClip empSound;
    public static OnFinishEMP_Phase onFinishEMP = null;

    public bool hasEMP {
        get {
            return (timerEMP > 0f);
        }
    }
	
    private UILabel empRestorationLabel;
    private RadialBlur rBlur;
    private PlayerMovement pm;
    private PlayerVitals pv;
    private Transform camTransform;
    private float timerEMP = 0f;

	void Start() {
        pm = GetComponent<PlayerMovement>();
        pv = GetComponent<PlayerVitals>();
        empRestorationLabel = GeneralVariables.uiController.empRecalibrate;
        rBlur = GeneralVariables.uiController.guiCamera.GetComponent<RadialBlur>();
		audioSource.volume = 0f;

        if(sunShafts != null) {
            camTransform = sunShafts.transform;
        }
	}
	
	void Update() {
        if(pm.controllerVeloMagn > minVolumeFallSpeed) {
			audioSource.volume = Mathf.Lerp(audioSource.volume, ((pm.controllerVeloMagn * velocityMultiplier) - minVolumeFallSpeed) * (1f / (maxVolumeFallSpeed - minVolumeFallSpeed)), Time.deltaTime * 4f);
		}
		else {
			audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * 4f);
		}

        if(sunShafts != null && sunShafts.shaftSource != null) {
            Vector3 shaftPos = ((sunShafts.directionShaft) ? -sunShafts.shaftSource.forward * 500000f : sunShafts.shaftSource.position);
            Vector3 dir = (shaftPos - camTransform.position).normalized;

            sunShafts.enabled = (GameSettings.settingsController.sunShafts == 1 && (Vector3.Dot(camTransform.forward, dir) >= lookDotThreshold));
        }

        if(Input.GetKeyDown(KeyCode.J)) {
            ExplosionVisualEffect(1f);
        }

        if(hasEMP) {
            timerEMP -= Time.deltaTime;
            if(timerEMP < 5f) {
                empRestorationLabel.enabled = (Time.time % 1 < 0.5f);

                if(timerEMP <= 0f) {
                    if(onFinishEMP != null) {
                        onFinishEMP();
                        onFinishEMP = null;
                    }
                }
            }
            else {
                empRestorationLabel.enabled = false;
            }
        }
        else {
            empRestorationLabel.enabled = false;
        }
	}

    public void StartPhase_EMP() {
        if(!hasEMP) {
            timerEMP = 21f;
            pv.distortEMP = 1.15f;
            pv.grainEMP = 0.75f;
            pv.vignetteEMP = 15f;
            pv.curShield = 0;
            pv.shrTimer = -30f;
            pv.startShRecoveryTimer = true;
            pv.shRecovering = false;
        
            NGUITools.PlaySound(empSound, 0.25f, 0.8f);
        }
    }

    public void ExplosionVisualEffect(float intensity) {
        StopCoroutine("ExplosionEffectRoutine");
        StartCoroutine(ExplosionEffectRoutine(Mathf.Clamp01(intensity)));
    }

    private IEnumerator ExplosionEffectRoutine(float intensity) {
        pv.hearingPenalty = 0.93f * Mathf.Max(0.4f, intensity);

        float time = 0f;
        while(time < 1f) {
            time += Time.deltaTime * 1.25f;

            float val = Mathf.Sin(time * Mathf.PI * 2f) * intensity;

            rBlur.blurIntensity = val * 2.6f;
            rBlur.blurWidth = (val * 0.9f) + 0.6f;
            rBlur.fisheyeEffect = val * 0.015f;
            yield return null;
        }

        rBlur.blurIntensity = 0f;
    }

    public void ClearExplosionEffect() {
        StopCoroutine("ExplosionEffectRoutine");
        rBlur.blurIntensity = 0f;
        rBlur.blurWidth = 0f;
        rBlur.fisheyeEffect = 0f;
    }
}