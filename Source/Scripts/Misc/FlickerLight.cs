using UnityEngine;
using System.Collections;

public class FlickerLight : MonoBehaviour {
	public enum FlickerMethod {Sine, Perlin};
	public FlickerMethod flickerMethod = FlickerMethod.Perlin;
	public float minIntensity = 0.5f;
	public float maxIntensity = 1;
	public float frequency = 1;
	
	public Renderer glowPlane;
	public string colorPropName = "_Color";
	
	private float defAlpha;
	
	void Start() {		
		if(glowPlane) {
			defAlpha = glowPlane.material.GetColor(colorPropName).a;
		}
	}
	
	void Update() {
        if(flickerMethod == FlickerMethod.Sine) {
		    GetComponent<Light>().intensity = minIntensity + Mathf.Abs(Mathf.Sin(Time.time * frequency) * (maxIntensity - minIntensity));
        }
        else if(flickerMethod == FlickerMethod.Perlin) {
            GetComponent<Light>().intensity = minIntensity + Mathf.Abs(Mathf.PerlinNoise(Time.time * frequency, 23.7f) * (maxIntensity - minIntensity));
        }
		
		if(glowPlane) {
			glowPlane.material.SetColor(colorPropName, DarkRef.SetAlpha(glowPlane.material.GetColor(colorPropName), defAlpha * (GetComponent<Light>().intensity / maxIntensity)));
		}
	}
}