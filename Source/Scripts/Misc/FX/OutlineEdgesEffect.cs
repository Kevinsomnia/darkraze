using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class OutlineEdgesEffect : MonoBehaviour {
    public Shader shader;
    public float threshold = 0.5f;
    public float edgeWidth = 1f;
    public float edgeIntensity = 1f;
    public float backgroundBrightness = 0.05f;
    public Vector4 colorTint = Vector4.one;
    public Texture2D grainTexture;
    public float grainIntensity = 0.05f;
    public float grainIntensityRandom = 0.025f;
    public float grainSize = 2.0f;
    public int updateFPS = 30;
    public FilterMode filterMode = FilterMode.Bilinear;

    private float lastNoiseTime;
    private float randomNoise;

    private Material mat;
    private Material curMaterial {
        get {
            if(mat == null) {
                mat = new Material(shader);
                mat.hideFlags = HideFlags.HideAndDontSave;
            }

            return mat;
        }
    }

    void Start() {
        if(!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures) {
            enabled = false;
            return;
        }

        lastNoiseTime = -100f;
    }

    void OnDisable() {
        if(mat != null) {
            DestroyImmediate(mat);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        curMaterial.SetFloat("_Threshold", threshold);
        curMaterial.SetFloat("_BackgroundBrightness", backgroundBrightness);
        curMaterial.SetVector("_ColorTint", colorTint);
        curMaterial.SetFloat("_EdgeIntensity", edgeIntensity);
        curMaterial.SetFloat("_EdgeWidth", edgeWidth);

        if(grainIntensity > 0f && grainTexture != null) {
            grainTexture.filterMode = filterMode;
            curMaterial.SetTexture("_GrainTex", grainTexture);
            float grainScale = 1.0f / grainSize;

            if(Application.isEditor && !Application.isPlaying) {
                curMaterial.SetVector("_GrainOffsetScale", new Vector4(0f, 0f, (float)Screen.width / (float)grainTexture.width * grainScale, (float)Screen.height / (float)grainTexture.height * grainScale));
            }
            else {
                if(Time.time - lastNoiseTime >= (1f / (float)Mathf.Max(1, updateFPS))) {
                    curMaterial.SetVector("_GrainOffsetScale", new Vector4(Random.value, Random.value, grainScale, grainScale));
                    randomNoise = Random.value * grainIntensityRandom;
                    lastNoiseTime = Time.time;
                }
            }

            curMaterial.SetFloat("_GrainIntensity", grainIntensity + grainIntensityRandom);
        }
        else {
            curMaterial.SetFloat("_GrainIntensity", 0f);
        }

        Graphics.Blit(source, destination, curMaterial);
    }
}