using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Controlling the game mode settings (server).
public class GM_SettingsControl : MonoBehaviour
{
    public class SettingInfo
    {
        public GameObject settingObjInstance;
        public string settingName;
        public GameType.SettingType settingType;
    }

    public string gameTypeName = "";

    public UIPopupList gameModePopup;
    public UILabel modeLabel;
    public BlurEffect blurGUI;
    public Transform spawnStart;
    public float spacing = 30f;
    public GameObject sliderSettingPrefab;
    public GameObject checkboxSettingPrefab;

    [HideInInspector]
    public List<SettingInfo> settingsList;

    private UIPanel panel;

    void Start()
    {
        panel = GetComponent<UIPanel>();
        settingsList = new List<SettingInfo>();

        gameModePopup.items.Clear();
        foreach (KeyValuePair<string, System.Type> pair in MultiplayerMenu.possibleGameTypes)
        {
            gameModePopup.items.Add(pair.Key);
        }

        gameModePopup.value = gameModePopup.items[PlayerPrefs.GetInt("SavedGameMode", 0)];
        gameTypeName = gameModePopup.value;
        modeLabel.text = "[A0A0A0]- " + gameModePopup.value + "[-]";
        panel.alpha = 0f;
    }

    public void ChangedGameType()
    {
        settingsList.Clear();
        gameTypeName = gameModePopup.value;
        modeLabel.text = "[A0A0A0]- " + gameTypeName + "[-]";
        NetworkingGeneral.currentGameType = (GameTypeInterface)System.Activator.CreateInstance(MultiplayerMenu.possibleGameTypes[gameTypeName]);
        NetworkingGeneral.currentGameType.InitializeSettings();

        foreach (Transform trans in spawnStart)
        {
            Destroy(trans.gameObject);
        }

        int i = 0;
        foreach (KeyValuePair<string, GameType.GameTypeSetting> pair in NetworkingGeneral.currentGameType.customSettings)
        {
            GameObject settingInst = null;
            if (pair.Value.settingType == GameType.SettingType.Slider)
            {
                settingInst = (GameObject)Instantiate(sliderSettingPrefab);

                SettingInfo info = new SettingInfo();
                info.settingObjInstance = settingInst;
                info.settingName = pair.Key;
                info.settingType = GameType.SettingType.Slider;
                SliderAction sAction = settingInst.GetComponentInChildren<SliderAction>();

                if (pair.Key == "Kill Limit")
                {
                    sAction.minValue = 25f;
                    sAction.maxValue = 500f;
                    sAction.defaultValue = PlayerPrefs.GetFloat("Kill Limit", float.Parse(pair.Value.currentValue));
                    sAction.suffix = " kills";
                    sAction.SetIntervalSteps((int)(sAction.maxValue - sAction.minValue));
                }

                settingsList.Add(info);
            }
            else if (pair.Value.settingType == GameType.SettingType.Checkbox)
            {
                settingInst = (GameObject)Instantiate(checkboxSettingPrefab);

                SettingInfo info = new SettingInfo();
                info.settingObjInstance = settingInst;
                info.settingName = pair.Key;
                info.settingType = GameType.SettingType.Checkbox;
                UIToggle toggle = settingInst.GetComponentInChildren<UIToggle>();

                if (pair.Key == "Friendly Fire")
                {
                    toggle.value = (PlayerPrefs.GetInt("Friendly Fire", (pair.Value.currentValue == "True") ? 1 : 0) == 1) ? true : false;
                }
                else if (pair.Key == "Team Auto-Balance")
                {
                    toggle.value = (PlayerPrefs.GetInt("Team Auto-Balance", (pair.Value.currentValue == "True") ? 1 : 0) == 1) ? true : false;
                }

                settingsList.Add(info);
            }

            if (settingInst != null)
            {
                settingInst.transform.parent = spawnStart;
                settingInst.transform.localPosition = -Vector3.up * i * spacing;
                settingInst.transform.localScale = Vector3.one;
                settingInst.name = pair.Key;
                settingInst.GetComponentInChildren<UILabel>().text = pair.Key + ":";
            }

            i++;
        }

        StartCoroutine(ApplySettings());
    }

    public void DisplayWindow(bool disp)
    {
        StartCoroutine(FadeAnimation(disp));
    }

    public IEnumerator ApplySettings()
    {
        yield return null; //Wait for the actions to complete during this frame, then apply during the next.

        foreach (SettingInfo sInfo in settingsList)
        {
            string val = "";
            if (sInfo.settingType == GameType.SettingType.Slider)
            {
                SliderAction sAction = sInfo.settingObjInstance.GetComponentInChildren<SliderAction>();
                val = sAction.currentValue.ToString();
                NetworkingGeneral.currentGameType.customSettings[sInfo.settingName].currentValue = val;
                PlayerPrefs.SetFloat(sInfo.settingName, sAction.currentValue);
            }
            else if (sInfo.settingType == GameType.SettingType.Checkbox)
            {
                UIToggle toggle = sInfo.settingObjInstance.GetComponentInChildren<UIToggle>();
                val = toggle.value.ToString();
                NetworkingGeneral.currentGameType.customSettings[sInfo.settingName].currentValue = val;
                PlayerPrefs.SetInt(sInfo.settingName, (toggle.value) ? 1 : 0);
            }

            if (Topan.Network.isConnected && Topan.Network.isServer)
            {
                Topan.Network.SetServerInfo(sInfo.settingName, val);
            }
        }
    }

    private IEnumerator FadeAnimation(bool f)
    {
        if (f)
        {
            float fade = 0f;
            while (fade < 1f)
            {
                fade += Time.deltaTime * 8f;
                panel.alpha = Mathf.Clamp01(fade);
                blurGUI.blurSpread = Mathf.Clamp01(fade) * 0.9f;
                yield return null;
            }
        }
        else
        {
            float fade = 1f;
            while (fade > 0f)
            {
                fade -= Time.deltaTime * 8f;
                panel.alpha = Mathf.Clamp01(fade);
                blurGUI.blurSpread = Mathf.Clamp01(fade) * 0.9f;
                yield return null;
            }

            StartCoroutine(ApplySettings());
        }
    }
}