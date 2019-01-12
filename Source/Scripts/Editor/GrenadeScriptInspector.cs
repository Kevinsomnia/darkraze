using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GrenadeScript))]
public class GrenadeScriptInspector : Editor {

	public override void OnInspectorGUI() {
		GrenadeScript gs = target as GrenadeScript;
		
		gs.grenadeType = (GrenadeType)EditorGUILayout.EnumPopup("Grenade Type:", gs.grenadeType);
        gs.startDelayOnImpact = EditorGUILayout.Toggle("Impact Detonation:", gs.startDelayOnImpact);
		gs.detonationDelay = EditorGUILayout.FloatField("Detonation Delay:", gs.detonationDelay);
		if(gs.grenadeType == GrenadeType.Explosive || gs.grenadeType == GrenadeType.Sticky) {
			gs.explosionPrefab = (GameObject)EditorGUILayout.ObjectField("Explosion Prefab:", gs.explosionPrefab, typeof(GameObject), true);
            gs.explosionDamage = EditorGUILayout.IntField("Explosion Damage:", gs.explosionDamage);
            gs.explosionRadius = EditorGUILayout.FloatField("Explosion Radius:", gs.explosionRadius);
		}
		if(gs.grenadeType == GrenadeType.Sticky) {
			gs.beepSound = (AudioClip)EditorGUILayout.ObjectField("Beep Sound:", gs.beepSound, typeof(AudioClip), true);
			if(!gs.gameObject.GetComponent<AudioSource>()) {
				GUILayout.Box("This object has no audio-source! Add one in order for the beep sound to work.");
			}
		}
		if(gs.grenadeType == GrenadeType.Smoke) {
			gs.smokeEmitter = (ParticleSystem)EditorGUILayout.ObjectField("Smoke Particles:", gs.smokeEmitter, typeof(ParticleSystem), true);
		}

        if(GUI.changed) {
            EditorUtility.SetDirty(gs);
        }
	}
}