using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerGUI : Topan.TopanMonoBehaviour
{
    public GameObject chatRoot;
    public GameObject topInfoRoot;
    public Transform timer;
    public UILabel[] timerLabels = new UILabel[3];
    public UIPanel leaderboard;
    public UIPanel chatPanel;
    public UIInput chatInput;
    public AlphaGroupUI chatIndicator;
    public UISprite inputBackground;
    public ChatListGUI chatOutput;
    public LeaderboardGUI lGUI;
    public KillFeedManager kfManager;
    public ActionFeedManager afManager;
    public float inactiveChatTime = 10f;

    public UISlider redTeamProgress;
    public UILabel redTeamValue;
    public UISlider blueTeamProgress;
    public UILabel blueTeamValue;
    public UILabel objectiveTargetText;

    public AlphaGroupUI waitForPlayers;
    public UILabel waitText;
    public UILabel playersLeft;

    public UILabel redTeamWinning;
    public UILabel blueTeamWinning;

    public Color dmYourColor = new Color(0.75f, 0.25f, 0.1f);
    public Color dmNotYourColor = new Color(0.4f, 0.4f, 0.4f);
    public UISprite glowRed;
    public UISprite glowBlue;

    private bool showGUI = false;
    private GameManager gMan;
    public GameManager gManager
    {
        get
        {
            if (gMan == null)
            {
                GameManager gm = transform.root.GetComponent<GameManager>();
                if (gm != null)
                {
                    gMan = gm;
                }
            }

            return gMan;
        }
    }

    private Topan.NetworkView cng;
    private Topan.NetworkView chatNetGeneral
    {
        get
        {
            if (cng == null)
            {
                cng = Topan.Network.GetNetworkView(GeneralVariables.Networking.gameObject);
            }

            return cng;
        }
    }

    [HideInInspector] public byte serverPL;
    [HideInInspector] public int redObjectiveProgress;
    [HideInInspector] public int blueObjectiveProgress;
    [HideInInspector] public int objectiveTarget;
    [HideInInspector] public bool youAreWinning;
    [HideInInspector] public string winningPlayerName;
    [HideInInspector] public string runnerUpName;

    private Vector4 selectedClipRange = new Vector4(15f, 115f, 200f, 254f);
    private Vector4 deselectedClipRange = new Vector4(15f, 23f, 200f, 180f);

    private Vector2 selectedClipSoftness = new Vector2(0f, 10f);
    private Vector2 deselectedClipSoftness = new Vector2(0f, 80f);

    private Color winColor = new Color(0.341f, 0.604f, 0.129f, 1f);
    private Color loseColor = new Color(0.788f, 0.094f, 0.094f, 1f);
    private Color drawColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private bool inSpawnScreen;
    private bool isTeamChat;

    private float inactivityTimer = 0f;
    private float syncTimer = 0f;
    private string allChat;
    private string teamChat;
    private int chatLength;
    private float startChatTime;
    private int lastSentPL = -1;

    private UILabel chatIndLabel;
    private float refreshObjTimer;
    private Dictionary<int, int> teamTotalKills;

    void Start()
    {
        GUICheckMP();

        refreshObjTimer = 0f;
        redTeamProgress.value = 0f;
        blueTeamProgress.value = 0f;
        redTeamValue.text = "0";
        blueTeamValue.text = "0";
        objectiveTargetText.text = "- - -";
        glowRed.alpha = 0f;
        glowBlue.alpha = 0f;

        inactivityTimer = inactiveChatTime;
        chatPanel.baseClipRegion = Vector4.Lerp(chatPanel.baseClipRegion, deselectedClipRange, Time.unscaledDeltaTime * 12f);
        chatPanel.clipSoftness = Vector2.Lerp(chatPanel.clipSoftness, deselectedClipSoftness, Time.unscaledDeltaTime * 12f);

        isTeamChat = false;
        allChat = cInput.GetText("General Chat", 1);
        teamChat = (GeneralVariables.gameModeHasTeams) ? cInput.GetText("Team Chat", 1) : cInput.GetText("General Chat", 2);
        chatInput.defaultText = allChat + " or " + teamChat + " to chat";
        chatLength = 0;
        chatIndicator.alpha = 0f;
        chatIndLabel = chatIndicator.widgets[1].GetComponent<UILabel>();
        syncTimer = 1f;
    }

    void Update()
    {
        inSpawnScreen = (GeneralVariables.spawnController != null && GeneralVariables.spawnController.inSpawnScreen);
        if (showGUI != (Topan.Network.isConnected && !inSpawnScreen))
        {
            GUICheckMP();
        }

        if (Topan.Network.isConnected)
        {
            if (leaderboard.alpha <= 0f && cInput.GetButtonDown("Leaderboard") && !RestrictionManager.restricted)
            {
                lGUI.Refresh(); //Refresh once leaderboard fades in.
            }

            if (!RestrictionManager.restricted && Time.time > 1f && cInput.GetButton("Leaderboard"))
            {
                leaderboard.alpha = Mathf.MoveTowards(leaderboard.alpha, 1f, Time.unscaledDeltaTime * 7f);
            }
            else
            {
                leaderboard.alpha = Mathf.MoveTowards(leaderboard.alpha, 0f, Time.unscaledDeltaTime * 7f);
            }

            if (GeneralVariables.Networking != null)
            {
                serverPL = (byte)Topan.Network.connectedPlayers.Length;
                if (!GeneralVariables.Networking.matchStarted)
                {
                    GeneralVariables.uiController.mpGUI.timerLabels[0].text = "--";
                    GeneralVariables.uiController.mpGUI.timerLabels[1].text = "--";

                    syncTimer += Time.deltaTime;
                    if (syncTimer >= 1f)
                    {
                        int peopleLeft = Mathf.Max(0, Mathf.CeilToInt(Topan.Network.connectedPlayers.Length * 0.25f) - GeneralVariables.Networking.availablePlayers.Length);
                        if (Topan.Network.isServer && GeneralVariables.server != null && peopleLeft != lastSentPL)
                        {
                            Topan.Network.SetServerInfo("pl", (byte)peopleLeft);
                            lastSentPL = peopleLeft;
                        }

                        syncTimer -= 1f;
                    }

                    if (Topan.Network.HasServerInfo("pl"))
                    {
                        serverPL = (byte)Topan.Network.GetServerInfo("pl");
                    }
                }

                waitForPlayers.gameObject.SetActive(true);

                if (!GeneralVariables.Networking.matchStarted && !GeneralVariables.Networking.finishedGame && !GeneralVariables.Networking.countingDown && serverPL > 0)
                {
                    waitForPlayers.alpha = Mathf.MoveTowards(waitForPlayers.alpha, 1f, Time.unscaledDeltaTime * 6f);
                    waitText.text = "BATTLE AWAITING";
                    playersLeft.text = serverPL.ToString() + " player" + ((serverPL > 1) ? "s" : "") + " left";
                }
                else if (GeneralVariables.Networking.countingDown && !GeneralVariables.Networking.finishedGame && GeneralVariables.Networking.countdown >= 0 && serverPL <= 0)
                {
                    waitForPlayers.alpha = Mathf.MoveTowards(waitForPlayers.alpha, 1f, Time.unscaledDeltaTime * 6f);
                    waitText.text = "BATTLE IMMINENT";
                    playersLeft.text = "Match starting in: " + GeneralVariables.Networking.countdown.ToString();
                }
                else
                {
                    waitForPlayers.alpha = Mathf.MoveTowards(waitForPlayers.alpha, 0f, Time.unscaledDeltaTime * 6f);
                }
            }
        }

        ObjectiveProgressGUI();

        if (showGUI)
        {
            if (gManager != null)
            {
                gManager.leaderboardBlur = leaderboard.alpha * 0.9f;
            }

            if (chatLength != NetworkingGeneral.gameChatList.Count)
            {
                chatOutput.CopyList(NetworkingGeneral.gameChatList);
                chatLength = NetworkingGeneral.gameChatList.Count;
                inactivityTimer = 0f;
            }

            if (!(RestrictionManager.restricted && !RestrictionManager.allInput) && !RoundEndManager.isRoundEnded)
            {
                bool allChatPressed = cInput.GetButtonDown("General Chat");
                bool teamChatPressed = GeneralVariables.gameModeHasTeams && cInput.GetButtonDown("Team Chat");
                if (allChatPressed || teamChatPressed)
                {
                    if (!chatInput.isSelected)
                    {
                        if (Time.unscaledTime - startChatTime >= 0.25f)
                        {
                            chatInput.isSelected = true;
                            //                            chatInput.restrictFrames = 1;
                            RestrictionManager.allInput = true;

                            if (teamChatPressed)
                            {
                                isTeamChat = true;
                                chatIndLabel.text = "TEAM";
                            }
                            else
                            {
                                chatIndLabel.text = "ALL";
                            }

                            chatIndicator.alpha = 1f;
                            startChatTime = Time.unscaledTime;
                        }
                    }
                    else if (Input.GetKeyDown(allChat.ToLower()))
                    {
                        bool sameAsLastMsg = (chatOutput.chatList.Count > 0) ? DarkRef.RemoveSpaces(chatOutput.chatList[chatOutput.chatList.Count - 1].ToLower()) == DarkRef.RemoveSpaces(chatInput.value.ToLower()) : false;
                        if (!string.IsNullOrEmpty(DarkRef.RemoveSpaces(chatInput.value)) && !sameAsLastMsg)
                        {
                            if (isTeamChat)
                            {
                                int myTeam = (int)((byte)Topan.Network.player.GetPlayerData("team"));
                                string message = "[TEAM] [DAA314]" + AccountManager.profileData.username + "[-]: " + chatInput.value;
                                NetworkingGeneral.gameChatList.Add(message);
                                chatNetGeneral.RPC(DarkRef.SendTeamMessage(myTeam), "ChatMessage", message);
                            }
                            else
                            {
                                chatNetGeneral.RPC(Topan.RPCMode.All, "ChatMessage", "[DAA314]" + AccountManager.profileData.username + "[-]: " + chatInput.value);
                            }
                        }

                        chatInput.value = "";
                        chatInput.isSelected = false;
                        isTeamChat = false;
                        RestrictionManager.restricted = false;
                    }
                }
            }

            if (Time.unscaledTime - startChatTime >= 0.6f)
            {
                chatIndicator.alpha -= Time.unscaledDeltaTime * 2.2f;
            }

            ChatFocusGUI();
        }
    }

    public void OnDisable()
    {
        RestrictionManager.allInput = false;
    }

    [RPC]
    public void AddToKillFeed(byte kID, byte vID, byte wepIndex)
    {
        Topan.NetworkPlayer killer = null;
        Topan.NetworkPlayer victim = null;
        BotPlayer killerBot = null;
        BotPlayer victimBot = null;

        bool killerIsBot = (kID >= 64);
        bool victimIsBot = (vID >= 64);

        if (killerIsBot)
        {
            killerBot = BotManager.allBotPlayers[kID - 64];
        }
        else
        {
            killer = Topan.Network.GetPlayerByID(kID);
        }

        if (victimIsBot)
        {
            victimBot = BotManager.allBotPlayers[vID - 64];
        }
        else
        {
            victim = Topan.Network.GetPlayerByID(vID);
        }

        if ((killerIsBot) ? killerBot == null : killer == null || (victimIsBot) ? victimBot == null : victim == null)
        {
            return;
        }

        byte killerTeam = (killerIsBot) ? killerBot.team : (byte)killer.GetPlayerData("team");
        byte victimTeam = (victimIsBot) ? victimBot.team : (byte)victim.GetPlayerData("team");

        string klrColor = (killerTeam == 0) ? "[D75216]" : "[407499]";
        string vctmColor = (victimTeam == 0) ? "[D75216]" : "[407499]";

        if (!GeneralVariables.gameModeHasTeams)
        {
            klrColor = (kID == Topan.Network.player.id) ? "[B05C25]" : "[707070]";
            vctmColor = (vID == Topan.Network.player.id) ? "[B05C25]" : "[707070]";
        }

        CombatantInfo kInfo = (killerIsBot) ? killerBot.botInfo : (CombatantInfo)killer.GetInitialData("dat");
        CombatantInfo vInfo = (victimIsBot) ? victimBot.botInfo : (CombatantInfo)victim.GetInitialData("dat");

        bool suicide = (kID == vID);
        if (suicide)
        {
            kfManager.AddToFeed(vctmColor + vInfo.username + "[-]", "himself", -1);

            if (GeneralVariables.spawnController != null && inSpawnScreen)
            {
                GeneralVariables.spawnController.AddToFeed(vctmColor + vInfo.username + "[-]", "himself", -1);
            }
        }
        else
        {
            kfManager.AddToFeed(klrColor + kInfo.username + "[-]", vctmColor + vInfo.username + "[-]", wepIndex);

            if (GeneralVariables.spawnController != null && inSpawnScreen)
            {
                GeneralVariables.spawnController.AddToFeed(klrColor + kInfo.username + "[-]", vctmColor + vInfo.username + "[-]", wepIndex);
            }
        }
    }

    private void GUICheckMP()
    {
        showGUI = Topan.Network.isConnected && !inSpawnScreen;
        chatRoot.SetActive(showGUI);
        topInfoRoot.SetActive(Topan.Network.isConnected);
        lGUI.gameObject.SetActive(Topan.Network.isConnected);
    }

    private void ObjectiveProgressGUI()
    {
        if (!Topan.Network.isConnected)
        {
            return;
        }

        refreshObjTimer += Time.unscaledDeltaTime;

        if (refreshObjTimer >= 0.3f)
        {
            if (GeneralVariables.gameModeHasTeams && NetworkingGeneral.currentGameType.killsPerTeam != null)
            {
                teamTotalKills = NetworkingGeneral.currentGameType.killsPerTeam; //Should get this info for team-based game modes.
            }

            if (NetworkingGeneral.currentGameType.typeName == "Team Deathmatch")
            {
                redObjectiveProgress = teamTotalKills[0];
                blueObjectiveProgress = teamTotalKills[1];

                if (NetworkingGeneral.currentGameType.customSettings["Kill Limit"].currentValue != null)
                {
                    objectiveTarget = Mathf.Max(1, int.Parse(NetworkingGeneral.currentGameType.customSettings["Kill Limit"].currentValue));
                }
            }
            else if (NetworkingGeneral.currentGameType.typeName == "Deathmatch")
            {
                if (lGUI.sortedPlayers != null && lGUI.sortedPlayers.Count > 0)
                {
                    winningPlayerName = lGUI.sortedPlayers[0].myInfo.username;
                    youAreWinning = (lGUI.sortedPlayers[0].realPlayer != null && lGUI.sortedPlayers[0].realPlayer.id == Topan.Network.player.id);
                    redObjectiveProgress = lGUI.sortedPlayers[0].myKills;

                    runnerUpName = (lGUI.sortedPlayers.Count > 1) ? lGUI.sortedPlayers[1].myInfo.username : "";

                    if (youAreWinning)
                    {
                        blueObjectiveProgress = (lGUI.sortedPlayers.Count > 1) ? lGUI.sortedPlayers[1].myKills : 0;
                        redTeamProgress.foregroundWidget.color = Color.Lerp(redTeamProgress.foregroundWidget.color, dmYourColor, Time.deltaTime * 8f);
                        blueTeamProgress.foregroundWidget.color = Color.Lerp(blueTeamProgress.foregroundWidget.color, dmNotYourColor, Time.deltaTime * 8f);
                    }
                    else
                    {
                        blueObjectiveProgress = (lGUI.yourPlayer != null) ? lGUI.yourPlayer.myKills : 0;
                        redTeamProgress.foregroundWidget.color = Color.Lerp(redTeamProgress.foregroundWidget.color, dmNotYourColor, Time.deltaTime * 8f);
                        blueTeamProgress.foregroundWidget.color = Color.Lerp(blueTeamProgress.foregroundWidget.color, dmYourColor, Time.deltaTime * 8f);
                    }
                }

                if (NetworkingGeneral.currentGameType.customSettings["Kill Limit"].currentValue != null)
                {
                    objectiveTarget = Mathf.Max(1, int.Parse(NetworkingGeneral.currentGameType.customSettings["Kill Limit"].currentValue));
                }
            }

            refreshObjTimer -= 0.3f;
        }

        if (GeneralVariables.gameModeHasTeams)
        {
            byte yourTeam = (byte)Topan.Network.player.GetPlayerData("team", (byte)0);
            glowRed.alpha = Mathf.MoveTowards(glowRed.alpha, (yourTeam == 0) ? glowRed.defaultAlpha : 0f, Time.deltaTime * 5f);
            glowBlue.alpha = Mathf.MoveTowards(glowBlue.alpha, (yourTeam == 1) ? glowBlue.defaultAlpha : 0f, Time.deltaTime * 5f);
        }
        else
        {
            if (youAreWinning)
            {
                glowRed.alpha = Mathf.MoveTowards(glowRed.alpha, glowRed.defaultAlpha, Time.deltaTime * 5f);
                glowBlue.alpha = Mathf.MoveTowards(glowBlue.alpha, 0f, Time.deltaTime * 5f);
            }
            else
            {
                glowRed.alpha = Mathf.MoveTowards(glowRed.alpha, 0f, Time.deltaTime * 5f);
                glowBlue.alpha = Mathf.MoveTowards(glowBlue.alpha, glowBlue.defaultAlpha, Time.deltaTime * 5f);
            }
        }

        objectiveTarget = Mathf.Max(1, objectiveTarget);
        redTeamProgress.value = Mathf.Lerp(redTeamProgress.value, (float)redObjectiveProgress / (float)objectiveTarget, Time.unscaledDeltaTime * 6f);
        redTeamValue.text = redObjectiveProgress.ToString();
        blueTeamProgress.value = Mathf.Lerp(blueTeamProgress.value, (float)blueObjectiveProgress / (float)objectiveTarget, Time.unscaledDeltaTime * 6f);
        blueTeamValue.text = blueObjectiveProgress.ToString();
        objectiveTargetText.text = objectiveTarget.ToString();

        if (GeneralVariables.gameModeHasTeams)
        {
            if (redObjectiveProgress != blueObjectiveProgress)
            {
                bool redTeamWin = (redObjectiveProgress > blueObjectiveProgress);
                bool blueTeamWin = (blueObjectiveProgress > redObjectiveProgress);

                redTeamWinning.text = ((redTeamWin) ? "WINNING" : "LOSING") + ((objectiveTarget > 0) ? (" (" + (redTeamProgress.value * 100f).ToString("F0") + "%)") : "");
                redTeamWinning.color = ((redTeamWin) ? winColor : loseColor);
                blueTeamWinning.text = ((blueTeamWin) ? "WINNING" : "LOSING") + ((objectiveTarget > 0) ? (" (" + (blueTeamProgress.value * 100f).ToString("F0") + "%)") : "");
                blueTeamWinning.color = ((blueTeamWin) ? winColor : loseColor);
            }
            else
            {
                redTeamWinning.text = "DRAW" + ((objectiveTarget > 0) ? (" (" + (redTeamProgress.value * 100f).ToString("F0") + "%)") : "");
                redTeamWinning.color = drawColor;
                blueTeamWinning.text = "DRAW" + ((objectiveTarget > 0) ? (" (" + (blueTeamProgress.value * 100f).ToString("F0") + "%)") : "");
                blueTeamWinning.color = drawColor;
            }
        }
    }

    private void ChatFocusGUI()
    {
        if (chatInput.isSelected)
        {
            chatPanel.baseClipRegion = Vector4.Lerp(chatPanel.baseClipRegion, selectedClipRange, Time.unscaledDeltaTime * 12f);
            chatPanel.clipSoftness = Vector2.Lerp(chatPanel.clipSoftness, selectedClipSoftness, Time.unscaledDeltaTime * 12f);
            inactivityTimer = 0f;
        }
        else
        {
            chatPanel.baseClipRegion = Vector4.Lerp(chatPanel.baseClipRegion, deselectedClipRange, Time.unscaledDeltaTime * 12f);
            chatPanel.clipSoftness = Vector2.Lerp(chatPanel.clipSoftness, deselectedClipSoftness, Time.unscaledDeltaTime * 12f);
            inactivityTimer += Time.unscaledDeltaTime;
        }

        if (inactivityTimer >= inactiveChatTime)
        {
            chatPanel.alpha = Mathf.Lerp(chatPanel.alpha, 0.425f, Time.unscaledDeltaTime * 8f);
            inputBackground.alpha = Mathf.Lerp(inputBackground.alpha, inputBackground.defaultAlpha * 0.5f, Time.unscaledDeltaTime * 8f);
            chatInput.label.alpha = Mathf.Lerp(chatInput.label.alpha, chatInput.label.defaultAlpha * 0.6f, Time.unscaledDeltaTime * 8f);
        }
        else
        {
            chatPanel.alpha = Mathf.Lerp(chatPanel.alpha, 1f, Time.unscaledDeltaTime * 8f);
            inputBackground.alpha = Mathf.Lerp(inputBackground.alpha, inputBackground.defaultAlpha, Time.unscaledDeltaTime * 8f);
            chatInput.label.alpha = Mathf.Lerp(chatInput.label.alpha, chatInput.label.defaultAlpha, Time.unscaledDeltaTime * 8f);
        }
    }
}