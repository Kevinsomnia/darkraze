using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GrenadeManager))]
public class GrenadeManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        GrenadeManager gm = target as GrenadeManager;
        GrenadeAmmoManager gam = gm.transform.parent.parent.GetComponent<GrenadeAmmoManager>();

        if (gm.nadeList == null)
        {
            gm.nadeList = (GrenadeList)EditorGUILayout.ObjectField("Grenade List Prefab:", gm.nadeList, typeof(GrenadeList), true);
            EditorGUILayout.HelpBox("Not assigning this variable will have a heavy performance cost!", MessageType.Warning);
        }

        GUILayout.Space(6);
        EditorGUILayout.LabelField(" To Be Instantiated:", EditorStyles.boldLabel);

        EditorGUI.indentLevel += 1;
        EditorGUILayout.ObjectField(" Grenade Slot #1:", GrenadeDatabase.GetGrenadeByID(gam.grenadeTypeOne), typeof(GrenadeController), false);
        EditorGUILayout.ObjectField(" Grenade Slot #2:", GrenadeDatabase.GetGrenadeByID(gam.grenadeTypeTwo), typeof(GrenadeController), false);
        EditorGUI.indentLevel -= 1;
    }
}