using UnityEngine;
using System.Collections;

public class DistortionEffect : MonoBehaviour {
    public Shader distortionShader;
    public Texture2D distortTexture;
    public float baseIntensity = 0.5f;
    public float minIntensityMod = 0.2f;
    public float distortFrequency = 0.15f;
    public int updateFPS = 60;
    public Vector2 distortScale = new Vector2(0.3f, 0.45f);

    public float splitFrequency = 0.05f;
    public float splitProbability = 0f;
    public Vector2 splitPos = new Vector2(0.4f, 0.6f);
    public float splitOffset = 0.1f;

    private bool shouldSplit = false;
    private float lastUpdateTime;
    private float lastSplitTime;

    private Material cMaterial;
    public Material curMaterial {
        get {
            if(cMaterial == null) {
                cMaterial = new Material(distortionShader);
                cMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return cMaterial;
        }
    }

    void Awake() {
        if(!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures) {
            this.enabled = false;
            return;
        }

        lastUpdateTime = -(1f / updateFPS) * 2f;
    }

    void OnDisable() {
        if(curMaterial != null) {
            DestroyImmediate(curMaterial);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if(distortionShader == null || baseIntensity <= 0f) {
            Graphics.Blit(source, destination);
            return;
        }

        curMaterial.SetTexture("_DispTex", distortTexture);
        float updateInterval = 1f / updateFPS;

        if(splitProbability > 0f) {
            if(Time.time - lastSplitTime >= splitFrequency) {
                shouldSplit = (Random.value <= splitProbability);
                curMaterial.SetFloat("_SplitPos", Random.Range(splitPos.x, splitPos.y));
                lastSplitTime = Time.time;
            }
        }
        else {
            shouldSplit = false;
        }

        if(Time.time - lastUpdateTime >= updateInterval) {
            bool flip = (Random.value <= 0.5f);
            curMaterial.SetVector("_Offset", new Vector4(Random.value, Random.value, Random.Range(0f, splitOffset) * ((shouldSplit) ? 1f : 0f) * ((flip) ? -1f : 1f), Random.Range(0f, splitOffset) * ((shouldSplit) ? 1f : 0f) * ((flip) ? 1f : -1f)));
            curMaterial.SetFloat("_Scale", Random.Range(distortScale.x, distortScale.y));
            curMaterial.SetFloat("_Intensity", Random.value * baseIntensity * 0.1f * ((Random.value < distortFrequency) ? 1f : minIntensityMod));
            lastUpdateTime = Time.time;
        }

        Graphics.Blit(source, destination, curMaterial);
    }
}