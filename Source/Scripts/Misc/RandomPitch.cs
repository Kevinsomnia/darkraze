using UnityEngine;
using System.Collections;

public class RandomPitch : MonoBehaviour {
	public float minPitch = 0.95f;
	public float maxPitch = 1.0f;
	
	private TimeScaleSound tss;

	void Start() {
		tss = GetComponent<TimeScaleSound>();
		
		if(tss) {
			tss.pitchMod = Random.Range(minPitch, maxPitch);
		}
		else {
			GetComponent<AudioSource>().pitch = Random.Range(minPitch, maxPitch);
		}
	}
	
	public void PlayAudio() {
		if(tss) {
			tss.pitchMod = Random.Range(minPitch, maxPitch);
		}
		else {
			GetComponent<AudioSource>().pitch = Random.Range(minPitch, maxPitch);
		}
		
		GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
	}
}