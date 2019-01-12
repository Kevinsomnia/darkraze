using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(DestroyAfterTime))]
public class DestroyAfterTimeInspector : Editor {
	private static bool isOpen = false;
	private static bool isOpen2 = false;
	
	public override void OnInspectorGUI() {
		DestroyAfterTime dat = target as DestroyAfterTime;
				
		EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
		EditorGUI.indentLevel += 1;
		dat.destroyTime = EditorGUILayout.FloatField("Destroy Time:", Mathf.Clamp(dat.destroyTime, 0f, 1000f));
		dat.randomness = EditorGUILayout.FloatField("Randomness:", dat.randomness);
		
		if(dat.randomness > 0f) {
			EditorGUILayout.LabelField("    Final Value:   [" + dat.destroyTime + " - " + (dat.destroyTime + dat.randomness) + "]");
		}
		else {
			EditorGUILayout.LabelField("    Final Value:   [" + dat.destroyTime + "]");
		}
		
		EditorGUI.indentLevel -= 1;

        GUILayout.Space(5f);
        dat.poolObject = EditorGUILayout.Toggle("Pool Object:", dat.poolObject);
		
		DarkRef.GUISeparator(5);
		
		dat.fadeOut = EditorGUILayout.Toggle(" Fade Out:", dat.fadeOut);
		if(dat.fadeOut) {
			EditorGUI.indentLevel += 1;
			dat.colorName = EditorGUILayout.TextField("Color Name (Shader)", dat.colorName);
			dat.fadeSpeed = EditorGUILayout.FloatField("Fade Speed", Mathf.Clamp(dat.fadeSpeed, 0.01f, 15f));
			
			isOpen2 = EditorGUILayout.Foldout(isOpen2, " Renderers");
			if(isOpen2) {
				GUI.color = new Color(1f, 0.5f, 0.2f, 1f);
				EditorGUI.indentLevel += 1;
				if(GUILayout.Button("Auto-fill", GUILayout.MaxWidth(80))) {
					//Could have just used GetComponentInChildren, but it wouldn't work on prefabs.
					List<Renderer> r = new List<Renderer>();
					if(dat.GetComponent<Renderer>()) {
						r.Add(dat.GetComponent<Renderer>());
					}
					foreach(Transform t in dat.transform) {
						if(t.GetComponent<Renderer>()) {
							r.Add(t.GetComponent<Renderer>());
						}
					}
					
					dat.renderers = new Renderer[r.Count];
					for(int i = 0; i < r.Count; i++) {
						dat.renderers[i] = r[i];
					}
				}
				GUI.color = Color.white;
				
				int length = dat.renderers.Length;
				Renderer[] tempStorage = dat.renderers;
				length = EditorGUILayout.IntField("Length:", length);
				if(length != dat.renderers.Length) {
					dat.renderers = new Renderer[length];
					for(int i = 0; i < tempStorage.Length; i++) {
						if(i < dat.renderers.Length) {
							dat.renderers[i] = tempStorage[i];
						}
					}
				}
				EditorGUI.indentLevel += 1;
                EditorGUIUtility.labelWidth = 110f;
				for(int i = 0; i < length; i++) {
					dat.renderers[i] = (Renderer)EditorGUILayout.ObjectField(" Element " + i.ToString(), dat.renderers[i], typeof(Renderer), true);
				}
				EditorGUIUtility.LookLikeControls();
				EditorGUI.indentLevel -= 1;
				EditorGUI.indentLevel -= 1;
			}
			
			EditorGUI.indentLevel -= 1;
		}
		
		DarkRef.GUISeparator();
		
		dat.isParticle = EditorGUILayout.Toggle(" Is Particle:", dat.isParticle);
		
		if(dat.isParticle) {
			EditorGUI.indentLevel += 1;
			
			isOpen = EditorGUILayout.Foldout(isOpen, " Particle Systems");
			if(isOpen) {
				GUI.color = new Color(1f, 0.5f, 0.2f, 1f);
				EditorGUI.indentLevel += 1;
				if(GUILayout.Button("Auto-fill", GUILayout.MaxWidth(80))) {
					//Could have just used GetComponentInChildren, but it wouldn't work on prefabs.
					List<ParticleEmitter> pe = new List<ParticleEmitter>();
					if(dat.GetComponent<ParticleEmitter>()) {
						pe.Add(dat.GetComponent<ParticleEmitter>());
					}
					foreach(Transform t in dat.transform) {
						if(t.GetComponent<ParticleEmitter>()) {
							pe.Add(t.GetComponent<ParticleEmitter>());
						}
					}
					
					dat.emitters = new ParticleEmitter[pe.Count];
					for(int i = 0; i < pe.Count; i++) {
						dat.emitters[i] = pe[i];
					}
				}
				GUI.color = Color.white;
				
				int length = dat.emitters.Length;
				ParticleEmitter[] tempStorage = dat.emitters;
				length = EditorGUILayout.IntField("Length:", length);
				if(length != dat.emitters.Length) {
					dat.emitters = new ParticleEmitter[length];
					for(int i = 0; i < tempStorage.Length; i++) {
						if(i < dat.emitters.Length) {
							dat.emitters[i] = tempStorage[i];
						}
					}
				}
				EditorGUI.indentLevel += 1;
                EditorGUIUtility.labelWidth = 110f;
				for(int i = 0; i < length; i++) {
					dat.emitters[i] = (ParticleEmitter)EditorGUILayout.ObjectField(" Element " + i.ToString(), dat.emitters[i], typeof(ParticleEmitter), true);
				}
				EditorGUIUtility.LookLikeControls();
				EditorGUI.indentLevel -= 1;
				EditorGUI.indentLevel -= 1;
			}
			
			EditorGUI.indentLevel -= 1;
		}
				
		if(GUI.changed) {
			EditorUtility.SetDirty(dat);
		}
	}
}