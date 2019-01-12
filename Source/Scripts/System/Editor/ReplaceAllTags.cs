using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ReplaceAllTags : EditorWindow
{
    private static string tagToFind = "Player";
    private static string tagToReplace = "Untagged";

    private static List<GameObject> curList = new List<GameObject>();
    private static string searchedTag;

    [MenuItem("Tools/Replace All Tags")]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow<ReplaceAllTags>();
    }

    void OnGUI()
    {
        tagToFind = EditorGUILayout.TextField("Tags to Find:", tagToFind);

        EditorGUI.indentLevel += 1;
        tagToReplace = EditorGUILayout.TextField("Replace tags with:", tagToReplace);
        EditorGUI.indentLevel -= 1;

        if (GUILayout.Button("Find Objects With Tag (IN SCENE)"))
        {
            curList.CopyTo(GameObject.FindGameObjectsWithTag(tagToFind));
            searchedTag = tagToFind;
        }

        if (Selection.gameObjects.Length == 1)
        {
            if (GUILayout.Button("Find Objects With Tag (RECURSIVELY)"))
            {
                FindInSelectionRecursive(Selection.activeGameObject);
            }
        }

        if (curList.Count <= 0)
        {
            return;
        }

        GUILayout.Space(10f);

        if (searchedTag != "")
        {
            EditorGUILayout.LabelField("Found " + curList.Count.ToString() + " objects with the tag of: " + searchedTag, EditorStyles.boldLabel);
        }
        GUI.color = new Color(1f, 0.8f, 0.6f);
        if (GUILayout.Button("Replace with Tag"))
        {
            foreach (GameObject go in curList)
            {
                go.transform.tag = tagToReplace;
            }
        }
        GUI.color = Color.white;
    }

    void Update()
    {
        Repaint();
    }

    private static void FindInSelectionRecursive(GameObject go)
    {
        if (go.transform.tag == tagToFind)
        {
            curList.Add(go);
        }

        foreach (Transform t in go.transform)
        {
            FindInSelectionRecursive(t.gameObject);
        }
    }
}