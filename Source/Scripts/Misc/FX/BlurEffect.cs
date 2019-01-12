using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BlurEffect : MonoBehaviour
{
    public int iterations = 3;
    public float blurSpread = 0.6f;
    public float qualityReduction = 1f;
    public float screenResFactor = 1f;
    public float pauseEffect = 0f;
    public float leaderboardEffect = 0f;
    public Shader blurShader;

    [HideInInspector]
    public float joiningServerBlur;

    private float xMod;
    private float yMod;
    private float blurDefPause;
    private float blurEffects;

    private Material mat;
    private Material material
    {
        get
        {
            if (mat == null)
            {
                mat = new Material(blurShader);
                mat.hideFlags = HideFlags.DontSave;
            }
            return mat;
        }
    }

    void OnDisable()
    {
        if (mat)
        {
            DestroyImmediate(mat);
        }
    }

    void Start()
    {
        if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
        {
            enabled = false;
            return;
        }

        if (!blurShader || !material.shader.isSupported)
        {
            enabled = false;
            return;
        }
    }

    private void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
    {
        float offset = iteration * (blurSpread + joiningServerBlur);
        Graphics.BlitMultiTap(source, dest, material,
            new Vector2(-offset * xMod, -offset * yMod),
            new Vector2(-offset * xMod, offset * yMod),
            new Vector2(offset * xMod, offset * yMod),
            new Vector2(offset * xMod, -offset * yMod)
        );
    }

    private void DownSample4x(RenderTexture source, RenderTexture dest, float blurFactor)
    {
        Graphics.BlitMultiTap(source, dest, material,
            new Vector2(-blurFactor, -blurFactor),
            new Vector2(-blurFactor, blurFactor),
            new Vector2(blurFactor, blurFactor),
            new Vector2(blurFactor, -blurFactor)
        );
    }

    void Update()
    {
        if (Application.isPlaying && (pauseEffect > 0f || leaderboardEffect > 0f))
        {
            float pause = (GameManager.isPaused) ? pauseEffect : 0f;
            float leader = (GeneralVariables.uicIsActive) ? leaderboardEffect * GeneralVariables.uiController.mpGUI.leaderboard.alpha : 0f;
            blurEffects = Mathf.MoveTowards(blurEffects, pause + leader, Time.unscaledDeltaTime * 10f);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        float blurFactor = Mathf.Clamp(blurSpread + blurEffects + joiningServerBlur, 0f, 2f);
        if (blurFactor <= 0.001f)
        {
            Graphics.Blit(source, destination);
            return;
        }

        screenResFactor = Mathf.Clamp(screenResFactor, 0.01f, 100f);
        xMod = (float)Screen.width / (640f * screenResFactor);
        yMod = (float)Screen.height / (480f * screenResFactor);

        qualityReduction = Mathf.Clamp(qualityReduction, 0f, 7f);
        iterations = Mathf.Clamp(iterations, 0, 10);

        int rtW = (int)((source.width * 1f) / (1f + (qualityReduction * blurFactor)));
        int rtH = (int)((source.height * 1f) / (1f + (qualityReduction * blurFactor)));
        RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);

        DownSample4x(source, buffer, blurFactor);

        for (int i = 0; i < iterations; i++)
        {
            RenderTexture buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
            FourTapCone(buffer, buffer2, i);
            RenderTexture.ReleaseTemporary(buffer);
            buffer = buffer2;
        }

        Graphics.Blit(buffer, destination);
        RenderTexture.ReleaseTemporary(buffer);
    }
}