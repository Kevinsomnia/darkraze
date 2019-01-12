using UnityEngine;
using System.Collections;

public class EnemyHealth : BaseStats {
	public bool hasRagdoll = true;
	public GameObject[] deathReplacement;
	public bool destroyWhenDead = false;
	public int minMoney = 15;
	public int maxMoney = 20;
	public int minExperience = 10;
	public int maxExperience = 15;
	public Renderer[] renderers; //Renderers for decomposing effect
	public MonoBehaviour[] disableScripts;
	public float dissolveDelay = 0f; //Set to 0 to disable decomposing.
	public Material dissolveMaterial;
	public float dissolveSpeed = 0.3f;
	
	private Rigidbody[] rigid;
	private Collider[] colz;
	private CharacterController controller;
	private Animation anim;
	private bool dead;
	
	private float hMod;
	private float vMod;
	
	private AudioSource audioS;
	private int oldHealth;

	void Start() {
		audioS = GetComponent<AudioSource>();
		controller = GetComponent<CharacterController>();
		anim = GetComponentInChildren<Animation>();
		if(hasRagdoll) {
			rigid = GetComponentsInChildren<Rigidbody>();
			foreach(Rigidbody r in rigid) {
				r.isKinematic = true;
				r.useGravity = false;
			}
			colz = GetComponentsInChildren<Collider>();
			foreach(Collider col in colz) {
				if(col != controller) {
					col.isTrigger = true;
				}
			}
		}
		
		if(hMod > 0) {
			maxHealth = Mathf.RoundToInt(maxHealth * hMod);
		}
		if(vMod > 0) {
			minMoney = Mathf.RoundToInt(minMoney * vMod);
			maxMoney = Mathf.RoundToInt(maxMoney * vMod);
		}
		
		curHealth = maxHealth;
	}
	
	void Update() {
		curHealth = Mathf.Clamp(curHealth, 0, maxHealth);
	}
	
	public override void ApplyDamageMain(int damage, bool showBlood) {
		if(dead) {
			return;
		}
		
		curHealth -= damage;
		if(curHealth <= 0 && !dead) {
			Die();
		}
	}
	
	private void Die() {
		if(dead) {
			return;
		}

		foreach(GameObject g in deathReplacement) {
			Instantiate(g, transform.position, transform.rotation);
		}

        PlayerReference pr = GeneralVariables.playerRef;
		if(pr != null) {
            pr.GetComponent<PlayerStats>().GetXP(Random.Range(minExperience, maxExperience + 1));
		}

        WaveManager wm = GeneralVariables.waveManager;
		if(wm != null) {
			wm.EnemyKilled();
		}
		
		if(destroyWhenDead) {
			Destroy(gameObject);
			dead = true;
			return;
		}
		
		if(hasRagdoll) {
			Vector3 xzVelo = (controller != null) ? controller.velocity : Vector3.zero;
			
			foreach(Rigidbody body in rigid) {
				body.isKinematic = false;
				body.useGravity = true;				
				body.AddForce((xzVelo * Random.Range(1.9f, 2.2f)) + new Vector3(Random.Range(-4f, 4f), Random.Range(-1f, 2f), Random.Range(-1f, 2f)), ForceMode.Impulse);
			}
			
			foreach(Collider col in colz) {
				if(col != controller) {
					col.isTrigger = false;
				}
			}
		}
		
		if(audioS) {
			Destroy(audioS);
		}

		if(controller != null) {
			controller.enabled = false;
		}
		if(anim != null) {
			anim.enabled = false;
		}
				
		foreach(MonoBehaviour mb in disableScripts) {
			mb.enabled = false;
		}		
		
		if(dissolveDelay > 0) {
			foreach(Renderer r in renderers) {
				r.gameObject.AddComponent<DissolveEffect>().Dissolve(dissolveMaterial, dissolveDelay, dissolveSpeed, new Color(1f, 0.3f, 0f, 1f), DissolveEffect.DissolveDirection.DissolveOut, true);
			}
		}
		
		dead = true;
	}
	
	public void Toughen(Tougheners t) {
		hMod = t.healthModifier;
		vMod = t.valueModifier;
	}
}