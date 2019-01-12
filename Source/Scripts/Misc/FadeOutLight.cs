using UnityEngine;
using System.Collections;

public class FadeOutLight : MonoBehaviour {
	public float delay = 2;
	public float speed = 1;
	
    private Light lite;
	private float startTime;
	
	void Start() {
        lite = GetComponent<Light>();
		startTime = Time.time;
	}

	void Update() {
        if(lite == null || (lite != null && !lite.enabled)) {
            return;
        }

		if(Time.time - startTime >= delay) {
			lite.intensity = Mathf.Lerp(lite.intensity, 0f, Time.deltaTime * speed);

            if(lite.intensity <= 0f) {
			    lite.enabled = false;
                return;
		    }
		}		
	}
}