using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class WetSurface : MonoBehaviour {
    public bool m_DisablePixelLights = true;
    public bool m_DisableShadows = false;
    public int m_TextureSize = 256;
    public float m_ClipPlaneOffset = 0.07f;
    public LayerMask m_ReflectLayers = -1;

    private Hashtable m_ReflectionCameras = new Hashtable();

    private RenderTexture m_ReflectionTexture = null;
    private int m_OldReflectionTextureSize = 0;

    private float textureDownsample = 1f;
    private float oldTextureDownsample = 1f;

    private bool pixelLightCached = true;

    private static bool s_InsideWater = false;
    private int lastRenderObjTime = 0;

    void Awake() {
        pixelLightCached = m_DisablePixelLights;
    }

    public void OnWillRenderObject() {
        if(!enabled || !GetComponent<Renderer>() || !GetComponent<Renderer>().sharedMaterial || !GetComponent<Renderer>().enabled)
            return;

        Camera cam = Camera.current;
        if(!cam)
            return;

        if(s_InsideWater)
            return;
        s_InsideWater = true;

        FindHardwareWaterSupport();

        Camera reflectionCamera;
        CreateWaterObjects(cam, out reflectionCamera);

        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        int oldPixelLightCount = QualitySettings.pixelLightCount;
        float oldShadowDist = QualitySettings.shadowDistance;
        if(m_DisablePixelLights)
            QualitySettings.pixelLightCount = 0;
        if(m_DisableShadows)
            QualitySettings.shadowDistance = 0f;

        UpdateCameraModes(cam, reflectionCamera);

        float d = -Vector3.Dot(normal, pos) - m_ClipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        Vector3 oldpos = cam.transform.position;
        Vector3 newpos = reflection.MultiplyPoint(oldpos);
        reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
        Matrix4x4 projection = cam.projectionMatrix;
        CalculateObliqueMatrix(ref projection, clipPlane);
        reflectionCamera.projectionMatrix = projection;
        reflectionCamera.renderingPath = RenderingPath.UsePlayerSettings;

        reflectionCamera.cullingMask = ~(1 << 4) & m_ReflectLayers.value; // never render water layer
        reflectionCamera.targetTexture = m_ReflectionTexture;
        GL.invertCulling = true;
        reflectionCamera.transform.position = newpos;
        Vector3 euler = cam.transform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
        reflectionCamera.Render();
        reflectionCamera.transform.position = oldpos;
        GL.invertCulling = false;
        GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex", m_ReflectionTexture);

        if(m_DisablePixelLights)
            QualitySettings.pixelLightCount = oldPixelLightCount;
        if(m_DisableShadows)
            QualitySettings.shadowDistance = oldShadowDist;

        s_InsideWater = false;
    }


    void OnDisable() {
        if(m_ReflectionTexture) {
            DestroyImmediate(m_ReflectionTexture);
            m_ReflectionTexture = null;
        }
        foreach(DictionaryEntry kvp in m_ReflectionCameras)
            DestroyImmediate(((Camera)kvp.Value).gameObject);
        m_ReflectionCameras.Clear();
    }

    void Update() {
        if(!GetComponent<Renderer>())
            return;
        Material mat = GetComponent<Renderer>().sharedMaterial;
        if(!mat)
            return;

        m_TextureSize = Mathf.Clamp(m_TextureSize, 16, 1024);

        m_DisablePixelLights = pixelLightCached;

        GameSettings gSet = GameSettings.settingsController;
        if(gSet != null) {
            if(gSet.waterQuality == "Very High") {
                textureDownsample = 1f;
            }
            else if(gSet.waterQuality == "High") {
                textureDownsample = 0.8f;
            }
            else if(gSet.waterQuality == "Medium") {
                textureDownsample = 0.6f;
            }
            else {
                textureDownsample = 0.5f;
                m_DisablePixelLights = true;
            }
        }

        Vector4 waveSpeed = mat.GetVector("_WaveSpeed");
        float waveScale = mat.GetFloat("_WaveScale");
        Vector4 waveScale4 = new Vector4(waveScale, waveScale, waveScale * 0.4f, waveScale * 0.45f);

        Vector4 offsetClamped = Vector4.Scale(waveSpeed, waveScale4) * Time.time * 0.05f;

        mat.SetVector("_WaveOffset", offsetClamped);
        mat.SetVector("_WaveScale4", waveScale4);

        Vector3 waterSize = GetComponent<Renderer>().bounds.size;
        Vector3 scale = new Vector3(waterSize.x * waveScale4.x, waterSize.z * waveScale4.y, 1);
        Matrix4x4 scrollMatrix = Matrix4x4.TRS(new Vector3(offsetClamped.x, offsetClamped.y, 0), Quaternion.identity, scale);
        mat.SetMatrix("_WaveMatrix", scrollMatrix);

        scale = new Vector3(waterSize.x * waveScale4.z, waterSize.z * waveScale4.w, 1);
        scrollMatrix = Matrix4x4.TRS(new Vector3(offsetClamped.z, offsetClamped.w, 0), Quaternion.identity, scale);
        mat.SetMatrix("_WaveMatrix2", scrollMatrix);
    }

    private void UpdateCameraModes(Camera src, Camera dest) {
        if(dest == null)
            return;
        // set water camera to clear the same way as current camera
        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;

        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
    }

    private void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera) {
        reflectionCamera = null;

        if(!m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize || oldTextureDownsample != textureDownsample) {
            if(m_ReflectionTexture)
                DestroyImmediate(m_ReflectionTexture);
            m_ReflectionTexture = new RenderTexture(Mathf.RoundToInt(m_TextureSize * textureDownsample), Mathf.RoundToInt(m_TextureSize * textureDownsample), 16);
            m_ReflectionTexture.hideFlags = HideFlags.HideAndDontSave;
            m_OldReflectionTextureSize = m_TextureSize;
            oldTextureDownsample = textureDownsample;
        }

        reflectionCamera = m_ReflectionCameras[currentCamera] as Camera;
        if(!reflectionCamera) {
            GameObject go = new GameObject("R" + Random.value, typeof(Camera));
            reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.enabled = false;
            reflectionCamera.transform.position = transform.position;
            reflectionCamera.transform.rotation = transform.rotation;
            reflectionCamera.gameObject.AddComponent<FlareLayer>();
            go.hideFlags = HideFlags.HideAndDontSave;
            m_ReflectionCameras[currentCamera] = reflectionCamera;
        }
    }

    private void FindHardwareWaterSupport() {
        if(!SystemInfo.supportsRenderTextures || !GetComponent<Renderer>() || !GetComponent<Renderer>().sharedMaterial)
            this.enabled = false;
    }

    private static float sgn(float a) {
        if(a > 0.0f) return 1.0f;
        if(a < 0.0f) return -1.0f;
        return 0.0f;
    }

    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign) {
        Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane) {
        Vector4 q = projection.inverse * new Vector4(
            sgn(clipPlane.x),
            sgn(clipPlane.y),
            1.0f,
            1.0f
        );
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
        // third row = clip plane - fourth row
        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];
    }

    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane) {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }
}
