using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MapsList))]
public class MapsListInspector : Editor
{
    private int removeAt = 0;

    public override void OnInspectorGUI()
    {
        MapsList ml = (MapsList)target;

        base.OnInspectorGUI();

        GUILayout.Space(10f);

        if (ml.maps.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove At", GUILayout.MaxWidth(85f)))
            {
                Map[] newMaps = new Map[ml.maps.Length - 1];
                int placementIndex = 0;
                for (int i = 0; i < ml.maps.Length; i++)
                {
                    if (removeAt == i)
                    {
                        continue;
                    }

                    newMaps[placementIndex] = ml.maps[i];
                    placementIndex++;
                }

                ml.maps = newMaps;
            }

            GUILayout.Space(5f);
            removeAt = EditorGUILayout.IntField("", removeAt, GUILayout.MaxWidth(40f));
            removeAt = Mathf.Clamp(removeAt, 0, ml.maps.Length - 1);

            GUILayout.Space(3f);
            EditorGUILayout.LabelField("[" + ml.maps[removeAt].mapName + "]");

            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(ml);
        }
    }
}