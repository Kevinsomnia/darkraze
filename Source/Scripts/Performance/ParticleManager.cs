using UnityEngine;
using System.Collections;

public class ParticleManager : MonoBehaviour {
    public bool playSound = false;
    public AudioClip[] randomClips;

    [HideInInspector] public Transform cachedTransform;
    [HideInInspector] public bool inUse = false;
    [HideInInspector] public ParticleEmitter[] emitters;

    private RandomPitch rp;

	void Awake() {
	    Initialize();
	}

    public void Initialize() {
        cachedTransform = transform;

        if(playSound) {
            rp = GetComponent<RandomPitch>();
        }
    }
	
	public void EmitAll() {
        if(inUse) {
            return;
        }

        inUse = true;
        if(playSound) {
            GetComponent<AudioSource>().clip = randomClips[Random.Range(0, randomClips.Length)];

            if(rp) {
                rp.PlayAudio();
            }
            else {
                GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
            }
        }

	    foreach(ParticleEmitter pe in emitters) {
            pe.Emit();
        }

        inUse = false;
	}
}