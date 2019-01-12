using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TimeScaleSound : MonoBehaviour {
	public AudioSource[] sources;

	[HideInInspector] public float pitchMod = 1f;

	private bool initialized = false;
	private float[] originalPitch;

	void Start() {
		Initialize();
	}

	private void Initialize() {
		if(initialized) {
			return;
		}

		if(sources.Length <= 0) {
			sources = new AudioSource[1];
			sources[0] = GetComponent<AudioSource>();
		}
		
		originalPitch = new float[sources.Length];
		for(int i = 0; i < sources.Length; i++) {
			originalPitch[i] = sources[i].pitch;
		}

		initialized = true;
	}
	
	void Update() {
		UpdatePitch();
	}
	
	public void UpdatePitch() {
		Initialize();

		for(int i = 0; i < sources.Length; i++) {
			sources[i].pitch = originalPitch[i] * Time.timeScale * pitchMod;
		}
	}

    public void UpdatePitch(float newPitch) {
		Initialize();
        pitchMod = newPitch;

		for(int i = 0; i < sources.Length; i++) {
			sources[i].pitch = originalPitch[i] * Time.timeScale * pitchMod;
		}
	}

	public void PlaySound() {
		Initialize();
		UpdatePitch();

		for(int i = 0; i < sources.Length; i++) {
			sources[i].Play();
		}
	}
}