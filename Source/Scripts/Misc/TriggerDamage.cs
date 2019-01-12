using UnityEngine;
using System.Collections;

public class TriggerDamage : MonoBehaviour {
	public int damage = 50;
	
	void OnTriggerEnter(Collider col) {
		col.SendMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
	}
}