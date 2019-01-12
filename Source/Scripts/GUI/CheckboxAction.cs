using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UIToggle))]
public class CheckboxAction : MonoBehaviour {
	public bool bloom = false;
	public bool sunShafts = false;
	public bool motionBlur = false;
	public bool SSAO = false;
	public bool colorCorrection = false;
	public bool glareEffect = false;
    public bool enableHUD = false;
    public bool postAntialiasing = false;
	public bool wDepthOfField = false;
	
	private UIToggle toggleBox;

	void Start() {
        toggleBox = GetComponent<UIToggle>();
		InitializeValues();
	}
	
	public void InitializeValues() {
		if(bloom) {
			toggleBox.value = (GameSettings.settingsController.bloom == 1);
		}
		else if(sunShafts) {
            toggleBox.value = (GameSettings.settingsController.sunShafts == 1);
		}
		else if(motionBlur) {
            toggleBox.value = (GameSettings.settingsController.motionBlur == 1);
		}
		else if(SSAO) {
            toggleBox.value = (GameSettings.settingsController.SSAO == 1);
		}
		else if(colorCorrection) {
            toggleBox.value = (GameSettings.settingsController.colorCorrection == 1);
		}
		else if(glareEffect) {
            toggleBox.value = (GameSettings.settingsController.glareEffect == 1);
		}
        else if(postAntialiasing) {
            toggleBox.value = (GameSettings.settingsController.antiAliasing == 1);
        }
		else if(wDepthOfField) {
            toggleBox.value = (GameSettings.settingsController.wDepthOfField == 1);
		}
        else if(enableHUD) {
            toggleBox.value = (GameSettings.settingsController.enableHUD == 1);
        }
	}
	
	public void ApplyChanges() {
        int isChecked = ((toggleBox.value) ? 1 : 0);

		if(bloom) {
            GameSettings.settingsController.bloom = isChecked;
			PlayerPrefs.SetInt("Bloom", isChecked);
		}
		else if(sunShafts) {
            GameSettings.settingsController.sunShafts = isChecked;
            PlayerPrefs.SetInt("Sun Shafts", isChecked);
		}
		else if(motionBlur) {
            GameSettings.settingsController.motionBlur = isChecked;
            PlayerPrefs.SetInt("Motion Blur", isChecked);
		}
		else if(SSAO) {
            GameSettings.settingsController.SSAO = isChecked;
            PlayerPrefs.SetInt("SSAO", isChecked);
		}
		else if(colorCorrection) {
            GameSettings.settingsController.colorCorrection = isChecked;
            PlayerPrefs.SetInt("Color Correction", isChecked);
		}
		else if(glareEffect) {
            GameSettings.settingsController.glareEffect = isChecked;
            PlayerPrefs.SetInt("Glare Effect", isChecked);
		}
        else if(postAntialiasing) {
            GameSettings.settingsController.antiAliasing = isChecked;
            PlayerPrefs.SetInt("AntiAliasing", isChecked);
        }
		else if(wDepthOfField) {
            GameSettings.settingsController.wDepthOfField = isChecked;
            PlayerPrefs.SetInt("Weapon DoF", isChecked);
		}
        else if(enableHUD) {
            GameSettings.settingsController.enableHUD = isChecked;
            PlayerPrefs.SetInt("EnableHUD", isChecked);
        }
	}
}