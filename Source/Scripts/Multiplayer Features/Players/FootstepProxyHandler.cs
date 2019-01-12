using UnityEngine;
using System.Collections;

public class FootstepProxyHandler : MonoBehaviour {
	public AudioSource footstepSource;
	public AudioClip[] concrete = new AudioClip[2];
	public AudioClip[] dirt = new AudioClip[2];
	public AudioClip[] metal = new AudioClip[2];
	public AudioClip[] wood = new AudioClip[2];
    public float runStepRate = 1.5f;
    public float sprintStepRate = 2f;
	
	private Transform tr;
    private RaycastHit hit;
	private float footstepTimer;
	private AudioClip footSound;
	private bool groundStep;
	private float impactVelo;
	
	private MovementSync_Proxy msP;
	private TimeScaleSound tss;
	
	void Start() {
		tr = transform;
		tss = footstepSource.GetComponent<TimeScaleSound>();
		msP = GetComponent<MovementSync_Proxy>();
		footSound = concrete[0];
	}
	
	void Update() {
		if(msP.isGrounded) {
			if(Mathf.Abs(msP.velocity.x) + Mathf.Abs(msP.velocity.z) > 0.04f) {
				footstepTimer += msP.velocity.magnitude;

                float stepRate = (msP.isSprinting) ? sprintStepRate : runStepRate;
			    if(footstepTimer >= stepRate) {
				    SelectFootstep();
				    tss.pitchMod = Random.Range(0.9f, 1.0f);
				
				    if(msP.isSprinting) {
					    footstepSource.GetComponent<AudioSource>().volume = 0.175f;
				    }
				    else if(msP.isCrouching || msP.isWalking) {
					    footstepSource.GetComponent<AudioSource>().volume = 0.07f;
				    }
				    else {
					    footstepSource.GetComponent<AudioSource>().volume = 0.12f;
				    }
				
				    tss.GetComponent<AudioSource>().PlayOneShot(footSound);
				    footstepTimer -= stepRate;
			    }
			}
						
			if(groundStep) {
				if(impactVelo > 0.008f) {
					SelectFootstep();
					
					footstepSource.volume = 0.12f + Mathf.Clamp(impactVelo * 2f, 0f, 0.7f);
					tss.GetComponent<AudioSource>().PlayOneShot(footSound);
					
					impactVelo = 0f;
				}
				
				groundStep = false;
			}
		}
				
		if(!msP.isGrounded) {
			impactVelo = Mathf.Abs(msP.velocity.y);
			groundStep = true;
		}
	}
		
	private void SelectFootstep() {
        AudioClip clipToPlay = null;

		if(Physics.Raycast(footstepSource.transform.position, Vector3.down, out hit, 1.2f)) {
			string footTag = hit.collider.tag;
            if(footTag == "Dirt") {
                do {
                    clipToPlay = dirt[Random.Range(0, dirt.Length)];
                }
                while(dirt.Length > 1 && clipToPlay == footSound);
            }
            else if(footTag == "Metal") {
                do {
                    clipToPlay = metal[Random.Range(0, metal.Length)];
                }
                while(metal.Length > 1 && clipToPlay == footSound);
            }
            else if(footTag == "Wood") {
                do {
                    clipToPlay = wood[Random.Range(0, wood.Length)];
                }
                while(wood.Length > 1 && clipToPlay == footSound);
            }
            else {
                do {
                    clipToPlay = concrete[Random.Range(0, concrete.Length)];
                }
                while(concrete.Length > 1 && clipToPlay == footSound);
            }
		}

        footSound = clipToPlay;
	}
}