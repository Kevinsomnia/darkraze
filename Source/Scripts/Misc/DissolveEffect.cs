using UnityEngine;
using System.Collections;

public class DissolveEffect : MonoBehaviour {
    public enum DissolveDirection {DissolveIn, DissolveOut}

    public bool dissolveOnStart = false; //Assuming the material is already dissolvable.
    public float dissolveSpeed = 0.1f;
    public Color dissolveColor = Color.red;
    public DissolveDirection dissolveDirection = DissolveDirection.DissolveIn;
    public bool destroyOnFinish = false;

	private float timer;
	private float timerStartDelay;
	private bool dof;
	
	private float _speed;
	private float dissolveAmount;
	private DissolveDirection dissolveDir;
	private Material material;
	private float startTime;

    void Start() {
        if(dissolveOnStart) {
            Dissolve(new Material(GetComponent<Renderer>().material), 0f, dissolveSpeed, dissolveColor, dissolveDirection, destroyOnFinish);
        }
    }

	public void Dissolve(Material mat, float dDelay, float dSpeed, Color dColor, DissolveDirection dir, bool destroyOnFinish) {
		timerStartDelay = dDelay;
        _speed = dSpeed;
		dissolveDir = dir;
		dof = destroyOnFinish;
        GetComponent<Renderer>().material = mat;
		material = GetComponent<Renderer>().material;
				
		if(dir == DissolveDirection.DissolveIn) {
            dissolveAmount = 1f;
			material.SetFloat("_Amount", 1f);
		}
		else if(dir == DissolveDirection.DissolveOut) {
            dissolveAmount = 0f;
			material.SetFloat("_Amount", 0f);
		}
		
		startTime = Time.time;
	}
	
	void Update() {
		material.SetFloat("_Amount", dissolveAmount);
		
		if(Time.time - startTime >= timerStartDelay) {
			if(dissolveDir == DissolveDirection.DissolveIn && dissolveAmount > 0) {
                dissolveAmount -= Time.deltaTime * _speed;
			}
			else if(dissolveDir == DissolveDirection.DissolveOut && dissolveAmount < 1) {
                dissolveAmount += Time.deltaTime * _speed;
			}			
		}
		
		if(dissolveAmount > 1 && dof && dissolveDir == DissolveDirection.DissolveOut) {
			Destroy(transform.root.gameObject);
		}		
	}
}