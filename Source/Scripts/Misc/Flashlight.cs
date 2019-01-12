using UnityEngine;
using System.Collections;

public class Flashlight : MonoBehaviour {
	public AudioClip clickSound;
	
	private bool turnedOn;
	
	void Update() {
		if(cInput.GetButtonUp("Flashlight") && !RestrictionManager.restricted) {
			turnedOn = !turnedOn;
			if(clickSound != null) {
				GetComponent<AudioSource>().PlayOneShot(clickSound);
			}
		}
		
		GetComponent<Light>().enabled = turnedOn;
	}
}