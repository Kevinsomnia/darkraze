using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(WeaponManager))]
public class WeaponManagerInspector : Editor {
	public override void OnInspectorGUI() {
		WeaponManager wm = target as WeaponManager;
		
		if(wm.wepList == null) {
			WeaponList savedWL = (WeaponList)Resources.Load("Static Prefabs/Weapon List", typeof(WeaponList));
			if(savedWL) {
				wm.wepList = savedWL;
				EditorUtility.SetDirty(wm);
			}

			wm.wepList = (WeaponList)EditorGUILayout.ObjectField("Weapon List Prefab:", wm.wepList, typeof(WeaponList), true);
			EditorGUILayout.HelpBox("Note that leaving this empty will have a HUGE performance impact at the start!", MessageType.Warning);
		}

		if(wm.poolList == null) {
			PoolingList savedPL = (PoolingList)Resources.Load("Static Prefabs/PoolingList", typeof(PoolingList));
			if(savedPL) {
				wm.poolList = savedPL;
				EditorUtility.SetDirty(wm);
			}
			
			wm.poolList = (PoolingList)EditorGUILayout.ObjectField("Pool List Prefab:", wm.poolList, typeof(PoolingList), true);
			EditorGUILayout.HelpBox("Note that leaving this empty will have a HUGE performance impact at the start!", MessageType.Warning);
		}
				
		GUILayout.Space(5);
		
		EditorGUILayout.LabelField("Starting Weapons", EditorStyles.boldLabel);
		
		EditorGUI.indentLevel += 1;
		
		int pValue = Mathf.Clamp(wm.startingPrimary, 0, WeaponDatabase.publicGunControllers.Length - 1);
		int sValue = Mathf.Clamp(wm.startingSecondary, 0, WeaponDatabase.publicGunControllers.Length - 1);

        EditorGUIUtility.labelWidth = 210f;
		wm.startingPrimary = EditorGUILayout.IntField("Primary Weapon (" + WeaponDatabase.GetWeaponByID(pValue).gunName + "):", pValue);
		wm.startingSecondary = EditorGUILayout.IntField("Secondary Weapon: (" + WeaponDatabase.GetWeaponByID(sValue).gunName + "):", sValue);
		EditorGUIUtility.LookLikeControls();
		
		EditorGUI.indentLevel -= 1;
		
		DarkRef.GUISeparator();
		
		wm.meleeController = (MeleeController)EditorGUILayout.ObjectField("Melee Controller:", wm.meleeController, typeof(MeleeController), true);
		wm.grenadeManager = (GrenadeManager)EditorGUILayout.ObjectField("Grenade Manager:", wm.grenadeManager, typeof(GrenadeManager), true);
		wm.hands = (GameObject)EditorGUILayout.ObjectField("Hands:", wm.hands, typeof(GameObject), true);
		
		GUILayout.Space(10);
		
		wm.drawTime = EditorGUILayout.FloatField("Base Draw Time:", wm.drawTime);
		wm.drawSound = (AudioClip)EditorGUILayout.ObjectField("Draw Sound:", wm.drawSound, typeof(AudioClip), true);
		wm.dropSound = (AudioClip)EditorGUILayout.ObjectField("Drop Sound:", wm.dropSound, typeof(AudioClip), true);
		
		if(GUI.changed) {
			EditorUtility.SetDirty(wm);
		}
	}
}