using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;

public enum CrosshairStyle { Disabled, Static, Dynamic }
public class GameSettings : MonoBehaviour
{
    public static GameSettings cachedSettingsPrefab;
    public static GameSettings settingsController
    {
        get
        {
            if (Application.isPlaying && instance == null)
            {
                GameSettings currentSC = (GameSettings)FindObjectOfType(typeof(GameSettings));

                if (currentSC == null)
                {
                    instance = (GameSettings)Instantiate((cachedSettingsPrefab != null) ? cachedSettingsPrefab : Resources.Load("System/[Game Settings]", typeof(GameSettings)));
                }
                else
                {
                    instance = currentSC;
                }
            }

            return instance;
        }
    }

    private static GameSettings instance;

    public float sensitivityX = 1f;
    public float sensitivityY = 1f;
    public float mouseSmoothing = 0f;
    public int FOV = 65;
    public string crosshairStyle = "Dynamic";
    public string aimToggle = "Hold (Press)";
    public int speakerMode = 2;

    [HideInInspector] public CrosshairStyle crossStyle;

    public float brightness = 1f;
    public int shadowDistance = 110;

    public string waterQuality = "Very High";
    public int textureQuality = 0;
    public string aniso = "Enable";
    public int terrainMeshDetail = 5;
    public int vegetationDistance = 150;
    public float vegetationDensity = 1;
    public int terrainTreeDrawDistance = 1000;
    public int terrainMaxTrees = 250;
    public float soundVolume = 1f;
    public string vsync = "Disabled";
    public float ragdollDestroyTimer = 15f;

    public int bloom = 1;
    public int sunShafts = 1;
    public int motionBlur = 0;
    public int SSAO = 1;
    public int colorCorrection = 1;
    public int glareEffect = 1;
    public int antiAliasing = 1;
    public int wDepthOfField = 0;
    public int enableHUD = 1;
    public int showFPS = 0;

    public int shadowQuality = 4;
    public int screenWidth;
    public int screenHeight;
    public string fullScreen = "Fullscreen";
    public int targetFrameRate = 60;

    private Terrain[] ter;

    private bool setFS;
    private bool deferredAA;
    private int updateGraphicsFrameCount;
    private int cameraCount = 0;
    private float lastPP = 0f;

    private void Awake()
    {
        setFS = false;
        GetSettings();
    }

    private void Start()
    {
        ter = Terrain.activeTerrains;

        updateGraphicsFrameCount = -1;
        UpdateGraphics();
    }

    private void Update()
    {
        if (!Application.genuine)
        {
            Debug.Log("This application is not genuine anymore.");
        }

        if (!Application.isEditor && Input.GetKeyDown(KeyCode.F11))
        {
            string rootDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            if (!Directory.Exists(Application.dataPath + "/Screenshots"))
            {
                Directory.CreateDirectory(Application.dataPath + "/Screenshots");
            }

            ScreenCapture.CaptureScreenshot(Application.dataPath + "/Screenshots/" + DateTimeFormatInfo.CurrentInfo.GetMonthName(DateTime.Now.Month) + " " + DateTime.Now.Day.ToString() + ", " + DateTime.Now.Year.ToString() + " " + DateTime.Now.ToLongTimeString() + ".png");
        }

        if (Time.unscaledTime - lastPP >= 2f || Camera.allCameras.Length != cameraCount)
        {
            UpdatePostProcess();
        }
    }

    public void Initialize()
    {
        //Initialization.
    }

    public void UpdateGraphics()
    {
        if (Time.frameCount == updateGraphicsFrameCount)
        {
            return;
        }

        StartCoroutine(DelayApply());
    }

    private IEnumerator DelayApply()
    {
        updateGraphicsFrameCount = Time.frameCount;
        yield return null; //Wait for one frame before applying.

        QualitySettings.SetQualityLevel(shadowQuality);

        if (setFS)
        {
            Screen.fullScreen = (fullScreen == "Fullscreen");

            if (Screen.currentResolution.width != screenWidth || Screen.currentResolution.height != screenHeight || Screen.fullScreen != (fullScreen == "Fullscreen"))
            {
                Screen.SetResolution(screenWidth, screenHeight, fullScreen == "Fullscreen");
            }
        }

        setFS = true;

        QualitySettings.masterTextureLimit = textureQuality;
        QualitySettings.vSyncCount = (vsync == "Enabled") ? 1 : 0;

        if (crosshairStyle == "Dynamic")
        {
            crossStyle = CrosshairStyle.Dynamic;
        }
        else if (crosshairStyle == "Static")
        {
            crossStyle = CrosshairStyle.Static;
        }
        else if (crosshairStyle == "Disabled")
        {
            crossStyle = CrosshairStyle.Disabled;
        }

        if (aniso == "Disable")
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        else if (aniso == "Enable")
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;

        QualitySettings.shadowDistance = shadowDistance;
        AudioListener.volume = soundVolume;

        foreach (Terrain terrain in ter)
        {
            terrain.heightmapPixelError = terrainMeshDetail;
            terrain.detailObjectDistance = vegetationDistance;
            terrain.detailObjectDensity = vegetationDensity;
            terrain.treeBillboardDistance = terrainTreeDrawDistance;
            terrain.treeMaximumFullLODCount = terrainMaxTrees;
        }

        if (GeneralVariables.uicIsActive)
        {
            GeneralVariables.uiController.UpdateHUD((enableHUD == 1) ? true : false);
        }

        UpdatePostProcess();
    }

    private void UpdatePostProcess()
    {
        foreach (Camera cam in Camera.allCameras)
        {
            SMAA defAntiAlias = cam.GetComponent<SMAA>();
            Bloom b = cam.GetComponent<Bloom>();
            BloomAndFlares baf = cam.GetComponent<BloomAndFlares>();
            ContrastEnhance ce = cam.GetComponent<ContrastEnhance>();
            MotionBlur mb = cam.GetComponent<MotionBlur>();
            WeaponDepthOfField wdof = cam.GetComponent<WeaponDepthOfField>();
            SunShafts ss = cam.GetComponent<SunShafts>();
            AmplifyOcclusionEffect ao = cam.GetComponent<AmplifyOcclusionEffect>();
            ColorCorrection ccc = cam.GetComponent<ColorCorrection>();

            if (defAntiAlias)
                defAntiAlias.enabled = (antiAliasing == 1);

            if (wdof)
                wdof.enabled = (wDepthOfField == 1);

            bool bloomBool = (bloom == 1);
            if (baf)
                baf.enabled = bloomBool;
            if (b)
                b.enabled = bloomBool;

            if (ss)
                ss.enabled = (sunShafts == 1);
            if (mb)
                mb.enabled = (motionBlur == 1);

            bool ssaoBool = (SSAO == 1);
            if (ao)
                ao.enabled = ssaoBool;

            bool ccBool = (colorCorrection == 1);
            if (ccc)
                ccc.enabled = ccBool;
            if (ce)
                ce.enabled = ccBool;
        }

        lastPP = Time.unscaledTime;
        cameraCount = Camera.allCameras.Length;
    }

    private void GetSettings()
    {
        sensitivityX = PlayerPrefs.GetFloat("Mouse Sensitivity X", 1.5f);
        sensitivityY = PlayerPrefs.GetFloat("Mouse Sensitivity Y", 1.5f);
        mouseSmoothing = PlayerPrefs.GetFloat("Mouse Smoothing", 0f);
        FOV = PlayerPrefs.GetInt("FOV", 70);
        speakerMode = PlayerPrefs.GetInt("Speaker Mode", 2);
        brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        enableHUD = PlayerPrefs.GetInt("EnableHUD", 1);

        waterQuality = PlayerPrefs.GetString("Water Quality", "Very High");
        textureQuality = PlayerPrefs.GetInt("Texture Quality", 0);
        aniso = PlayerPrefs.GetString("Anisotropic", "Enable");
        shadowDistance = PlayerPrefs.GetInt("Shadow Distance", 110);
        shadowQuality = PlayerPrefs.GetInt("Shadow Quality", 5);
        terrainMeshDetail = PlayerPrefs.GetInt("Terrain Mesh Detail", 5);
        vegetationDistance = PlayerPrefs.GetInt("Vegetation Distance", 150);
        vegetationDensity = PlayerPrefs.GetFloat("Vegetation Density", 1f);
        terrainTreeDrawDistance = PlayerPrefs.GetInt("Tree Draw Distance", 1000);
        terrainMaxTrees = PlayerPrefs.GetInt("Tree Mesh Limit", 250);
        vsync = PlayerPrefs.GetString("VSync", "Disabled");
        bloom = PlayerPrefs.GetInt("Bloom", 1);
        sunShafts = PlayerPrefs.GetInt("Sun Shafts", 1);
        motionBlur = PlayerPrefs.GetInt("Motion Blur", 0);
        SSAO = PlayerPrefs.GetInt("SSAO", 1);
        colorCorrection = PlayerPrefs.GetInt("Color Correction", 1);
        glareEffect = PlayerPrefs.GetInt("Glare Effect", 1);
        antiAliasing = PlayerPrefs.GetInt("AntiAliasing", 1);
        wDepthOfField = PlayerPrefs.GetInt("Weapon DoF", 0);
        screenWidth = PlayerPrefs.GetInt("ScreenWidth", Screen.currentResolution.width);
        screenHeight = PlayerPrefs.GetInt("ScreenHeight", Screen.currentResolution.height);
        fullScreen = PlayerPrefs.GetString("DisplayMode", "Fullscreen");
        showFPS = PlayerPrefs.GetInt("ShowFPS", 0);
        aimToggle = PlayerPrefs.GetString("Aim Toggle", "Hold (Press)");
        crosshairStyle = PlayerPrefs.GetString("Crosshair Style", "Dynamic");

        soundVolume = PlayerPrefs.GetFloat("Sound Volume", 1f);
        AudioListener.volume = soundVolume;

        for (int i = 1; i < 7; i++)
        {
            AudioSpeakerMode asm = (AudioSpeakerMode)i;
            if (speakerMode == i && AudioSettings.speakerMode != asm)
            {
                AudioSettings.speakerMode = asm;
            }
        }

        targetFrameRate = PlayerPrefs.GetInt("TargetFPS", 60);
        Application.targetFrameRate = Mathf.Clamp(targetFrameRate, 15, 300);
        Shader.globalMaximumLOD = 800;
    }

    private void OnApplicationQuit()
    {
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            Application.CancelQuit();
            return;
        }
    }

    public static void UpdateAllReflections()
    {
        MeshRenderer[] allRenderers = (MeshRenderer[])GameObject.FindObjectsOfType(typeof(MeshRenderer));

        foreach (MeshRenderer mr in allRenderers)
        {
            foreach (Material mat in mr.materials)
            {
                if (!mat.HasProperty("_ReflectColor"))
                {
                    continue;
                }

                Color oldColor = mat.GetColor("_ReflectColor");
                mat.SetColor("_ReflectColor", oldColor * GeneralVariables.lightingFactor);
            }
        }
    }
}