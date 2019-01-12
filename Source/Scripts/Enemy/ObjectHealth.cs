using UnityEngine;
using System.Collections;

public class ObjectHealth : BaseStats {
	public GameObject[] destroyObjects;
	public MonoBehaviour[] disableScripts;
	public GameObject[] replacementObjects;
	
	private bool dead = false;

	void Update() {
		curHealth = Mathf.Clamp(curHealth, 0, maxHealth);
	}
	
	public override void ApplyDamageMain(int damage, bool showBlood) {
		curHealth -= damage;
		if(curHealth <= 0 && !dead) {
			Die();
		}
	}
	
	public void Die() {
		if(replacementObjects.Length > 0) {
			foreach (GameObject ro in replacementObjects){
				Instantiate(ro, transform.position, Quaternion.identity);
			}
		}
		if(destroyObjects.Length > 0) {
			foreach (GameObject dobj in destroyObjects){
				Destroy(dobj);
			}
		}
		if(disableScripts.Length > 0) {
			foreach (MonoBehaviour ds in disableScripts){
				ds.enabled = false;
			}
		}
		
		dead = true;
	}
}