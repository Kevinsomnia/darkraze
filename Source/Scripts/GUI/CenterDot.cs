using UnityEngine;
using System.Collections;

public class CenterDot : MonoBehaviour
{
    public int dotSize = 1;

    private Texture2D tex;

    void Start()
    {
        tex = new Texture2D(1, 1, TextureFormat.RGB24, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect((Screen.width - dotSize) / 2, (Screen.height - dotSize) / 2, dotSize, dotSize), tex);
    }
}