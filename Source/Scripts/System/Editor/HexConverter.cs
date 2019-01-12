using UnityEngine;
using UnityEditor;

public class HexConverter : EditorWindow
{
    private static Color colorToConvert;
    private static Color oldColor;
    private static string outputRgbHex;

    private static string inputHexRgb = "000000";
    private static Color outputHexRgb;

    [MenuItem("Tools/Convert Color")]
    static void Init()
    {
        colorToConvert = new Color(EditorPrefs.GetFloat("ColConvertR", 0.5f), EditorPrefs.GetFloat("ColConvertG", 0.5f), EditorPrefs.GetFloat("ColConvertB", 0.5f), 1f);

        EditorWindow window = (HexConverter)EditorWindow.GetWindow(typeof(HexConverter));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("RGB to Hex:", EditorStyles.boldLabel);
        colorToConvert = EditorGUILayout.ColorField("RGB Color:", colorToConvert);
        outputRgbHex = DarkRef.RGBtoHex(colorToConvert);

        if (colorToConvert != oldColor)
        {
            EditorPrefs.SetFloat("ColConvertR", colorToConvert.r);
            EditorPrefs.SetFloat("ColConvertG", colorToConvert.g);
            EditorPrefs.SetFloat("ColConvertB", colorToConvert.b);
            oldColor = colorToConvert;
        }

        EditorGUILayout.TextField("Hex Code:", outputRgbHex);

        DarkRef.GUISeparator();

        GUILayout.Label("Hex to RGB:", EditorStyles.boldLabel);

        inputHexRgb = inputHexRgb.Substring(0, Mathf.Min(inputHexRgb.Length, 6));
        inputHexRgb = EditorGUILayout.TextField("Hex Code:", inputHexRgb);

        EditorGUILayout.ColorField("RGB Color:", DarkRef.HexToRGB(inputHexRgb));
    }
}