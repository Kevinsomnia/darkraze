using UnityEngine;
using UnityEditor;

public class CopyMonoBehaviour : EditorWindow
{
    private static MonoBehaviour clipboard;

    [MenuItem("Tools/Copy MonoBehaviour to Clipboard", false, 2000)]
    static void OpenWindow()
    {
        EditorWindow.GetWindow<CopyMonoBehaviour>(true);
    }

    void OnGUI()
    {
        clipboard = (MonoBehaviour)EditorGUILayout.ObjectField("Clipboard:", clipboard, typeof(MonoBehaviour), true);
        if (GUI.Button(new Rect(position.width - 90, 30, 75, 15), "Clear"))
        {
            clipboard = null;
            return;
        }

        GUILayout.Space(25);

        if (Selection.activeGameObject)
        {
            if (clipboard != null)
            {
                if (Selection.activeGameObject.GetComponent<ObjectHealth>())
                {
                    GUILayout.Label("Selection has component");
                }
                else
                {
                    GUILayout.Button("Selected object requires a valid script to paste to", GUILayout.ExpandWidth(false));
                    return;
                }

                if (GUILayout.Button("Paste MonoBehaviour to selected object", GUILayout.ExpandWidth(false)))
                {
                    GameObject shit = Selection.activeGameObject;
                    if (shit.GetComponent<ObjectHealth>())
                    {
                        ObjectHealth oh = shit.GetComponent<ObjectHealth>();
                        MonoBehaviour[] ohList = oh.disableScripts;
                        oh.disableScripts = new MonoBehaviour[oh.disableScripts.Length + 1];
                        for (int i = 0; i < ohList.Length; i++)
                        {
                            oh.disableScripts[i] = ohList[i];
                        }
                        oh.disableScripts[oh.disableScripts.Length - 1] = clipboard;
                    }
                }
            }
            else
            {
                GUILayout.Button("Please assign a clipboard monobehaviour!", GUILayout.ExpandWidth(false));
            }
        }
    }

    void Update()
    {
        Repaint();
    }
}