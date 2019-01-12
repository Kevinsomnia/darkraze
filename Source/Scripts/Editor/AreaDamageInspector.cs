using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AreaDamage))]
public class AreaDamageInspector : Editor
{
    public SerializedProperty layersToAffect;

    public static bool isReadyToPaste = false;
    public static LayerMask copyPasteMask = -1;

    private static float rangeValue;

    public void OnEnable()
    {
        layersToAffect = serializedObject.FindProperty("layersToDamage");
    }

    public override void OnInspectorGUI()
    {
        AreaDamage ad = target as AreaDamage;

        ad.lifetime = EditorGUILayout.FloatField("Lifetime:", Mathf.Clamp(ad.lifetime, 0f, 1000f));

        GUILayout.Space(8);

        ad.damageOnce = EditorGUILayout.Toggle("Damage Once:", ad.damageOnce);
        if (!ad.damageOnce)
        {
            EditorGUI.indentLevel += 1;
            GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            ad.damageRate = EditorGUILayout.FloatField("Damage Rate:", Mathf.Clamp(ad.damageRate, 0f, ad.lifetime));
            GUI.color = Color.white;
            EditorGUI.indentLevel -= 1;
        }

        DarkRef.GUISeparator();

        ad.raycastCheck = EditorGUILayout.Toggle("Raycast Check:", ad.raycastCheck);
        if (ad.raycastCheck)
        {
            EditorGUI.indentLevel += 1;
            GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            ad.raycastOffset = EditorGUILayout.Vector3Field("Raycast Offset:", ad.raycastOffset);
            GUI.color = Color.white;
            EditorGUI.indentLevel -= 1;
        }

        DarkRef.GUISeparator();

        ad.isEMP = EditorGUILayout.Toggle("Is EMP:", ad.isEMP);

        GUILayout.Space(5f);

        ad.shakeCamera = EditorGUILayout.Toggle("Shake Camera:", ad.shakeCamera);
        if (ad.shakeCamera)
        {
            EditorGUI.indentLevel += 1;
            GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            ad.shakeRadius = EditorGUILayout.FloatField("Shake Radius:", Mathf.Clamp(ad.shakeRadius, 0f, ad.damageFalloff.keys[ad.damageFalloff.keys.Length - 1].time * 100f));
            ad.shakeLength = EditorGUILayout.FloatField("Shake Duration:", ad.shakeLength);
            ad.shakeSpeed = EditorGUILayout.FloatField("Shake Speed:", ad.shakeSpeed);
            ad.shakeIntensity = EditorGUILayout.FloatField("Shake Intensity:", ad.shakeIntensity);
            GUI.color = Color.white;
            EditorGUI.indentLevel -= 1;
        }

        GUILayout.Space(5f);

        EditorGUIUtility.labelWidth = 165f;
        ad.explosionCameraEffect = EditorGUILayout.Toggle("Explosion Camera Effect:", ad.explosionCameraEffect);
        EditorGUIUtility.labelWidth = 0f;

        DarkRef.GUISeparator();

        EditorGUILayout.PropertyField(layersToAffect, new GUIContent("Layers to Affect:"));

        if (GUILayout.Button("Copy Layer Mask", GUILayout.MaxWidth(250f)))
        {
            isReadyToPaste = true;
            copyPasteMask = ad.layersToDamage;
        }

        if (isReadyToPaste && GUILayout.Button("Paste Layer Mask", GUILayout.MaxWidth(250f)))
        {
            ad.layersToDamage = copyPasteMask;
            serializedObject.Update();
        }

        GUILayout.Space(10f);

        ad.damageFalloff = EditorGUILayout.CurveField("Damage Falloff", ad.damageFalloff);

        for (int i = 0; i < ad.damageFalloff.length; i++)
        {
            Keyframe modKey = ad.damageFalloff.keys[i];
            modKey.time = Mathf.Round(Mathf.Max(0f, ad.damageFalloff.keys[i].time) * 100f) / 100f;
            modKey.value = Mathf.RoundToInt(Mathf.Max(0f, ad.damageFalloff.keys[i].value));
            ad.damageFalloff.MoveKey(i, modKey);
        }

        EditorGUI.indentLevel += 1;
        GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        float maxRange = ad.damageFalloff.keys[ad.damageFalloff.keys.Length - 1].time;
        EditorGUIUtility.labelWidth = 160f;
        rangeValue = EditorGUILayout.FloatField("[Input] Range (0 - " + maxRange.ToString("F1") + "):", Mathf.Clamp(rangeValue, 0, maxRange));
        EditorGUIUtility.labelWidth = 0f;
        EditorGUILayout.LabelField("[Output] Damage: " + ((int)ad.damageFalloff.Evaluate(rangeValue)));
        GUI.color = Color.white;
        EditorGUI.indentLevel -= 1;

        ad.damageForce = EditorGUILayout.FloatField("Damage Force", Mathf.Clamp(ad.damageForce, 0, 1000));
        ad.forceUpwards = EditorGUILayout.FloatField("Upward Force", Mathf.Clamp(ad.forceUpwards, 0, 100));

        if (GUI.changed)
        {
            EditorUtility.SetDirty(ad);
            serializedObject.ApplyModifiedProperties();
        }
    }
}