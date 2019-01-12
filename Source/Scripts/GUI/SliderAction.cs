using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UISlider))]
public class SliderAction : MonoBehaviour {
	public UILabel label;
	public float minValue = 0;
	public float maxValue = 1;
	public float defaultValue = 1;
	public int decimalPlaces = 0;
	public string prefix = "";
	public string suffix = "";
	
	public bool isSensitivitySlider;
	public enum SensitivityDir {X, Y}
	public SensitivityDir sDir = SensitivityDir.X;
	
    public bool isMouseSmoothingSlider;
	public bool isShadowDistanceSlider;
	public bool isVegetationDistanceSlider;
	public bool isVegetationDensitySlider;
	public bool isMaxTreesSlider;
	public bool isTreeDrawDistanceSlider;
	public bool isSoundVolumeSlider;
	public bool isFOVSlider;
    public bool isGammaSlider;
	public bool isGameDurationSlider;
    public bool isRoundAmountSlider;
    public bool isIdleTimerSlider;
    public bool isBotCountSlider;
    public bool isMaxPlayerSlider;

	public int[] availableDurations = new int[9]{5, 8, 10, 15, 20, 25, 30, 45, 60};
    public int[] availableRoundCounts = new int[11]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 255}; //IMPORTANT: THESE ARE BYTE VALUES (even though they're ints) FOR NETWORK PURPOSES
    public int[] availableIdleLimit = new int[9]{1275, 30, 45, 60, 90, 120, 180, 240, 300}; //1275 = disabled. USE IN 5 SECOND INTERVALS. Maximum: 1270
	
	private UISlider slider;
    private float gammaSaveTimer;
    private bool dirtySave;
	
	[HideInInspector] public float currentValue;
	[HideInInspector] public int currentDuration;
    [HideInInspector] public int currentRoundAmount;
    [HideInInspector] public int currentIdleTime;

	public void Start() {
		slider = GetComponent<UISlider>();

        if(isGameDurationSlider) {
			minValue = 0;
			maxValue = (availableDurations.Length - 1);
            slider.numberOfSteps = availableDurations.Length;
		}
        else if(isRoundAmountSlider) {
            minValue = 0;
            maxValue = (availableRoundCounts.Length - 1);
            slider.numberOfSteps = availableRoundCounts.Length;
        }
        else if(isIdleTimerSlider) {
            minValue = 0;
            maxValue = (availableIdleLimit.Length - 1);
            slider.numberOfSteps = availableIdleLimit.Length;
        }
        else if(isGammaSlider) {
            EventDelegate newEvent = new EventDelegate();
            newEvent.Set(this, "OnSliderChange");
            slider.onChange.Add(newEvent);

            slider.numberOfSteps = Mathf.RoundToInt((maxValue - minValue) / 0.01f) + 1;
        }
		
		InitializeValues();        
	}
	
	public void InitializeValues() {
		if(isSensitivitySlider) {
			if(sDir == SensitivityDir.X) {
				slider.value = ((PlayerPrefs.GetFloat("Mouse Sensitivity X", defaultValue) - minValue) / (maxValue - minValue));
			}
			if(sDir == SensitivityDir.Y) {
				slider.value = ((PlayerPrefs.GetFloat("Mouse Sensitivity Y", defaultValue) - minValue) / (maxValue - minValue));
			}
		}
        else if(isMouseSmoothingSlider) {
            slider.value = ((PlayerPrefs.GetFloat("Mouse Smoothing", defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isShadowDistanceSlider) {
            slider.value = ((PlayerPrefs.GetInt("Shadow Distance", (int)defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isVegetationDistanceSlider) {
            slider.value = ((PlayerPrefs.GetInt("Vegetation Distance", (int)defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isVegetationDensitySlider) {
            slider.value = ((PlayerPrefs.GetFloat("Vegetation Density", defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isTreeDrawDistanceSlider) {
            slider.value = ((PlayerPrefs.GetInt("Tree Draw Distance", (int)defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isMaxTreesSlider) {
            slider.value = ((PlayerPrefs.GetInt("Tree Mesh Limit", (int)defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isSoundVolumeSlider) {
            slider.value = GameSettings.settingsController.soundVolume;
        }
        else if(isFOVSlider) {
            slider.value = ((PlayerPrefs.GetInt("FOV", (int)defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isGammaSlider) {
            slider.value = ((PlayerPrefs.GetFloat("GammaCorrect", defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isGameDurationSlider) {
            slider.value = ((PlayerPrefs.GetInt("SavedGameDuration", (int)defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isRoundAmountSlider) {
            slider.value = ((PlayerPrefs.GetInt("SavedRoundAmount", (int)defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isIdleTimerSlider) {
            slider.value = ((PlayerPrefs.GetInt("SavedIdleTime", (int)defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isBotCountSlider) {
            slider.value = ((PlayerPrefs.GetInt("SavedBotCount", (int)defaultValue) - minValue) / (maxValue - minValue));
        }
        else if(isMaxPlayerSlider) {
            slider.value = ((PlayerPrefs.GetInt("SavedMaxPlayers", (int)defaultValue) - minValue) / (maxValue - minValue));
        }
        else {
            slider.value = (defaultValue - minValue) / (maxValue - minValue);
        }
	}
	
	void Update() {
		if(label != null) {
			if(isGameDurationSlider && availableDurations.Length > 0) {
				currentDuration = availableDurations[Mathf.RoundToInt(slider.value * (availableDurations.Length - 1))];
			}
            else if(isRoundAmountSlider && availableRoundCounts.Length > 0) {
                currentRoundAmount = availableRoundCounts[Mathf.RoundToInt(slider.value * (availableRoundCounts.Length - 1))];
            }
            else if(isIdleTimerSlider && availableIdleLimit.Length > 0) {
                currentIdleTime = availableIdleLimit[Mathf.RoundToInt(slider.value * (availableIdleLimit.Length - 1))];
            }
			
			currentValue = minValue + (slider.value * (maxValue - minValue));
			
			if(isVegetationDensitySlider || isGammaSlider) {
				label.text = (currentValue * 100f).ToString("F0") + "%";
			}
            else if(isMouseSmoothingSlider) {
                if(currentValue <= 0f) {
                    label.text = "Disabled";
                }
                else {
                    label.text = (currentValue * 100f).ToString("F0") + "%";
                }
            }
			else if(isGameDurationSlider) {
				label.text = prefix + currentDuration.ToString() + suffix;
			}
            else if(isRoundAmountSlider) {
                if(currentRoundAmount >= 255) {
                    label.text = prefix + "Unlimited" + suffix;
                }
                else {
                    label.text = prefix + currentRoundAmount.ToString() + suffix;
                }
            }
            else if(isIdleTimerSlider) {
                if(currentIdleTime >= 1275) {
                    label.text = prefix + "DISABLED";
                }
                else {
                    label.text = prefix + currentIdleTime.ToString() + suffix;
                }
            }
            else if(isBotCountSlider) {
                if(currentValue <= 0) {
                    label.text = prefix + "DISABLED";
                }
                else {
                    label.text = prefix + currentValue.ToString() + (Mathf.Approximately(currentValue, 1f) ? " bot" : " bots");
                }
            }
            else {
                label.text = prefix + currentValue.ToString("F" + decimalPlaces.ToString()) + suffix;
            }
		}
		
		if(isGammaSlider && (Time.unscaledTime - gammaSaveTimer) >= 0.1f && dirtySave) {
            PlayerPrefs.SetFloat("GammaCorrect", currentValue);
            dirtySave = false;
        }
	}

    public void OnSliderChange() {
        if(!isGammaSlider) {
            return;
        }

        currentValue = minValue + (slider.value * (maxValue - minValue));
        GameSettings.settingsController.brightness = currentValue;
        gammaSaveTimer = Time.unscaledTime;
        dirtySave = true;
    }
	
	public void ApplyChanges() {
        if(isSensitivitySlider) {
            if(sDir == SensitivityDir.X) {
                GameSettings.settingsController.sensitivityX = currentValue;
                PlayerPrefs.SetFloat("Mouse Sensitivity X", currentValue);
            }
            if(sDir == SensitivityDir.Y) {
                GameSettings.settingsController.sensitivityY = currentValue;
                PlayerPrefs.SetFloat("Mouse Sensitivity Y", currentValue);
            }
        }
        else if(isMouseSmoothingSlider) {
            GameSettings.settingsController.mouseSmoothing = currentValue;
            PlayerPrefs.SetFloat("Mouse Smoothing", currentValue);
        }
        else if(isShadowDistanceSlider) {
            GameSettings.settingsController.shadowDistance = (int)currentValue;
            PlayerPrefs.SetInt("Shadow Distance", (int)currentValue);
        }
        else if(isVegetationDistanceSlider) {
            GameSettings.settingsController.vegetationDistance = (int)currentValue;
            PlayerPrefs.SetInt("Vegetation Distance", (int)currentValue);
        }
        else if(isVegetationDensitySlider) {
            GameSettings.settingsController.vegetationDensity = currentValue;
            PlayerPrefs.SetFloat("Vegetation Density", currentValue);
        }
        else if(isMaxTreesSlider) {
            GameSettings.settingsController.terrainMaxTrees = (int)currentValue;
            PlayerPrefs.SetInt("Tree Mesh Limit", (int)currentValue);
        }
        else if(isTreeDrawDistanceSlider) {
            GameSettings.settingsController.terrainTreeDrawDistance = (int)currentValue;
            PlayerPrefs.SetInt("Tree Draw Distance", (int)currentValue);
        }
        else if(isSoundVolumeSlider) {
            GameSettings.settingsController.soundVolume = slider.value;
            PlayerPrefs.SetFloat("Sound Volume", slider.value);
        }
        else if(isFOVSlider) {
            GameSettings.settingsController.FOV = (int)currentValue;
            PlayerPrefs.SetInt("FOV", (int)currentValue);
        }
        else if(isGameDurationSlider) {
            PlayerPrefs.SetInt("SavedGameDuration", (int)(slider.value * (availableDurations.Length - 1)));
            return;
        }
        else if(isRoundAmountSlider) {
            PlayerPrefs.SetInt("SavedRoundAmount", (int)(slider.value * (availableRoundCounts.Length - 1)));
            return;
        }
        else if(isIdleTimerSlider) {
            PlayerPrefs.SetInt("SavedIdleTime", (int)(slider.value * (availableIdleLimit.Length - 1)));
            return;
        }

		GameSettings.settingsController.UpdateGraphics();
	}
	
	public void LowestGraphics() {
		if(isShadowDistanceSlider) {
			slider.value = (40 - minValue) / (maxValue - minValue);
		}
		else if(isVegetationDistanceSlider) {
			slider.value = (30 - minValue) / (maxValue - minValue);
		}
		else if(isVegetationDensitySlider) {
			slider.value = 0.25f;
		}
		else if(isMaxTreesSlider) {
			slider.value = (50 - minValue) / (maxValue - minValue);
		}
		else if(isTreeDrawDistanceSlider) {
			slider.value = (150 - minValue) / (maxValue - minValue);
		}
	}
	
	public void HighestGraphics() {
		if(isShadowDistanceSlider) {
			slider.value = (125 - minValue) / (maxValue - minValue);
		}
		else if(isVegetationDistanceSlider) {
			slider.value = 1f;
		}
		else if(isVegetationDensitySlider || isMaxTreesSlider || isTreeDrawDistanceSlider) {
			slider.value = 1f;
		}
	}

	public void SetIntervalSteps(int steps) {
		if(slider == null) {
			Start();
		}

		slider.numberOfSteps = steps + 1;
	}
}