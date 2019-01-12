using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class OpticShadowSway : MonoBehaviour
{
    public static class ShaderProps
    {
        public static readonly int _Overlay = Shader.PropertyToID("_Overlay");
        public static readonly int _OverlayParams = Shader.PropertyToID("_OverlayParams");
        public static readonly int _ClampOffset = Shader.PropertyToID("_ClampOffset");
        public static readonly int _ClampColor = Shader.PropertyToID("_ClampColor");
        public static readonly int _EyeReliefFactor = Shader.PropertyToID("_EyeReliefFactor");
    }

    public Vector2 offsetAmount = Vector2.zero;
    public Vector2 scale = Vector2.one;
    public Color clampColor = Color.black;
    public float eyeReliefFactor = 1f;

    [Range(0f, 0.49f)]
    public float clampOffset = 0f;

    public Texture2D texture;
    public Shader overlayShader;

    private Material overlayMaterial;

    private void OnEnable()
    {
        if (overlayShader == null || !overlayShader.isSupported)
        {
            enabled = false;
            return;
        }

        overlayMaterial = new Material(overlayShader);
        overlayMaterial.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnDisable()
    {
        if (overlayMaterial != null)
            DestroyImmediate(overlayMaterial);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        overlayMaterial.SetTexture(ShaderProps._Overlay, texture);
        overlayMaterial.SetVector(ShaderProps._OverlayParams, new Vector4(offsetAmount.x, offsetAmount.y, scale.x, scale.y));
        overlayMaterial.SetFloat(ShaderProps._ClampOffset, clampOffset);
        overlayMaterial.SetColor(ShaderProps._ClampColor, clampColor);
        overlayMaterial.SetFloat(ShaderProps._EyeReliefFactor, eyeReliefFactor);

        Graphics.Blit(source, destination, overlayMaterial);
    }
}