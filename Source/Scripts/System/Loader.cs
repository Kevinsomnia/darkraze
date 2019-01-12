using UnityEngine;
using System.Collections;

public delegate void OnFinishLoading();
public class Loader : MonoBehaviour {
	public Camera loaderCam;
	public UITexture loaderBackground;
	public UILabel levelTitle;
	public UILabel levelSubheader;
	public UILabel levelDescription;
	public UILabel percentageLabel;
	public UISlider progressBar;
    public UISprite fadeBlack;
    public float fadeSpeed = 3.5f;

    private bool doneFadeIn;
	
	public static string loadData = "";
	public static bool isDone = false;
	public static OnFinishLoading _finished;
	
	private static GameObject currentPackMap = null;
	
	public static OnFinishLoading finished {
		get {
			return _finished;	
		}
		set {
			_finished = value;
		}
	}
	
	void Awake() {
		if(loadData == string.Empty) {
			return;
		}

		DontDestroyOnLoad(gameObject);
		percentageLabel.text = "0%";
        fadeBlack.alpha = 1f;
		StartCoroutine(LoadTheLevel());
	}

    void Update() {
        if(!doneFadeIn) {
            fadeBlack.alpha = Mathf.MoveTowards(fadeBlack.alpha, 0f, Time.deltaTime * fadeSpeed);

            if(fadeBlack.alpha <= 0f) {
                doneFadeIn = true;
            }
        }
    }

	private IEnumerator LoadTheLevel() {
        RestrictionManager.allInput = true;
		Topan.Network.isMessageQueueRunning = false;
        AudioListener.pause = true;
        GeneralVariables.lightingFactor = 1f;
			
		if(currentPackMap != null) {
			Destroy(currentPackMap);
			currentPackMap = null;
		}
		
		AsyncOperation loading = Application.LoadLevelAsync(loadData);
			
		if(loadData == "Main Menu") {
			loaderCam.enabled = false;
            AudioListener.pause = false;
				
			while(!loading.isDone) {
				yield return null;
			}
		}
		else {
			MapsList ml = ((GameObject)Resources.Load("Static Prefabs/MapList")).GetComponent<MapsList>();
			Map m = ml.maps[ml.GetIndex(loadData)];
				
			loaderCam.enabled = true;
				
			if(m.loaderScreenshot != null) {
				loaderBackground.enabled = true;
				loaderBackground.mainTexture = m.loaderScreenshot;
			}
				
			levelTitle.text = m.mapName;
			levelSubheader.text = m.loaderSubheader.ToUpper();
			levelDescription.text = "   " + m.loaderDescription;
         
            GeneralVariables.lightingFactor = m.lightingMultiplier;
		}

        float loadLerp = 0f;
        while(loadLerp < 0.999f) {
            loadLerp = Mathf.MoveTowards(loadLerp, loading.progress, Time.deltaTime * 2f);
            progressBar.value = loadLerp;
            percentageLabel.text = (loadLerp * 100f).ToString("F0") + "%";
            yield return null;
        }

		StartCoroutine(FinishedLoading());			
	}
	
	private IEnumerator FinishedLoading() {	
		Topan.Network.isMessageQueueRunning = true;	

        if(Loader.finished != null) {
            Loader.finished();
            Loader.finished = null;
        }

		GetComponent<AudioListener>().enabled = false;
		Resources.UnloadUnusedAssets();
        yield return null;

        GameSettings.UpdateAllReflections();

        AudioListener.pause = false;
        RestrictionManager.restricted = false;

        if(loadData != "Main Menu") {
            while(fadeBlack.alpha < 1f) {
                fadeBlack.alpha = Mathf.MoveTowards(fadeBlack.alpha, 1f, Time.deltaTime * fadeSpeed);
                yield return null;
            }
        }

        yield return null;

        Shader.WarmupAllShaders();
        loadData = string.Empty;
        isDone = true;
		Destroy(gameObject);
	}
	
	public static void LoadLevel(string levelName) {
		if(levelName == "Main Menu") {
			Loader.finished += () => {
				Time.timeScale = 1f;
				Screen.lockCursor = false;
			};
		}
		
		isDone = false;
		loadData = levelName;

		Application.LoadLevel("Loader");
	}
	
	public static void LoadPackMap(string mapName) {
		loadData = mapName;

		Application.LoadLevel("Loader");
	}	
}