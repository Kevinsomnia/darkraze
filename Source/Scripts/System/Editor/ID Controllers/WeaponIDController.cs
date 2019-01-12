using UnityEngine;
using UnityEditor;
using System.Collections;

public class WeaponIDController : EditorWindow
{
    private static bool inEditMode = false;
    private static WeaponList settingsPrefab;
    private static int displayLength;

    private Vector2 scrollPos;

    [MenuItem("Tools/Weapon ID Controller", false, 2)]
    static void OpenWindow()
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(WeaponIDController)) as WeaponIDController;
        window.title = "Weapon IDs";
        window.Show();
    }

    void OnGUI()
    {
        if (settingsPrefab == null)
        {
            GUI.enabled = false;
        }

        if (GUILayout.Button("Initialize Assignment of Weapon IDs"))
        {
            WeaponDatabase.Initialize();
        }
        if (GUILayout.Button("Clear Weapon IDs"))
        {
            WeaponDatabase.customWeaponList = new GunController[0];
            WeaponList temp = (WeaponList)Instantiate(settingsPrefab);
            temp.savedWeapons = new GunController[0];
            WeaponDatabase.RefreshIDs();
            PrefabUtility.ReplacePrefab(temp.gameObject, settingsPrefab, ReplacePrefabOptions.Default);
            DestroyImmediate(temp.gameObject);
            WeaponDatabase.Initialize();
        }
        if (GUILayout.Button("Auto-fill Weapon IDs"))
        {
            Object[] resourcesGC = Resources.LoadAll("Weapons", typeof(GunController));
            WeaponDatabase.customWeaponList = new GunController[resourcesGC.Length];

            WeaponList temp = (WeaponList)Instantiate(settingsPrefab);
            temp.savedWeapons = new GunController[resourcesGC.Length];

            for (int i = 0; i < resourcesGC.Length; i++)
            {
                temp.savedWeapons[i] = (GunController)resourcesGC[i];
            }

            WeaponDatabase.RefreshIDs();
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
            EditorGUILayout.ObjectField("Settings Prefab:", settingsPrefab, typeof(WeaponList), false, GUILayout.MaxWidth(350));
        }
        else
        {
            WeaponList savedWL = (WeaponList)Resources.Load("Static Prefabs/Weapon List", typeof(WeaponList));
            if (savedWL)
            {
                settingsPrefab = savedWL;
                WeaponDatabase.customWeaponList = savedWL.savedWeapons;
            }
            else
            {
                GUI.color = new Color(1f, 0.2f, 0f, 1f);
                if (GUILayout.Button("Generate Prefab", GUILayout.MaxWidth(120)))
                {
                    if (settingsPrefab == null)
                    {
                        GameObject go = new GameObject("Weapon List");
                        go.AddComponent<WeaponList>();

                        settingsPrefab = PrefabUtility.CreatePrefab("Assets/MAIN - Blackraze/Resources/Static Prefabs/Weapon List.prefab", go, ReplacePrefabOptions.ConnectToPrefab).GetComponent<WeaponList>();
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
        else if (!settingsPrefab.GetComponent<WeaponList>())
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
                WeaponDatabase.RefreshIDs();
                WeaponDatabase.Initialize();

                if (settingsPrefab != null)
                {
                    WeaponList temp = (WeaponList)Instantiate(settingsPrefab);
                    PrefabUtility.ReplacePrefab(temp.gameObject, settingsPrefab, ReplacePrefabOptions.Default);
                    DestroyImmediate(temp.gameObject);
                }
            }
            else
            {
                displayLength = WeaponDatabase.customWeaponList.Length;
            }
        }

        EditorGUILayout.LabelField("Weapon ID List", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUI.indentLevel += 1;
        if (inEditMode)
        {
            GunController[] tempStorage = WeaponDatabase.customWeaponList;
            EditorGUIUtility.labelWidth = 90f;
            displayLength = EditorGUILayout.IntField("Length:", Mathf.Clamp(displayLength, 0, 100), GUILayout.MaxWidth(150));
            EditorGUIUtility.LookLikeControls();
            if (displayLength != WeaponDatabase.customWeaponList.Length && (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                WeaponDatabase.customWeaponList = new GunController[displayLength];
                for (int i = 0; i < tempStorage.Length; i++)
                {
                    if (i < WeaponDatabase.customWeaponList.Length)
                    {
                        WeaponDatabase.customWeaponList[i] = tempStorage[i];
                    }
                }
                Event.current.Use();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(Screen.width - 10), GUILayout.Height(Mathf.Clamp(Screen.height - 210, 1, Screen.height)));
            EditorGUI.indentLevel += 1;
            for (int i = 0; i < WeaponDatabase.customWeaponList.Length; i++)
            {
                EditorGUIUtility.labelWidth = 90f;
                WeaponDatabase.customWeaponList[i] = (GunController)EditorGUILayout.ObjectField("Element #" + i.ToString(), WeaponDatabase.customWeaponList[i], typeof(GunController), false, GUILayout.MaxWidth(330));
                EditorGUIUtility.LookLikeControls();
            }
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndScrollView();

            if (WeaponDatabase.customWeaponList != settingsPrefab.savedWeapons || GUI.changed)
            {
                settingsPrefab.savedWeapons = WeaponDatabase.customWeaponList;
            }
        }
        else
        {
            if (WeaponDatabase.publicGunControllers.Length <= 0)
            {
                EditorGUILayout.LabelField("[Press the 'EDIT' button above to get started]");
            }
            else
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(Screen.width - 10), GUILayout.Height(Mathf.Clamp(Screen.height - 210, 1, Screen.height)));
                for (int i = 0; i < WeaponDatabase.publicGunControllers.Length; i++)
                {
                    GunController gc = WeaponDatabase.publicGunControllers[i];

                    if (gc != null)
                    {
                        if (gc.weaponID <= -1)
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

        EditorGUI.indentLevel -= 1;

        EditorGUILayout.EndHorizontal();
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}