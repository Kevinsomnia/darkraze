using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(WeaponDepthOfField))]
public class WeaponDoFInspector : Editor
{
    public SerializedObject serObj;

    public SerializedProperty visualizeFocus;
    public SerializedProperty focalLength;
    public SerializedProperty focalSize;
    public SerializedProperty aperture;
    public SerializedProperty maxBlurSize;

    public SerializedProperty blurSampleCount;

    public SerializedProperty nearBlur;
    public SerializedProperty foregroundOverlap;

    void OnEnable()
    {
        serObj = new SerializedObject(target);

        visualizeFocus = serObj.FindProperty("visualizeFocus");

        focalLength = serObj.FindProperty("focalLength");
        focalSize = serObj.FindProperty("focalSize");
        aperture = serObj.FindProperty("aperture");
        maxBlurSize = serObj.FindProperty("maxBlurSize");

        blurSampleCount = serObj.FindProperty("blurSampleCount");

        nearBlur = serObj.FindProperty("nearBlur");
        foregroundOverlap = serObj.FindProperty("foregroundOverlap");
    }

    public override void OnInspectorGUI()
    {
        serObj.Update();

        GUILayout.Space(5f);

        GUILayout.Label("Focal Settings");
        EditorGUILayout.PropertyField(visualizeFocus, new GUIContent(" Visualize"));
        EditorGUILayout.PropertyField(focalLength, new GUIContent(" Focal Distance"));
        EditorGUILayout.PropertyField(focalSize, new GUIContent(" Focal Size"));
        EditorGUILayout.PropertyField(aperture, new GUIContent(" Aperture"));

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(blurSampleCount, new GUIContent(" Sample Count"));

        EditorGUILayout.PropertyField(maxBlurSize, new GUIContent(" Max Blur Distance"));

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(nearBlur, new GUIContent("Near Blur"));
        EditorGUILayout.PropertyField(foregroundOverlap, new GUIContent("  Overlap Size"));

        serObj.ApplyModifiedProperties();
    }
}
