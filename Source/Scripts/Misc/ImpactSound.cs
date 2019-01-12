using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ImpactSound : MonoBehaviour {
	public AudioClip impactSound;
	public float impactVolumeModifier = 1f;
	public Vector2 randomPitch = new Vector2(1f, 1f);
	
	void OnCollisionEnter(Collision col) {
		GetComponent<AudioSource>().pitch = Random.Range(randomPitch.x, randomPitch.y);
		GetComponent<AudioSource>().PlayOneShot(impactSound, Mathf.Clamp01(col.relativeVelocity.magnitude * 0.1f * impactVolumeModifier));
	}
}