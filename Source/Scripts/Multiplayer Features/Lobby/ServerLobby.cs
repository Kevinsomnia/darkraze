using UnityEngine;
using System.Collections;

public class ServerLobby : Topan.TopanMonoBehaviour {
	public UIButton joinServerButton;
	public UIPanel joiningServerPanel;
	public GameObject playerInfoPrefab;
	public float spacing = 30f;
	public Transform redPanel;
	public Transform bluePanel;
	public float refreshInterval = 0.5f;
	public UILabel serverInfo;
	public UITexture serverMapScreenshot;
	public GameObject countdownParent;
	public UIButton startGameButton;
	public UIButton backButton;
    public AudioClip disconnectionClip;
	public UIPanel disconnectionPanel;
    public BlurEffect disconnectionBlur;
    public UILabel disconnectionLabel;
	public Transform chatParent;
    public UILabel countdownText;
    public GameObject cachedNetworking;
	public CameraMove camMove;
	public SliderAction gameDurationSlider;
	public MultiplayerMenu mpMenu;
    public AlphaGroupUI waitingRed;
    public AlphaGroupUI waitingBlue;
	
	public AudioClip countdownSound;
	public LobbyChat lobbyChat;
    public IntroManager im;
    public UIPanel fadeToBlack;
    public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(3f, 0f));
    public GameObject spawnInterfacePrefab;
    public ServerList serverList;
    public UILabel redTeamLabel;
    public UILabel blueTeamLabel;
    public UILabel[] miscGMLabels;
    public BoxCollider[] switchTeams;
	
    [HideInInspector] public bool joiningLobby;
	[HideInInspector] public bool startedCountdown;
    [HideInInspector] public int countdown;

	private GameObject[] playerInfoSlots;
	private bool onlineList = true;
	private bool allSuccess = false;
	private int redIndex = 0;
	private int blueIndex = 0;
    private int unassignedIndex = 0;
    private int redCount;
    private int blueCount;
    private float curvePos;
    private float refreshTime;
    
    private HostInfo joinHostInfo;
    private int mapID;
	private int currentServerIndex;
	private bool joining;
    private bool fadeBlur;
		
	public void ServerDetails(int pageNumber, int serverIndex, bool isOnline) {
		currentServerIndex = ((pageNumber - 1) * 16) + serverIndex;
		joinServerButton.GetComponent<ButtonAction>().sendMessage.numericalMessage.valueToSend = currentServerIndex;
		onlineList = isOnline;
	}
	
	void Awake() {
		GeneralVariables.lobbyManager = this;
		Topan.Network.AddNetworkEventListener(this);
	}
	
	void Start() {
		PopulatePlayerSlots();
		serverInfo.text = "Getting Data";
		startGameButton.isEnabled = false;
		backButton.isEnabled = true;
        startedCountdown = false;
		countdownParent.SetActive(false);
        refreshTime = refreshInterval + 1f;
        countdown = 3;
        waitingRed.alpha = 0f;
        waitingBlue.alpha = 0f;
        mapID = -1;

        if(MultiplayerMenu.disconnectMsg == 4) {
            NGUITools.PlaySound(disconnectionClip, 0.6f);
            camMove.transform.localPosition = new Vector3(3840f, 0f, -700f);

            disconnectionLabel.text = "You have lost connection to the server" + "\n" + "[A0A0A0]Reason: " + GetDisconnectionReason(4) + "[-]";
            MultiplayerMenu.disconnectMsg = -1;

            disconnectionPanel.alpha = 1f;
            disconnectionBlur.blurSpread = 0.9f;
        }
	}
	
	public void StartServerButton() {
        if(startedCountdown) {
            return;
        }

        UICamera.selectedObject = null;
		StartCoroutine(StartServerCoroutine());
	}
	
	private IEnumerator StartServerCoroutine() {
        startedCountdown = true;
		while(countdown >= 0) {
			GeneralVariables.connectionView.RPC(Topan.RPCMode.AllBuffered, "UpdateCountdown", (byte)countdown);
			yield return new WaitForSeconds(1f);
			countdown--;
		}
		
		yield return null;
		
		Map curMap = StaticMapsList.mapsArraySorted[(byte)Topan.Network.GetServerInfo("m")];
        		
		GeneralVariables.connectionView.RPC(Topan.RPCMode.All, "LoadGame", "");
		GeneralVariables.server.currentMapHash = "";		
		
		Loader.finished = () => {
            GeneralVariables.server.defaultGameTime = (byte)Topan.Network.GetServerInfo("dur") * 60;
            GeneralVariables.server.gameTime = GeneralVariables.server.defaultGameTime;
			GeneralVariables.Networking.finishedGame = false;
			Server.inLobby = false;
            Instantiate(spawnInterfacePrefab);
			Topan.Network.isMessageQueueRunning = true;
		};
		
		Loader.LoadLevel(curMap.sceneName);
	}	
	
	void Topan_OnPlayerConnected(Topan.NetworkPlayer p) {
//		Topan.Network.Instantiate(p, "NetworkSpeaker", Vector3.zero, Quaternion.identity, 0);	
	}

	void Update() {
        if(Time.time - refreshTime >= refreshInterval) {
            RefreshPlayerInfo();
            refreshTime = Time.time;
        }

        if(!GeneralVariables.gameModeHasTeams) {
            redTeamLabel.color = Color.white * 0.9f;
            redTeamLabel.text = "Players";
            blueTeamLabel.enabled = false;

            for(int i = 0; i < miscGMLabels.Length; i++) {
                miscGMLabels[i].color = new Color(0.7f, 0.7f, 0.7f, 1f);
            }
        }
        else {
            redTeamLabel.color = redTeamLabel.defaultColor;
            redTeamLabel.text = "Team Red";
            blueTeamLabel.enabled = true;

            for(int i = 0; i < miscGMLabels.Length; i++) {
                miscGMLabels[i].color = miscGMLabels[i].defaultColor;
            }
        }

        for(int i = 0; i < switchTeams.Length; i++) {
            switchTeams[i].enabled = GeneralVariables.gameModeHasTeams;
        }
        
        if(im == null) {
            curvePos = Mathf.MoveTowards(curvePos, Mathf.Clamp(3 - countdown, 0, 3), Time.deltaTime * 1.1f);
            fadeToBlack.alpha = fadeCurve.Evaluate(curvePos);
        }

        waitingRed.alpha = Mathf.MoveTowards(waitingRed.alpha, (redPanel.childCount <= 0) ? 1f : 0f, Time.deltaTime * 8f);
        waitingBlue.alpha = Mathf.MoveTowards(waitingBlue.alpha, (bluePanel.childCount <= 0) ? 1f : 0f, Time.deltaTime * 8f);

        joiningServerPanel.alpha = Mathf.MoveTowards(joiningServerPanel.alpha, (joiningLobby) ? 1f : 0f, Time.unscaledDeltaTime * 8f);
        disconnectionBlur.joiningServerBlur = joiningServerPanel.alpha * 0.95f;

        if(Topan.Network.isConnected && mapID > -1) {
            serverMapScreenshot.color = Color.Lerp(serverMapScreenshot.color, Color.white, Time.unscaledDeltaTime * 5f);
        }
        else {
            serverMapScreenshot.color = Color.black;
        }
	}

    public void Topan_DisconnectedFromServer() {
        if(Topan.Network.isServer) {
            return;
        }

        if(MultiplayerMenu.disconnectMsg < 0) {
            MultiplayerMenu.disconnectMsg = 0;
        }

        StartCoroutine(DisconnectionFromServer());
        joiningLobby = false;
    }

    public IEnumerator DisconnectionFromServer() {
        NGUITools.PlaySound(disconnectionClip, 0.6f);
        camMove.TargetPos(new Vector3(3840f, 0f, -700f));

        disconnectionLabel.text = "You have lost connection to the server" + "\n" + "[A0A0A0]Reason: " + GetDisconnectionReason(MultiplayerMenu.disconnectMsg) + "[-]";
        MultiplayerMenu.disconnectMsg = -1;

        while(disconnectionPanel.alpha < 1f) {
            disconnectionPanel.alpha = Mathf.MoveTowards(disconnectionPanel.alpha, 1f, Time.deltaTime * 6f);
            disconnectionBlur.blurSpread = disconnectionPanel.alpha * 0.9f;
            yield return null;
        }
    }

	public void Topan_ConnectionSuccessful() {
        joiningLobby = false;
	}
	
	public void OnJoin(int number) {
		if(joining) {
			return;
		}

		if(onlineList) {
			if(Topan.MasterServer.hosts == null) {
				return;
			}
			else if(Topan.MasterServer.hosts.Count <= 0) {
				return;
			}

            if(Topan.MasterServer.hosts[number].playerCount >= Topan.MasterServer.hosts[number].maxPlayers) {
                return;
            }
		}
		else {
			if(Topan.Network.foundLocalGames == null) {
				return;
			}
			else if(Topan.Network.foundLocalGames.Count <= 0) {
				return;
			}

            /* Local version of max player limit restriction. Dunno?
            if(Topan.MasterServer.hosts[number].playerCount == Topan.MasterServer.hosts[number].maxPlayers) {
                return;
            }
            */
		}
		
		lobbyChat.chatOutput.ClearChatList();
		NetworkingGeneral.CreateInstance(cachedNetworking);
        joiningLobby = true;
        
		TopanData initData = new TopanData();
		initData.Add("dat", NetworkingGeneral.ConvertToCombatant(AccountManager.profileData));
		UICamera.selectedObject = null;

		if(onlineList) {
            joinHostInfo = serverList.displayHostedServers[number];
			Topan.Network.Connect(joinHostInfo, initData);
		}
		else {
			Topan.FoundGame toConnectLocal = Topan.Network.foundLocalGames[number];
			Topan.Network.Connect(toConnectLocal.serverEndpoint, initData);
		}
	}
	
	public void RefreshPlayerInfo() {
		redIndex = 0;
		blueIndex = 0;
        unassignedIndex = 0;
		
		RefreshPlayers();
				
		if(Topan.Network.isConnected) {
            if(Topan.Network.HasServerInfo("m")) {
                mapID = (byte)Topan.Network.GetServerInfo("m");
            }
            else {
                mapID = -1;
            }

			serverInfo.text = "";
			if(Topan.Network.HasServerInfo("wnr")){
				int winnerteam = ((byte)Topan.Network.GetServerInfo("wnr")) - 1;
				if(winnerteam > -1) {
					serverInfo.text += ((winnerteam == 0) ? "[[C44524]Red Team[-]" : "[[377FB2]Blue Team[-]");
					serverInfo.text += " Won] \n";
				}
				else {
					serverInfo.text += "[Draw Match] \n";	
				}
			}
            
			int serverDuration = 0;
			if(Topan.Network.HasServerInfo("dur") && Topan.Network.GetServerInfo("dur") != null) {
				serverDuration = (byte)Topan.Network.GetServerInfo("dur") * 60;
			}
			else if(Topan.Network.isServer) {
				Topan.Network.SetServerInfo("dur", (byte)gameDurationSlider.currentDuration);
			}

            CombatantInfo serverProfile = (CombatantInfo)Topan.Network.server.GetInitialData("dat");
			serverInfo.text += "[D77A39]Room Name:[-] " + Topan.Network.GameName;
            int botsDisp = Mathf.Clamp(GeneralVariables.Networking.botCount, 0, Topan.Network.MaxPlayers - Topan.Network.connectedPlayers.Length);
            serverInfo.text += "\n" + "[D77A39]Players:[-] " + Topan.Network.connectedPlayers.Length + ((botsDisp > 0) ? " [AEBF95][+" + botsDisp.ToString() + "][-]" : "") + "/" + Topan.Network.MaxPlayers;
            serverInfo.text += "\n" + "[D77A39]Host:[-] " + ((serverProfile.clan != "") ? DarkRef.ClanColor(false) + "[" + serverProfile.clan + "][-] " : "") + serverProfile.username;
            serverInfo.text += "\n" + "[D77A39]Game Mode:[-] " + NetworkingGeneral.currentGameType.typeName;
			serverInfo.text += "\n" + "[D77A39]Round Duration:[-] " + (serverDuration / 60).ToString() + " minutes";

            if(mapID > -1) {
                serverInfo.text += "\n" + "[D77A39]Map Name:[-] " + StaticMapsList.mapsArraySorted[mapID].mapName;
                serverMapScreenshot.mainTexture = StaticMapsList.mapsArraySorted[mapID].previewIcon;
            }
		}
		else {
			serverInfo.text = "Not connected to a network";
			serverMapScreenshot.mainTexture = null;
            mapID = -1;
		}
	}

    public void SetLoading(bool e) {
        joiningLobby = e;

        if(!e && joinHostInfo != null) {
            Topan.Network.StopConnecting(joinHostInfo);
        }
    }
	
	public void ResetButtons() {
		startGameButton.isEnabled = true;
        backButton.isEnabled = true;
	}
	
	public void Disconnect() {
		if(Topan.Network.isConnected) {
			Topan.Network.Disconnect();
		}

        if(NetworkingGeneral.currentGameType != null) {
            NetworkingGeneral.currentGameType.ClearPlayerList();
		}

		redIndex = 0;
		blueIndex = 0;
        unassignedIndex = 0;
		RefreshPlayers();
		lobbyChat.chatOutput.ClearChatList();

        RefreshPlayerInfo();
		mpMenu.ServerEditMode(false);
		camMove.TargetPos(new Vector3(3840f, 0f, -700f));
        serverMapScreenshot.color = Color.black;

		if(GeneralVariables.Networking != null) {
			Destroy(GeneralVariables.Networking.gameObject);
			GeneralVariables.Networking = null;
		}

        StartCoroutine(RefreshDelayed());
	}

    private IEnumerator RefreshDelayed() {
        yield return new WaitForSeconds(0.2f);
        serverList.ForceRefreshServer();
    }
			
	private void RefreshPlayers() {
		allSuccess = true;

        for(int i = 0; i < playerInfoSlots.Length; i++) {
            PlayerLobbyGUI plg = playerInfoSlots[i].GetComponent<PlayerLobbyGUI>();
            plg.usernameLabel.text = "";
            plg.rankLabel.text = "";
            plg.transform.parent = transform;
        }
		
		if(Topan.Network.isConnected) {
            for(int p = 0; p < Topan.Network.connectedPlayers.Length; p++) {
                Topan.NetworkPlayer player = Topan.Network.connectedPlayers[p];
                byte team = (byte)player.GetPlayerData("team", (byte)0);
                AddPlayerToList(team, player, null);
            }

            if(GeneralVariables.gameModeHasTeams) {
                BotPlayer[] redBots = NetworkingGeneral.GetBotParticipants(0);
                for(int i = 0; i < redBots.Length; i++) {
                    AddPlayerToList(0, null, redBots[i]);
                }

                BotPlayer[] blueBots = NetworkingGeneral.GetBotParticipants(1);
                for(int i = 0; i < blueBots.Length; i++) {
                    AddPlayerToList(0, null, blueBots[i]);
                }
            }
            else if(BotManager.allBotPlayers != null) {
                for(int i = 0; i < BotManager.allBotPlayers.Length && i < GeneralVariables.Networking.botCount; i++) {
                    AddPlayerToList(2, null, BotManager.allBotPlayers[i]);
                }
            }
        }

        for(int i = 0; i < playerInfoSlots.Length; i++) {
            PlayerLobbyGUI plg = playerInfoSlots[i].GetComponent<PlayerLobbyGUI>();
            if(plg.usernameLabel.text == "") {
                plg.rankIcon.enabled = false;
            }
        }
	}

    private void AddPlayerToList(int team, Topan.NetworkPlayer player, BotPlayer bot = null) {
        int realTeamNum = team;
        if(player != null && !player.HasPlayerData("team")) {
            allSuccess = false;
            return;
        }

        bool thisIsBot = (player == null && bot != null);
        if(thisIsBot && team < 2) {
            realTeamNum = (int)bot.team;
        }

        GameObject info = GetAvailableSlot();
        if(info == null) {
            return;
        }

        bool showInfo = true;
        int index = 0;
        if(!GeneralVariables.gameModeHasTeams) {
            index = unassignedIndex;

            if(realTeamNum == 2) {
                if(index < 8) {
                    info.transform.parent = redPanel;
                }
                else {
                    info.transform.parent = bluePanel;
                }
            }
            else {
                showInfo = false;
            }

            info.transform.localPosition = new Vector3(0f, -(index % 8) * spacing, 0f);
        }
        else {
            if(realTeamNum == 0 && redIndex < 8) {
                index = redIndex;
                info.transform.parent = redPanel;
            }
            else if(realTeamNum == 1 && blueIndex < 8) {
                index = blueIndex;
                info.transform.parent = bluePanel;
            }
            else {
                showInfo = false;
            }

            info.transform.localPosition = new Vector3(0f, -index * spacing, 0f);
        }

        info.SetActive(showInfo);
        info.transform.localRotation = Quaternion.identity;
        info.transform.localScale = Vector3.one;

        PlayerLobbyGUI plg = info.GetComponent<PlayerLobbyGUI>();

        CombatantInfo pInfo = null;
        if(thisIsBot) {
            pInfo = bot.botInfo;
        }
        else {
            pInfo = (CombatantInfo)player.GetInitialData("dat");
        }

        plg.rankIcon.enabled = true;
        plg.usernameLabel.text = ((pInfo.clan != "") ? (((thisIsBot) ? (DarkRef.ClanColor(true) + "(" + pInfo.clan + ")") : (DarkRef.ClanColor(false) + "[" + pInfo.clan + "]")) + "[-] ") : "") + pInfo.username;
        plg.rankLabel.text = pInfo.rank.ToString();

        index++;
        if(!GeneralVariables.gameModeHasTeams) {
            if(realTeamNum == 2) {
                unassignedIndex = index;
            }
        }
        else {
            if(realTeamNum == 0) {
                redIndex = index;
            }
            else if(realTeamNum == 1) {
                blueIndex = index;
            }
        }
    }

    public void SwitchTeams(int team) {
        if(!GeneralVariables.gameModeHasTeams) {
            return;
        }

        Topan.NetworkPlayer p = Topan.Network.player;
        if(p.HasPlayerData("team") && ((byte)p.GetPlayerData("team") == (byte)team)) {
            return;
        }

        GeneralVariables.connectionView.RPC(Topan.RPCMode.Server, "SwitchTeams", (byte)Topan.Network.player.id, (byte)team);
    }

	private void PopulatePlayerSlots() {
		playerInfoSlots = new GameObject[16];
		
		for(int i = 0; i < playerInfoSlots.Length; i++) {
			playerInfoSlots[i] = (GameObject)Instantiate(playerInfoPrefab);
			playerInfoSlots[i].transform.parent = transform;

			PlayerLobbyGUI plg = playerInfoSlots[i].GetComponent<PlayerLobbyGUI>();
			plg.rankIcon.enabled = false;
			plg.usernameLabel.text = "";
			plg.rankLabel.text = "";
		}
	}

	private GameObject GetAvailableSlot() {
		for(int i = 0; i < playerInfoSlots.Length; i++) {
			GameObject slot = playerInfoSlots[i];
            if(string.IsNullOrEmpty(slot.GetComponent<PlayerLobbyGUI>().usernameLabel.text)) {
                return slot;
            }
		}

		return null;
	}

    private string GetDisconnectionReason(int type) {
        string result = "";

        if(type == 0) {
            result = "Host disconnected";
        }
        else if(type == 1) {
            result = "Kicked by host";
        }
        else if(type == 2) {
            result = "Exceeded ping limit (" + MultiplayerMenu.pingLimitSetting.ToString() + " ms)";
            MultiplayerMenu.pingLimitSetting = 0;
        }
        else if(type == 3) {
            result = "You idled for too long (" + MultiplayerMenu.idleTimeSetting.ToString() + " seconds)";
            MultiplayerMenu.idleTimeSetting = 0;
        }
        else if(type == 4) {
            result = "Response or sync timeout";
        }

        return result;
    }
}