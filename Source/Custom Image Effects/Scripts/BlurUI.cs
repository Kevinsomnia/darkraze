using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BlurUI : MonoBehaviour
{
    private const float HEIGHT_REFERENCE = 900f;

    [Range(1, 5)]
    public int downsample = 2;
    [Range(0f, 20f)]
    public float blurSize = 5f;
    [Range(1, 8)]
    public int blurIterations = 2;

    public Shader blurShader;

    private Material mat;
    private int _BlurSize;

    private void OnEnable()
    {
        if (blurShader == null || !blurShader.isSupported)
        {
            enabled = false;
            return;
        }

        mat = new Material(blurShader);
        mat.hideFlags = HideFlags.HideAndDontSave;

        _BlurSize = Shader.PropertyToID("_BlurSize");
    }

    private void OnDisable()
    {
        if (mat != null)
            DestroyImmediate(mat);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!CheckSupport())
        {
            enabled = false;
            return;
        }

        float widthMod = 1f / (1 << downsample);
        widthMod *= Screen.height / HEIGHT_REFERENCE;

        widthMod /= Mathf.LerpUnclamped(1f, blurIterations, 0.2f);
        mat.SetFloat(_BlurSize, blurSize * widthMod);

        int rtW = source.width >> downsample;
        int rtH = source.height >> downsample;

        RenderTexture rt1 = RenderTexture.GetTemporary(rtW, rtH, 0);
        RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0);

        Graphics.Blit(source, rt1, mat, 0);
        float iterSizeMod = 1.0f;

        for (int i = 0; i < blurIterations; i++)
        {
            Graphics.Blit(rt1, rt2, mat, 1);
            Graphics.Blit(rt2, rt1, mat, 2);

            iterSizeMod *= 1.4142135f; // sqrt(2)
            mat.SetFloat(_BlurSize, blurSize * widthMod * iterSizeMod);
        }

        Shader.SetGlobalTexture("_BlurredUI", rt2);

        RenderTexture.ReleaseTemporary(rt1);
        RenderTexture.ReleaseTemporary(rt2);

        Graphics.Blit(source, destination);
    }

    private bool CheckSupport()
    {
        return (SystemInfo.supportsImageEffects && mat != null);
    }
}