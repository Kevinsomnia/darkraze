using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class FlickeringAlpha : MonoBehaviour {
	public Vector2 alphaRange = new Vector2(0f, 255f);
	public float flickeringSpeed = 10f;
	public string colorProperty = "_Color";
	
	private float randomStart;
	private float timer;
	private Material mat;

	void Start() {
		randomStart = Random.value * 10f;
		timer = 0f;
		mat = GetComponent<Renderer>().material;
	}
	
	void Update() {
		timer += flickeringSpeed * Time.deltaTime;
		mat.SetColor(colorProperty, DarkRef.SetAlpha(mat.GetColor(colorProperty), Mathf.Clamp01(Mathf.Lerp(alphaRange.x / 255f, alphaRange.y / 255f, Mathf.PerlinNoise(timer, randomStart)))));

		if(timer > 1200f) {
			timer -= 1200f;
		}
	}
}