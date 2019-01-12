using UnityEngine;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class MultiplayerMenu : MonoBehaviour
{
    public static MultiplayerMenu Instance;

    //These two dictionaries correspond to each other.
    public static Dictionary<string, int> gameTypeIndexes = null;
    public static Dictionary<int, string> gameTypeNames = null;

    public static Dictionary<string, Type> possibleGameTypes;
    public static int disconnectMsg = -1;
    public static int pingLimitSetting = 0;
    public static int idleTimeSetting = 0;
    public static bool multiplayerEnabled = false;

    public UIInput IPInput;
    public UIInput portInput;
    public UIInput gameNameInput;
    public UIToggle portForwardCheckbox;
    public UIInput hostPortInput;
    public UIToggle hostLocalCheckbox;
    public SliderAction maxPlayerSlider;
    public SliderAction gameDurationSlider;
    public SliderAction roundCountSlider;
    public SliderAction idleTimeSlider;
    public UIInput pingLimitInput;
    public GameObject mapSelection;
    public GameObject cachedNetworking;

    public UISprite mapSelectionBox;
    public Transform sliderStart;
    public float mapSpacing = 210f;

    public GameObject backButton;
    public UIButton hostServerButton;
    public UIButton editServerButton;
    public UIButton moreInfoButton;

    public UILabel mServerStatus;
    public UILabel mServerPing;
    public UIButton mServerPingButton;
    public UISprite mCheckingIcon;
    public CameraMove moveCam;
    public GM_SettingsControl mSettingControl;
    public BotManager bsMenu;

    public UIButton multiplayerButton;
    public ShowTooltip mpErrorTooltip;

    private ShowTooltip tooltipInfo;
    private int selectedMap = -1;
    private int oldSelection = 0;
    private bool pinging;
    private bool serverEditMode = false;


    private static int mPingTime = -1;
    public static bool mServerIsOnline = false;
    public static DateTime mServerCheckTime = DateTime.MinValue;

    void Awake()
    {
        //Static initialization
        Instance = this;

        AudioListener.pause = false;

        possibleGameTypes = new Dictionary<string, Type>();
        possibleGameTypes.Add("Team Deathmatch", typeof(TeamDeathmatch));
        possibleGameTypes.Add("Deathmatch", typeof(Deathmatch));

        //Automatically initialize the lists, DON'T TOUCH!
        if (gameTypeIndexes == null || gameTypeNames == null)
        {
            gameTypeIndexes = new Dictionary<string, int>();
            gameTypeNames = new Dictionary<int, string>();

            List<string> gameTypeKeys = new List<string>(possibleGameTypes.Keys);
            for (int i = 0; i < gameTypeKeys.Count; i++)
            {
                gameTypeIndexes.Add(gameTypeKeys[i], i);
                gameTypeNames.Add(i, gameTypeKeys[i]);
            }
        }

        Topan.Network.sendNetworkViewsOnConnect = false;
        Topan.Network.enableDebugging = true;
        tooltipInfo = moreInfoButton.GetComponent<ShowTooltip>();
        ServerEditMode(false);

        mapSelectionBox.transform.localPosition = sliderStart.localPosition;

        for (int i = 0; i < StaticMapsList.mapsArraySorted.Count; i++)
        {
            GameObject newHostMap = (GameObject)Instantiate(mapSelection);
            newHostMap.transform.parent = sliderStart.parent;
            newHostMap.transform.localPosition = sliderStart.localPosition + (Vector3.right * mapSpacing * i);
            newHostMap.transform.localRotation = sliderStart.localRotation;
            newHostMap.transform.localScale = Vector3.one;
            ButtonAction hostba = newHostMap.GetComponent<ButtonAction>();
            hostba.sendMessage.numericalMessage.enabled = true;
            hostba.sendMessage.numericalMessage.isInt = true;
            hostba.sendMessage.numericalMessage.messageName = "SelectMap";
            hostba.sendMessage.numericalMessage.messageReceiver = gameObject;
            hostba.sendMessage.numericalMessage.valueToSend = i;

            MapGUI MGUI = newHostMap.GetComponent<MapGUI>();
            Map thisMap = StaticMapsList.mapsArraySorted[i];
            MGUI.nameLabel.text = thisMap.mapName;
            MGUI.description = thisMap.mapName + "|| " + thisMap.loaderSubheader;

            if (thisMap.previewIcon != null)
            {
                MGUI.mapScreenshot.mainTexture = thisMap.previewIcon;
            }
        }

        selectedMap = Mathf.Clamp(PlayerPrefs.GetInt("SavedMapIndex", 0), 0, StaticMapsList.mapsArraySorted.Count - 1);
        UpdateSelection();

        IPInput.value = PlayerPrefs.GetString("SavedIPConnect", "127.0.0.1");
        portInput.value = PlayerPrefs.GetString("SavedPortConnect", "7100");
        pingLimitInput.value = PlayerPrefs.GetString("SavedPingLimit", "9999");
        portForwardCheckbox.value = (PlayerPrefs.GetInt("PortForward", 0) == 1) ? true : false;
        hostLocalCheckbox.value = (PlayerPrefs.GetInt("LocalServer", 0) == 1) ? true : false;

        mpErrorTooltip.text = "Validating client version...";
        InvokeRepeating("GetModeSettings", 0f, 1f); //For the clients
        cInput.Init(); //Initialize cInput at start of game, so we can avoid initializing during gameplay.
    }

    void Start()
    {
        if (disconnectMsg > -1)
        {
            moveCam.TargetPos(new Vector3(3840f, 0f, -700f));
            GeneralVariables.lobbyManager.Topan_DisconnectedFromServer();
        }

        StartCoroutine(PingMasterServer());
        StartCoroutine(VersionCheckCoroutine());
        //        CheckTime(); Serves as a time restriction.
    }

    private void CheckTime()
    {
        DateTime curTime = DarkRef.GetInternetTime();
        if (curTime == DateTime.MinValue || curTime >= new DateTime(2014, 1, 7))
        {
            Application.Quit();
        }
    }

    private IEnumerator VersionCheckCoroutine()
    {
        multiplayerEnabled = true; //Set to false when version check is ready

        while (!multiplayerEnabled)
        {
            WWW verChk = new WWW("http://darkraze.byethost6.com/darkraze_files/versionChk.txt");

            yield return verChk;

            bool unavailable = false;
            if (verChk.error != null)
            {
                mpErrorTooltip.text = "Version validation service is unavailable.";
                unavailable = true;
            }
            else
            {
                if (DarkRef.RemoveSpaces(DarkRef.GetBuildVersion(true)) == DarkRef.RemoveSpaces(verChk.text))
                {
                    multiplayerEnabled = true;
                    yield break;
                }
                else
                {
                    Debug.LogWarning("Your version is not the latest! Multiplayer will be disabled");
                    mpErrorTooltip.text = "Your client version must be up-to-date to participate in multiplayer!";
                }
            }

            float waitTime = 0f;
            while (waitTime < 5.1f)
            {
                waitTime += Time.deltaTime;

                if (unavailable)
                {
                    mpErrorTooltip.text = "Version validation service is unavailable (retrying in: " + Mathf.Max(0f, 5.1f - waitTime).ToString("F0") + ").";
                }

                yield return null;
            }
        }
    }

    private IEnumerator PingMasterServer()
    {
        if (pinging)
        {
            yield break;
        }

        pinging = true;
        WWW response = new WWW(Topan.MasterServer.masterserverURL);

        float timeout = 0f;
        while (!response.isDone)
        {
            timeout += Time.deltaTime;
            if (timeout >= 5f)
            {
                mServerIsOnline = false;
                mServerCheckTime = DateTime.UtcNow;
                mPingTime = -1;
                pinging = false;
                yield break;
            }

            yield return null;
        }

        if (string.IsNullOrEmpty(response.error))
        {
            mServerIsOnline = true;
        }
        else
        {
            mServerIsOnline = false;
            mServerCheckTime = DateTime.UtcNow;
            mPingTime = -1;
            pinging = false;
            yield break;
        }

        Ping pServer = new Ping("185.27.134.135"); //darkraze.byethost6.com

        float waitTime = 0f;
        while (!pServer.isDone)
        {
            waitTime += Time.deltaTime;
            if (waitTime >= 5f)
            {
                mServerCheckTime = DateTime.UtcNow;
                mPingTime = 5001;
                pinging = false;
                yield break;
            }

            yield return null;
        }

        mServerCheckTime = DateTime.UtcNow;
        mPingTime = pServer.time;
        pinging = false;
    }

    void Update()
    {
        multiplayerButton.isEnabled = (DarkRef.CheckAccess() || multiplayerEnabled);
        editServerButton.isEnabled = (Topan.Network.isConnected && Topan.Network.isServer && !GeneralVariables.lobbyManager.startedCountdown);
        moreInfoButton.isEnabled = Topan.Network.isConnected;
        hostPortInput.gameObject.SetActive(portForwardCheckbox.value);

        if (Topan.Network.isConnected && tooltipInfo.display)
        {
            tooltipInfo.text = "";
            int i = 0;
            foreach (KeyValuePair<string, GameType.GameTypeSetting> pair in NetworkingGeneral.currentGameType.customSettings)
            {
                tooltipInfo.text += pair.Key + ": " + pair.Value.currentValue;
                if (NetworkingGeneral.currentGameType.customSettings.Count > 1 && i < NetworkingGeneral.currentGameType.customSettings.Count - 1)
                {
                    tooltipInfo.text += "\n";
                }
                i++;
            }

            byte roundCount = (byte)Topan.Network.GetServerInfo("rc");
            tooltipInfo.text += "\n" + "Rounds Per Match: " + ((roundCount == 255) ? "Unlimited" : roundCount.ToString());
            tooltipInfo.text += "\n" + "Ping Limit: " + (int)Topan.Network.GetServerInfo("plim") + " ms";
            byte idleTime = (byte)Topan.Network.GetServerInfo("it");
            tooltipInfo.text += "\n" + "Idle Time Limit: " + ((idleTime == 255) ? "Disabled" : ((int)idleTime * 5).ToString() + " seconds");
            byte botCount = (byte)Topan.Network.GetServerInfo("bC");
            tooltipInfo.text += "\n" + "Bot Count: " + ((botCount == 0) ? "Disabled" : botCount.ToString());
        }

        float timeDiffSec = (float)DateTime.UtcNow.Subtract(mServerCheckTime).TotalSeconds;
        int checkTimeDiff = (mServerCheckTime != DateTime.MinValue) ? Mathf.FloorToInt(timeDiffSec / 60f) : -1;
        mCheckingIcon.enabled = pinging;
        mServerPingButton.isEnabled = (!pinging && timeDiffSec >= ((mServerIsOnline) ? 5f : 0f));

        mServerPing.text = "-- ms";
        if (mPingTime >= 5000)
        {
            mServerPing.text = ">5000 ms";
        }
        else if (mPingTime >= 0 && mPingTime < 5000)
        {
            mServerPing.text = mPingTime.ToString() + " ms";
        }

        string cTimeString = checkTimeDiff.ToString() + " minutes ago";
        if (checkTimeDiff > -1)
        {
            int hour = Mathf.FloorToInt(timeDiffSec / 3600f);
            if (hour > 0)
            {
                if (hour == 1)
                {
                    cTimeString = hour.ToString() + " hour ago";
                }
                else if (hour > 1)
                {
                    cTimeString = hour.ToString() + " hours ago";
                }
            }
            else
            {
                if (checkTimeDiff == 0)
                {
                    cTimeString = "a moment ago";
                }
                else if (checkTimeDiff == 1)
                {
                    cTimeString = checkTimeDiff.ToString() + " minute ago";
                }
            }
        }
        else
        {
            cTimeString = "never";
        }

        if (mServerIsOnline)
        {
            mServerStatus.text = "Master server is [8CC40F]ONLINE[-]" + "\n" + "Last check: " + cTimeString;
        }
        else
        {
            mServerStatus.text = "Master server is [B61515]OFFLINE[-]" + "\n" + "Last check: " + cTimeString;
        }
    }

    public void StartServer()
    {
        if (Topan.Network.isConnected)
        {
            return;
        }

        gameDurationSlider.ApplyChanges();
        roundCountSlider.ApplyChanges();
        idleTimeSlider.ApplyChanges();
        StartCoroutine(StartServerCoroutine());
    }

    private IEnumerator StartServerCoroutine()
    {
        Topan.Network.AddNetworkEventListener(this);
        TopanData init = new TopanData();
        init.Add("dat", NetworkingGeneral.ConvertToCombatant(AccountManager.profileData));

        int port = 7100; //As default.
        if (portForwardCheckbox.value)
        {
            port = int.Parse(hostPortInput.value);
        }

        PlayerPrefs.SetInt("SavedMaxPlayers", (int)maxPlayerSlider.currentValue);
        PlayerPrefs.SetInt("PortForward", (portForwardCheckbox.value) ? 1 : 0);
        PlayerPrefs.SetInt("LocalServer", (hostLocalCheckbox.value) ? 1 : 0);
        PlayerPrefs.SetString("SavedPingLimit", pingLimitInput.value);
        PlayerPrefs.SetInt("SavedMapIndex", selectedMap);
        PlayerPrefs.SetInt("SavedGameMode", mSettingControl.gameModePopup.selectionIndex);

        Topan.Network.InitializeServer((int)(maxPlayerSlider.currentValue), port, !portForwardCheckbox.value, init);
        Topan.Network.GameName = gameNameInput.value;
        NetworkingGeneral.CreateInstance(cachedNetworking);

        if (!hostLocalCheckbox.value && !portForwardCheckbox.value)
        {
            int mapIndx = selectedMap;
            Topan.MasterServer.RegisterMasterServer(gameNameInput.value, true, AccountManager.profileData.username, mapIndx, gameTypeIndexes[mSettingControl.gameTypeName]);
        }

        GeneralVariables.server.InstantiateServer();

        yield return null;
        yield return null;

        UICamera.selectedObject = null;
        Topan.Network.SetServerInfo("s", UnityEngine.Random.seed);
        Topan.Network.SetServerInfo("dur", (byte)gameDurationSlider.currentDuration);
        Topan.Network.SetServerInfo("rc", (byte)roundCountSlider.currentRoundAmount);
        StartCoroutine(SelectMapRoutine(selectedMap));
        GeneralVariables.server.amountOfRounds = roundCountSlider.currentRoundAmount;
        GeneralVariables.lobbyManager.ResetButtons();
        moveCam.TargetPos(new Vector3(3840f, -800f, -700f));

        Topan.Network.SetServerInfo("sm", false);
        Topan.Network.SetServerInfo("rTK", (UInt16)0);
        Topan.Network.SetServerInfo("bTK", (UInt16)0);
        Topan.Network.SetServerInfo("rTD", (UInt16)0);
        Topan.Network.SetServerInfo("bTD", (UInt16)0);
        Topan.Network.SetServerInfo("rVic", (byte)0);
        Topan.Network.SetServerInfo("bVic", (byte)0);

        Topan.Network.SetServerInfo("gm", NetworkingGeneral.currentGameType.typeName);
        foreach (GM_SettingsControl.SettingInfo sInfo in mSettingControl.settingsList)
        {
            Topan.Network.SetServerInfo(sInfo.settingName, NetworkingGeneral.currentGameType.customSettings[sInfo.settingName].currentValue);
        }

        int setPingLimit = Mathf.Clamp(int.Parse(pingLimitInput.value), 100, 9999);
        GeneralVariables.server.pingLimit = setPingLimit;
        Topan.Network.SetServerInfo("plim", setPingLimit);
        Topan.Network.SetServerInfo("it", (byte)(idleTimeSlider.currentIdleTime / 5));

        yield return null;

        Topan.Network.SetServerInfo("bC", (byte)bsMenu.amountSlider.currentValue);

        BotPlayer[] newBots = new BotPlayer[16]; //16 = maximum limit of players in a server
        List<string> prevNames = new List<string>();
        for (int i = 0; i < newBots.Length; i++)
        {
            CombatantInfo newInfo = new CombatantInfo();

            string randomName = DarkRef.RandomBotName();
            int trials = 0;
            while (prevNames.Contains(randomName) && trials < 10)
            {
                randomName = DarkRef.RandomBotName();
                trials++;
            }

            newInfo.username = randomName;
            prevNames.Add(randomName);

            newInfo.clan = "BOT";
            newInfo.rank = UnityEngine.Random.Range(1, 11);

            newBots[i] = new BotPlayer();
            newBots[i].botInfo = newInfo;

            newBots[i].botStats = new BotStats();
            newBots[i].team = (byte)(i % 2);
            newBots[i].botStats.kills = 0;
            newBots[i].botStats.deaths = 0;
            newBots[i].botStats.headshots = 0;
            newBots[i].botStats.score = 0;
            Topan.Network.SetServerInfo("bS" + i.ToString(), BotManager.ParseToBotFormat(newBots[i].botStats));
        }

        prevNames = null;
        BotManager._sBots = newBots;
        Topan.Network.SetServerInfo("bots", newBots);
    }

    public void SelectMap(int index)
    {
        if (index != selectedMap)
        {
            selectedMap = index;
            UpdateSelection();
        }
    }

    private IEnumerator SelectMapRoutine(int mapID)
    {
        if (Topan.Network.isConnected && Topan.Network.HasServerInfo("m") && Topan.Network.GetServerInfo("m") != null && mapID == selectedMap)
        {
            yield break;
        }

        while (Topan.Network.isConnected && (!Topan.Network.HasServerInfo("m") || Topan.Network.GetServerInfo("m") == null))
        {
            Topan.Network.SetServerInfo("m", (byte)mapID);
            yield return new WaitForSeconds(0.1f);
        }

        selectedMap = mapID;

        if (mapID != selectedMap)
        {
            UpdateSelection();
        }
    }

    public void ServerEditMode(bool edit)
    {
        if (edit && !Topan.Network.isServer)
        {
            return;
        }

        serverEditMode = edit;
        backButton.SetActive(!edit);
        portForwardCheckbox.GetComponent<UIButton>().isEnabled = !edit;
        hostLocalCheckbox.GetComponent<UIButton>().isEnabled = !edit;
        ButtonAction hsbBA = hostServerButton.GetComponent<ButtonAction>();

        gameNameInput.GetComponent<AlphaGroupUI>().alpha = (edit) ? 0.5f : 1f;
        gameNameInput.GetComponent<BoxCollider>().enabled = !edit;

        maxPlayerSlider.GetComponent<AlphaGroupUI>().alpha = (edit) ? 0.5f : 1f;
        maxPlayerSlider.GetComponent<BoxCollider>().enabled = !edit;
        //maxPlayerSlider.GetComponent<UISlider>().thumb.GetComponent<BoxCollider>().enabled = !edit;

        if (edit)
        {
            hostServerButton.cachedLabel.text = "SAVE CHANGES";
            hsbBA.sendMessage.genericMessage.messageName = "UpdateSettings";
        }
        else
        {
            hostServerButton.cachedLabel.text = "START SERVER";
            hsbBA.sendMessage.genericMessage.messageName = "StartServer";
        }
    }

    public void UpdateSettings()
    {
        if (!serverEditMode)
        {
            return;
        }

        if (!Topan.Network.HasServerInfo("dur") || (byte)Topan.Network.GetServerInfo("dur") != (byte)gameDurationSlider.currentDuration)
        {
            Topan.Network.SetServerInfo("dur", (byte)gameDurationSlider.currentDuration);
        }

        if (!Topan.Network.HasServerInfo("rc") || (byte)Topan.Network.GetServerInfo("rc") != (byte)roundCountSlider.currentRoundAmount)
        {
            Topan.Network.SetServerInfo("rc", (byte)roundCountSlider.currentRoundAmount);
        }

        if (!Topan.Network.HasServerInfo("it") || (byte)Topan.Network.GetServerInfo("it") != (byte)(idleTimeSlider.currentIdleTime / 5))
        {
            Topan.Network.SetServerInfo("dur", (byte)(idleTimeSlider.currentIdleTime / 5));
        }

        if (!Topan.Network.HasServerInfo("bC") || (byte)Topan.Network.GetServerInfo("bC") != (byte)bsMenu.amountSlider.currentValue)
        {
            Topan.Network.SetServerInfo("bC", (byte)bsMenu.amountSlider.currentValue);
        }

        gameDurationSlider.ApplyChanges();
        roundCountSlider.ApplyChanges();
        idleTimeSlider.ApplyChanges();

        int setPingLimit = Mathf.Clamp(int.Parse(pingLimitInput.value), 100, 9999);
        if (!Topan.Network.HasServerInfo("plim") || GeneralVariables.server.pingLimit != setPingLimit)
        {
            Topan.Network.SetServerInfo("plim", setPingLimit);
            GeneralVariables.server.pingLimit = setPingLimit;
        }

        PlayerPrefs.SetString("SavedPingLimit", pingLimitInput.value);
        PlayerPrefs.SetInt("SavedMapIndex", selectedMap);
        PlayerPrefs.SetInt("SavedGameMode", mSettingControl.gameModePopup.selectionIndex);

        if (!Topan.Network.HasServerInfo("m") || selectedMap != oldSelection)
        {
            Topan.Network.SetServerInfo("m", (byte)selectedMap);
        }

        oldSelection = selectedMap;
        Topan.MasterServer.UpdateServerInfo(selectedMap, gameTypeIndexes[mSettingControl.gameTypeName]);

        if (!Topan.Network.HasServerInfo("gm") || Topan.Network.GetServerInfo("gm").ToString() != NetworkingGeneral.currentGameType.typeName)
        {
            Topan.Network.SetServerInfo("gm", NetworkingGeneral.currentGameType.typeName);
        }

        foreach (GM_SettingsControl.SettingInfo sInfo in mSettingControl.settingsList)
        {
            Topan.Network.SetServerInfo(sInfo.settingName, NetworkingGeneral.currentGameType.customSettings[sInfo.settingName].currentValue);
        }

        NetworkingGeneral.currentGameType.ClearPlayerList();
        for (int i = 0; i < Topan.Network.connectedPlayers.Length; i++)
        {
            Topan.NetworkPlayer p = Topan.Network.connectedPlayers[i];
            p.SetPlayerData("team", (byte)NetworkingGeneral.currentGameType.GetTeamAssign(p.id));
        }

        hostServerButton.cachedLabel.text = "";
        moveCam.TargetPos(new Vector3(3840f, -800f, -700f));
        ServerEditMode(false);
    }

    public void UpdateSelection()
    {
        if (mapSelectionBox == null)
        {
            return;
        }

        StartCoroutine(SelectionFade());
    }

    private IEnumerator SelectionFade()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 10f;
            mapSelectionBox.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }

        mapSelectionBox.transform.localPosition = (sliderStart.localPosition + (Vector3.right * mapSpacing * selectedMap));

        alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 10f;
            mapSelectionBox.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }
    }

    private void GetModeSettings()
    {
        if (!Topan.Network.isConnected || Topan.Network.isServer)
        {
            return;
        }

        string gameMode = Topan.Network.GetServerInfo("gm").ToString();
        if (gameMode != NetworkingGeneral.currentGameType.typeName)
        {
            NetworkingGeneral.currentGameType = (GameTypeInterface)System.Activator.CreateInstance(MultiplayerMenu.possibleGameTypes[gameMode]);
            NetworkingGeneral.currentGameType.InitializeSettings();
        }

        foreach (KeyValuePair<string, GameType.GameTypeSetting> pair in NetworkingGeneral.currentGameType.customSettings)
        {
            string netSetting = Topan.Network.GetServerInfo(pair.Key).ToString();
            pair.Value.currentValue = netSetting;
        }
    }

    public void Topan_ConnectionSuccessful()
    {
        if (Topan.Network.isServer)
        {
            return;
        }

        moveCam.TargetPos(new Vector3(3840f, -800f, -700f));
    }

    public void ConnectIP()
    {
        if (Topan.Network.isConnected)
        {
            return;
        }

        NetworkingGeneral.CreateInstance(cachedNetworking);
        Topan.Network.AddNetworkEventListener(this);
        TopanData initData = new TopanData();
        initData.Add("dat", NetworkingGeneral.ConvertToCombatant(AccountManager.profileData));
        PlayerPrefs.SetString("SavedIPConnect", DarkRef.RemoveSpaces(IPInput.value));
        PlayerPrefs.SetString("SavedPortConnect", portInput.value);
        Topan.Network.Connect(DarkRef.RemoveSpaces(IPInput.value), int.Parse(portInput.value), initData);
        GeneralVariables.lobbyManager.joiningLobby = true;
    }
}