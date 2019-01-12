using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SMAA : MonoBehaviour
{
    //Reference variables
    public Shader curShader;
    public Texture2D alphaTex;
    public Texture2D luminTex;
    public Texture2D searchTex;
    public Texture2D black;

    private Material mat;
    public Material curMaterial
    {
        get
        {
            if (mat == null)
            {
                mat = new Material(curShader);
                mat.hideFlags = HideFlags.HideAndDontSave;
            }

            return mat;
        }
    }

    void Awake()
    {
        if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
        {
            this.enabled = false;
            return;
        }
    }

    void OnDisable()
    {
        if (curMaterial != null)
        {
            DestroyImmediate(curMaterial);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        int width = Screen.width;
        int height = Screen.height;

        Vector4 metrics = new Vector4(1f / (float)width, 1f / (float)height, width, height);

        curMaterial.SetTexture("areaTex", alphaTex);
        curMaterial.SetTexture("luminTex", luminTex);
        curMaterial.SetTexture("searchTex", searchTex);
        curMaterial.SetTexture("_SrcTex", source);
        curMaterial.SetVector("SMAA_RT_METRICS", metrics);

        RenderTexture rt = RenderTexture.GetTemporary(width, height, 0);
        RenderTexture rt2 = RenderTexture.GetTemporary(width, height, 0);
        RenderTexture rt3 = RenderTexture.GetTemporary(width, height, 0);

        Graphics.Blit(source, rt3);

        Graphics.Blit(black, rt);
        Graphics.Blit(black, rt2);

        Graphics.Blit(rt3, rt, curMaterial, 0);

        Graphics.Blit(rt, rt2, curMaterial, 1);
        Graphics.Blit(rt2, rt3, curMaterial, 2);

        Graphics.Blit(rt3, destination);

        rt.Release();
        rt2.Release();
        rt3.Release();
    }
}