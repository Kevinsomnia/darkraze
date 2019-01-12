using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GaussianBlur : MonoBehaviour {
    private const float HEIGHT_REFERENCE = 720f;

    [Range(0, 4)]
    public int downsample = 1;
    [Range(0f, 20f)]
    public float blurSize = 3f;
    [Range(1, 8)]
    public int blurIterations = 2;

    public bool screenProportional = true;
    public bool extraIterations = false;
    public Shader blurShader;
    
    private Material mat;

    private void OnEnable() {
        if(blurShader == null || !blurShader.isSupported) {
            enabled = false;
            return;
        }

        mat = new Material(blurShader);
        mat.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnDisable() {
        if(mat != null)
            DestroyImmediate(mat);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
		if(!CheckSupport()) {
			enabled = false;
			return;
		}
		
        if(blurSize <= 0f) {
            Graphics.Blit(source, destination);
            return;
        }
		
        float widthMod = 1f / (1 << downsample);

        if(screenProportional) {
            widthMod *= Screen.height / HEIGHT_REFERENCE;
        }
        int finalIterationCount = blurIterations;

        if(extraIterations)
            finalIterationCount = Mathf.Clamp(Mathf.RoundToInt(blurIterations * 2.5f), blurIterations + 1, 8);
        
        widthMod /= Mathf.LerpUnclamped(1f, finalIterationCount, 0.2f);
        mat.SetFloat("_BlurSize", blurSize * widthMod);

        int rtW = source.width >> downsample;
        int rtH = source.height >> downsample;

        RenderTexture rt1 = RenderTexture.GetTemporary(rtW, rtH, 0);
        RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0);

        if(downsample > 1)
            Graphics.Blit(source, rt1, mat, 0);
        else
            Graphics.Blit(source, rt1);
        
        for(int i = 0; i < finalIterationCount; i++) {
            Graphics.Blit(rt1, rt2, mat, 1);
            Graphics.Blit(rt2, rt1, mat, 2);
        }

        Graphics.Blit(rt1, destination);
        RenderTexture.ReleaseTemporary(rt1);
        RenderTexture.ReleaseTemporary(rt2);
    }
	
	private bool CheckSupport() {
		return (SystemInfo.supportsImageEffects && mat != null);
	}
}