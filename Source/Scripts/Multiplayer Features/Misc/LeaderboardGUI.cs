using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class NetPlayerInfo {
    public bool thisPlayerIsBot {
        get {
            return (botPlayer != null);
        }
    }

    public Topan.NetworkPlayer realPlayer = null;
    public BotPlayer botPlayer = null;

    public CombatantInfo myInfo {
        get {
            if(thisPlayerIsBot) {
                return botPlayer.botInfo;
            }
            else if(realPlayer.HasInitialData("dat")) {
                return (CombatantInfo)realPlayer.GetInitialData("dat");
            }

            return null;
        }
    }

    public int myTeam {
        get {
            return (thisPlayerIsBot) ? botPlayer.team : (byte)realPlayer.GetPlayerData("team", (byte)0);
        }
    }

    public int myKills = 0;
    public int myDeaths = 0;
    public int myHeads = 0;
    public int myScore = 0;
}

public class LeaderboardGUI : MonoBehaviour {
    public GameObject teamBasedLB;
    public GameObject individualBasedLB;
	public UILabel tdmSubheader;
    public UILabel dmSubheader;
	
	[HideInInspector]
	public UserStatsGUI[] redTeam;
	public Transform redStart;
	public UILabel totalStatsRed;
	
	[HideInInspector]
	public UserStatsGUI[] blueTeam;
	public Transform blueStart;
	public UILabel totalStatsBlue;

    [HideInInspector]
    public UserStatsGUI[] individuals;
    public Transform individualStart;
    public UILabel yourPlace;
	
	public UserStatsGUI individualUserPrefab;
    public UserStatsGUI teamUserPrefab;
    public float rowSpacing = 25f;
	public float refreshTime = 0.5f;
    public UILabel roundVictoryStats;
    public AlphaGroupUI waitForRed;
    public AlphaGroupUI waitForBlue;
    public AlphaGroupUI waitForPlayers;

    [HideInInspector]
    public List<NetPlayerInfo> sortedPlayers;
    [HideInInspector]
    public NetPlayerInfo yourPlayer;

	private float refreshTimer;

    private bool waitingForRed;
    private bool waitingForBlue;

    private UInt16 tkRed;
    private UInt16 tkBlue;
    private UInt16 tdRed;
    private UInt16 tdBlue;
	
	void Awake() {
        if(GeneralVariables.gameModeHasTeams) {
            teamBasedLB.SetActive(true);
            individualBasedLB.SetActive(false);

            redTeam = new UserStatsGUI[8];
            blueTeam = new UserStatsGUI[8];

            for(int i = 0; i < redTeam.Length; i++) {
                UserStatsGUI instance = (UserStatsGUI)Instantiate(teamUserPrefab);
                instance.gameObject.SetActive(false);
                instance.transform.parent = redStart;
                instance.transform.localPosition = Vector3.down * (i * rowSpacing);
                instance.transform.localScale = Vector3.one;
                redTeam[i] = instance;
            }

            for(int i = 0; i < blueTeam.Length; i++) {
                UserStatsGUI instance = (UserStatsGUI)Instantiate(teamUserPrefab);
                instance.gameObject.SetActive(false);
                instance.transform.parent = blueStart;
                instance.transform.localPosition = Vector3.down * (i * rowSpacing);
                instance.transform.localScale = Vector3.one;
                blueTeam[i] = instance;
            }
        }
        else {
            teamBasedLB.SetActive(false);
            individualBasedLB.SetActive(true);

            individuals = new UserStatsGUI[16];
            for(int i = 0; i < individuals.Length; i++) {
                UserStatsGUI instance = (UserStatsGUI)Instantiate(individualUserPrefab);
                instance.gameObject.SetActive(false);
                instance.transform.parent = individualStart;
                instance.transform.localPosition = Vector3.down * (i * rowSpacing);
                instance.transform.localScale = Vector3.one;
                individuals[i] = instance;
            }
        }
	}
	
	void Start() {
		refreshTimer = refreshTime;	
	}
	
	void Update() {
        if(!Topan.Network.isConnected) {
            return;
        }

		refreshTimer -= Time.unscaledDeltaTime;
		
		if(refreshTimer <= 0f) {
			Refresh();
		}

        if(GeneralVariables.gameModeHasTeams) {
            waitForRed.alpha = Mathf.Lerp(waitForRed.alpha, (GeneralVariables.gameModeHasTeams && waitingForRed) ? 1f : 0f, Time.unscaledDeltaTime * 8f);
            waitForBlue.alpha = Mathf.Lerp(waitForBlue.alpha, (GeneralVariables.gameModeHasTeams && waitingForBlue) ? 1f : 0f, Time.unscaledDeltaTime * 8f);
        }
        else {
            waitForPlayers.alpha = Mathf.Lerp(waitForPlayers.alpha, (GeneralVariables.gameModeHasTeams && GeneralVariables.Networking != null && Topan.Network.connectedPlayers.Length + GeneralVariables.Networking.botCount <= 1) ? 1f : 0f, Time.unscaledDeltaTime * 8f);
        }
	}	
	
	public void Refresh() {
        if(!Topan.Network.isConnected) {
            return;
        }

        sortedPlayers = new List<NetPlayerInfo>();

        for(int i = 0; i < Topan.Network.connectedPlayers.Length; i++) {
            Topan.NetworkPlayer curPlayer = Topan.Network.connectedPlayers[i];
            NetPlayerInfo npi = new NetPlayerInfo();

            npi.realPlayer = curPlayer;
            npi.myKills = (int)((UInt16)curPlayer.GetPlayerData("k", (UInt16)0));
            npi.myDeaths = (int)((UInt16)curPlayer.GetPlayerData("d", (UInt16)0));
            npi.myHeads = (int)((UInt16)curPlayer.GetPlayerData("h", (UInt16)0));
            npi.myScore = (int)curPlayer.GetPlayerData("sc", 0);
            sortedPlayers.Add(npi);

            if(curPlayer.id == Topan.Network.player.id) {
                yourPlayer = npi;
            }
        }

        if(GeneralVariables.gameModeHasTeams) {
            int redIndex = 0;
            int blueIndex = 0;
            int redTotalScore = 0;
            int blueTotalScore = 0;

            waitingForRed = true;
            waitingForBlue = true;

            BotPlayer[] redBots = NetworkingGeneral.GetBotParticipants(0);
            for(int i = 0; i < redBots.Length; i++) {
                BotStats hisStats = BotManager.GetBotStats(redBots[i].index);

                NetPlayerInfo npi = new NetPlayerInfo();
                npi.botPlayer = redBots[i];
                npi.myKills = hisStats.kills;
                npi.myDeaths = hisStats.deaths;
                npi.myHeads = hisStats.headshots;
                npi.myScore = hisStats.score;
                sortedPlayers.Add(npi);
            }

            BotPlayer[] blueBots = NetworkingGeneral.GetBotParticipants(1);
            for(int i = 0; i < blueBots.Length; i++) {
                BotStats hisStats = BotManager.GetBotStats(blueBots[i].index);

                NetPlayerInfo npi = new NetPlayerInfo();
                npi.botPlayer = blueBots[i];
                npi.myKills = hisStats.kills;
                npi.myDeaths = hisStats.deaths;
                npi.myHeads = hisStats.headshots;
                npi.myScore = hisStats.score;
                sortedPlayers.Add(npi);
            }

            if(NetworkingGeneral.currentGameType.sortPlayersBy == SortPlayersBy.Kills) {
                sortedPlayers.Sort((p1, p2) => p2.myKills.CompareTo(p1.myKills));
            }
            else if(NetworkingGeneral.currentGameType.sortPlayersBy == SortPlayersBy.Score) {
                sortedPlayers.Sort((p1, p2) => p2.myScore.CompareTo(p1.myScore));
            }

            tdmSubheader.text = NetworkingGeneral.currentGameType.typeName + " [" + Topan.Network.GameName + "]";
            for(int i = 0; i < sortedPlayers.Count; i++) {
                NetPlayerInfo current = sortedPlayers[i];
                if(current.botPlayer == null && current.realPlayer == null) {
                    continue;
                }

                if(current.myInfo == null) {
                    continue;
                }

                int kills = current.myKills;
                int deaths = current.myDeaths;
                int score = current.myScore;
                float kd = (deaths > 0) ? ((float)kills / (float)deaths) : kills;

                UserStatsGUI usg = null;
                if(current.myTeam == 0) {
                    if(redIndex >= 8) {
                        continue;
                    }

                    waitingForRed = false;
                    usg = redTeam[redIndex];
                    usg.gameObject.SetActive(true);
                    redTotalScore += score;
                    redIndex++;
                }
                else if(current.myTeam == 1) {
                    if(blueIndex >= 8) {
                        continue;
                    }

                    waitingForBlue = false;
                    usg = blueTeam[blueIndex];
                    usg.gameObject.SetActive(true);
                    blueTotalScore += score;
                    blueIndex++;
                }

                int thisListIndex = (current.myTeam == 0) ? redIndex : blueIndex;

                string clanNameFinal = (current.myInfo.clan != "") ? ((current.thisPlayerIsBot) ? DarkRef.ClanColor(true) + "(" + current.myInfo.clan + ")[-] " : DarkRef.ClanColor(false) + "[" + current.myInfo.clan + "][-] ") : "";
                usg.SetInfo(current.myInfo.rank.ToString(),
                    clanNameFinal + current.myInfo.username,
                    kills.ToString(),
                    deaths.ToString(),
                    kd.ToString("F2"),
                    current.myHeads.ToString(),
                    score.ToString(),
                    ((current.thisPlayerIsBot) ? Topan.Network.server.GetPlayerData("ping", 0).ToString() : current.realPlayer.GetPlayerData("ping", 0)).ToString(),
                    (!current.thisPlayerIsBot && current.realPlayer == Topan.Network.player),
                    (thisListIndex % 2 == 0));
            }

            if(redIndex < 7) {
                for(int i = redIndex; i < 8; i++) {
                    redTeam[i].gameObject.SetActive(false);
                }
            }

            if(blueIndex < 7) {
                for(int i = blueIndex; i < 8; i++) {
                    blueTeam[i].gameObject.SetActive(false);
                }
            }

            try {
                tkRed = (UInt16)Topan.Network.GetServerInfo("rTK");
                tkBlue = (UInt16)Topan.Network.GetServerInfo("bTK");
                tdRed = (UInt16)Topan.Network.GetServerInfo("rTD");
                tdBlue = (UInt16)Topan.Network.GetServerInfo("bTD");
            }
            catch {
                tkRed = 0;
                tkBlue = 0;
                tdRed = 0;
                tdBlue = 0;
            }

            string redTotal = "Total Kills: " + tkRed;
            redTotal += "\n" + "Total Deaths: " + tdRed;
            float redKD = (tdRed > 0) ? ((float)tkRed / (float)tdRed) : tkRed;
            redTotal += "\n" + "Total K/D: " + redKD.ToString("F2");
            redTotal += "\n" + "Team Score: " + redTotalScore.ToString();
            redTotal += "\n----------------";
            redTotal += "\n" + "Team Captures: 0"; //Placeholder, captures if capture the flag, defuses if demolition.

            string blueTotal = "Total Kills: " + tkBlue;
            blueTotal += "\n" + "Total Deaths: " + tdBlue;
            float blueKD = (tdBlue > 0) ? ((float)tkBlue / (float)tdBlue) : tkBlue;
            blueTotal += "\n" + "Total K/D: " + blueKD.ToString("F2");
            blueTotal += "\n" + "Team Score: " + blueTotalScore.ToString();
            blueTotal += "\n----------------";
            blueTotal += "\n" + "Team Captures: 0"; //Placeholder, captures if capture the flag, defuses if demolition.

            totalStatsRed.text = redTotal;
            totalStatsBlue.text = blueTotal;

            roundVictoryStats.text = "[CE1C1C](RED)[-] " + (byte)Topan.Network.GetServerInfo("rVic") + " [A0A0A0]|[-] " + (byte)Topan.Network.GetServerInfo("bVic") + " [2546A5](BLUE)[-]";
        }
        else {
            int currentIndex = 0;
            int yourIndex = 1;

            for(int i = 0; i < BotManager.allBotPlayers.Length && i < GeneralVariables.Networking.botCount; i++) {
                BotStats hisStats = BotManager.GetBotStats(i);

                NetPlayerInfo npi = new NetPlayerInfo();
                npi.botPlayer = BotManager.allBotPlayers[i];
                npi.myKills = hisStats.kills;
                npi.myDeaths = hisStats.deaths;
                npi.myHeads = hisStats.headshots;
                npi.myScore = hisStats.score;
                sortedPlayers.Add(npi);
            }

            if(NetworkingGeneral.currentGameType.sortPlayersBy == SortPlayersBy.Kills) {
                sortedPlayers.Sort((p1, p2) => p2.myKills.CompareTo(p1.myKills));
            }
            else if(NetworkingGeneral.currentGameType.sortPlayersBy == SortPlayersBy.Score) {
                sortedPlayers.Sort((p1, p2) => p2.myScore.CompareTo(p1.myScore));
            }

            dmSubheader.text = NetworkingGeneral.currentGameType.typeName + " [" + Topan.Network.GameName + "]";
            for(int i = 0; i < sortedPlayers.Count; i++) {
                NetPlayerInfo current = sortedPlayers[i];
                if(current.botPlayer == null && current.realPlayer == null) {
                    continue;
                }

                if(current.myInfo == null) {
                    continue;
                }

                int kills = current.myKills;
                int deaths = current.myDeaths;
                int score = current.myScore;
                float kd = (deaths > 0) ? ((float)kills / (float)deaths) : kills;

                UserStatsGUI usg = null;

                if(currentIndex >= 16) {
                    continue;
                }

                usg = individuals[currentIndex];
                usg.gameObject.SetActive(true);
                currentIndex++;

                if(!current.thisPlayerIsBot && current.realPlayer == Topan.Network.player) {
                    yourIndex = i;
                }

                string clanNameFinal = (current.myInfo.clan != "") ? ((current.thisPlayerIsBot) ? DarkRef.ClanColor(true) + "(" + current.myInfo.clan + ")[-] " : DarkRef.ClanColor(false) + "[" + current.myInfo.clan + "][-] ") : "";
                usg.SetInfo(current.myInfo.rank.ToString(),
                    clanNameFinal + current.myInfo.username,
                    kills.ToString(),
                    deaths.ToString(),
                    kd.ToString("F2"),
                    current.myHeads.ToString(),
                    score.ToString(),
                    ((current.thisPlayerIsBot) ? Topan.Network.server.GetPlayerData("ping", 0).ToString() : current.realPlayer.GetPlayerData("ping", 0)).ToString(),
                    (!current.thisPlayerIsBot && current.realPlayer == Topan.Network.player),
                    (currentIndex % 2 == 0));
            }

            if(currentIndex < 15) {
                for(int i = currentIndex; i < 16; i++) {
                    individuals[i].gameObject.SetActive(false);
                }
            }

            tkRed = 0;
            tkBlue = 0;
            tdRed = 0;
            tdBlue = 0;

            yourPlace.text = "You're in " + DarkRef.OrdinalIndicatorFormat(yourIndex + 1) + " place";
        }

		refreshTimer += refreshTime;
	}
}