//ObjectiveMarkerInspector.cs created by DaBossTMR for Darkraze FPS Project (Unity3D)

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ObjectiveMarker))]
public class ObjectiveMarkerInspector : Editor
{

    public override void OnInspectorGUI()
    {
        ObjectiveMarker marker = target as ObjectiveMarker;

        marker.enabled = EditorGUILayout.Toggle("GUI Enabled:", marker.enabled);

        if (!marker.enabled) { return; }

        GUILayout.Label("General Settings", EditorStyles.boldLabel);
        marker.target = (Transform)EditorGUILayout.ObjectField("    Target:", marker.target, typeof(Transform), true);

        EditorGUI.indentLevel += 1;
        marker.baseScale = EditorGUILayout.Vector2Field("Base Scale:", marker.baseScale);
        EditorGUI.indentLevel -= 1;

        marker.markerTexture = (UITexture)EditorGUILayout.ObjectField("    Marker Texture", marker.markerTexture, typeof(UITexture), true);
        marker.distanceLabel = (UILabel)EditorGUILayout.ObjectField("    Distance Label", marker.distanceLabel, typeof(UILabel), true);
        marker.descriptionLabel = (UILabel)EditorGUILayout.ObjectField("    Description Label", marker.descriptionLabel, typeof(UILabel), true);
        marker.distanceRefreshRate = EditorGUILayout.FloatField("    Distance Refresh Rate:", marker.distanceRefreshRate);

        EditorGUI.indentLevel += 1;
        marker.edgeOffset = EditorGUILayout.Vector2Field("Edge Offset:", marker.edgeOffset);
        EditorGUI.indentLevel -= 1;

        DarkRef.GUISeparator();

        GUILayout.Label("Effect Settings", EditorStyles.boldLabel);
        marker.scalingEnabled = EditorGUILayout.Toggle("    Scaling Enabled:", marker.scalingEnabled);

        if (marker.scalingEnabled)
        {
            marker.nearDistance = EditorGUILayout.FloatField("        Near Distance:", marker.nearDistance);
            marker.nearScale = EditorGUILayout.FloatField("        Near Scale:", marker.nearScale);

            EditorGUI.indentLevel += 2;
            DarkRef.GUISeparator();
            EditorGUI.indentLevel -= 2;

            marker.farDistance = EditorGUILayout.FloatField("        Far Distance:", marker.farDistance);
            marker.farScale = EditorGUILayout.FloatField("        Far Scale:", marker.farScale);

            GUILayout.Space(10f);

            EditorGUI.indentLevel += 1;
            marker.textBorder = EditorGUILayout.Vector3Field("Text Border:", marker.textBorder);
            EditorGUI.indentLevel -= 1;
        }

        DarkRef.GUISeparator();

        GUILayout.Label("GUI Settings", EditorStyles.boldLabel);

        if (marker.GUITextures.Length < 5)
        {
            marker.GUITextures = new Texture2D[5];
        }

        EditorGUI.indentLevel += 1;
        marker.GUITextures[0] = (Texture2D)EditorGUILayout.ObjectField("On-Screen Indicator: ", marker.GUITextures[0], typeof(Texture2D), false);
        marker.GUITextures[1] = (Texture2D)EditorGUILayout.ObjectField("Left Indicator: ", marker.GUITextures[1], typeof(Texture2D), false);
        marker.GUITextures[2] = (Texture2D)EditorGUILayout.ObjectField("Right Indicator: ", marker.GUITextures[2], typeof(Texture2D), false);
        marker.GUITextures[3] = (Texture2D)EditorGUILayout.ObjectField("Up Indicator: ", marker.GUITextures[3], typeof(Texture2D), false);
        marker.GUITextures[4] = (Texture2D)EditorGUILayout.ObjectField("Down Indicator: ", marker.GUITextures[4], typeof(Texture2D), false);
        EditorGUI.indentLevel -= 1;

        EditorUtility.SetDirty(marker);
    }
}