using UnityEngine;
using System.Collections;

[System.Serializable]
public class DestroyAfterTime : PoolItem {
	public float destroyTime = 10f;
	public float randomness = 0f;
	public bool fadeOut = false;
	public float fadeSpeed = 3;
	public string colorName = "_Color";
	public bool isParticle;
    public bool poolObject = false;
	public ParticleEmitter[] emitters;
	public Renderer[] renderers;

	private bool startedDestroy;
    private float[] defAlpha;
	private float time;
	private float timer;

    void Awake() {
        if(!poolObject) {
            InstantiateStart();
        }
    }
		
	public override void InstantiateStart() {
		time = destroyTime + (Random.value * randomness);
		
		if(isParticle) {
			foreach(ParticleEmitter e in emitters) {
				e.Emit();
			}
		}

        if(fadeOut) {
			if(defAlpha == null) {
            	defAlpha = new float[renderers.Length];
			}

            for(int i = 0; i < renderers.Length; i++) {
                if(renderers[i] == null)
                    continue;

				Color col = renderers[i].material.GetColor(colorName);

				if(defAlpha[i] <= 0f) {
                	defAlpha[i] = col.a;
				}

				renderers[i].material.SetColor(colorName, DarkRef.SetAlpha(col, defAlpha[i]));
			}
        }

		timer = 0f;
		startedDestroy = false;
	}

	void Update() {
		if(timer >= time) {
			if(!startedDestroy) {
				StartCoroutine(StartDestroy());
			}
		}
		else {
			timer += Time.deltaTime;
		}
	}

	private IEnumerator StartDestroy() {
		startedDestroy = true;

		if(fadeOut) {
			float mod = 1f;
			while(mod > 0f) {
				mod -= Time.deltaTime * fadeSpeed;
				for(int i = 0; i < renderers.Length; i++) {
					Color col = renderers[i].material.GetColor(colorName);
					renderers[i].material.SetColor(colorName, DarkRef.SetAlpha(col, defAlpha[i] * Mathf.Clamp01(mod)));
				}
				yield return null;
			}            
		}

        if(transform.parent != null && transform.parent.name == "KeepScale") {
            Transform parent = transform.parent;
            transform.parent = null;
            Destroy(parent.gameObject);
        }

        if(poolObject) {
            AddToPool();
        }
        else {
			Destroy(gameObject);
        }

		timer = 0f;
	}
}