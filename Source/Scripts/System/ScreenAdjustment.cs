using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
//All basic screen adjustments combined into one.
public class ScreenAdjustment : MonoBehaviour
{
    public Shader brightnessShader;
    public float saturationAmount = 1.0f;
    public Vector4 colorTint = Vector4.one;
    public float shadowThreshold = 0.25f;
    public float shadowStrength = 0f;
    public float highlightThreshold = 0.7f;
    public float highlightStrength = 0f;
    public float smoothingAmount = 0.1f;

    private Material cMaterial;
    public Material curMaterial
    {
        get
        {
            if (cMaterial == null)
            {
                cMaterial = new Material(brightnessShader);
                cMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return cMaterial;
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
        if (cMaterial != null)
        {
            DestroyImmediate(cMaterial);
            cMaterial = null;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        float brightness = (GameSettings.settingsController != null) ? Mathf.Clamp(GameSettings.settingsController.brightness, 0.75f, 1.25f) : 1f;
        saturationAmount = Mathf.Clamp(saturationAmount, 0f, 10f);

        if (brightnessShader == null || (Mathf.Approximately(brightness, 1f) && Mathf.Approximately(saturationAmount, 1f) && colorTint == Vector4.one && shadowStrength == 0f && highlightStrength == 0f))
        {
            Graphics.Blit(source, destination);
            return;
        }

        shadowThreshold = Mathf.Clamp01(shadowThreshold);
        highlightThreshold = Mathf.Clamp01(highlightThreshold);

        curMaterial.SetFloat("_Brightness", brightness);
        curMaterial.SetFloat("_SaturationAmount", saturationAmount);
        curMaterial.SetVector("_ColorTint", colorTint);
        curMaterial.SetVector("_SelectiveVariables", new Vector4(shadowThreshold, shadowStrength, highlightThreshold, highlightStrength));
        curMaterial.SetFloat("_Smoothness", smoothingAmount);

        Graphics.Blit(source, destination, curMaterial);
    }
}