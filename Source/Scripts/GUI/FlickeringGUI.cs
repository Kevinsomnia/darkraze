using UnityEngine;
using System.Collections;

public class FlickeringGUI : MonoBehaviour {
	public bool flickering = true;
	public float updateFrequency = 0.1f;
	public float flickerFrequency = 0.5f;
	public float dimAlpha = 0.5f;
	public float maxAlpha = 1f;
	
	private UIPanel panel;
	private UIWidget widget;
	
	void Start() {
		panel = GetComponent<UIPanel>();
		widget = GetComponent<UIWidget>();
		StartCoroutine(ExecuteFlicker());
	}
	
	private IEnumerator ExecuteFlicker() {
		while(flickering) {
            if(panel != null) {
                panel.alpha = (Random.value < flickerFrequency) ? dimAlpha : maxAlpha;
            }
            else if(widget != null) {
                widget.alpha = (Random.value < flickerFrequency) ? dimAlpha : maxAlpha;
            }
			
			yield return new WaitForSeconds(updateFrequency);
		}
	}
}