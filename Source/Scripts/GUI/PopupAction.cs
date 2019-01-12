using UnityEngine;

public class PopupAction : MonoBehaviour
{
    public bool fullscreenResolution;
    public bool fullscreenMod;
    public bool textureQuality;
    public bool anisoQuality;
    public bool waterQuality;
    public bool terrainQuality;
    public bool lightingMethod;
    public bool shadowQuality;
    public bool vsyncCount;
    public bool crosshairStyle;
    public bool fpsCounter;
    public bool aimingMethod;
    public bool speakerMode;

    public UILabel restartNote;

    private UIPopupList popupList;
    private Resolution[] resolutions;
    private string[] resolutionMenuText;
    private string selectedWidth;
    private string selectedHeight;

    private bool runOnce;

    void Start()
    {
        popupList = GetComponent<UIPopupList>();
        popupList.items.Clear();

        string[] settings = new string[0];
        if (fullscreenResolution)
        {
            resolutions = Screen.resolutions;
            resolutionMenuText = new string[resolutions.Length];

            for (int i = 0; i < resolutions.Length; i++)
            {
                resolutionMenuText[i] = resolutions[i].width.ToString() + "x" + resolutions[i].height.ToString();
                popupList.items.Add(resolutionMenuText[i]);
            }
        }
        else if (fullscreenMod)
        {
            popupList.items.Add("Windowed");
            popupList.items.Add("Fullscreen");
        }
        else if (vsyncCount)
        {
            settings = new string[2] { "Disabled", "Enabled" };
        }
        else if (textureQuality)
        {
            settings = new string[4] { "Low", "Medium", "High", "Maximum" };
        }
        else if (anisoQuality)
        {
            settings = new string[2] { "Disabled", "Enabled" };
        }
        else if (waterQuality)
        {
            settings = new string[4] { "Low", "Medium", "High", "Very High" };
        }
        else if (terrainQuality)
        {
            settings = new string[5] { "Low", "Medium", "High", "Very High", "Ultra" };
        }
        else if (lightingMethod)
        {
            settings = new string[2] { "Forward", "Deferred" };
        }
        else if (shadowQuality)
        {
            settings = new string[5] { "Low", "Medium", "High", "Very High", "Ultra" };
        }
        else if (crosshairStyle)
        {
            settings = new string[3] { "Disabled", "Static", "Dynamic" };
        }
        else if (fpsCounter)
        {
            settings = new string[2] { "False", "True" };
        }
        else if (aimingMethod)
        {
            settings = new string[2] { "Hold (Press)", "Toggle (Click)" };
        }
        else if (speakerMode)
        {
            settings = new string[5] { "Stereo", "Quad", "Surround", "5.1 Surround", "7.1 Surround" };
        }

        if (!fullscreenResolution)
        {
            for (int i = 0; i < settings.Length; i++)
            {
                popupList.items.Add(settings[i]);
            }
        }

        InitializeValues();
    }

    public void InitializeValues()
    {
        if (fullscreenResolution)
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutionMenuText[i] == (Screen.width.ToString() + "x" + Screen.height.ToString()))
                {
                    popupList.value = resolutionMenuText[i];
                    break;
                }
            }
        }
        else if (fullscreenMod)
        {
            popupList.value = GameSettings.settingsController.fullScreen;
        }
        else if (vsyncCount)
        {
            popupList.value = GameSettings.settingsController.vsync;
        }
        else if (textureQuality)
        {
            popupList.value = popupList.items[popupList.items.Count - 1 - GameSettings.settingsController.textureQuality];
        }
        else if (anisoQuality)
        {
            if (GameSettings.settingsController.aniso == "Disable")
            {
                popupList.value = popupList.items[0];
            }
            else if (GameSettings.settingsController.aniso == "Enable")
            {
                popupList.value = popupList.items[1];
            }
        }
        else if (waterQuality)
        {
            popupList.value = GameSettings.settingsController.waterQuality;
        }
        else if (terrainQuality)
        {
            if (GameSettings.settingsController.terrainMeshDetail == 2)
            {
                popupList.value = popupList.items[4];
            }
            else if (GameSettings.settingsController.terrainMeshDetail == 5)
            {
                popupList.value = popupList.items[3];
            }
            else if (GameSettings.settingsController.terrainMeshDetail == 12)
            {
                popupList.value = popupList.items[2];
            }
            else if (GameSettings.settingsController.terrainMeshDetail == 25)
            {
                popupList.value = popupList.items[1];
            }
            else if (GameSettings.settingsController.terrainMeshDetail == 45)
            {
                popupList.value = popupList.items[0];
            }
        }
        else if (shadowQuality)
        {
            popupList.value = popupList.items[GameSettings.settingsController.shadowQuality];
        }
        else if (crosshairStyle)
        {
            popupList.value = GameSettings.settingsController.crosshairStyle;
        }
        else if (fpsCounter)
        {
            popupList.value = ((GameSettings.settingsController.showFPS == 1) ? "True" : "False");
        }
        else if (aimingMethod)
        {
            popupList.value = GameSettings.settingsController.aimToggle;
        }
        else if (speakerMode)
        {
            popupList.value = popupList.items[GameSettings.settingsController.speakerMode - 2];
        }
    }

    void Update()
    {
        if (fullscreenResolution)
        {
            if (!string.IsNullOrEmpty(popupList.value))
            {
                string[] curDimensions = popupList.value.Split(new string[] { "x" }, System.StringSplitOptions.None);

                if (curDimensions.Length == 2)
                {
                    selectedWidth = curDimensions[0];
                    selectedHeight = curDimensions[1];
                }
            }
        }
        else if (speakerMode && restartNote != null)
        {
            restartNote.enabled = (GameSettings.settingsController.speakerMode != (int)AudioSettings.speakerMode);
        }
    }

    public void ApplyChanges()
    {
        if (fullscreenResolution)
        {
            int w = 0;
            int h = 0;
            if (int.TryParse(selectedWidth, out w) && int.TryParse(selectedHeight, out h))
            {
                if ((selectedWidth + "x" + selectedHeight) == (Screen.width.ToString() + "x" + Screen.height.ToString()))
                {
                    return;
                }

                PlayerPrefs.SetInt("ScreenWidth", w);
                PlayerPrefs.SetInt("ScreenHeight", h);

                GameSettings.settingsController.screenWidth = w;
                GameSettings.settingsController.screenHeight = h;
            }
        }
        else if (fullscreenMod)
        {
            GameSettings.settingsController.fullScreen = popupList.value;
            PlayerPrefs.SetString("DisplayMode", GameSettings.settingsController.fullScreen);
        }
        else if (textureQuality)
        {
            int curTextureNum = 0;
            for (int i = 0; i < popupList.items.Count; i++)
            {
                if (popupList.items[i] == popupList.value)
                {
                    curTextureNum = (popupList.items.Count - 1) - i;
                }
            }

            GameSettings.settingsController.textureQuality = curTextureNum;
            PlayerPrefs.SetInt("Texture Quality", curTextureNum);
        }
        else if (vsyncCount)
        {
            GameSettings.settingsController.vsync = popupList.value;
            PlayerPrefs.SetString("VSync", GameSettings.settingsController.vsync);
        }
        else if (anisoQuality)
        {
            if (popupList.value == popupList.items[0])
            {
                GameSettings.settingsController.aniso = "Disable";
                PlayerPrefs.SetString("Anisotropic", "Disable");
            }
            if (popupList.value == popupList.items[1])
            {
                GameSettings.settingsController.aniso = "Enable";
                PlayerPrefs.SetString("Anisotropic", "Enable");
            }
        }
        else if (waterQuality)
        {
            if ((Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) && popupList.value == popupList.items[2])
            {
                popupList.value = popupList.items[1];
            }

            GameSettings.settingsController.waterQuality = popupList.value;
            PlayerPrefs.SetString("Water Quality", popupList.value);
        }
        else if (terrainQuality)
        {
            if (popupList.value == popupList.items[0])
            {
                GameSettings.settingsController.terrainMeshDetail = 45;
                PlayerPrefs.SetInt("Terrain Mesh Detail", 45);
            }
            else if (popupList.value == popupList.items[1])
            {
                GameSettings.settingsController.terrainMeshDetail = 25;
                PlayerPrefs.SetInt("Terrain Mesh Detail", 25);
            }
            else if (popupList.value == popupList.items[2])
            {
                GameSettings.settingsController.terrainMeshDetail = 12;
                PlayerPrefs.SetInt("Terrain Mesh Detail", 12);
            }
            else if (popupList.value == popupList.items[3])
            {
                GameSettings.settingsController.terrainMeshDetail = 5;
                PlayerPrefs.SetInt("Terrain Mesh Detail", 5);
            }
            else
            {
                GameSettings.settingsController.terrainMeshDetail = 2;
                PlayerPrefs.SetInt("Terrain Mesh Detail", 2);
            }
        }
        else if (shadowQuality)
        {
            for (int i = 0; i < popupList.items.Count; i++)
            {
                if (popupList.value == popupList.items[i])
                {
                    GameSettings.settingsController.shadowQuality = i;
                    PlayerPrefs.SetInt("Shadow Quality", i);
                    break;
                }
            }
        }
        else if (crosshairStyle)
        {
            GameSettings.settingsController.crosshairStyle = popupList.value;
            PlayerPrefs.SetString("Crosshair Style", popupList.value);
        }
        else if (fpsCounter)
        {
            GameSettings.settingsController.showFPS = popupList.selectionIndex;
            PlayerPrefs.SetInt("ShowFPS", popupList.selectionIndex);
        }
        else if (aimingMethod)
        {
            GameSettings.settingsController.aimToggle = popupList.value;
            PlayerPrefs.SetString("Aim Toggle", popupList.value);
        }
        else if (speakerMode)
        {
            GameSettings.settingsController.speakerMode = popupList.selectionIndex + 1;
            PlayerPrefs.SetInt("Speaker Mode", popupList.selectionIndex + 2);
        }

        GameSettings.settingsController.UpdateGraphics();
    }

    public void LowestGraphics()
    {
        if (fullscreenResolution || fullscreenMod || crosshairStyle || fpsCounter || aimingMethod || speakerMode)
        {
            return;
        }

        popupList.value = popupList.items[0];
    }

    public void HighestGraphics()
    {
        if (fullscreenResolution || fullscreenMod || crosshairStyle || fpsCounter || aimingMethod || speakerMode || vsyncCount)
        {
            return;
        }

        popupList.value = popupList.items[popupList.items.Count - 1];
    }
}