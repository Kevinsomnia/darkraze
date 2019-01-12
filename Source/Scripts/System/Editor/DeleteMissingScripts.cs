using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DeleteMissingScripts : EditorWindow
{
    private static int goCount;
    private static int componentCount;
    private static int missingCount;

    private static bool isOpen;
    private static List<GameObject> missingList = new List<GameObject>();
    private static Vector2 scrollPos;

    [MenuItem("Tools/Delete All Missing Components")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<DeleteMissingScripts>();
    }

    [MenuItem("Tools/Clean Leaked Objects")]
    public static void CleanLeaked()
    {
        Debug.Log("Nothing!");
        //		Texture2D[] foundTextures = (Texture2D[])FindObjectsOfType(typeof(Texture2D));
        //		for(int i = 0; i < foundTextures.Length; i++) {
        //			DestroyImmediate(foundTextures[i]);
        //		}
    }

    void OnGUI()
    {
        string[] allStrings = EditorApplication.currentScene.Split(new char[] { '/' }, System.StringSplitOptions.None);
        string sceneName = allStrings[allStrings.Length - 1];
        EditorGUILayout.LabelField("Stats for current scene (" + sceneName.Substring(0, sceneName.Length - 6) + "):", EditorStyles.boldLabel);
        EditorGUI.indentLevel += 1;
        EditorGUILayout.LabelField("GameObject Count: " + goCount.ToString() + " object(s)");
        EditorGUILayout.LabelField("Component Count: [" + missingCount.ToString() + "] missing / [" + componentCount.ToString() + "] scripts");
        EditorGUI.indentLevel -= 1;

        GUILayout.Space(5f);

        if (GUILayout.Button("Find Missing MonoBehaviours in Scene"))
        {
            FindInScene();
        }

        if (Selection.gameObjects.Length > 0)
        {
            if (GUILayout.Button("Find Missing MonoBehaviours in Selection"))
            {
                FindInSelection();
            }
            if (GUILayout.Button("Find Missing MonoBehaviours in Selection (RECURSIVE)"))
            {
                FindInSelectionRecursive();
            }
        }

        GUILayout.Space(10f);
        if (missingList.Count > 0)
        {
            EditorGUI.indentLevel += 1;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int i = 0; i < missingList.Count; i++)
            {
                if (missingList[i] == null)
                {
                    continue;
                }
                EditorGUILayout.ObjectField("Object #" + (i + 1).ToString() + ":", missingList[i], typeof(GameObject), true);
            }
            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel -= 1;
        }
    }

    void Update()
    {
        Repaint();
    }

    private static void FindInScene()
    {
        Object[] allObjects = FindObjectsOfType(typeof(GameObject));

        goCount = 0;
        componentCount = 0;
        missingCount = 0;
        missingList.Clear();
        foreach (Object obj in allObjects)
        {
            goCount++;
            MonoBehaviour[] monos = ((GameObject)obj).GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour m in monos)
            {
                componentCount++;
                if (m == null)
                {
                    missingCount++;
                    missingList.Add((GameObject)obj);
                }
            }
        }
    }

    private static void FindInSelection()
    {
        GameObject[] gos = Selection.gameObjects;

        goCount = 0;
        componentCount = 0;
        missingCount = 0;
        missingList.Clear();
        foreach (GameObject obj in gos)
        {
            goCount++;
            MonoBehaviour[] monos = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour m in monos)
            {
                componentCount++;
                if (m == null)
                {
                    missingCount++;
                    missingList.Add(obj);
                }
            }
        }
    }

    private static void FindInSelectionRecursive()
    {
        GameObject[] gos = Selection.gameObjects;

        goCount = 0;
        componentCount = 0;
        missingCount = 0;
        missingList.Clear();
        foreach (GameObject obj in gos)
        {
            FISRAction(obj);
        }
    }

    private static void FISRAction(GameObject go)
    {
        goCount++;
        MonoBehaviour[] monos = go.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour m in monos)
        {
            componentCount++;
            if (m == null)
            {
                missingCount++;
                missingList.Add(go);
            }
        }

        foreach (Transform t in go.transform)
        {
            FISRAction(t.gameObject);
        }
    }
}