using UnityEngine;
using System.Collections;

public class IntroManager : MonoBehaviour {	
	public UIPanel introPanel;
	public float fadeSpeed = 1.5f;
	
	void Awake() {
        introPanel.alpha = 1f;
        Time.timeScale = 1f;
	}

    void Update() {
        introPanel.alpha = Mathf.MoveTowards(introPanel.alpha, 0f, Time.deltaTime * fadeSpeed);

        if(introPanel.alpha <= 0.001f) {
            Destroy(this);
        }
    }
}