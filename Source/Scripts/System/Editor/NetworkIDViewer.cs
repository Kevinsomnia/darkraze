using UnityEngine;
using UnityEditor;
using System.Collections;

public class NetworkIDViewer : EditorWindow {
	private Vector2 scrollPos;
	
	[MenuItem("Tools/Network ID Viewer")]
	private static void OpenWindow() {
		EditorWindow.GetWindow<NetworkIDViewer>();
	}
	
	void OnGUI() {
		Topan.NetworkView[] netViewsInScene = (Topan.NetworkView[])FindObjectsOfType(typeof(Topan.NetworkView));
		string[] allStrings = EditorApplication.currentScene.Split(new string[]{"/"}, System.StringSplitOptions.None);
		string sceneName = allStrings[allStrings.Length - 1];
		EditorGUILayout.LabelField("Overview of current network IDs (" + netViewsInScene.Length + ") [" + sceneName.Substring(0, sceneName.Length - 6) + "]", EditorStyles.boldLabel);
		
		EditorGUI.indentLevel += 1;
		
		if(netViewsInScene.Length <= 0) {
			GUI.color = Color.red;
			EditorGUILayout.LabelField("No network views in scene...");
			GUI.color = Color.white;
			return;
		}
		
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(Screen.width - 25), GUILayout.Height(Mathf.Clamp(Screen.height - 50, 1, Screen.height)));
		for(int i = 0; i < netViewsInScene.Length; i++) {
			netViewsInScene[i] = (Topan.NetworkView)EditorGUILayout.ObjectField("ID #" + netViewsInScene[i].m_viewID, netViewsInScene[i], typeof(Topan.NetworkView), true);
		}
		EditorGUILayout.EndScrollView();
		EditorGUI.indentLevel -= 1;
	}
	
	void Update() {
		Repaint();
	}
}