using UnityEngine;
using UnityEditor;
using System.Collections;

public class GrenadeIDController : EditorWindow
{
    private static bool inEditMode = false;
    private static GrenadeList settingsPrefab;

    private Vector2 scrollPos;

    [MenuItem("Tools/Grenade ID Controller", false, 2)]
    static void OpenWindow()
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(GrenadeIDController)) as GrenadeIDController;
        window.Show();
    }

    void OnGUI()
    {
        if (settingsPrefab == null)
        {
            GUI.enabled = false;
        }

        if (GUILayout.Button("Initialize Assignment of Grenade IDs"))
        {
            GrenadeDatabase.Initialize();
        }
        if (GUILayout.Button("Clear Grenade IDs"))
        {
            GrenadeDatabase.customGrenadeList = new GrenadeController[0];
            GrenadeList temp = (GrenadeList)Instantiate(settingsPrefab);
            temp.savedGrenades = new GrenadeController[0];
            GrenadeDatabase.RefreshIDs();
            PrefabUtility.ReplacePrefab(temp.gameObject, settingsPrefab, ReplacePrefabOptions.Default);
            DestroyImmediate(temp.gameObject);
            GrenadeDatabase.Initialize();
        }
        if (GUILayout.Button("Auto-fill Grenade IDs"))
        {
            Object[] resourcesGC = Resources.LoadAll("Explosive Controllers", typeof(GrenadeController));
            GrenadeDatabase.customGrenadeList = new GrenadeController[resourcesGC.Length];

            for (int i = 0; i < resourcesGC.Length; i++)
            {
                GrenadeDatabase.customGrenadeList[i] = (GrenadeController)resourcesGC[i];
            }

            GrenadeList temp = (GrenadeList)Instantiate(settingsPrefab);
            temp.savedGrenades = GrenadeDatabase.customGrenadeList;

            GrenadeDatabase.RefreshIDs();
            PrefabUtility.ReplacePrefab(temp.gameObject, settingsPrefab, ReplacePrefabOptions.Default);
            DestroyImmediate(temp.gameObject);
            WeaponDatabase.Initialize();
        }

        GUI.enabled = true;

        GUILayout.Space(10);

        EditorGUIUtility.labelWidth = 120f;
        GUILayout.Box("Prefab Directory:   MAIN - Blackraze/Resources/Static Prefabs", GUILayout.MaxWidth(500), GUILayout.Height(20));

        if (settingsPrefab)
        {
            EditorGUILayout.ObjectField("Settings Prefab:", settingsPrefab, typeof(GrenadeList), false, GUILayout.MaxWidth(350));
        }
        else
        {
            GrenadeList savedGL = (GrenadeList)Resources.Load("Static Prefabs/Grenade List", typeof(GrenadeList));
            if (savedGL)
            {
                settingsPrefab = savedGL;
                GrenadeDatabase.customGrenadeList = savedGL.savedGrenades;
            }
            else
            {
                GUI.color = new Color(1f, 0.2f, 0f, 1f);
                if (GUILayout.Button("Generate Prefab", GUILayout.MaxWidth(120)))
                {
                    if (settingsPrefab == null)
                    {
                        GameObject go = new GameObject("Grenade List");
                        go.AddComponent<GrenadeList>();

                        settingsPrefab = PrefabUtility.CreatePrefab("Assets/MAIN - Blackraze/Resources/Static Prefabs/Grenade List.prefab", go, ReplacePrefabOptions.ConnectToPrefab).GetComponent<GrenadeList>();
                        DestroyImmediate(go);
                    }
                }
                GUI.color = Color.white;
            }
        }

        EditorGUIUtility.LookLikeControls();

        if (settingsPrefab == null)
        {
            GUI.color = Color.gray;
            GUI.enabled = false;
        }
        else if (!settingsPrefab.GetComponent<GrenadeList>())
        {
            GUI.color = Color.gray;
            GUI.enabled = false;
        }

        DarkRef.GUISeparator(10f);

        if (GUILayout.Button((inEditMode) ? "DONE" : "EDIT", GUILayout.MaxWidth(80)))
        {
            inEditMode = !inEditMode;

            if (!inEditMode)
            {
                GrenadeDatabase.RefreshIDs();
            }
        }

        EditorGUILayout.LabelField("Grenade ID List", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUI.indentLevel += 1;
        if (inEditMode)
        {
            int length = GrenadeDatabase.customGrenadeList.Length;
            GrenadeController[] tempStorage = GrenadeDatabase.customGrenadeList;
            EditorGUIUtility.labelWidth = 90f;
            length = EditorGUILayout.IntField("Length:", Mathf.Clamp(length, 0, 100), GUILayout.MaxWidth(150));
            EditorGUIUtility.LookLikeControls();
            if (length != GrenadeDatabase.customGrenadeList.Length)
            {
                GrenadeDatabase.customGrenadeList = new GrenadeController[length];
                for (int i = 0; i < tempStorage.Length; i++)
                {
                    if (i < GrenadeDatabase.customGrenadeList.Length)
                    {
                        GrenadeDatabase.customGrenadeList[i] = tempStorage[i];
                    }
                }
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(Screen.width - 10), GUILayout.Height(Mathf.Clamp(Screen.height - 210, 1, Screen.height)));
            EditorGUI.indentLevel += 1;
            for (int i = 0; i < length; i++)
            {
                EditorGUIUtility.labelWidth = 90f;
                GrenadeDatabase.customGrenadeList[i] = (GrenadeController)EditorGUILayout.ObjectField("Element #" + i.ToString(), GrenadeDatabase.customGrenadeList[i], typeof(GrenadeController), false, GUILayout.MaxWidth(330));
                EditorGUIUtility.LookLikeControls();
            }
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndScrollView();

            if (GrenadeDatabase.customGrenadeList != settingsPrefab.savedGrenades)
            {
                settingsPrefab.savedGrenades = new GrenadeController[length];
                for (int i = 0; i < GrenadeDatabase.customGrenadeList.Length; i++)
                {
                    if (i < settingsPrefab.savedGrenades.Length)
                    {
                        settingsPrefab.savedGrenades[i] = GrenadeDatabase.customGrenadeList[i];
                    }
                }
            }
        }
        else
        {
            if (GrenadeDatabase.publicGrenadeControllers.Length <= 0)
            {
                EditorGUILayout.LabelField("[Press the 'EDIT' button above to get started]");
            }
            else
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(Screen.width - 10), GUILayout.Height(Mathf.Clamp(Screen.height - 210, 1, Screen.height)));
                for (int i = 0; i < GrenadeDatabase.publicGrenadeControllers.Length; i++)
                {
                    GrenadeController gc = GrenadeDatabase.publicGrenadeControllers[i];

                    if (gc != null)
                    {
                        if (gc.grenadeID <= -1)
                        {
                            EditorGUILayout.LabelField(i + " - (UNASSIGNED)");
                        }
                        else
                        {
                            EditorGUILayout.LabelField(i + " - " + gc.name);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(i + " - (NULL)");
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel -= 1;
    }
}