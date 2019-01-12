using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GrenadeController))]
public class GrenadeControllerInspector : Editor
{

    public override void OnInspectorGUI()
    {
        GrenadeController gc = target as GrenadeController;

        GUI.color = new Color(1f, 0.8f, 0.6f, 1f);
        gc.grenadeName = EditorGUILayout.TextField("Grenade Name:", gc.grenadeName);
        GUI.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        EditorGUIUtility.labelWidth = 140f;
        gc.grenadeIcon = (Texture2D)EditorGUILayout.ObjectField("  Icon Texture:", gc.grenadeIcon, typeof(Texture2D));
        EditorGUIUtility.LookLikeControls();
        GUI.color = Color.white;

        GUILayout.Space(10);

        gc.isDetonatable = EditorGUILayout.Toggle(" Is Detonatable:", gc.isDetonatable);

        if (gc.isDetonatable)
        {
            gc.baseDelay = EditorGUILayout.FloatField(" Initial Delay:", Mathf.Clamp(gc.baseDelay, 0f, 5f));
            gc.detonationDelay = EditorGUILayout.FloatField(" Detonation Interval:", Mathf.Clamp(gc.detonationDelay, 0f, 5f));
        }

        GUILayout.Space(10);

        gc.throwPos = (Transform)EditorGUILayout.ObjectField(" Throw Position:", gc.throwPos, typeof(Transform), true);
        gc.grenadePrefab = (Rigidbody)EditorGUILayout.ObjectField((gc.isDetonatable) ? " Explosive Prefab:" : " Grenade Prefab:", gc.grenadePrefab, typeof(Rigidbody), true);
        gc.displayMesh = (MeshRenderer)EditorGUILayout.ObjectField(" Display Mesh:", gc.displayMesh, typeof(MeshRenderer), true);

        GUILayout.Space(10);

        EditorGUIUtility.labelWidth = 150f;
        gc.throwThreshold = EditorGUILayout.FloatField(" Throw Threshold:", Mathf.Clamp(gc.throwThreshold, 0f, 5f));
        gc.cookingThreshold = EditorGUILayout.FloatField(" Cooking Threshold:", Mathf.Clamp(gc.cookingThreshold, 0f, 10f));
        GUILayout.Space(5);
        gc.throwStrength = EditorGUILayout.FloatField(" Throw Strength:", Mathf.Clamp(gc.throwStrength, 0f, 1000f));

        if (!gc.isDetonatable)
        {
            gc.tossStrength = EditorGUILayout.FloatField(" Toss Strength:", Mathf.Clamp(gc.tossStrength, 0f, 1000f));
        }

        EditorGUIUtility.LookLikeControls();

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Sounds", EditorStyles.boldLabel);
        EditorGUI.indentLevel += 1;

        gc.pullPinSound = (AudioClip)EditorGUILayout.ObjectField((gc.isDetonatable) ? "Detonation Sound:" : "Pull Pin Sound:", gc.pullPinSound, typeof(AudioClip), true);

        gc.throwSound = (AudioClip)EditorGUILayout.ObjectField("Throw Sound:", gc.throwSound, typeof(AudioClip), true);
        EditorGUI.indentLevel -= 1;

        GUILayout.Space(6);

        EditorGUILayout.LabelField("Third Person Variables (MP)", EditorStyles.boldLabel);
        EditorGUI.indentLevel += 1;
        EditorGUILayout.LabelField("Local Position: " + DarkRef.PreciseStringVector3(gc.thirdPersonPosition));
        EditorGUILayout.LabelField("Local Rotation: " + DarkRef.PreciseStringVector3(gc.thirdPersonRotation.eulerAngles));

        GUILayout.Space(8f);

        if (gc.transform.parent != null && gc.transform.parent.name == "WeaponsParent" && GUILayout.Button("Preview Transform Info"))
        {
            gc.transform.localPosition = gc.thirdPersonPosition;
            gc.transform.localRotation = gc.thirdPersonRotation;
        }

        if (GUILayout.Button("Set Transform Info"))
        {
            GrenadeController prefab = GrenadeDatabase.GetGrenadeByID(gc.grenadeID);
            prefab.thirdPersonPosition = gc.transform.localPosition;
            prefab.thirdPersonRotation = gc.transform.localRotation;

            gc.thirdPersonPosition = gc.transform.localPosition;
            gc.thirdPersonRotation = gc.transform.localRotation;
        }

        EditorGUI.indentLevel -= 1;

        DarkRef.GUISeparator(8f);

        GUI.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        EditorGUIUtility.labelWidth = 210f;
        EditorGUILayout.IntField("Grenade ID:", gc.grenadeID);
        EditorGUIUtility.LookLikeControls();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(gc);
        }
    }
}