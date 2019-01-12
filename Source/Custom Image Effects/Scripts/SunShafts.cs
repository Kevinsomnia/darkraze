using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SunShafts : PostEffectsBaseC
{
    public enum SunShaftsResolution { Low, Normal, High }
    public enum ShaftsScreenBlendMode { Screen, Additive }

    public SunShaftsResolution resolution = SunShaftsResolution.Normal;
    public ShaftsScreenBlendMode screenBlendMode = ShaftsScreenBlendMode.Screen;

    public bool autoDetectSource = false;

    public Transform shaftSource;
    public bool directionShaft = true;
    public int blurIterations = 2;
    public float shaftBlurRadius = 2.5f;
    public float shaftIntensity = 1.15f;
    public Color sunColor = Color.white;

    public float maxRadius = 0.75f;
    public bool useDepthTexture = true;

    public Shader sunShaftShader;
    private Material sunShaftMaterial;

    public Shader simpleClearShader;
    private Material simpleClearMaterial;

    private bool CheckResources()
    {
        CheckSupport(useDepthTexture);

        sunShaftMaterial = CheckShaderAndCreateMaterial(sunShaftShader, sunShaftMaterial);
        simpleClearMaterial = CheckShaderAndCreateMaterial(simpleClearShader, simpleClearMaterial);

        return isSupported;
    }

    void Start()
    {
        if (autoDetectSource)
        {
            AutoDetectSource();
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!CheckResources() || shaftSource == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (useDepthTexture)
        {
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        }

        int divider = 4;
        if (resolution == SunShaftsResolution.Normal)
        {
            divider = 2;
        }
        else if (resolution == SunShaftsResolution.High)
        {
            divider = 1;
        }

        Vector3 shaftPos = ((directionShaft) ? -shaftSource.forward * 500000f : shaftSource.position);
        Vector3 v = new Vector3(0.5f, 0.5f, 0f);
        v = GetComponent<Camera>().WorldToViewportPoint(shaftPos);

        int rtW = source.width / divider;
        int rtH = source.height / divider;

        RenderTexture lrColorB;
        RenderTexture lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0);

        sunShaftMaterial.SetVector("_BlurRadius4", new Vector4(1f, 1f, 0f, 0f) * shaftBlurRadius);
        sunShaftMaterial.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, maxRadius));

        if (!useDepthTexture)
        {
            RenderTexture tmpBuffer = RenderTexture.GetTemporary(source.width, source.height, 0);
            RenderTexture.active = tmpBuffer;
            GL.ClearWithSkybox(false, GetComponent<Camera>());

            Graphics.Blit(source, lrDepthBuffer, sunShaftMaterial, 3);
            RenderTexture.ReleaseTemporary(tmpBuffer);
        }
        else
        {
            Graphics.Blit(source, lrDepthBuffer, sunShaftMaterial, 2);
        }

        DrawBorder(lrDepthBuffer, simpleClearMaterial);

        blurIterations = Mathf.Clamp(blurIterations, 1, 4);
        float ofs = shaftBlurRadius * (1f / 768f);

        sunShaftMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0f, 0f));
        sunShaftMaterial.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, maxRadius));

        for (int it2 = 0; it2 < blurIterations; it2++)
        {
            lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0);
            Graphics.Blit(lrDepthBuffer, lrColorB, sunShaftMaterial, 1);
            RenderTexture.ReleaseTemporary(lrDepthBuffer);
            ofs = shaftBlurRadius * ((it2 * 2f + 1f) * 6f) / 768f;
            sunShaftMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0f, 0f));

            lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0);
            Graphics.Blit(lrColorB, lrDepthBuffer, sunShaftMaterial, 1);
            RenderTexture.ReleaseTemporary(lrColorB);
            ofs = shaftBlurRadius * ((it2 * 2f + 1f) * 6f) / 768f;
            sunShaftMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0f, 0f));
        }

        sunShaftMaterial.SetVector("_SunColor", ((v.z >= 0f) ? (new Vector4(sunColor.r, sunColor.g, sunColor.b, sunColor.a) * shaftIntensity) : Vector4.zero));
        sunShaftMaterial.SetTexture("_ColorBuffer", lrDepthBuffer);
        Graphics.Blit(source, destination, sunShaftMaterial, (screenBlendMode == ShaftsScreenBlendMode.Screen) ? 0 : 4);
        RenderTexture.ReleaseTemporary(lrDepthBuffer);
    }

    public void AutoDetectSource()
    {
        if (shaftSource == null || (shaftSource != null && !shaftSource.GetComponent<Light>().enabled))
        {
            Light[] lights = (Light[])FindObjectsOfType(typeof(Light));
            foreach (Light lite in lights)
            {
                if (lite.type == LightType.Directional && lite.enabled)
                {
                    shaftSource = lite.transform;
                    directionShaft = true;
                    break;
                }
            }
        }

        if (shaftSource != null)
        {
            shaftIntensity = shaftSource.GetComponent<Light>().intensity * 0.025f;
            sunColor = shaftSource.GetComponent<Light>().color;
        }
    }
}