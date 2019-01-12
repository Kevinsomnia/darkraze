using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GrenadeAmmoManager))]
public class GrenadeAmmoManagerInspector : Editor {
	
	public override void OnInspectorGUI() {
		GrenadeAmmoManager gam = target as GrenadeAmmoManager;
		
		GUILayout.Space(5);
		
		if(GrenadeDatabase.publicGrenadeControllers.Length <= 0) {
			GUILayout.Box("You must have at least 1 grenade in the database to enable this manager!");
			gam.grenadeTypeOne = -1;
			gam.grenadeTypeTwo = -1;
			return;
		}
		
		int typeOneValue = Mathf.Clamp(gam.grenadeTypeOne, -1, GrenadeDatabase.publicGrenadeControllers.Length - 1);
		EditorGUILayout.LabelField("Grenade Slot #1 (" + ((typeOneValue == -1) ? "None" : GrenadeDatabase.GetGrenadeByID(typeOneValue).name) + ")", EditorStyles.boldLabel);
		EditorGUI.indentLevel += 1;
		gam.grenadeTypeOne = EditorGUILayout.IntField("Grenade ID:", typeOneValue);
		
		if(gam.grenadeTypeOne == -1) {
			GUI.enabled = false;
		}
		
		gam.typeOneGrenades = EditorGUILayout.IntSlider("  Grenade Amount:", gam.typeOneGrenades, 0, gam.typeOneMaxGrenades);
		gam.typeOneMaxGrenades = EditorGUILayout.IntField("  Max Grenades:", gam.typeOneMaxGrenades);
		GUI.enabled = true;
		EditorGUI.indentLevel -= 1;
		
		DarkRef.GUISeparator();
		
		int typeTwoValue = Mathf.Clamp(gam.grenadeTypeTwo, -1, GrenadeDatabase.publicGrenadeControllers.Length - 1);
		if(typeTwoValue == typeOneValue && typeOneValue > -1) {
			if(typeTwoValue < GrenadeDatabase.publicGrenadeControllers.Length - 1) {
				typeTwoValue++;
			}
			else {
				typeTwoValue--;
			}
		}
		
		if(GrenadeDatabase.publicGrenadeControllers.Length < 2) {
			GUILayout.Box("You must have at least 2 grenades in the database in order to enable the second slot!");
			GUI.enabled = false;
		}
		else {
			EditorGUILayout.LabelField("Grenade Slot #2 (" + ((typeTwoValue == -1) ? "None" : GrenadeDatabase.GetGrenadeByID(typeTwoValue).name) + ")", EditorStyles.boldLabel);
			EditorGUI.indentLevel += 1;
			gam.grenadeTypeTwo = EditorGUILayout.IntField("Grenade ID:", typeTwoValue);
			
			if(gam.grenadeTypeTwo == -1) {
				GUI.enabled = false;
			}
			
			gam.typeTwoGrenades = EditorGUILayout.IntSlider("  Grenade Amount:", gam.typeTwoGrenades, 0, gam.typeTwoMaxGrenades);
			gam.typeTwoMaxGrenades = EditorGUILayout.IntField("  Max Grenades:", gam.typeTwoMaxGrenades);
			EditorGUI.indentLevel -= 1;
		}
		
		if(GUI.changed) {
			EditorUtility.SetDirty(gam);
		}
	}
}