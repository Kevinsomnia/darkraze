using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Camera/Weapon Depth of Field")]
public class WeaponDepthOfField : PostEffectsBaseC {	
	public bool visualizeFocus = false;
	public float focalLength = 10.0f;
	public float focalSize = 0.05f; 
	public float aperture = 11.5f;
	public float maxBlurSize = 2.0f;

	public enum BlurSampleCount {
		Low = 0,
		Medium = 1,
		High = 2,
	}
	
	public BlurSampleCount blurSampleCount = BlurSampleCount.High;
	
	public bool nearBlur = false;	
	public float foregroundOverlap = 1.0f;
	
	public Shader dofHdrShader;		
	private Material dofHdrMaterial = null;
	
	private float focalDistance = 10.0f;
    private float oldFocalLength;
	private ComputeBuffer cbDrawArgs;
	private ComputeBuffer cbPoints;	
	private float internalBlurWidth = 1.0f;
	
	public override bool CheckResources() {
		CheckSupport(true);
		
		dofHdrMaterial = CheckShaderAndCreateMaterial(dofHdrShader, dofHdrMaterial);
		
		return isSupported;		  
	}
	
	void OnEnable() {
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;	
	}	
	
	void OnDisable() {
		ReleaseComputeResources();
		
		if(dofHdrMaterial) DestroyImmediate(dofHdrMaterial);
		dofHdrMaterial = null;
	}
	
	private void ReleaseComputeResources() {
		if(cbDrawArgs != null) {
			cbDrawArgs.Release(); 
		}
		if(cbPoints != null) {
			cbPoints.Release(); 
		}

		cbDrawArgs = null;
		cbPoints = null;
	}
	
	private void CreateComputeResources() {
		if(cbDrawArgs == null) {
			cbDrawArgs = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);
			int[] args = new int[4]{0, 1, 0, 0};
			cbDrawArgs.SetData(args);
		}

		if(cbPoints == null) {
			cbPoints = new ComputeBuffer(90000, 12+16, ComputeBufferType.Append);
		}
	}
	
	private void WriteCoc(RenderTexture fromTo) {
		dofHdrMaterial.SetTexture("_FgOverlap", null); 

		foregroundOverlap = Mathf.Max(0f, foregroundOverlap);
		if(nearBlur && foregroundOverlap > 0f) {
			int rtW = fromTo.width / 2;
			int rtH = fromTo.height / 2;
			
			// capture fg coc
			RenderTexture temp2 = RenderTexture.GetTemporary (rtW, rtH, 0, fromTo.format);
			Graphics.Blit (fromTo, temp2, dofHdrMaterial, 2); 
			
			// special blur
            float fgAdjustment = foregroundOverlap * 3f * Mathf.Clamp01(maxBlurSize * 0.5f);
			
			dofHdrMaterial.SetVector("_Offsets", new Vector4(0.0f, fgAdjustment, 0.0f, fgAdjustment));
			RenderTexture temp1 = RenderTexture.GetTemporary(rtW, rtH, 0, fromTo.format);
			Graphics.Blit (temp2, temp1, dofHdrMaterial, 1);
			RenderTexture.ReleaseTemporary(temp2);
			
			dofHdrMaterial.SetVector("_Offsets", new Vector4 (fgAdjustment, 0.0f, 0.0f, fgAdjustment));		
			temp2 = RenderTexture.GetTemporary(rtW, rtH, 0, fromTo.format);
			Graphics.Blit (temp1, temp2, dofHdrMaterial, 1);
			RenderTexture.ReleaseTemporary(temp1);
			
			// "merge up" with background COC
			dofHdrMaterial.SetTexture("_FgOverlap", temp2);
			fromTo.MarkRestoreExpected(); // only touching alpha channel, RT restore expected
			Graphics.Blit(fromTo, fromTo, dofHdrMaterial, 6);
			RenderTexture.ReleaseTemporary(temp2);
		}
		else {
			// capture full coc in alpha channel (fromTo is not read, but bound to detect screen flip)
			Graphics.Blit (fromTo, fromTo, dofHdrMaterial, 0);	
		}
	}
	
	void OnRenderImage(RenderTexture source, RenderTexture destination) {		
		if(!CheckResources() || maxBlurSize <= 0f) {
			Graphics.Blit(source, destination);
			return; 
		}
		
		aperture = Mathf.Max(0f, aperture);
		maxBlurSize = Mathf.Max(0f, maxBlurSize);
		focalSize = Mathf.Clamp(focalSize, 0.0f, 2.0f);

        if(focalLength != oldFocalLength) {
            focalDistance = GetComponent<Camera>().WorldToViewportPoint((focalLength - GetComponent<Camera>().nearClipPlane) * GetComponent<Camera>().transform.forward + GetComponent<Camera>().transform.position).z / (GetComponent<Camera>().farClipPlane - GetComponent<Camera>().nearClipPlane);
            oldFocalLength = focalLength;
        }

        dofHdrMaterial.SetVector("_CurveParams", new Vector4(1.0f, focalSize, aperture * 0.1f, focalDistance));
		
		RenderTexture rtLow = null;		
		RenderTexture rtLow2 = null;

		if(visualizeFocus) {
			WriteCoc(source);
			Graphics.Blit(source, destination, dofHdrMaterial, 7);
		}
		else { 		
			WriteCoc(source);

			rtLow = RenderTexture.GetTemporary (source.width / 2, source.height / 2, 0, source.format);
			rtLow2 = RenderTexture.GetTemporary (source.width / 2, source.height / 2, 0, source.format);

			int blurPass = (blurSampleCount == BlurSampleCount.High || blurSampleCount == BlurSampleCount.Medium) ? 8 : 4;

            dofHdrMaterial.SetVector("_Offsets", new Vector4(0.0f, maxBlurSize, 0.1f, maxBlurSize));
			
			// blur
			Graphics.Blit(source, rtLow, dofHdrMaterial, 3);
			Graphics.Blit(rtLow, rtLow2, dofHdrMaterial, blurPass);
			
			dofHdrMaterial.SetTexture("_LowRez", rtLow2);
			dofHdrMaterial.SetTexture("_FgOverlap", null);
            dofHdrMaterial.SetVector("_Offsets", Vector4.one * 0.5f * maxBlurSize);
			Graphics.Blit(source, destination, dofHdrMaterial, 5);
		}
		
		if(rtLow) RenderTexture.ReleaseTemporary(rtLow);
		if(rtLow2) RenderTexture.ReleaseTemporary(rtLow2);		
	}	
}