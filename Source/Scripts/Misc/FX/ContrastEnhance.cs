using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ContrastEnhance : PostEffectsBaseC {
    public float intensity = 0.5f;
    public float threshold = 0f;
    public float blurSpread = 1f;

    public Shader contrastShader;
    public Shader blurShader;

    private Material contrastMaterial;
    private Material blurMaterial;

    public override bool CheckResources() {
        CheckSupport(false);

        contrastMaterial = CheckShaderAndCreateMaterial(contrastShader, contrastMaterial);
        blurMaterial = CheckShaderAndCreateMaterial(blurShader, blurMaterial);

        return isSupported;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if(!CheckResources() || intensity <= 0f) {
            Graphics.Blit(source, destination);
            return;
        }

        int rtW = source.width;
        int rtH = source.height;

        RenderTexture rt = RenderTexture.GetTemporary(rtW / 4, rtH / 4, 0);

        Graphics.Blit(source, rt);
        RenderTexture rt2 = RenderTexture.GetTemporary(rtW / 8, rtH / 8, 0);
        Graphics.Blit(rt, rt2);
        RenderTexture.ReleaseTemporary(rt);

        blurMaterial.SetVector("offsets", new Vector4(0f, blurSpread / rt2.height, 0f, 0f));
        RenderTexture rt3 = RenderTexture.GetTemporary(rtW / 8, rtH / 8, 0);
        Graphics.Blit(rt2, rt3, blurMaterial);
        RenderTexture.ReleaseTemporary(rt2);

        blurMaterial.SetVector("offsets", new Vector4(blurSpread / rt2.width, 0f, 0f, 0f));
        rt2 = RenderTexture.GetTemporary(rtW / 8, rtH / 8, 0);
        Graphics.Blit(rt3, rt2, blurMaterial);
        RenderTexture.ReleaseTemporary(rt3);

        contrastMaterial.SetTexture("_MainTexBlurred", rt2);
        contrastMaterial.SetFloat("intensity", intensity);
        contrastMaterial.SetFloat("threshhold", threshold);
        Graphics.Blit(source, destination, contrastMaterial);

        RenderTexture.ReleaseTemporary(rt2);
    }
}