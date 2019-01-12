using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NetworkingGeneral : MonoBehaviour {
    [System.Serializable]
    public class GrenadeSync {
        public Rigidbody instance = null;
        public Vector3 syncErrorOffset = Vector3.zero;
    }

    /// <summary>
    /// Current game mode that is selected
    /// </summary>
    public static GameTypeInterface currentGameType = new TeamDeathmatch();

    /// <summary>
    /// Chat list to be synced through all chat options. Should be cleared at every start of a new network instance.
    /// </summary>
    public static List<string> gameChatList = new List<string>();
    
    public static bool initialized = false;
    public static Dictionary<int, GrenadeSync> syncGrenadesList;

    public static Topan.NetworkPlayer[] GetTeamPlayers(int team) {
        if(!Topan.Network.isConnected || !GeneralVariables.gameModeHasTeams) {
            return null;
        }

        return Topan.Network.connectedPlayers.Where(pl => (byte)pl.GetPlayerData("team", (byte)0) == team).ToArray();
    }

    public static bool friendlyFire {
        get {
            if(Topan.Network.isConnected && currentGameType.customSettings.ContainsKey("Friendly Fire")) {
                return DarkRef.ConvertStringToBool(currentGameType.customSettings["Friendly Fire"].currentValue);
            }

            return Topan.Network.isConnected;
        }
    }

	public AudioClip[] clipsToPlay = new AudioClip[0];
    public AudioClip matchCountSound;
    public AudioClip newMessageSound;
    public RoundEndManager roundEndPrefab;
    public float grenadeSyncLerpTime = 8f;

    [HideInInspector] public SpawnController_MP spawnControl;
    [HideInInspector] public int countdown = 10;
    [HideInInspector] public bool countingDown;
    [HideInInspector] public bool finishedGame = false;

    public int botCount {
        get {
            if(!Topan.Network.HasServerInfo("bC")) {
                return 0;
            }

            return (byte)Topan.Network.GetServerInfo("bC");
        }
    }

    [HideInInspector] public Transform[] playerInstances;
    [HideInInspector] public Transform[] botInstances;

    public Transform[] availablePlayers {
        get {
            return playerInstances.Where(pl => pl != null).ToArray();
        }
    }

    /// <summary>
    /// Bot instances excluding nulls
    /// </summary>
    public Transform[] availableBots {
        get {
            return botInstances.Where(bot => bot != null).ToArray();
        }
    }

    public bool matchStarted {
        get {
            return (Topan.Network.isConnected && Topan.Network.HasServerInfo("sm") && (bool)Topan.Network.GetServerInfo("sm"));
        }
    }
	
	void Awake() {
        InitializeSyncObjects();
        playerInstances = new Transform[Topan.Network.MaxPlayers];
        botInstances = new Transform[16];
		Topan.Network.AddNetworkEventListener(this);
		DontDestroyOnLoad(gameObject);
        countdown = 5;
        countingDown = false;
	}

    void Update() {
        InterpolatePositions();
    }

    public static void InitializeSyncObjects() {
        if(initialized) {
            return;
        }

        syncGrenadesList = new Dictionary<int, GrenadeSync>();
        initialized = true;
    }
	
	void Topan_DisconnectedFromServer() {
		if(Application.loadedLevelName == "Main Menu") {
			return;
		}

        if(!Topan.Network.isServer) {
            if(MultiplayerMenu.disconnectMsg < 0) {
                MultiplayerMenu.disconnectMsg = 0;
            }
        }

		Loader.LoadLevel("Main Menu");
	}
		
	public static void CreateInstance(GameObject cached = null) {
		if(GeneralVariables.Networking == null) {
			GameObject inst = (GameObject)Instantiate((cached == null) ? Resources.Load("Networking") : cached);
            GeneralVariables.Networking = inst.GetComponent<NetworkingGeneral>();
		}
	}

	[RPC]
	public void PlaySound(Vector3 pos, byte clipIndex) {
		AudioSource.PlayClipAtPoint(clipsToPlay[clipIndex], pos);
	}

    [RPC]
	public void ChatMessage(string msg) {
		if(!string.IsNullOrEmpty(msg)) {
            gameChatList.Add(msg);
            NGUITools.PlaySound(newMessageSound, 0.1f, 2f);
		}
	}
	
	[RPC]
    public void JoinMSG(string username) {
        string msg = "[DAA314]" + username + "[-] entered the room.";
        gameChatList.Add(msg);
        NGUITools.PlaySound(newMessageSound, 0.1f, 2f);
	}
	
	[RPC]
    public void LeaveMSG(string username) {
        if(username != AccountManager.profileData.username) {
            string msg = "[DAA314]" + username + "[-] left the room.";
            gameChatList.Add(msg);
            NGUITools.PlaySound(newMessageSound, 0.1f, 2f);
        }
	}

    [RPC]
    public void AddToActionFeed(bool headshot, bool isBot, float assistVal) {
        ActionFeedManager afm = GeneralVariables.uiController.mpGUI.afManager;

        if(afm == null) {
            return;
        }

        if(assistVal <= 0f) {
            if(isBot) {
                afm.AddToFeed("Kill [bot]", 50);
            }
            else {
                afm.AddToFeed("Kill", 100);
            }
            
            GeneralVariables.uiController.crosshairs.ShowHitMarker(true);

            if(headshot) {
                afm.AddToFeed("Headshot", (isBot) ? 13 : 25, true);
            }
        }
        else {
            afm.AddToFeed("Assist" + ((isBot) ? " [bot]" : ""), Mathf.RoundToInt(assistVal * 100f * ((isBot) ? 0.5f : 1f)), true);
        }
    }

    [RPC]
	public void UpdateCountdown(byte num) {
        ServerLobby lobby = GeneralVariables.lobbyManager;
        lobby.countdown = (int)num;
		lobby.countdownParent.SetActive(true);
		lobby.startGameButton.isEnabled = false;
        lobby.backButton.isEnabled = false;
		NGUITools.PlaySound(lobby.countdownSound, 0.15f);
		lobby.countdownText.text = "GAME STARTING IN: " + lobby.countdown.ToString();
		
		if(lobby.countdown <= 0 && !Topan.Network.isServer) {
			StartCoroutine(FinishedCountdown());
		}
	}

    [RPC]
    public void MatchCountdown(byte count) {
        countdown = (int)count;
        countingDown = true;
        NGUITools.PlaySound(matchCountSound, 0.1f, 0.9f);

        if(count == 0) {
            Invoke("StopCounting", 1f);
        }
    }

    private void StopCounting() {
        countingDown = false;
    }

    [RPC]
    public void SyncGrenade(int gID, Vector3 pos, Vector3 velo) {
        if(!syncGrenadesList.ContainsKey(gID) || (syncGrenadesList.ContainsKey(gID) && syncGrenadesList[gID].instance == null)) {
            return;
        }

        GrenadeSync gSync = syncGrenadesList[gID];
        gSync.syncErrorOffset = (pos - gSync.instance.position);

        if(!gSync.instance.isKinematic) {
            gSync.instance.velocity = velo;
        }
    }

    private void InterpolatePositions() {
        List<int> grenadeIDs = new List<int>(syncGrenadesList.Keys);

        foreach(int id in grenadeIDs) {
            if(syncGrenadesList[id] == null) {
                continue;
            }

            float distanceFromTarget = syncGrenadesList[id].syncErrorOffset.magnitude;

            if(distanceFromTarget > 0f) {
                Vector3 offset = syncGrenadesList[id].syncErrorOffset.normalized * Mathf.Min(distanceFromTarget, Time.deltaTime * grenadeSyncLerpTime);
                syncGrenadesList[id].instance.position += offset;
                syncGrenadesList[id].syncErrorOffset -= offset;
            }
        }
    }

    [RPC]
    public void EndRound(byte teamID, byte roundNum) {
        finishedGame = true;

        RoundEndManager rem = (RoundEndManager)Instantiate(roundEndPrefab, Vector3.down * 175f, Quaternion.identity);
        rem.TeamWinner(teamID - 1, roundNum);
    }

    [RPC]
    public void KickPlayer(byte kickInfo) {
        if(Topan.Network.isServer) {
            return;
        }

        if(kickInfo == 2) {
            MultiplayerMenu.pingLimitSetting = (int)Topan.Network.GetServerInfo("pl");
        }
        else if(kickInfo == 3) {
            MultiplayerMenu.idleTimeSetting = (byte)Topan.Network.GetServerInfo("it") * 5;
        }

        MultiplayerMenu.disconnectMsg = kickInfo;
        Topan.Network.Disconnect();
        Loader.LoadLevel("Main Menu");
    }

    private IEnumerator FinishedCountdown() {
		yield return new WaitForSeconds(1f);
		GeneralVariables.lobbyManager.countdownText.text = "Awaiting connection...";
	}

    public void StartSpectating() {
        StartCoroutine(SpectateTimer());
    }

    private IEnumerator SpectateTimer() {
        float timer = 0f;
        while(timer < 5f) {
            timer += Time.deltaTime;
            yield return null;
        }

        GeneralVariables.Networking.DisplaySpawnScreen();
    }

    public void DisplaySpawnScreen() {
        StartCoroutine(FadeToSpawnScreen());
    }

    private IEnumerator FadeToSpawnScreen() {
        UIController uic = GeneralVariables.uiController;

        while(uic.fadeFromBlack.alpha < 1f) {
            uic.fadeFromBlack.alpha += Time.unscaledDeltaTime * 4.5f;
            yield return null;
        }

        UICamera.selectedObject = null;
        RestrictionManager.restricted = false;
        string transferText = "";
        if(!string.IsNullOrEmpty(uic.mpGUI.chatInput.value)) {
            transferText = uic.mpGUI.chatInput.value;
            uic.mpGUI.chatInput.value = string.Empty;
        }
        
        spawnControl.spectatorCamera.enabled = false;
        yield return null;
        spawnControl.gameObject.SetActive(true);
        spawnControl.FadeInOut(true);

        if(transferText != "") {
            spawnControl.chatInput.value = transferText;
            UICamera.selectedObject = spawnControl.chatInput.gameObject;
        }

        yield return null;
        spawnControl.spectatorCamera.enabled = true;

        if(GeneralVariables.spectatorCamera != null) {
            Destroy(GeneralVariables.spectatorCamera.gameObject);
        }
    }

    public static BotPlayer[] GetBotParticipants(int team) {
        int redCount = NetworkingGeneral.GetTeamPlayers(0).Length;
        int blueCount = NetworkingGeneral.GetTeamPlayers(1).Length;

        List<BotPlayer> toReturn = new List<BotPlayer>();
        int targetBots = Mathf.Clamp(Topan.Network.MaxPlayers - (redCount + blueCount), 0, GeneralVariables.Networking.botCount);

        if(!GeneralVariables.gameModeHasTeams) {
            for(int i = 0; i < targetBots; i++) {
                toReturn.Add(BotManager.allBotPlayers[i]);
            }
            
            return toReturn.ToArray();
        }

        int rBotIndex = 0;
        int bBotIndex = 0;
        
        while(rBotIndex + bBotIndex < targetBots) {
            if(redCount <= blueCount && redCount < 8) {
                if(team == 0) {
                    toReturn.Add(BotManager.redBotPlayers[rBotIndex]);
                }

                rBotIndex++;
                redCount++;
            }
            else if(blueCount < redCount && blueCount < 8) {
                if(team == 1) {
                    toReturn.Add(BotManager.blueBotPlayers[bBotIndex]);
                }

                bBotIndex++;
                blueCount++;
            }
        }

        return toReturn.ToArray();
    }

    public static CombatantInfo ConvertToCombatant(AccountManager.ProfileInfo profData) {
        CombatantInfo newInfo = new CombatantInfo();
        newInfo.username = profData.username;
        newInfo.clan = profData.clan;
        newInfo.rank = profData.rank;
        return newInfo;
    }
}

/// <summary>
/// This class is used for syncing over gameplay for leaderboard (or other display) types of access.
/// </summary>
[System.Serializable]
public class CombatantInfo {
    public string username = "Guest";
    public string clan = "";
    public int rank = 1;
}