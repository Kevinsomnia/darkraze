using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BloomAndFlares : PostEffectsBaseC {
    public enum BloomScreenBlendMode {
        Screen = 0,
        Additive = 1,
    }

    public enum BloomQuality {
        Cheap = 0,
        Full = 1,
    }

    public BloomScreenBlendMode screenBlendMode = BloomScreenBlendMode.Additive;
    public BloomQuality quality = BloomQuality.Full;

    public float bloomIntensity = 0.5f;
    public float bloomThreshold = 0.5f;
    public int bloomBlurIterations = 2;
    public float blurSpread = 2.5f;

    public int hollywoodFlareBlurIterations = 1;
    public float hollyStretchWidth = 5f;
    public float lensFlareIntensity = 0f;
    public float lensFlareThreshold = 0.3f;
    public float lensFlareSaturation = 0.75f;
    public Color flareColor = new Color(0.4f, 0.4f, 0.8f, 1f);

    public Texture2D dirtTexture;
    public float glareIntensity = 3f;

    public Shader screenBlendShader;
    public Shader blurAndFlaresShader;
    public Shader brightPassFilterShader;
    public Shader lensFlareShader;

    private Material screenBlend;
    private Material blurAndFlares;
    private Material brightPassFilter;
    private Material lensFlare;

    public override bool CheckResources() {
        CheckSupport(false);

        screenBlend = CheckShaderAndCreateMaterial(screenBlendShader, screenBlend);
        lensFlare = CheckShaderAndCreateMaterial(lensFlareShader, lensFlare);
        blurAndFlares = CheckShaderAndCreateMaterial(blurAndFlaresShader, blurAndFlares);
        brightPassFilter = CheckShaderAndCreateMaterial(brightPassFilterShader, brightPassFilter);

        return isSupported;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if(!CheckResources()) {
            Graphics.Blit(source, destination);
            return;
        }

        int rtW2 = source.width / 2;
        int rtH2 = source.height / 2;
        int rtW4 = source.width / 4;
        int rtH4 = source.height / 4;

        float widthOverHeight = (float)source.width / (float)source.height;
        float oneOverBaseSize = 1f / 512f;

        RenderTexture quarterResDown = RenderTexture.GetTemporary(rtW4, rtH4, 0);
        RenderTexture halfResDown = RenderTexture.GetTemporary(rtW4, rtH4, 0);

        if(quality > BloomQuality.Cheap) {
            Graphics.Blit(source, halfResDown, screenBlend, 2);
            RenderTexture rtDown4 = RenderTexture.GetTemporary(rtW4, rtH4, 0);
            Graphics.Blit(halfResDown, rtDown4, screenBlend, 2);
            Graphics.Blit(rtDown4, quarterResDown, screenBlend, 6);
            RenderTexture.ReleaseTemporary(rtDown4);
            
        }
        else {
            Graphics.Blit(source, halfResDown);
            Graphics.Blit(halfResDown, quarterResDown, screenBlend, 6);
        }

        RenderTexture.ReleaseTemporary(halfResDown);

        RenderTexture secondQuarterResDown = RenderTexture.GetTemporary(rtW4, rtH4, 0);
        BrightFilter(bloomThreshold, quarterResDown, secondQuarterResDown);

        bloomBlurIterations = Mathf.Clamp(bloomBlurIterations, 1, 10);

        for(int i = 0; i < bloomBlurIterations; i++) {
            float spreadForPass = (1f + (i * 0.25f)) * blurSpread;

            RenderTexture blur4 = RenderTexture.GetTemporary(rtW4, rtH4, 0);
            blurAndFlares.SetVector("_Offsets", new Vector4(0f, (spreadForPass / widthOverHeight) * oneOverBaseSize, 0f, 0f));
            Graphics.Blit(secondQuarterResDown, blur4, blurAndFlares, 4);
            RenderTexture.ReleaseTemporary(secondQuarterResDown);
            secondQuarterResDown = blur4;

            blur4 = RenderTexture.GetTemporary(rtW4, rtH4, 0);
            blurAndFlares.SetVector("_Offsets", new Vector4((spreadForPass / widthOverHeight) * oneOverBaseSize, 0f, 0f, 0f));
            Graphics.Blit(secondQuarterResDown, blur4, blurAndFlares, 4);
            RenderTexture.ReleaseTemporary(secondQuarterResDown);
            secondQuarterResDown = blur4;

            if(quality > BloomQuality.Cheap) {
                if(i == 0) {
                    Graphics.SetRenderTarget(quarterResDown);
                    GL.Clear(false, true, Color.black);
                    Graphics.Blit(secondQuarterResDown, quarterResDown);
                }
                else {
                    quarterResDown.MarkRestoreExpected();
                    Graphics.Blit(secondQuarterResDown, quarterResDown, screenBlend, 10);
                }
            }
        }

        if(quality > BloomQuality.Cheap) {
            Graphics.SetRenderTarget(secondQuarterResDown);
            GL.Clear(false, true, Color.black);
            Graphics.Blit(quarterResDown, secondQuarterResDown, screenBlend, 6);
        }

        if(lensFlareIntensity > 0f) {
            RenderTexture rtFlares4 = RenderTexture.GetTemporary(rtW4, rtH4, 0);

            float stretchWidth = ((float)hollyStretchWidth / widthOverHeight) * oneOverBaseSize;
            blurAndFlares.SetVector("_Offsets", new Vector4(1f, 0f, 0f, 0f));
            blurAndFlares.SetVector("_Threshhold", new Vector4(lensFlareThreshold, 1f, 0f, 0f));
            blurAndFlares.SetVector("_TintColor", flareColor * lensFlareIntensity);
            blurAndFlares.SetFloat("_Saturation", lensFlareSaturation);

            quarterResDown.DiscardContents();
            Graphics.Blit(rtFlares4, quarterResDown, blurAndFlares, 2);

            rtFlares4.DiscardContents();
            Graphics.Blit(quarterResDown, rtFlares4, blurAndFlares, 3);

            blurAndFlares.SetVector("_Offsets", new Vector4(stretchWidth, 0f, 0f, 0f));

            blurAndFlares.SetFloat("_StretchWidth", hollyStretchWidth);
            quarterResDown.DiscardContents();
            Graphics.Blit(rtFlares4, quarterResDown, blurAndFlares, 1);

            blurAndFlares.SetFloat("_StretchWidth", hollyStretchWidth * 2f);
            rtFlares4.DiscardContents();
            Graphics.Blit(quarterResDown, rtFlares4, blurAndFlares, 1);

            for(int i = 0; i < hollywoodFlareBlurIterations; i++) {
                stretchWidth = (hollyStretchWidth * 2f / widthOverHeight) * oneOverBaseSize;

                blurAndFlares.SetVector("_Offsets", new Vector4(stretchWidth, 0.0f, 0.0f, 0.0f));
                rtFlares4.DiscardContents();
                Graphics.Blit(quarterResDown, rtFlares4, blurAndFlares, 4);

                blurAndFlares.SetVector("_Offsets", new Vector4(stretchWidth, 0.0f, 0.0f, 0.0f));
                quarterResDown.DiscardContents();
                Graphics.Blit(rtFlares4, quarterResDown, blurAndFlares, 4);
            }

            AddTo(1f, quarterResDown, secondQuarterResDown);

            RenderTexture.ReleaseTemporary(rtFlares4);
        }

        bloomIntensity = Mathf.Clamp(bloomIntensity, 0f, 25f);
        screenBlend.SetFloat("_Intensity", bloomIntensity);
        screenBlend.SetTexture("_ColorBuffer", source);
        screenBlend.SetTexture("_GlareTexture", dirtTexture);
        screenBlend.SetFloat("_GlareIntensity", glareIntensity * 10f);

        if(quality > BloomQuality.Cheap) {
            RenderTexture halfResUp = RenderTexture.GetTemporary(rtW2, rtH2, 0);
            Graphics.Blit(secondQuarterResDown, halfResUp);
            Graphics.Blit(halfResUp, destination, screenBlend, (int)screenBlendMode);
            RenderTexture.ReleaseTemporary(halfResUp);
        }
        else {
            Graphics.Blit(secondQuarterResDown, destination, screenBlend, (int)screenBlendMode);
        }

        RenderTexture.ReleaseTemporary(quarterResDown);
        RenderTexture.ReleaseTemporary(secondQuarterResDown);
    }

    private void AddTo(float intensity, RenderTexture from, RenderTexture to) {
        screenBlend.SetFloat("_Intensity", 1f);
        to.MarkRestoreExpected();
        Graphics.Blit(from, to, screenBlend, 9);
    }

    private void BrightFilter(float threshold, RenderTexture from, RenderTexture to) {
        brightPassFilter.SetFloat("_Threshhold", threshold);
        Graphics.Blit(from, to, brightPassFilter, 0);
    }
}