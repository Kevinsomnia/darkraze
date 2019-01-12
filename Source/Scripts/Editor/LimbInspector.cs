using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Limb))]
public class LimbInspector : Editor {
	public override void OnInspectorGUI() {
		Limb lb = target as Limb;
		
		if(lb.rootStats != null) {
			lb.limbType = (Limb.LimbType)EditorGUILayout.EnumPopup("Limb Type:", lb.limbType);
		}
		lb.rootStats = (BaseStats)EditorGUILayout.ObjectField("Root Stats:", lb.rootStats, typeof(BaseStats), true);
		
		if(lb.rootStats == null) {
			GUILayout.Box("Please assign a BaseStats component to 'Root Stats' in order to continue");
		}
		
		if(lb.rootStats != null) {
			DarkRef.GUISeparator(5f);
			
			lb.overrideMultiplier = EditorGUILayout.Toggle("Override:", lb.overrideMultiplier);
			
			EditorGUI.indentLevel += 1;
			if(lb.overrideMultiplier) {
                EditorGUIUtility.labelWidth = 200f;
				lb.damageMultOverride = EditorGUILayout.FloatField("Damage Multiplier:", Mathf.Clamp(lb.damageMultOverride, 0f, 10f));
				EditorGUIUtility.LookLikeControls();
			}
			else {
				float readDisplayThing = 0f;
				if(lb.limbType == Limb.LimbType.Head) {
					lb.damageMultOverride = 4f;
					readDisplayThing = 4f;
				}
				else if(lb.limbType == Limb.LimbType.Chest) {
					lb.damageMultOverride = 1f;
					readDisplayThing = 1f;
				}
				else {
					lb.damageMultOverride = 0.7f;
					readDisplayThing = 0.7f;
				}

                EditorGUIUtility.labelWidth = 200f;
				GUI.color = Color.gray;
				readDisplayThing = EditorGUILayout.FloatField("Damage Multiplier:", readDisplayThing);
				GUI.color = Color.white;
				EditorGUIUtility.LookLikeControls();
			}
			EditorGUI.indentLevel -= 1;
		}
		GUI.color = Color.red;
		if(lb.GetComponent<BaseStats>()) {
			GUILayout.Box("NOTE: This component is assigned to an object that already have BaseStats. Are you sure this is correct?");
		}
		
		if(!lb.GetComponent<Collider>()) {
			GUILayout.Box("WARNING: This component is assigned to an object without a collider! You must have a collider in order for this to work!");
		}
		
		if(GUI.changed) {
			EditorUtility.SetDirty(lb);
		}
	}
}