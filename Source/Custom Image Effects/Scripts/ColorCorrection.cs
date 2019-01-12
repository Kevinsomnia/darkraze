using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Color Correction")]
public class ColorCorrection : PostEffectsBaseC {
    public AnimationCurve redChannel = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f));
    public AnimationCurve greenChannel = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f));
    public AnimationCurve blueChannel = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f));
    public Shader colorCorrectionShader = null;

    private Material ccMaterial;
    private Texture2D rgbChannelTex;

    public override bool CheckResources() {
        CheckSupport(false);

        ccMaterial = CheckShaderAndCreateMaterial(colorCorrectionShader, ccMaterial);

        if(rgbChannelTex == null)
            rgbChannelTex = new Texture2D(256, 3, TextureFormat.RGB24, false, true);

        rgbChannelTex.hideFlags = HideFlags.DontSave;
        rgbChannelTex.wrapMode = TextureWrapMode.Clamp;

        return isSupported;
    }

    public void Start() {
        CheckResources();

        for(int i = 0; i <= 255; i++) {
            float rCh = redChannel.Evaluate(i / 255f);
            float gCh = greenChannel.Evaluate(i / 255f);
            float bCh = blueChannel.Evaluate(i / 255f);

            rgbChannelTex.SetPixel(i, 0, new Color(rCh, rCh, rCh));
            rgbChannelTex.SetPixel(i, 1, new Color(gCh, gCh, gCh));
            rgbChannelTex.SetPixel(i, 2, new Color(bCh, bCh, bCh));
        }

        rgbChannelTex.Apply();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if(!CheckResources()) {
            Graphics.Blit(source, destination);
            return;
        }

        ccMaterial.SetTexture("_RgbTex", rgbChannelTex);
        Graphics.Blit(source, destination, ccMaterial);
    }
}