using UnityEngine;
using System.Collections;
using UnityEditor;

public class BoneAnimationHelper : ScriptableObject {
	private static Vector3 startRotation;
	private static Vector3 endRotation;
	private static string objName;
	private static int instanceID;
	
	[MenuItem("Tools/Bone Animation Helper/Record Start-rotation %J")]
	private static void RecordStart() {
		if(Selection.activeTransform == null) return;
		
		objName = Selection.activeTransform.name;
		Debug.Log("Recording starting rotation for: " + objName);
		startRotation = Selection.activeTransform.localEulerAngles;
		endRotation = Vector3.zero;
		instanceID = Selection.activeTransform.GetInstanceID();
	}
	
	[MenuItem("Tools/Bone Animation Helper/Record End-rotation %K")]
	private static void RecordEnd() {
		if(Selection.activeTransform.GetInstanceID() != instanceID) {
			Debug.Log("Calculation failed! You are not comparing the same object!");
			return;
		}
		if(Selection.activeTransform == null) return;
		
		endRotation = Selection.activeTransform.localEulerAngles;
		Vector3 difference = endRotation - startRotation;
		
		float maxValue = (Mathf.Max(Mathf.Abs(difference.x), Mathf.Abs(difference.y), Mathf.Abs(difference.z)));
		
		float x = difference.x / maxValue;
		float y = difference.y / maxValue;
		float z = difference.z / maxValue;
		
		Debug.Log("Results for " + objName + ": " + (x * 100) + "%, " + (y * 100) + "%, " + (z * 100) + "%");
		startRotation = Vector3.zero;
		startRotation = Vector3.zero;
		objName = "";
		instanceID = 0;
	}
}