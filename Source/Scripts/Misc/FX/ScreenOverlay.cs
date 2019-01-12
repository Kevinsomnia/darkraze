using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ScreenOverlay : PostEffectsBaseC {
	public enum OverlayBlendMode {
		Additive = 0,
		ScreenBlend = 1,
		Multiply = 2,
        Overlay = 3,
        AlphaBlend = 4,	
	}
	
	public OverlayBlendMode blendMode = OverlayBlendMode.Overlay;
	public float intensity = 1.0f;
	public Texture2D texture;
    public Vector2 tiling = Vector2.one;
    public Vector2 scrollSpeed = Vector2.zero;
    public float scrollInterval = 0f; //intervals per cycle, 0 disables, < -1 makes it auto-adjust to pixels
    public bool stretchFullscreen = true;
			
	public Shader overlayShader;
	private Material overlayMaterial = null;

    private Vector4 uvTrans;
    private Vector2 offset;
    private Vector2 tilingFactor;
    private Vector2 scrollValue;

    void Start() {
        offset = Vector2.zero;
        uvTrans = new Vector4(1f, 0f, 0f, 1f);
    }

	public override bool CheckResources() {
		CheckSupport(false);
		
		overlayMaterial = CheckShaderAndCreateMaterial(overlayShader, overlayMaterial);

		return isSupported;
	}
	
	void OnRenderImage(RenderTexture source, RenderTexture destination) {		
		if(!CheckResources() || intensity <= 0f) {
			Graphics.Blit(source, destination);
			return;
		}

        intensity = Mathf.Clamp(intensity, 0f, 100f);

        if(!stretchFullscreen) {
            tilingFactor = new Vector2(Screen.width / 640f, Screen.height / 480f);
        }
        else {
            tilingFactor = Vector2.one;
        }

        offset += scrollSpeed * Time.deltaTime;
		
		overlayMaterial.SetVector("_UV_Transform", uvTrans);
		overlayMaterial.SetFloat("_Intensity", intensity);
		overlayMaterial.SetTexture("_Overlay", texture);

        if(scrollInterval > 0f) {
            scrollValue = new Vector2(Mathf.Round(offset.x * scrollInterval) / scrollInterval, Mathf.Round(offset.y * scrollInterval) / scrollInterval);
        }
        else if(scrollInterval <= -1f) {
            scrollValue = new Vector2(Mathf.Round(offset.x * Screen.width) / Screen.width, Mathf.Round(offset.y * Screen.height) / Screen.height);
        }
        else {
            scrollValue = offset;
        }
        overlayMaterial.SetVector("_UVDetail", new Vector4(tiling.x * tilingFactor.x, tiling.y * tilingFactor.y, scrollValue.x, scrollValue.y));
		Graphics.Blit(source, destination, overlayMaterial, (int)blendMode);
	}
}