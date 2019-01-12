using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class RenderTextureScope : MonoBehaviour
{
    public Vector2 defaultSize = new Vector2(256f, 256f);
    public bool disableCamera = false;
    public bool lowerQuality = false;
    public Vector2 lowQuality = new Vector2(128f, 128f);
    public float aspectRatio = 1.25f;
    public MeshRenderer scopeRenderer;

    private RenderTexture renderTex;
    private AimController ac;

    void Start()
    {
        if (GeneralVariables.player == null)
        {
            Cleanup();
            return;
        }

        GetComponent<Camera>().enabled = true;
        renderTex = new RenderTexture(Mathf.RoundToInt(defaultSize.x), Mathf.RoundToInt(defaultSize.y), 16);
        GetComponent<Camera>().aspect = aspectRatio;
        GetComponent<Camera>().targetTexture = renderTex;
        scopeRenderer.material.mainTexture = GetComponent<Camera>().targetTexture;
        ac = GeneralVariables.playerRef.ac;
    }

    void Update()
    {
        if (transform.root.gameObject != GeneralVariables.player)
        {
            Cleanup();
            return;
        }

        Vector2 quality = ((ac.isAiming) ? defaultSize : lowQuality);
        if (lowerQuality && renderTex.width != Mathf.RoundToInt(quality.x))
        {
            if (renderTex != null)
            {
                Destroy(renderTex);
                renderTex = null;
            }

            renderTex = new RenderTexture(Mathf.RoundToInt(quality.x), Mathf.RoundToInt(quality.y), 16);
            GetComponent<Camera>().aspect = aspectRatio;
            GetComponent<Camera>().targetTexture = renderTex;
            scopeRenderer.material.mainTexture = GetComponent<Camera>().targetTexture;
        }

        if (disableCamera)
        {
            GetComponent<Camera>().enabled = ac.isAiming;
        }
    }

    private void Cleanup()
    {
        if (renderTex != null)
        {
            Destroy(renderTex);
        }

        Destroy(gameObject);
    }
}