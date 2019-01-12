using UnityEngine;
using System.Collections;

public class FPSCounter : MonoBehaviour {
	public float refreshRate = 0.2f; //0.2 second intervals (5 per second).
	public Vector2 offset = new Vector2(2f, 0f);
	public int fontSize = 10;
	public Font customFont = null;
	public Color lowFPS = new Color(1f, 0.2f, 0f, 0.8f); //Lower than 30 FPS
	public Color highFPS = new Color(1f, 1f, 1f, 0.8f); //Higher than 30 FPS (normal).
    public bool displayMilliseconds = false;
	
	private float rTimer;
	private int frameCount;
	
	private GameSettings gameSettings;
    private Color guiColor;
	private bool showFPS;
	private float finalFPS;
    private string milliseconds;
    private Rect guiRect;
	
	void OnGUI() {
		if(!showFPS) {
			return;
		}
		
		if(customFont != null && GUI.skin.font != customFont) {
			GUI.skin.font = customFont;
		}

		GUI.skin.label.alignment = TextAnchor.UpperRight;		
		GUI.skin.label.fontSize = fontSize;
        GUI.color = guiColor;
		if(finalFPS >= Mathf.Infinity) {
			GUI.Label(guiRect, "-- FPS" + milliseconds);
		}
		else {
			GUI.Label(guiRect, finalFPS.ToString() + " FPS" + milliseconds);
		}
	}
	
	void Start() {
		rTimer = refreshRate;
		gameSettings = GameSettings.settingsController;
	}
	
	void Update() {
		showFPS = ((gameSettings.showFPS == 1) ? true : false);
		if(!showFPS) {
			return;
		}
		
		rTimer += Time.unscaledDeltaTime;
		frameCount++;
		
		if(rTimer >= refreshRate) {
			finalFPS = Mathf.Round(frameCount / rTimer);
            
            guiRect = new Rect(-offset.x, offset.y, Screen.width, Screen.height);
            guiColor = Color.Lerp(lowFPS, highFPS, Mathf.Clamp01(finalFPS / 30f));

            if(displayMilliseconds) {
                milliseconds = (finalFPS > 0f) ? " (" + ((1f / finalFPS) * 1000f).ToString("F1") + " ms)" : " (-- ms)";
            }
            else {
                milliseconds = "";
            }
			
			frameCount = 0;
			rTimer = 0f;
		}
	}
}