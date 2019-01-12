using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ParticleManager))]
public class ParticleManagerInspector : Editor {

    public override void OnInspectorGUI() {
        ParticleManager pm = (ParticleManager)target;

        DrawDefaultInspector();

        if(GUILayout.Button("Assign Particle Systems (" + ((pm.emitters != null) ? pm.emitters.Length.ToString() : "0") + ")")) {
            List<ParticleEmitter> temp = new List<ParticleEmitter>();

            foreach(Transform t in pm.transform) {
                ParticleEmitter pe = t.GetComponent<ParticleEmitter>();
                if(pe != null) {
                    temp.Add(pe);
                }
            }

            ParticleEmitter[] setPE = temp.ToArray();
            pm.emitters = setPE;
        }
    }
}