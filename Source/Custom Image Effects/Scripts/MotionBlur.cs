// Kino/Motion - Motion blur effect
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class MotionBlur : MonoBehaviour
{
    private const float MAX_BLUR_RADIUS = 3f;

    public Shader motionBlurShader;
    public bool adjustWithFrameRate = true; // Less motion blur at lower frame-rate.

    [Range(0f, 1f)]
    public float velocityScale = 1f;
    [Range(1, 8)]
    public int sampleCount = 4;

    private Camera cam;
    private Material mat;
    private RenderTextureFormat vectorFormat;
    private RenderTextureFormat packedFormat;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        if (motionBlurShader != null && motionBlurShader.isSupported && CheckTextureFormatSupport())
        {
            mat = new Material(motionBlurShader);
            mat.hideFlags = HideFlags.HideAndDontSave;

            cam.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
        }
        else
        {
            enabled = false;
        }
    }

    private void OnDisable()
    {
        if (mat != null)
        {
            DestroyImmediate(mat);
            mat = null;

            cam.depthTextureMode &= ~DepthTextureMode.MotionVectors;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (mat == null || velocityScale <= 0f)
        {
            Graphics.Blit(source, destination);
            enabled = false;
            return;
        }

        int maxBlurPixels = (int)((source.height * MAX_BLUR_RADIUS) / 100f);
        int tileSize = ((maxBlurPixels - 1) / 8 + 1) * 8;

        if (adjustWithFrameRate)
            mat.SetFloat("_VelocityScale", velocityScale * Mathf.Clamp((1f / Time.deltaTime) / 60f, 0f, 5f));
        else
            mat.SetFloat("_VelocityScale", velocityScale);

        mat.SetFloat("_MaxBlurRadius", maxBlurPixels);
        mat.SetFloat("_RcpMaxBlurRadius", 1f / maxBlurPixels);
        mat.SetFloat("_LoopCount", Mathf.Clamp(sampleCount, 1, 8));

        RenderTexture velBuffer = GetTemporaryRT(source, 1, packedFormat);
        Graphics.Blit(null, velBuffer, mat, 0);

        RenderTexture tile2 = GetTemporaryRT(source, 2, vectorFormat);
        Graphics.Blit(velBuffer, tile2, mat, 1);

        RenderTexture tile4 = GetTemporaryRT(source, 4, vectorFormat);
        Graphics.Blit(tile2, tile4, mat, 2);
        RenderTexture.ReleaseTemporary(tile2);

        RenderTexture tile8 = GetTemporaryRT(source, 8, vectorFormat);
        Graphics.Blit(tile4, tile8, mat, 2);
        RenderTexture.ReleaseTemporary(tile4);

        mat.SetFloat("_TileMaxOffs", ((tileSize / 8f) - 1f) * -0.5f);
        mat.SetInt("_TileMaxLoop", tileSize / 8);

        RenderTexture tile = GetTemporaryRT(source, tileSize, vectorFormat);
        Graphics.Blit(tile8, tile, mat, 3);
        RenderTexture.ReleaseTemporary(tile8);

        RenderTexture neighborMax = GetTemporaryRT(source, tileSize, vectorFormat);
        Graphics.Blit(tile, neighborMax, mat, 4);
        RenderTexture.ReleaseTemporary(tile);

        mat.SetTexture("_NeighborMaxTex", neighborMax);
        mat.SetTexture("_VelocityTex", velBuffer);
        Graphics.Blit(source, destination, mat, 5);

        RenderTexture.ReleaseTemporary(velBuffer);
        RenderTexture.ReleaseTemporary(neighborMax);
    }

    private bool CheckTextureFormatSupport()
    {
        vectorFormat = RenderTextureFormat.RGHalf;
        packedFormat = RenderTextureFormat.ARGB2101010;

        if (!SystemInfo.SupportsRenderTextureFormat(vectorFormat))
            return false;

        if (!SystemInfo.SupportsRenderTextureFormat(packedFormat))
            packedFormat = RenderTextureFormat.ARGB32;

        return true;
    }

    private RenderTexture GetTemporaryRT(RenderTexture source, int divider, RenderTextureFormat format)
    {
        int w = source.width / divider;
        int h = source.height / divider;
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, format, RenderTextureReadWrite.Linear);
        rt.filterMode = FilterMode.Point;
        return rt;
    }
}