using UnityEngine;
using System.Collections;

public class TypingEffect : MonoBehaviour {
	public float charsPerSecond = 20;
	public float startDelay = 0;
	
	private UILabel label;
	private string text = "";
	private int offset = 0;
	private float nextChar = 0;
	private float timer;
	private float typeTimer = 0f;
	private float delay;
	
	void Start() {
		label = GetComponent<UILabel>();

        if(!string.IsNullOrEmpty(label.text)) {
            text = label.text;
        }

		label.text = "";
		delay = 1f / charsPerSecond;
	}
	
	void Update() {
        typeTimer += Time.unscaledDeltaTime;
		
		if(timer >= startDelay) {
			if(offset < text.Length && typeTimer >= delay) {
		        offset++;
				char c = text[offset - 1];
                
                delay = 1f / charsPerSecond;
				if(c == '.' || c == '\n' || c == '!' || c == '?') {
					delay *= 4f;
				}

				label.text = text.Substring(0, offset);
				typeTimer -= delay;
			}
		}
        else {
            timer += Time.unscaledDeltaTime;
        }
	}
	
	public void UpdateText(string txt) {
		offset = 0;
		text = txt;
	}
}