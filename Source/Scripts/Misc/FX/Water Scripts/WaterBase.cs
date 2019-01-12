using UnityEngine;

public enum WaterQuality
{
    High = 2,
    Medium = 1,
    Low = 0,
}

[ExecuteInEditMode]
public class WaterBase : MonoBehaviour
{
    public Material sharedMaterial;
    public WaterQuality waterQuality = WaterQuality.High;
    public bool edgeBlend = true;

    private GameSettings gSettings;
    private PlanarReflection pr;

    void Start()
    {
        pr = GetComponent<PlanarReflection>();
        gSettings = GameSettings.settingsController;
    }

    void Update()
    {
        if (gSettings != null)
        {
            string wQual = gSettings.waterQuality;
            if (wQual == "High" || wQual == "Very High")
            {
                waterQuality = WaterQuality.High;
            }
            else if (wQual == "Medium")
            {
                waterQuality = WaterQuality.Medium;
            }
            else
            {
                waterQuality = WaterQuality.Low;
            }
        }

        if (sharedMaterial != null)
        {
            UpdateShader();
        }
    }

    public void WaterTileBeingRendered(Transform tr, Camera currentCam)
    {
        if (currentCam && edgeBlend)
        {
            currentCam.depthTextureMode |= DepthTextureMode.Depth;
        }
    }

    private void UpdateShader()
    {
        if (waterQuality == WaterQuality.High)
        {
            sharedMaterial.shader.maximumLOD = 501;
        }
        else if (waterQuality == WaterQuality.Medium)
        {
            sharedMaterial.shader.maximumLOD = 301;
        }
        else
        {
            sharedMaterial.shader.maximumLOD = 201;
        }

        pr.enabled = (waterQuality != WaterQuality.Low);

        if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
        {
            edgeBlend = false;
        }

        if (edgeBlend)
        {
            Shader.EnableKeyword("WATER_EDGEBLEND_ON");
            Shader.DisableKeyword("WATER_EDGEBLEND_OFF");

            // just to make sure (some peeps might forget to add a water tile to the patches)
            if (Camera.main)
            {
                Camera.main.depthTextureMode |= DepthTextureMode.Depth;
            }
        }
        else
        {
            Shader.EnableKeyword("WATER_EDGEBLEND_OFF");
            Shader.DisableKeyword("WATER_EDGEBLEND_ON");
        }
    }
}