using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PostEffectsBaseC : MonoBehaviour {
    protected bool supportHDRTextures = true;
    protected bool supportDX11 = false;
    protected bool isSupported = true;

    public Material CheckShaderAndCreateMaterial(Shader s, Material mat) {
        if(s == null) {
            Debug.LogError("Shader is currently missing!", this);
            isSupported = false;
            enabled = false;
            return null;
        }

        if(s.isSupported && mat != null && mat.shader == s) {
            return mat;
        }

        if(!s.isSupported) {
            NotSupported();
            return null;
        }
        else {
            mat = new Material(s);
            mat.hideFlags = HideFlags.DontSave;
            return mat;
        }
    }

    public Material CreateMaterial(Shader s, Material mat) {
        if(s == null) {
            Debug.LogError("Shader is currently missing!", this);
            isSupported = false;
            return null;
        }

        if(mat != null && mat.shader == s && s.isSupported) {
            return mat;
        }

        if(!s.isSupported) {
            NotSupported();
            return null;
        }
        else {
            mat = new Material(s);
            mat.hideFlags = HideFlags.DontSave;
            return mat;
        }
    }

    void OnEnable() {
        isSupported = true;
    }

    public bool CheckSupport() {
        return CheckSupport(false);
    }

    public virtual bool CheckResources() {
        Debug.Log("Override me please!", this);
        return isSupported;
    }

    public bool CheckSupport(bool needDepth) {
        isSupported = true;
        supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
        supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;

        if(!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures) {
            NotSupported();
            return false;
        }

        if(needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth)) {
            NotSupported();
            return false;
        }

        if(needDepth)
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

        return true;
    }

    public bool CheckSupport(bool needDepth, bool needHdr) {
        if(!CheckSupport(needDepth)) {
            return false;
        }

        if(needHdr && !supportHDRTextures) {
            NotSupported();
            return false;
        }

        return true;
    }

    public bool Dx11Support() {
        return supportDX11;
    }

    public void NotSupported() {
        Debug.LogError("This image effect is not supported!", this);
        enabled = false;
        isSupported = false;
        return;
    }

    public void DrawBorder(RenderTexture dest, Material material) {
        float x1;
        float x2;
        float y1;
        float y2;

        RenderTexture.active = dest;
        bool invertY = true;
        // Set up the simple Matrix
        GL.PushMatrix();
        GL.LoadOrtho();

        for(int i = 0; i < material.passCount; i++) {
            material.SetPass(i);

            float y1_;
            float y2_;
            if(invertY) {
                y1_ = 1.0f;
                y2_ = 0.0f;
            }
            else {
                y1_ = 0.0f;
                y2_ = 1.0f;
            }

            // left	        
            x1 = 0.0f;
            x2 = 0.0f + 1.0f / (dest.width * 1.0f);
            y1 = 0.0f;
            y2 = 1.0f;
            GL.Begin(GL.QUADS);

            GL.TexCoord2(0.0f, y1_);
            GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_);
            GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_);
            GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_);
            GL.Vertex3(x1, y2, 0.1f);

            // right
            x1 = 1.0f - 1.0f / (dest.width);
            x2 = 1.0f;
            y1 = 0.0f;
            y2 = 1.0f;

            GL.TexCoord2(0.0f, y1_);
            GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_);
            GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_);
            GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_);
            GL.Vertex3(x1, y2, 0.1f);

            // top
            x1 = 0.0f;
            x2 = 1.0f;
            y1 = 0.0f;
            y2 = 0.0f + 1.0f / (dest.height);

            GL.TexCoord2(0.0f, y1_);
            GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_);
            GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_);
            GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_);
            GL.Vertex3(x1, y2, 0.1f);

            // bottom
            x1 = 0.0f;
            x2 = 1.0f;
            y1 = 1.0f - 1.0f / (dest.height);
            y2 = 1.0f;

            GL.TexCoord2(0.0f, y1_);
            GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_);
            GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_);
            GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_);
            GL.Vertex3(x1, y2, 0.1f);

            GL.End();
        }

        GL.PopMatrix();
    }
}