using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ToggleAllChildren : EditorWindow
{
    public static int toggleCount = 0;

    [MenuItem("Tools/Toggle Children Helper")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<ToggleAllChildren>();
    }

    void OnGUI()
    {
        if (Selection.gameObjects.Length > 0)
        {
            bool isEnable = !Selection.gameObjects[0].activeSelf;
            string displayString = "Toggle ALL Children in Selection" + (" (" + ((isEnable) ? "CURRENTLY DISABLED" : "CURRENTLY ENABLED") + ")");

            GUI.backgroundColor = new Color(1f, 0.8f, 0.8f);
            if (GUILayout.Button(displayString, GUILayout.MaxWidth(400f), GUILayout.MinHeight(30f)))
            {
                ToggleInSelection(isEnable);
            }
        }
    }

    void Update()
    {
        Repaint();
    }

    private static void ToggleInSelection(bool t)
    {
        GameObject[] gos = Selection.gameObjects;
        toggleCount = 0;

        try
        {
            foreach (GameObject obj in gos)
            {
                ToggleAction(obj, t);
            }

            Debug.Log("Successfully toggled " + toggleCount.ToString() + " game objects to: " + t.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogError("Something went wrong when toggling children!  ||  " + e.Message);
        }
    }

    private static void ToggleAction(GameObject go, bool toggle)
    {
        go.SetActive(toggle);
        toggleCount++;

        foreach (Transform t in go.transform)
        {
            ToggleAction(t.gameObject, toggle);
        }
    }
}