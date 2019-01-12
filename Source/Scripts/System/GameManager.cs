using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
    public static bool isPaused = false;

    private static MapBoundaries _boundSet = null;
    public static MapBoundaries boundarySettings {
        get {
            if(_boundSet == null && Topan.Network.isConnected) {
                _boundSet = (MapBoundaries)FindObjectOfType(typeof(MapBoundaries));

                if(_boundSet == null) {
                    GameObject mapRoot = GameObject.Find("Map");
                    if(mapRoot != null) {
                        _boundSet = mapRoot.GetComponent<MapBoundaries>();

                        if(_boundSet == null) {
                            _boundSet = mapRoot.AddComponent<MapBoundaries>();
                        }
                    }
                    else {
                        Debug.LogError("Cannot calculate the map boundaries, please assign it to the map root!");
                        return null;
                    }
                }
            }

            return _boundSet;
        }
    }

    public float pauseBlurIntensity = 1f;

	[HideInInspector] public float leaderboardBlur;
    [HideInInspector] public float damageBlur;
    [HideInInspector] public float remBlur;
	
	private GameObject pauseMenu;
	private BlurEffect blurEffect;
	private BlurEffect blurEffect2;
	private UIPanel settingsPanel;
	private GameObject[] activeOnPlay;

	private UIPanel pausePanel;
	private float lastTimeScale;
	private bool isTransitioningPause;
	private bool isTransitioningSettings;

    private Vector3 inputPos;
    private float idleTime;

	void Start() {
		UIController uicontroller = GeneralVariables.uiController;
		
		pauseMenu = uicontroller.pauseMenu;
		pausePanel = pauseMenu.GetComponent<UIPanel>();
		blurEffect = uicontroller.pauseBlur;
		blurEffect2 = uicontroller.pauseBlur2;
		settingsPanel = uicontroller.settingsPanel;

		lastTimeScale = 1f;
		leaderboardBlur = 0f;
        damageBlur = 0f;
		pausePanel.alpha = 0f;
		settingsPanel.alpha = 0f;
        isPaused = false;
        AudioListener.pause = false;
	}
	
	void Update() {
		if(!RestrictionManager.allInput && Input.GetKeyDown(KeyCode.Escape)) {
			PauseFunction();
		}

        if(Topan.Network.isConnected && !Topan.Network.isServer && GeneralVariables.player != null && Topan.Network.HasServerInfo("it") && Input.mousePosition == inputPos && !Input.anyKey) {
            idleTime += Time.unscaledDeltaTime;

            if(idleTime >= ((byte)Topan.Network.GetServerInfo("it") * 5)) {
                GeneralVariables.Networking.KickPlayer(3);
                idleTime = -1f;
            }
        }
        else {
            idleTime = 0f;
        }

        inputPos = Input.mousePosition;

		if(blurEffect != null) {
			if((pausePanel.alpha - settingsPanel.alpha) > 0.0001f || leaderboardBlur > 0.0001f || damageBlur > 0.0001f || remBlur > 0.0001f) {
				blurEffect.enabled = true;
				blurEffect.blurSpread = (Mathf.Clamp01(pausePanel.alpha - settingsPanel.alpha) * pauseBlurIntensity) + leaderboardBlur + (damageBlur * (1f - Mathf.Clamp01(pausePanel.alpha + settingsPanel.alpha))) + remBlur;
			}
			else {
				blurEffect.enabled = false;
			}
		}

		if(blurEffect2 != null) {
			if(settingsPanel.alpha > 0.0001f) {
				blurEffect2.enabled = true;
				blurEffect2.blurSpread = settingsPanel.alpha * pauseBlurIntensity;
			}
			else {
				blurEffect2.enabled = false;
			}
		}        
	}

	public void OpenSettings(bool o) {
		StartCoroutine(FadeSettings(o));
	}
	
	public void PauseFunction() {
        StartCoroutine(FadePauseMenu(!isPaused));
	}
	
	private IEnumerator FadePauseMenu(bool e) {
		if(isTransitioningPause) {
			yield break;
		}
		
		isPaused = e;
		isTransitioningPause = true;
		
		if(e) {
            RestrictionManager.pauseMenu = true;
			pauseMenu.SetActive(true);
			Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
			
			if(!Topan.Network.isConnected) {
				AudioListener.pause = true;
				lastTimeScale = Time.timeScale;
				DarkRef.SetTimeScale(0f);
			}
			
			while(pausePanel.alpha < 1f) {
				pausePanel.alpha = Mathf.Clamp01(Mathf.MoveTowards(pausePanel.alpha, 1f, Time.unscaledDeltaTime * 7f));
				yield return null;
			}
		}
		else {
			AudioListener.pause = false;
			DarkRef.SetTimeScale(lastTimeScale);
			StartCoroutine(FadeSettings(false));
			
			while(pausePanel.alpha > 0f) {
				pausePanel.alpha = Mathf.Clamp01(Mathf.MoveTowards(pausePanel.alpha, 0f, Time.unscaledDeltaTime * 7f));
                yield return null;
			}
			
			pauseMenu.SetActive(false);
            RestrictionManager.pauseMenu = false;

            if(GeneralVariables.player != null) {
                yield return null;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
		}
		
		isTransitioningPause = false;
	}
	
	private IEnumerator FadeSettings(bool e) {
		if(isTransitioningSettings) {
			yield break;
		}
		
		isTransitioningSettings = true;

		if(e) {
			settingsPanel.gameObject.SetActive(true);

            float sAlpha = settingsPanel.alpha;
            while(sAlpha < 1f) {
                sAlpha = Mathf.Clamp01(Mathf.MoveTowards(sAlpha, 1f, Time.unscaledDeltaTime * 7f));
                settingsPanel.alpha = sAlpha;
                yield return null;
			}
		}
		else {
            float sAlpha = settingsPanel.alpha;
            while(sAlpha > 0f) {
                sAlpha = Mathf.Clamp01(Mathf.MoveTowards(sAlpha, 0f, Time.unscaledDeltaTime * 7f));
                settingsPanel.alpha = sAlpha;
				yield return null;
			}

			settingsPanel.gameObject.SetActive(false);
		}
		
		isTransitioningSettings = false;
	}
}