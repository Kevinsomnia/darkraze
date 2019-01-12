using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ReflectionUpdateGroup))]
public class ReflectionUpdateGroupInspector : Editor
{
    private List<Renderer> rendList = new List<Renderer>();

    public override void OnInspectorGUI()
    {
        ReflectionUpdateGroup rug = (ReflectionUpdateGroup)target;

        base.OnInspectorGUI();
        GUILayout.Space(10f);

        if (GUILayout.Button("AUTO-ASSIGN"))
        {
            rendList = new List<Renderer>();
            RecursiveFindRenderers(rug.transform);
            rug.allRenderers = rendList.ToArray();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(rug);
        }
    }

    private void RecursiveFindRenderers(Transform rootObj)
    {
        foreach (Transform tr in rootObj)
        {
            RecursiveFindRenderers(tr);

            Renderer mr = tr.GetComponent<Renderer>();
            if (mr != null && mr.GetType() == typeof(MeshRenderer) || mr.GetType() == typeof(SkinnedMeshRenderer))
            {
                rendList.Add(mr);
            }
        }
    }
}