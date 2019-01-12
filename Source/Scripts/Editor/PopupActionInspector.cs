using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PopupAction))]
public class PopupActionInspector : Editor {

	public override void OnInspectorGUI() {
		PopupAction pa = target as PopupAction;
		
		pa.fullscreenResolution = EditorGUILayout.Toggle("Full-screen Resolution:", pa.fullscreenResolution);
		pa.fullscreenMod = EditorGUILayout.Toggle("Display Mode:", pa.fullscreenMod);
		pa.textureQuality = EditorGUILayout.Toggle("Texture Quality:", pa.textureQuality);
		pa.anisoQuality = EditorGUILayout.Toggle("Anisotropic Quality:", pa.anisoQuality);
		pa.waterQuality = EditorGUILayout.Toggle("Water Quality:", pa.waterQuality);
		pa.terrainQuality = EditorGUILayout.Toggle("Terrain Quality:", pa.terrainQuality);
		pa.lightingMethod = EditorGUILayout.Toggle("Lighting Method:", pa.lightingMethod);
		pa.shadowQuality = EditorGUILayout.Toggle("Shadow Resolution:", pa.shadowQuality);
		pa.vsyncCount = EditorGUILayout.Toggle("V-Sync Count:", pa.vsyncCount);
		pa.crosshairStyle = EditorGUILayout.Toggle("Crosshair Style:", pa.crosshairStyle);
		pa.fpsCounter = EditorGUILayout.Toggle("FPS Counter:", pa.fpsCounter);
		pa.aimingMethod = EditorGUILayout.Toggle("Aiming Method:", pa.aimingMethod);
		pa.speakerMode = EditorGUILayout.Toggle("Speaker Mode:", pa.speakerMode);

		if(pa.speakerMode) {
			EditorGUI.indentLevel += 1;
			pa.restartNote = (UILabel)EditorGUILayout.ObjectField("Restart Note:", pa.restartNote, typeof(UILabel), true);
			if(pa.restartNote == null) {
				EditorGUILayout.HelpBox("Tell user that he has to restart game to take effect.", MessageType.None);
			}
			EditorGUI.indentLevel -= 1;
		}
		
		if(GUI.changed) {
			EditorUtility.SetDirty(pa);
		}
	}
}