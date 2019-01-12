using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class VignettingC : PostEffectsBaseC
{
    public float intensity = 0.5f;
    public float chromaticAberration = 0.2f;

    public float blur = 0f;
    public float blurSpread = 0.75f;

    [HideInInspector] public float sniperBlur;
    [HideInInspector] public float sniperIntensity;
    [HideInInspector] public float heartbeatBlur;
    [HideInInspector] public float aimingBlur;

    public Shader vignetteShader;
    private Material vignetteMaterial;

    public Shader separableBlurShader;
    private Material separableBlurMaterial;

    public Shader chromAberrationShader;
    private Material chromAberrationMaterial;

    public override bool CheckResources()
    {
        CheckSupport(false);

        vignetteMaterial = CheckShaderAndCreateMaterial(vignetteShader, vignetteMaterial);
        separableBlurMaterial = CheckShaderAndCreateMaterial(separableBlurShader, separableBlurMaterial);
        chromAberrationMaterial = CheckShaderAndCreateMaterial(chromAberrationShader, chromAberrationMaterial);

        return isSupported;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!CheckResources())
        {
            Graphics.Blit(source, destination);
            return;
        }

        int rtW = source.width;
        int rtH = source.height;

        bool doPrepass = ((blur + sniperBlur + heartbeatBlur + aimingBlur) > 0f || (intensity + sniperIntensity) > 0f);

        float widthOverHeight = (1.0f * rtW) / (1.0f * rtH);
        float oneOverBaseSize = 1.0f / 512.0f;

        RenderTexture color = null;
        RenderTexture color2a = null;
        RenderTexture color2b = null;

        if (doPrepass)
        {
            color = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);

            if (blur > 0f)
            {
                color2a = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);

                Graphics.Blit(source, color2a, chromAberrationMaterial, 0);

                separableBlurMaterial.SetVector("offsets", new Vector4(0f, (blurSpread + heartbeatBlur) * oneOverBaseSize, 0f, 0f));
                color2b = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);
                Graphics.Blit(color2a, color2b, separableBlurMaterial);
                RenderTexture.ReleaseTemporary(color2a);

                separableBlurMaterial.SetVector("offsets", new Vector4((blurSpread + heartbeatBlur) * oneOverBaseSize / widthOverHeight, 0f, 0f, 0f));
                color2a = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);
                Graphics.Blit(color2b, color2a, separableBlurMaterial);
                RenderTexture.ReleaseTemporary(color2b);
            }

            vignetteMaterial.SetFloat("_Intensity", intensity + sniperIntensity);
            vignetteMaterial.SetFloat("_Blur", blur + sniperBlur + heartbeatBlur + aimingBlur);
            vignetteMaterial.SetTexture("_VignetteTex", color2a);

            Graphics.Blit(source, color, vignetteMaterial, 0);
        }

        chromAberrationMaterial.SetFloat("_ChromaticAberration", chromaticAberration);

        Graphics.Blit(doPrepass ? color : source, destination, chromAberrationMaterial, 1);

        RenderTexture.ReleaseTemporary(color);
        RenderTexture.ReleaseTemporary(color2a);
    }
}