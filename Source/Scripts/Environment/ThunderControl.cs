using UnityEngine;
using System.Collections;

//For controlling THUNDAH!
public class ThunderControl : Topan.TopanMonoBehaviour {
	public ThunderObject[] thunder;
    public float repeatFrequency = 9f;
    public float chanceFrequency = 0.4f;
    public float lightningTimeMin = 1f;
    public float lightningTimeMax = 1.5f;
	public ParticleSystem lightningSky;

    private bool isMultiplayer = false;

	void Start() {
        if(Topan.Network.isConnected) {
            isMultiplayer = true;
            if(Topan.Network.isServer) {
                InvokeRepeating("ChanceThunder", repeatFrequency * 0.5f, repeatFrequency);
            }
        }
        else {
            isMultiplayer = false;
		    InvokeRepeating("ChanceThunder", repeatFrequency * 0.5f, repeatFrequency);
        }
	}
	
	private void ChanceThunder() {
		if(Random.value <= chanceFrequency) {
            int randNum = Random.Range(0, thunder.Length);
		    ThunderObject randObj = thunder[randNum];

			if(!randObj.light.enabled) {
                float randomWait = Random.Range(lightningTimeMin, lightningTimeMax);
                if(isMultiplayer && Topan.Network.isServer) {
                    topanNetworkView.RPC(Topan.RPCMode.All, "DoThunder", (byte)randNum, randomWait);
                }
                else {
			        StartCoroutine(ActivateThunder(randObj, randomWait));
                }
			}
		}
	}

    [RPC]
    void DoThunder(byte objNum, float waitTime) {
        StartCoroutine(ActivateThunder(thunder[objNum], waitTime));
    }
	
	private IEnumerator ActivateThunder(ThunderObject thunder, float waitTime) {
		if(thunder.light.enabled) {
			yield break;
		}

		thunder.light.enabled = true;
		thunder.audio.Play();

		if(lightningSky != null) {
			lightningSky.Emit(1);
		}

		float timer = 0f;
		float randSpeed = Random.Range(12f, 15f);
		float defIntensity = thunder.light.intensity;
		while(timer < waitTime) {
			timer += Time.deltaTime;
			thunder.light.intensity = defIntensity * (0.6f + (Mathf.PerlinNoise(timer * randSpeed, randSpeed * 0.5f) * 0.4f));
			yield return null;
		}

		float fadeOut = 1f;
		while(fadeOut > 0f) {
			fadeOut = Mathf.MoveTowards(fadeOut, 0f, Time.deltaTime * 8f);
			thunder.light.intensity = defIntensity * fadeOut;
			yield return null;
		}

		thunder.light.enabled = false;
		thunder.light.intensity = defIntensity;
	}
	
	[System.Serializable]
	public class ThunderObject {
		public Light light;
		public AudioSource audio;
	}
}