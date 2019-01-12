using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DirectionalBlur : MonoBehaviour
{
    public Shader shader;
    public float blurStrength = 1.0f;
    public Vector2 velocity = Vector2.zero;

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
        blurStrength = Mathf.Clamp01(blurStrength);

        if (mat == null || velocity == Vector2.zero || blurStrength <= 0f)
        {
            Graphics.Blit(source, destination);
            return;
        }

        mat.SetFloat("_BlurAmount", blurStrength);
        mat.SetVector("_DirVect", new Vector4(velocity.x, velocity.y, 0f, 0f));
        Graphics.Blit(source, destination, mat);
    }
}