using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Topan.CustomTypes;

public class Server : Topan.TopanMonoBehaviour {
    public static List<string> connectionBanList = new List<string>();
	public static bool inLobby = true;
	
	public GameObject prefabOwner;
	public GameObject prefabProxy;
    public GameObject prefabBot;

	public int gameTime = 600;

    [HideInInspector] public List<int> botRespawnQueue = new List<int>();
    [HideInInspector] public int defaultGameTime = 600;
	[HideInInspector] public string currentMapHash = "";
    [HideInInspector] public int currentRound = 1;
    [HideInInspector] public int amountOfRounds = 3;
    [HideInInspector] public int pingLimit = 500;

    [HideInInspector] public int redTeamRoundWins = 0;
    [HideInInspector] public int blueTeamRoundWins = 0;

    [HideInInspector] public bool startedMatch;
    [HideInInspector] public bool spawnBots = false;
	
    private bool countingDown;
    private bool setCheck;
    private float countTimer = 0f;
    private NetworkingGeneral nGeneral;
	private GameObject[] redTeamSpawns;
	private GameObject[] blueTeamSpawns;
    private GameObject[] unassignedSpawns;

    private int _count = 0;
    private byte lastSent = 255;
    private int countdown {
        get {
            return _count;
        }
        set {
            _count = value;

            byte val = (byte)Mathf.Clamp(_count, 0, 255);
            if(Topan.Network.isConnected && val != lastSent) {
                topanNetworkView.RPC(Topan.RPCMode.All, "MatchCountdown", val);
                lastSent = val;
            }
        }
    }

    private float pingCheckTimer = 0f;
    private int[] pingViolations = new int[16];

    private List<int> inflictors = new List<int>();
    private List<float> dmgValues = new List<float>();
    private List<int> toUpdateBots = new List<int>();

    private BotPlayer[] redParticipantBots;
    private BotPlayer[] blueParticipantBots;
    private BotPlayer[] oldBots;
	
	public void InstantiateServer() {
        startedMatch = false;
	    NetworkingGeneral.gameChatList.Add("[9ABF19]Initialized server[-]");

        currentRound = 1;
	}
		
	[RPC]
	public void LoadGame(string hash) {
		Topan.Network.isMessageQueueRunning = false;
	}
	
	private void CounterReduce() {
		if(!inLobby) {
			gameTime--;

            if(gameTime <= 0 && !GetComponent<NetworkingGeneral>().finishedGame) {
				FinishGame(NetworkingGeneral.currentGameType.GetWinnerDefinite());	
			}
			
			topanNetworkView.RPC(Topan.RPCMode.All, "SyncServerTime", gameTime);
		}	
	}
	
	void Awake() {
		if(Topan.Network.isServer) {
			Topan.Network.AddNetworkEventListener(this);
			topanNetworkView.observedComponents.Add(this);
            spawnBots = true;
		}
		else {
			Destroy(this);
            return;
		}

        nGeneral = GetComponent<NetworkingGeneral>();
	}

    void Update() {
        if(!Topan.Network.isConnected) {
            return;
        }

        //Bot Management
        if(!inLobby && BotManager.allBotPlayers != null) {
            oldBots = BotManager.allBotPlayers;

            if(GeneralVariables.gameModeHasTeams) {
                for(int i = 0; i < BotManager.allBotPlayers.Length; i++) {
                    BotManager.allBotPlayers[i].isParticipating = false;
                }

                redParticipantBots = NetworkingGeneral.GetBotParticipants(0);
                for(int i = 0; i < redParticipantBots.Length; i++) {
                    redParticipantBots[i].isParticipating = true;
                }

                blueParticipantBots = NetworkingGeneral.GetBotParticipants(1);
                for(int i = 0; i < blueParticipantBots.Length; i++) {
                    blueParticipantBots[i].isParticipating = true;
                }
            }
            else {
                for(int i = 0; i < BotManager.allBotPlayers.Length; i++) {
                    BotManager.allBotPlayers[i].isParticipating = (i < GeneralVariables.Networking.botCount);
                }
            }

            /* Experimental stuff...
            for(int i = 0; i < BotManager.allBotPlayers.Length; i++) {
                if(!BotManager.allBotPlayers[i].isParticipating) {
                    if(GeneralVariables.Networking.botInstances[i] != null) {
                        GeneralVariables.Networking.botInstances[i].gameObject.GetComponent<Topan.NetworkView>().Deallocate();
                        Debug.Log("Destroying instance of bot");
                    }
                }
            }

            for(int i = 0; i < BotManager.allBotPlayers.Length; i++) {
                if(BotManager.allBotPlayers[i].isParticipating != oldBots[i].isParticipating) {
                    if(BotManager.allBotPlayers[i].isParticipating) {
                        botRespawnQueue.Add(i);
                        Debug.Log("Re-participating bot, respawning!: " + BotManager.allBotPlayers[i].botInfo.username);
                    }
                }
            }
            */
        }

        if(Topan.Network.HasServerInfo("pl") && (byte)Topan.Network.GetServerInfo("pl") <= 0) {
            StartMatch();
        }

        if(!Topan.Network.HasServerInfo("sm") || startedMatch != setCheck) {
            Topan.Network.SetServerInfo("sm", startedMatch);
            setCheck = startedMatch;
        }

        if(nGeneral.countingDown && spawnBots) {
            StartCoroutine(SpawnAllBots());
            spawnBots = false;
        }

        if(startedMatch && !nGeneral.finishedGame) {
            countTimer += Time.deltaTime;
            if(countTimer >= 1f) {
                CounterReduce();
                countTimer -= 1f;
            }
        }
        else if(nGeneral.countingDown) {
            if(Input.GetKey(KeyCode.X) && Input.GetKeyDown(KeyCode.V)) {
                countdown = 0;
            }
        }

        for(int i = 0; i < botRespawnQueue.Count; i++) {
            if(!BotManager.allBotPlayers[botRespawnQueue[i]].isParticipating) {
                botRespawnQueue.RemoveAt(i);
                continue;
            }

            StartCoroutine(RespawnBot(botRespawnQueue[i], 5f + UnityEngine.Random.value));
            botRespawnQueue.RemoveAt(i);
        }

        if(!inLobby) {
            pingCheckTimer += Time.unscaledDeltaTime;
            if(pingCheckTimer >= 1f) {
                CheckPing();
                pingCheckTimer -= 1f;
            }
        }
    }

    private IEnumerator SpawnAllBots() {
        for(int i = 0; i < BotManager.allBotPlayers.Length; i++) {
            if(!BotManager.allBotPlayers[i].isParticipating) {
                continue;
            }
            
            InstantiateBot(i);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.15f, 0.3f));
        }
    }

    [RPC]
    public void RequestInstantiate(byte pID) {
        Topan.NetworkPlayer player = Topan.Network.GetPlayerByID(pID);
        string teamTag = ((byte)player.GetPlayerData("team") == 0) ? "RedSpawn" : "BlueSpawn";
        if(!GeneralVariables.gameModeHasTeams) {
            teamTag = "UnassignedSpawn";
        }

        GameObject[] spawns = GetSpawnPoints(teamTag);
        if(!GeneralVariables.gameModeHasTeams && spawns.Length <= 0) {
            Debug.Log("Consider adding unassigned spawn points for this map...");
            spawns = GetSpawnPoints((UnityEngine.Random.Range(0, 2) == 1) ? "RedSpawn" : "BlueSpawn");
        }

        Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));

        if(spawns.Length > 0) {
            Transform spawn = spawns[UnityEngine.Random.Range(0, spawns.Length)].transform;
            Topan.Network.Instantiate(player, prefabProxy, prefabOwner, prefabProxy, (spawn.position + randomPos), spawn.rotation, 0);
        }
        else {
            Topan.Network.Instantiate(player, prefabProxy, prefabOwner, prefabProxy, randomPos, Quaternion.identity, 0);
        }
    }

    private IEnumerator RespawnBot(int botIndex, float delay) {
        yield return new WaitForSeconds(delay);

        while(GeneralVariables.Networking.botInstances[botIndex] != null) {
            yield return null;
        }

        InstantiateBot(botIndex);
    }

    public void InstantiateBot(int botIndex) {
        if(!Topan.Network.isServer || GeneralVariables.Networking.botInstances[botIndex] != null) {
            return;
        }

        /*
        if(AstarPath.active == null) {
            Debug.LogError("No AI Path object is active! Please create one in order to support bots for this map!");
            return;
        }

        BotPlayer bot = BotManager.allBotPlayers[botIndex];
        if(!bot.isParticipating) {
            return;
        }
        string teamTag = (bot.team == 0) ? "RedSpawn" : "BlueSpawn";
        if(!GeneralVariables.gameModeHasTeams) {
            teamTag = "UnassignedSpawn";
        }

        GameObject[] spawns = GetSpawnPoints(teamTag);
        if(!GeneralVariables.gameModeHasTeams && spawns.Length <= 0) {
            Debug.Log("Consider adding unassigned spawn points for this map...");
            spawns = GetSpawnPoints((DarkRef.RandomRange(0, 1) == 1) ? "RedSpawn" : "BlueSpawn");
        }

        Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));

        Dictionary<string, object> initData = new Dictionary<string, object>();
        initData.Add("i", (byte)botIndex);
        initData.Add("sw", (byte)UnityEngine.Random.Range(0, WeaponDatabase.publicGunControllers.Length));

        if(spawns.Length > 0) {
            Transform spawn = spawns[UnityEngine.Random.Range(0, spawns.Length)].transform;
            Topan.Network.Instantiate(Topan.Network.server, prefabBot, (spawn.position + randomPos), spawn.rotation, 0, initData);
        }
        else {
            Topan.Network.Instantiate(Topan.Network.server, prefabBot, randomPos, Quaternion.identity, 0, initData);
        }        */

    }

    [RPC]
    public void KilledPlayer(string data) {
        if(!startedMatch) {
            return;
        }

        bool isSuicide = false;
        bool didHeadshot = false;
        bool isGrenade = false;
        
        int victim = 0;
        int killerDamageID = 0;
        int weaponID = 0;
        float totalDamage = 0f;

        inflictors.Clear();
        dmgValues.Clear();
        toUpdateBots.Clear();

        string[] splitCateg = data.Split(new string[]{"."}, System.StringSplitOptions.None);

        if(splitCateg.Length == 1) {
            string[] playerData = splitCateg[0].Split(new string[]{","}, System.StringSplitOptions.RemoveEmptyEntries);
            victim = int.Parse(playerData[0]);

            if(playerData.Length == 1) {
                isSuicide = true;
            }
        }
        else if(splitCateg.Length >= 2) {
            string[] playerData = splitCateg[0].Split(new string[]{","}, System.StringSplitOptions.RemoveEmptyEntries);
            victim = int.Parse(playerData[0]);

            if(playerData.Length >= 2) {
                for(int i = 1; i < playerData.Length; i++) {
                    string realID = playerData[i];
                    if(playerData[i].Contains("!")) {
                        realID = realID.Substring(0, realID.Length - 1);
                        didHeadshot = true;
                    }

                    if(playerData.Length == 2) {
                        killerDamageID = i - 1;
                        inflictors.Insert(0, int.Parse(realID));
                    }
                    else if(playerData[i].Contains("k")) {
                        killerDamageID = i - 1;
                        realID = realID.Substring(0, realID.Length - 1);
                        inflictors.Insert(0, int.Parse(realID));
                    }
                    else {
                        inflictors.Add(int.Parse(realID));
                    }
                }
            }

            if(playerData.Length > 2) {
                string[] dmgData = splitCateg[1].Split(new string[]{","}, System.StringSplitOptions.RemoveEmptyEntries);

                for(int i = 0; i < dmgData.Length; i++) {
                    if(dmgData[i].Contains("-")) {
                        continue;
                    }

                    dmgValues.Add(float.Parse(dmgData[i]));
                    totalDamage += dmgValues[i];
                }

                dmgValues.RemoveAt(killerDamageID);
            }

            if(splitCateg.Length == 3) {
                if(splitCateg[2].Contains("*")) {
                    isGrenade = true;
                    splitCateg[2] = splitCateg[2].Substring(0, splitCateg[2].Length - 1);
                }

                weaponID = int.Parse(splitCateg[2]);
            }
        }

        bool killerIsBot = (inflictors.Count > 0 && inflictors[0] >= 64);
        bool victimIsBot = (victim >= 64);

        Topan.NetworkPlayer klr = null;
        Topan.NetworkPlayer vctm = null;
        BotPlayer klrBot = null;
        BotPlayer vctmBot = null;

        if(!isSuicide) {
            if(killerIsBot) {
                klrBot = BotManager.allBotPlayers[inflictors[0] - 64];
            }
            else {
                klr = Topan.Network.GetPlayerByID(inflictors[0]);
            }
        }

        if(victimIsBot) {
            vctmBot = BotManager.allBotPlayers[victim - 64];
        }
        else {
            vctm = Topan.Network.GetPlayerByID(victim);
        }

        byte vctmTeam = ((victimIsBot) ? vctmBot.team : (byte)vctm.GetPlayerData("team"));
        if(!isSuicide) {
            byte klrTeam = ((killerIsBot) ? klrBot.team : (byte)klr.GetPlayerData("team"));
            bool isTeamKill = (klrTeam == vctmTeam);

            if(!(isTeamKill && GeneralVariables.gameModeHasTeams)) {
                if(!killerIsBot) {
                    UInt16 pastKills = (UInt16)klr.GetPlayerData("k");
                    klr.SetPlayerData("k", (UInt16)(pastKills + 1));

                    int pastScore = (int)klr.GetPlayerData("sc");
                    klr.SetPlayerData("sc", pastScore + ((victimIsBot) ? 50 : 100) + ((didHeadshot) ? ((victimIsBot) ? 13 : 25) : 0));

                    if(klr == Topan.Network.server) {
                        nGeneral.AddToActionFeed(didHeadshot, victimIsBot, 0f);
                    }
                    else {
                        topanNetworkView.RPC(new Topan.NetworkPlayer[] { klr }, "AddToActionFeed", didHeadshot, victimIsBot, (TopanFloat)0f);
                    }
                }
                else {
                    klrBot.botStats.kills++;
                    klrBot.botStats.score += (victimIsBot) ? 50 : 100;

                    if(!toUpdateBots.Contains(klrBot.index)) {
                        toUpdateBots.Add(klrBot.index);
                    }
                }

                if(klrTeam == 0) {
                    UInt16 prevRedTotal = (UInt16)Topan.Network.GetServerInfo("rTK");
                    Topan.Network.SetServerInfo("rTK", (UInt16)(prevRedTotal + 1));
                }
                else if(klrTeam == 1) {
                    UInt16 prevBlueTotal = (UInt16)Topan.Network.GetServerInfo("bTK");
                    Topan.Network.SetServerInfo("bTK", (UInt16)(prevBlueTotal + 1));
                }

                if(didHeadshot) {
                    if(!killerIsBot) {
                        UInt16 pastHS = (UInt16)klr.GetPlayerData("h");
                        klr.SetPlayerData("h", (UInt16)(pastHS + 1));
                    }
                    else {
                        klrBot.botStats.headshots++;
                        klrBot.botStats.score += (victimIsBot) ? 13 : 25;
                    }
                }
            }
        }

        for(int i = 1; i < inflictors.Count; i++) {
            bool assistorIsBot = (inflictors[i] >= 64);
            Topan.NetworkPlayer asstr = null;

            if(!assistorIsBot) {
                asstr = Topan.Network.GetPlayerByID(inflictors[i]);

                UInt16 pastAssists = (UInt16)asstr.GetPlayerData("a", (UInt16)0);
                asstr.SetPlayerData("a", (UInt16)(pastAssists + 1));

                if(asstr == Topan.Network.server) {
                    nGeneral.AddToActionFeed(false, victimIsBot, dmgValues[i - 1] / totalDamage);
                }
                else {
                    topanNetworkView.RPC(new Topan.NetworkPlayer[]{asstr}, "AddToActionFeed", false, victimIsBot, (TopanFloat)(dmgValues[i - 1] / totalDamage));
                }
            }
            else {
                BotPlayer iBot = BotManager.allBotPlayers[inflictors[i] - 64];
                iBot.botStats.score += Mathf.RoundToInt((dmgValues[i - 1] / totalDamage) * 100 * ((victimIsBot) ? 0.5f : 1f));

                if(!toUpdateBots.Contains(iBot.index)) {
                    toUpdateBots.Add(iBot.index);
                }
            }
        }

        if(!victimIsBot) {
            UInt16 pastDeaths = (UInt16)vctm.GetPlayerData("d");
            vctm.SetPlayerData("d", (UInt16)(pastDeaths + 1));
        }
        else {
            vctmBot.botStats.deaths++;

            if(!toUpdateBots.Contains(vctmBot.index)) {
                toUpdateBots.Add(vctmBot.index);
            }
        }

        if(vctmTeam == 0) {
            UInt16 prevRedTotal = (UInt16)Topan.Network.GetServerInfo("rTD");
            Topan.Network.SetServerInfo("rTD", (UInt16)(prevRedTotal + 1));
        }
        else if(vctmTeam == 1) {
            UInt16 prevBlueTotal = (UInt16)Topan.Network.GetServerInfo("bTD");
            Topan.Network.SetServerInfo("bTD", (UInt16)(prevBlueTotal + 1));
        }

        for(int i = 0; i < toUpdateBots.Count; i++) {
            BotPlayer upd = BotManager.allBotPlayers[toUpdateBots[i]];
            Topan.Network.SetServerInfo("bS" + upd.index.ToString(), BotManager.ParseToBotFormat(upd.botStats));
        }

        if(GeneralVariables.multiplayerGUI != null) {
            GeneralVariables.multiplayerGUI.AddToKillFeed((isSuicide) ? (byte)victim : (byte)inflictors[0], (byte)victim, (byte)Mathf.Clamp(weaponID + ((isGrenade) ? 200 : 0), 0, 255));
            GeneralVariables.multiplayerGUI.topanNetworkView.RPC(Topan.RPCMode.Others, "AddToKillFeed", (isSuicide) ? (byte)victim : (byte)inflictors[0], (byte)victim, (byte)Mathf.Clamp(weaponID + ((isGrenade) ? 200 : 0), 0, 255));
        }

        int winner = NetworkingGeneral.currentGameType.GetWinner();
        if(winner != -1) {
            FinishGame(winner);
            return;
        }
    }
	
	[RPC]
	public void LoadLobby() {
        redTeamSpawns = null;
        blueTeamSpawns = null;

		if(Application.loadedLevelName != "Main Menu") {
			Loader.finished = () => {
				GameObject.FindWithTag("MainCamera").GetComponent<CameraMove>().TargetPos(new Vector3(3840, -800, -700));
				inLobby = true;
				GeneralVariables.lobbyManager.ResetButtons();
				GeneralVariables.lobbyManager.lobbyChat.Start();
			};	
			Loader.LoadLevel("Main Menu");
		}
		else {
			GameObject.FindWithTag("MainCamera").GetComponent<CameraMove>().TargetPos(new Vector3(3840, -800, -700));
		}
	}

    [RPC]
    public void SwitchTeams(byte pID, byte team) {
        Topan.NetworkPlayer p = Topan.Network.GetPlayerByID((int)pID);
        if(p.HasPlayerData("team") && ((byte)p.GetPlayerData("team") == (byte)team)) {
            return;
        }

        bool autoBalance = true;
        if(NetworkingGeneral.currentGameType.customSettings.ContainsKey("Team Auto-Balance")) {
            autoBalance = DarkRef.ConvertStringToBool(NetworkingGeneral.currentGameType.customSettings["Team Auto-Balance"].currentValue);
        }

        if(autoBalance && !NetworkingGeneral.currentGameType.ValidTeamSwitch((int)team)) {
            return;
        }

        NetworkingGeneral.currentGameType.RemovePlayer((int)pID);
        NetworkingGeneral.currentGameType.AddPlayer((int)pID, (int)team);
        p.SetPlayerData("team", team);
    }
	
	public void FinishGame(int winner) {
        if(winner == 0) {
            redTeamRoundWins++;
            Topan.Network.SetServerInfo("rVic", (byte)redTeamRoundWins);
        }
        else if(winner == 1) {
            blueTeamRoundWins++;
            Topan.Network.SetServerInfo("bVic", (byte)blueTeamRoundWins);
        }

        topanNetworkView.RPC(Topan.RPCMode.All, "EndRound", (byte)(winner + 1), (byte)currentRound);
	}
		
	public void Topan_OnPlayerConnected(Topan.NetworkPlayer p) {
        if(p.id > 0 && connectionBanList.Contains(p.peerObject.UniqueIdentifier.ToString())) {
            topanNetworkView.RPC(new int[1]{p.id}, "KickPlayer", (byte)1);
            return;
        }

		p.SetPlayerData("k", (UInt16)0);
        p.SetPlayerData("a", (UInt16)0);
		p.SetPlayerData("d", (UInt16)0);
		p.SetPlayerData("h", (UInt16)0);
        p.SetPlayerData("sc", 0);

        int playerTeam = NetworkingGeneral.currentGameType.GetTeamAssign(p.id);
		p.SetPlayerData("team", (byte)playerTeam);	

		CombatantInfo pData = (CombatantInfo)p.GetInitialData("dat");
		topanNetworkView.RPC(Topan.RPCMode.All, "JoinMSG", pData.username);
		
		if(!p.isServer) {
			if(!inLobby) {
				topanNetworkView.RPC(new int[1]{p.id}, "LoadGame", currentMapHash);
			}
			else {
				topanNetworkView.RPC(new int[1]{p.id}, "LoadLobby");	
			}
		}
	}

    public void Topan_OnPlayerDisconnected(Topan.NetworkPlayer p) {
		NetworkingGeneral.currentGameType.RemovePlayer(p.id);
        CombatantInfo pData = (CombatantInfo)p.GetInitialData("dat");

        if(!connectionBanList.Contains(p.peerObject.UniqueIdentifier.ToString())) {
            topanNetworkView.RPC(Topan.RPCMode.All, "LeaveMSG", pData.username);
        }

		Topan.Network.DestroyPlayerViews(p.id);
	}

    public void CheckPing() {
        for(int i = 1; i < Topan.Network.connectedPlayers.Length; i++) {
            Topan.NetworkPlayer player = Topan.Network.GetPlayerByID(i);
            if(player == null) {
                continue;
            }

            if(player.HasPlayerData("ping") && (int)player.GetPlayerData("ping", 0) >= pingLimit) {
                pingViolations[i]++;
                Debug.Log("Player with ID of " + i + " has violated ping limit [x" + pingViolations[i] + "]");
            }
            else {
                pingViolations[i] = 0;
            }

            if(pingViolations[i] >= 5) {
                topanNetworkView.RPC(new int[1]{i}, "KickPlayer", (byte)2);
                Debug.Log("Kicked player " + i + " because of ping limit violations");
            }
        }
    }
	
	private GameObject[] GetSpawnPoints(string teamTag) {
		if(redTeamSpawns == null || blueTeamSpawns == null || unassignedSpawns == null) {
			redTeamSpawns = GameObject.FindGameObjectsWithTag("RedSpawn");
			blueTeamSpawns = GameObject.FindGameObjectsWithTag("BlueSpawn");
            unassignedSpawns = GameObject.FindGameObjectsWithTag("UnassignedSpawn");
		}
		
		if(teamTag == "RedSpawn") {
			return redTeamSpawns;
		}
		else if(teamTag == "BlueSpawn") {
			return blueTeamSpawns;
		}
        else if(teamTag == "UnassignedSpawn") {
            return unassignedSpawns;
        }

		return null;
	}

    public void StartMatch() {
        if(startedMatch || countingDown || nGeneral.finishedGame) {
            return;
        }

        StartCoroutine(StartedMatch());
    }

    private IEnumerator StartedMatch() {
        countingDown = true;
        countdown = 10;

        while(countdown >= 0) {
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        countingDown = false;
        startedMatch = true;
        gameTime++;
        countTimer = 1f;
    }

    public void RestartRound() {
        startedMatch = false;
        setCheck = false;
        spawnBots = true;
        Topan.Network.SetServerInfo("sm", false);
        gameTime = defaultGameTime;
        topanNetworkView.RPC(Topan.RPCMode.All, "SyncServerTime", gameTime);
    }
}