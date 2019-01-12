using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class CubemapToPNG : ScriptableWizard
{
    public Cubemap cubemap;

    [MenuItem("Tools/Save Cubemap to PNG")]
    private static void OpenWindow()
    {
        ScriptableWizard.DisplayWizard("Cubemap to PNG", typeof(CubemapToPNG), "Save");
    }

    private void OnWizardUpdate()
    {
        helpString = "Select a cubemap to continue.";
    }

    private void OnWizardCreate()
    {
        if (cubemap == null)
        {
            return;
        }

        int width = cubemap.width;
        int height = cubemap.height;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        string getPath = AssetDatabase.GetAssetPath(cubemap.GetInstanceID());
        string[] cubemapTargetPath = getPath.Split(new string[] { "/" }, System.StringSplitOptions.None);
        string path = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + getPath.Substring(0, cubemapTargetPath[cubemapTargetPath.Length - 1].Length + 1) + cubemap.name;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        tex.SetPixels(cubemap.GetPixels(CubemapFace.PositiveX));
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path + "/Right.png", bytes);

        tex.SetPixels(cubemap.GetPixels(CubemapFace.NegativeX));
        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path + "/Left.png", bytes);

        tex.SetPixels(cubemap.GetPixels(CubemapFace.PositiveY));
        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path + "/Up.png", bytes);

        tex.SetPixels(cubemap.GetPixels(CubemapFace.NegativeY));
        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path + "/Down.png", bytes);

        tex.SetPixels(cubemap.GetPixels(CubemapFace.PositiveZ));
        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path + "/Forward.png", bytes);

        tex.SetPixels(cubemap.GetPixels(CubemapFace.NegativeZ));
        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path + "/Backward.png", bytes);

        DestroyImmediate(tex);
        AssetDatabase.Refresh();
    }
}