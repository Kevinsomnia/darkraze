using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RadialBlur : MonoBehaviour
{
    public Shader shader;
    public float blurIntensity = 1.0f;
    public float blurWidth = 0.5f;
    public float fisheyeEffect = 0.01f;

    private Material _mat;
    public Material mat
    {
        get
        {
            if (_mat == null && shader != null)
            {
                _mat = new Material(shader);
                _mat.hideFlags = HideFlags.HideAndDontSave;
            }

            return _mat;
        }
    }

    void Awake()
    {
        if (shader == null || !shader.isSupported || !SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
        {
            enabled = false;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (mat == null || blurIntensity <= 0f || blurWidth <= 0f)
        {
            Graphics.Blit(source, destination);
            return;
        }

        mat.SetFloat("_BlurWidth", blurWidth);
        mat.SetFloat("_BlurIntensity", blurIntensity);
        mat.SetFloat("_FisheyeEffect", fisheyeEffect);
        Graphics.Blit(source, destination, mat);
    }
}