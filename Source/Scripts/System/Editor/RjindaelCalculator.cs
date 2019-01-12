using UnityEngine;
using UnityEditor;
using System.Collections;

public class RjindaelCalculator : EditorWindow
{
    private static string encryptInput;
    private static string decryptInput;
    private static int encryptIteration;
    private static int decryptIteration;

    [MenuItem("Tools/Rjindael Calculator")]
    public static void OpenWindow()
    {
        encryptInput = EditorPrefs.GetString("EncInput");
        decryptInput = EditorPrefs.GetString("DecInput");
        encryptIteration = EditorPrefs.GetInt("EncIter");
        decryptIteration = EditorPrefs.GetInt("DecIter");

        EditorWindow window = (EditorWindow)EditorWindow.GetWindow<RjindaelCalculator>();
        window.title = "Rjindael Calculator";
        window.Show();
    }

    private void OnGUI()
    {
        encryptInput = EditorGUILayout.TextField("Encrypt: ", encryptInput);
        encryptIteration = EditorGUILayout.IntSlider("Encrypt Iterations:", encryptIteration, 1, 5);
        if (encryptInput != "")
        {
            EditorGUILayout.TextField("  RESULT:", DarkRef.EncryptString(encryptInput, encryptIteration));
        }

        GUILayout.Space(10f);

        decryptInput = EditorGUILayout.TextField("Decrypt: ", decryptInput);
        decryptIteration = EditorGUILayout.IntSlider("Decrypt Iterations:", decryptIteration, 1, 5);
        if (decryptInput != "")
        {
            EditorGUILayout.TextField("  RESULT:", DarkRef.DecryptString(decryptInput, decryptIteration));
        }

        if (GUI.changed)
        {
            EditorPrefs.SetString("EncInput", encryptInput);
            EditorPrefs.SetString("DecInput", decryptInput);
            EditorPrefs.SetInt("EncIter", encryptIteration);
            EditorPrefs.SetInt("DecIter", decryptIteration);
        }
    }
}