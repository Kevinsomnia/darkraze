using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RoundEndManager : MonoBehaviour
{
    public static bool isRoundEnded = false;

    public Transform panelRoot;
    public GameObject teamBasedRoot;
    public GameObject individualBasedRoot;

    public float statsSpacing = 25f;
    public VignettingC vignetting;
    public UILabel roundStatusLabel;
    public UILabel extraInfo;
    public Color32 victoryColor = new Color32(89, 199, 26, 255);
    public Color32 drawColor = new Color32(128, 128, 128, 255);
    public Color32 defeatColor = new Color32(189, 26, 26, 255);
    public UILabel redTeamStatus;
    public UILabel blueTeamStatus;
    public UISprite blackStuff;
    public Vector3 teamHeaderTargetPos = new Vector3(0f, 270f, 0f);
    public Vector3 indHeaderTargetPos = new Vector3(0f, 250f, 0f);
    public AlphaGroupUI headerUI;
    public UIPanel otherPanel;
    public Transform redStart;
    public Transform blueStart;
    public Transform individualStart;
    public UserStatsGUI scoreboardPrefab;
    public UILabel roundLabel;
    public UILabel countdownLabel;
    public UILabel redWins;
    public UILabel blueWins;
    public UILabel matchStatistics;
    public UILabel spectatorList;
    public UISprite statsBackground;
    public UISprite statsOutline;
    public UIProgressBar expProgressBar;
    public UILabel xpLabel;
    public UILabel xpLeftLabel;
    public UILabel currencyLabel;
    public UILabel rankLabel;
    public UILabel nextRankLabel;
    public AudioSource expAccumulateSound;
    public float expAnimationDelay = 1f;
    public float expBaseAnimationTime = 2f;
    public float expSlowdownTime = 0.9f;

    private int startExp;
    private int earnedExp;
    private int earnedCurrency;
    private float currentExpValue;
    private float currentCurrencyValue;
    private float animationSpeed;

    private int countToNextRound;
    private int curRoundDisplay;
    private int roundLimit;
    private int winningTeam;

    private List<NetPlayerInfo> sortedPlayers;
    private string redCol = "[F0452D]";
    private string neutralCol = "[FFFFFF]";
    private string greenCol = "[7CCE3B]";

    void Awake()
    {
        countToNextRound = 10;
        vignetting.intensity = 0f;
        blackStuff.alpha = 0f;
        Screen.lockCursor = false;
        UICamera.selectedObject = null;

        sortedPlayers = new List<NetPlayerInfo>();

        for (int i = 0; i < Topan.Network.connectedPlayers.Length; i++)
        {
            Topan.NetworkPlayer curPlayer = Topan.Network.connectedPlayers[i];

            NetPlayerInfo npi = new NetPlayerInfo();
            npi.realPlayer = curPlayer;
            npi.myKills = (int)((UInt16)curPlayer.GetPlayerData("k", (UInt16)0));
            npi.myDeaths = (int)((UInt16)curPlayer.GetPlayerData("d", (UInt16)0));
            npi.myHeads = (int)((UInt16)curPlayer.GetPlayerData("h", (UInt16)0));
            npi.myScore = (int)curPlayer.GetPlayerData("sc", 0);
            sortedPlayers.Add(npi);
        }

        if (GeneralVariables.gameModeHasTeams)
        {
            teamBasedRoot.SetActive(true);
            Destroy(individualBasedRoot);

            int redIndex = 0;
            int blueIndex = 0;

            sortedPlayers = new List<NetPlayerInfo>();
            for (int i = 0; i < Topan.Network.connectedPlayers.Length; i++)
            {
                Topan.NetworkPlayer curPlayer = Topan.Network.connectedPlayers[i];

                NetPlayerInfo npi = new NetPlayerInfo();
                npi.realPlayer = curPlayer;
                npi.myKills = (int)((UInt16)curPlayer.GetPlayerData("k", (UInt16)0));
                npi.myDeaths = (int)((UInt16)curPlayer.GetPlayerData("d", (UInt16)0));
                npi.myHeads = (int)((UInt16)curPlayer.GetPlayerData("h", (UInt16)0));
                npi.myScore = (int)curPlayer.GetPlayerData("sc", 0);
                sortedPlayers.Add(npi);
            }

            BotPlayer[] redBots = NetworkingGeneral.GetBotParticipants(0);
            for (int i = 0; i < redBots.Length; i++)
            {
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
            for (int i = 0; i < blueBots.Length; i++)
            {
                BotStats hisStats = BotManager.GetBotStats(blueBots[i].index);

                NetPlayerInfo npi = new NetPlayerInfo();
                npi.botPlayer = blueBots[i];
                npi.myKills = hisStats.kills;
                npi.myDeaths = hisStats.deaths;
                npi.myHeads = hisStats.headshots;
                npi.myScore = hisStats.score;
                sortedPlayers.Add(npi);
            }

            if (NetworkingGeneral.currentGameType.sortPlayersBy == SortPlayersBy.Kills)
            {
                sortedPlayers.Sort((p1, p2) => p2.myKills.CompareTo(p1.myKills));
            }
            else if (NetworkingGeneral.currentGameType.sortPlayersBy == SortPlayersBy.Score)
            {
                sortedPlayers.Sort((p1, p2) => p2.myScore.CompareTo(p1.myScore));
            }

            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                NetPlayerInfo current = sortedPlayers[i];
                if (current.botPlayer == null && current.realPlayer == null)
                {
                    continue;
                }

                if (current.myInfo == null)
                {
                    continue;
                }

                int kills = current.myKills;
                int deaths = current.myDeaths;
                int score = current.myScore;
                float kd = (deaths > 0) ? ((float)kills / (float)deaths) : kills;

                UserStatsGUI usg = null;
                if (current.myTeam == 0)
                {
                    UserStatsGUI instance = (UserStatsGUI)Instantiate(scoreboardPrefab);
                    instance.transform.parent = redStart;
                    instance.transform.localPosition = Vector3.down * (redIndex * statsSpacing);
                    instance.transform.localScale = Vector3.one;
                    usg = instance;
                    redIndex++;
                }
                else
                {
                    UserStatsGUI instance = (UserStatsGUI)Instantiate(scoreboardPrefab);
                    instance.transform.parent = blueStart;
                    instance.transform.localPosition = Vector3.down * (blueIndex * statsSpacing);
                    instance.transform.localScale = Vector3.one;
                    usg = instance;
                    blueIndex++;
                }

                int thisListIndex = (current.myTeam == 0) ? redIndex : blueIndex;

                string clanNameFinal = (current.myInfo.clan != "") ? ((current.thisPlayerIsBot) ? DarkRef.ClanColor(true) + "(" + current.myInfo.clan + ")[-] " : DarkRef.ClanColor(false) + "[" + current.myInfo.clan + "][-] ") : "";
                usg.SetInfo(((current.myInfo != null) ? current.myInfo.rank : 1).ToString(),
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
        }
        else
        {
            Destroy(teamBasedRoot);
            individualBasedRoot.SetActive(true);

            int currentIndex = 0;

            for (int i = 0; i < BotManager.allBotPlayers.Length && i < GeneralVariables.Networking.botCount; i++)
            {
                BotStats hisStats = BotManager.GetBotStats(i);

                NetPlayerInfo npi = new NetPlayerInfo();
                npi.botPlayer = BotManager.allBotPlayers[i];
                npi.myKills = hisStats.kills;
                npi.myDeaths = hisStats.deaths;
                npi.myHeads = hisStats.headshots;
                npi.myScore = hisStats.score;
                sortedPlayers.Add(npi);
            }

            if (NetworkingGeneral.currentGameType.sortPlayersBy == SortPlayersBy.Kills)
            {
                sortedPlayers.Sort((p1, p2) => p2.myKills.CompareTo(p1.myKills));
            }
            else if (NetworkingGeneral.currentGameType.sortPlayersBy == SortPlayersBy.Score)
            {
                sortedPlayers.Sort((p1, p2) => p2.myScore.CompareTo(p1.myScore));
            }

            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                NetPlayerInfo current = sortedPlayers[i];
                if (current.botPlayer == null && current.realPlayer == null)
                {
                    continue;
                }

                if (current.myInfo == null)
                {
                    continue;
                }

                int kills = current.myKills;
                int deaths = current.myDeaths;
                int score = current.myScore;
                float kd = (deaths > 0) ? ((float)kills / (float)deaths) : kills;

                if (currentIndex >= 16)
                {
                    continue;
                }

                UserStatsGUI instance = (UserStatsGUI)Instantiate(scoreboardPrefab);
                instance.transform.parent = individualStart;
                instance.transform.localPosition = Vector3.down * (currentIndex * statsSpacing);
                instance.transform.localScale = Vector3.one;

                currentIndex++;

                string clanNameFinal = (current.myInfo.clan != "") ? ((current.thisPlayerIsBot) ? DarkRef.ClanColor(true) + "(" + current.myInfo.clan + ")[-] " : DarkRef.ClanColor(false) + "[" + current.myInfo.clan + "][-] ") : "";
                instance.SetInfo(((current.myInfo != null) ? current.myInfo.rank : 1).ToString(),
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
        }

        if (GeneralVariables.gameModeHasTeams)
        {
            roundLabel.cachedTrans.localPosition = new Vector3(0f, 245f, -7f);
            spectatorList.cachedTrans.localPosition = new Vector3(-395f, -316f, 0f);
            panelRoot.localPosition = Vector3.zero;
            statsBackground.SetDimensions(270, 530);
            statsOutline.SetDimensions(278, 540);
        }
        else
        {
            roundLabel.cachedTrans.localPosition = new Vector3(0f, 225f, -7f);
            spectatorList.cachedTrans.localPosition = new Vector3(-395f, -261f, 0f);
            panelRoot.localPosition = Vector3.up * -20f;
            statsBackground.SetDimensions(270, 440);
            statsOutline.SetDimensions(278, 450);
        }

        int roundScore = (int)Topan.Network.player.GetPlayerData("sc", 0);
        int targetExp = AccountManager.GetTargetExperienceForRank(AccountManager.profileData.rank);
        startExp = AccountManager.profileData.curXP;
        currentExpValue = startExp;
        earnedExp = roundScore;
        earnedCurrency = Mathf.RoundToInt(roundScore * UnityEngine.Random.Range(0.95f, 1.05f) * 0.237f);
        expBaseAnimationTime = Mathf.Max(0.01f, expBaseAnimationTime);
        animationSpeed = (float)earnedExp / expBaseAnimationTime;
        expSlowdownTime = Mathf.Clamp(expSlowdownTime, 0f, 0.99f);

        expAccumulateSound.volume = 0f;
        expProgressBar.value = (float)startExp / (float)targetExp;
        xpLabel.text = startExp.ToString() + " XP / " + targetExp.ToString() + " XP";
        xpLeftLabel.text = (targetExp - startExp).ToString() + " XP REMAINING";
        currencyLabel.alpha = 0f;
        currencyLabel.text = "+0 CREDITS";
        rankLabel.text = "[u]" + AccountManager.profileData.username + "[/u]" + "\n" + "RANK " + AccountManager.profileData.rank.ToString() + " [ROOKIE]";
        nextRankLabel.text = "RANK " + (AccountManager.profileData.rank + 1).ToString() + " [ROOKIE]";

        StartCoroutine(ProcessData());
    }

    void Update()
    {
        roundLimit = (byte)Topan.Network.GetServerInfo("rc");
        roundLabel.text = "ROUND " + curRoundDisplay.ToString() + ((roundLimit < 255) ? "/" + roundLimit.ToString() : "") + " ENDED";
        countdownLabel.text = ((curRoundDisplay == roundLimit) ? "RETURNING TO LOBBY IN: " : "NEXT ROUND STARTS IN: ") + countToNextRound.ToString();
    }

    private IEnumerator StartProgressAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);

        bool expDone = false;

        float accumExp = 0f;
        int expLeft = earnedExp;
        while (!expDone)
        {
            int expToNextRank = AccountManager.GetTargetExperienceForRank(AccountManager.profileData.rank);

            int stepAmount = Mathf.Min(Mathf.RoundToInt(Time.deltaTime * animationSpeed * (1f - (Mathf.Clamp01(accumExp / earnedExp) * expSlowdownTime))), expLeft);
            currentExpValue += (float)stepAmount;
            expLeft -= stepAmount;

            float roundedValue = Mathf.Round(currentExpValue);

            if (roundedValue >= expToNextRank)
            {
                LevelUp();
            }

            expProgressBar.value = currentExpValue / expToNextRank;
            xpLabel.text = roundedValue.ToString() + " XP / " + expToNextRank.ToString() + " XP";
            xpLeftLabel.text = (expToNextRank - roundedValue).ToString() + " XP REMAINING";

            expDone = (expLeft <= 0);
            expAccumulateSound.volume = (expDone) ? 0f : 0.1f;
            expAccumulateSound.pitch = 1.1f;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        AccountManager.profileData.curXP = Mathf.RoundToInt(currentExpValue);

        while (currencyLabel.alpha < 1f)
        {
            currencyLabel.alpha += Time.deltaTime * 5f;
            yield return null;
        }

        bool currencyDone = false;
        while (!currencyDone)
        {
            currentCurrencyValue = Mathf.Lerp(currentCurrencyValue, earnedCurrency, Time.deltaTime * 2f);
            currencyLabel.text = "+[b]" + Mathf.Ceil(currentCurrencyValue).ToString() + "[/b] CREDITS";

            currencyDone = (Mathf.Ceil(currentCurrencyValue) == (float)earnedCurrency);
            expAccumulateSound.volume = (currencyDone) ? 0f : 0.1f;
            expAccumulateSound.pitch = 0.9f;
            yield return null;
        }

        yield return new WaitForSeconds(2.5f);

        AccountManager.profileData.currency += earnedCurrency;
        currencyLabel.GetComponent<TextTransition>().UpdateText("[b]" + AccountManager.profileData.currency.ToString() + "[/b] CREDITS");
    }

    private void LevelUp()
    {
        int deduction = AccountManager.GetTargetExperienceForRank(AccountManager.profileData.rank);
        currentExpValue -= deduction;
        AccountManager.profileData.rank++;
        rankLabel.text = "[u]" + AccountManager.profileData.username + "[/u]" + "\n" + "RANK " + AccountManager.profileData.rank.ToString() + " [ROOKIE]";
        nextRankLabel.text = "RANK " + (AccountManager.profileData.rank + 1).ToString() + " [ROOKIE]";
    }

    private IEnumerator ProcessData()
    {
        WWWForm newForm = new WWWForm();
        newForm.AddField("id", AccountManager.databaseID);
        newForm.AddField("e", earnedExp);
        newForm.AddField("c", earnedCurrency);
        newForm.AddField("k", (int)((UInt16)Topan.Network.player.GetPlayerData("k", (UInt16)0)));
        newForm.AddField("d", (int)((UInt16)Topan.Network.player.GetPlayerData("d", (UInt16)0)));
        newForm.AddField("h", (int)((UInt16)Topan.Network.player.GetPlayerData("h", (UInt16)0)));
        newForm.AddField("a", (int)((UInt16)Topan.Network.player.GetPlayerData("a", (UInt16)0)));

        WWW updateData = new WWW("http://darkraze.byethost6.com/dir/darkraze_files/accounts/update_request.php", newForm);

        yield return updateData;
    }

    public void TeamWinner(int team, int curRound)
    {
        winningTeam = team;
        curRoundDisplay = curRound;
        isRoundEnded = true;

        if (GeneralVariables.gameModeHasTeams)
        {
            byte yourTeam = (byte)Topan.Network.player.GetPlayerData("team");

            if (team <= -1)
            {
                redTeamStatus.text = "[505050](DRAW)";
                blueTeamStatus.text = "[505050](DRAW)";
            }
            else
            {
                redTeamStatus.text = (team == 0) ? "[6F9042](VICTORY)" : "[B92214](DEFEAT)";
                blueTeamStatus.text = (team == 1) ? "[6F9042](VICTORY)" : "[B92214](DEFEAT)";
            }

            if (team == yourTeam)
            {
                roundStatusLabel.text = "VICTORY";
                roundStatusLabel.color = victoryColor;
                extraInfo.text = "YOUR TEAM HAS WON!";
            }
            else if (team <= -1)
            {
                roundStatusLabel.text = "DRAW";
                roundStatusLabel.color = drawColor;
                extraInfo.text = "YOUR TEAM HAS TIED!";
            }
            else
            {
                roundStatusLabel.text = "DEFEAT";
                roundStatusLabel.color = defeatColor;
                extraInfo.text = "YOUR TEAM HAS LOST!";
            }

            byte rWins = (byte)Topan.Network.GetServerInfo("rVic");
            byte bWins = (byte)Topan.Network.GetServerInfo("bVic");

            string rTextCol = neutralCol;
            if (rWins > bWins)
            {
                rTextCol = greenCol;
            }
            else if (rWins < bWins)
            {
                rTextCol = redCol;
            }

            redWins.text = "[FFFFFF]-[-] " + rTextCol + rWins.ToString() + " WIN" + ((rWins == 1) ? "" : "S");

            string bTextCol = neutralCol;
            if (bWins > rWins)
            {
                bTextCol = greenCol;
            }
            else if (bWins < rWins)
            {
                bTextCol = redCol;
            }

            blueWins.text = "[FFFFFF]-[-] " + bTextCol + bWins.ToString() + " WIN" + ((bWins == 1) ? "" : "S");
        }
        else
        {
            string winnerName = (team >= 64) ? BotManager.allBotPlayers[team - 64].botInfo.username : ((CombatantInfo)Topan.Network.GetPlayerByID(team).GetInitialData("dat")).username;

            if (team == Topan.Network.player.id)
            {
                roundStatusLabel.text = "VICTORY";
                roundStatusLabel.color = victoryColor;
                extraInfo.text = "YOU HAVE DOMINATED!";
            }
            else
            {
                roundStatusLabel.text = "DEFEAT";
                roundStatusLabel.color = defeatColor;
                extraInfo.text = winnerName.ToUpper() + " WON THE MATCH!";
            }
        }

        string builtText = "[b]MATCH STATISTICS[/b]";
        builtText += "\n" + "\n" + "Placeholder text" + "\n" + "Ran 2851 meters" + "\n" + "Fired 1522 bullets" + "\n" + "Best Weapon: NR-94";

        if (GeneralVariables.gameModeHasTeams)
        {
            builtText += "\n" + "\n" + "RED TEAM" + "\n" + "\n";
            builtText += "Total Kills: 0" + "\n" + "Total Deaths: 0" + "\n" + "Total K/D: 0.00" + "\n" + "Total Score: 0" + "\n" + "-----------------" + "\n";
            builtText += "Team Captures: 0" + "\n" + "Bomb Plants: 0" + "\n" + "Bomb Defuses: 0";
            builtText += "\n" + "\n" + "-------------------------------";
            builtText += "\n" + "\n" + "BLUE TEAM" + "\n" + "\n";
            builtText += "Total Kills: 0" + "\n" + "Total Deaths: 0" + "\n" + "Total K/D: 0.00" + "\n" + "Total Score: 0" + "\n" + "-----------------" + "\n";
            builtText += "Team Captures: 0" + "\n" + "Bomb Plants: 0" + "\n" + "Bomb Defuses: 0";
        }
        else
        {
            builtText += "\n" + "\n" + "Best Sharpshooter: DaBossTMR";
            builtText += "\n" + "Best Comedian: DaBossTMR";
            builtText += "\n" + "Everything Else: DaBossTMR";
        }

        matchStatistics.text = builtText;

        StartCoroutine(FadeGUI());
    }

    private IEnumerator FadeGUI()
    {
        RestrictionManager.allInput = true;
        headerUI.transform.localPosition = Vector3.zero;
        headerUI.transform.localScale = Vector3.one * 1.3f;

        headerUI.alpha = 0f;
        otherPanel.alpha = 0f;
        roundLabel.alpha = 0f;

        yield return new WaitForSeconds(0.65f);

        while (blackStuff.alpha < 0.6f || headerUI.alpha < 1f)
        {
            vignetting.intensity = Mathf.MoveTowards(vignetting.intensity, 1.5f, Time.deltaTime * 3f);
            blackStuff.alpha = Mathf.MoveTowards(blackStuff.alpha, 0.6f, Time.deltaTime * 1.2f);
            GeneralVariables.uiController.GetComponent<GameManager>().remBlur = blackStuff.alpha * 1.6f;

            if (GeneralVariables.spawnController)
            {
                GeneralVariables.spawnController.blurREM = blackStuff.alpha * 1.2f;
            }

            headerUI.alpha = Mathf.MoveTowards(headerUI.alpha, 1f, Time.deltaTime * 0.9f);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        Vector3 realTargetPos = (GeneralVariables.gameModeHasTeams) ? teamHeaderTargetPos : indHeaderTargetPos;
        while ((realTargetPos - headerUI.transform.localPosition).sqrMagnitude > 0.01f)
        {
            headerUI.transform.localPosition = Vector3.Lerp(headerUI.transform.localPosition, realTargetPos, Time.deltaTime * 4.4f);
            headerUI.transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, headerUI.transform.localPosition.sqrMagnitude / realTargetPos.sqrMagnitude);
            yield return null;
        }

        float time = 0f;
        while (time < 1f)
        {
            otherPanel.alpha = time;
            roundLabel.alpha = time;
            time += Time.deltaTime * 2f;
            yield return null;
        }

        StartCoroutine(StartProgressAnimation(expAnimationDelay));

        countToNextRound = 30;
        while (countToNextRound > 0)
        {
            yield return new WaitForSeconds(1f);
            countToNextRound--;
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(StartNewRound());
    }

    private IEnumerator StartNewRound()
    {
        float time = 1f;
        while (blackStuff.alpha < 1f || time > 0f)
        {
            blackStuff.alpha = Mathf.MoveTowards(blackStuff.alpha, 1f, Time.deltaTime);
            time -= Time.deltaTime * 2f;
            headerUI.alpha = time;
            otherPanel.alpha = time;
            roundLabel.alpha = time;
            yield return null;
        }

        GeneralVariables.uiController.GetComponent<GameManager>().remBlur = 0f;

        yield return null;

        if (roundLimit < 255)
        {
            if (curRoundDisplay >= roundLimit)
            {
                Topan.Network.SetServerInfo("wnr", (byte)(winningTeam + 1));
                GeneralVariables.Networking.GetComponent<Topan.NetworkView>().RPC(Topan.RPCMode.All, "LoadLobby");
            }
        }

        if (Topan.Network.isServer)
        {
            Topan.Network.SetServerInfo("pl", (byte)Topan.Network.MaxPlayers);
            GeneralVariables.server.currentRound++;
            GeneralVariables.server.RestartRound();
        }

        GeneralVariables.spawnController.gameObject.SetActive(true);
        GeneralVariables.spawnController.FadeInOut(true);
        GeneralVariables.spawnController.blurREM = 0f;

        GeneralVariables.Networking.finishedGame = false;

        if (GeneralVariables.player != null)
        {
            Destroy(GeneralVariables.player);
        }

        for (int i = 0; i < GeneralVariables.Networking.availablePlayers.Length; i++)
        {
            GeneralVariables.Networking.availablePlayers[i].GetComponent<Topan.NetworkView>().Destroy();
        }

        for (int i = 0; i < GeneralVariables.Networking.availableBots.Length; i++)
        {
            GeneralVariables.Networking.availableBots[i].GetComponent<Topan.NetworkView>().Destroy();
        }

        if (Topan.Network.isServer)
        {
            foreach (Topan.NetworkPlayer p in Topan.Network.connectedPlayers)
            {
                p.SetPlayerData("k", (UInt16)0);
                p.SetPlayerData("a", (UInt16)0);
                p.SetPlayerData("d", (UInt16)0);
                p.SetPlayerData("h", (UInt16)0);
                p.SetPlayerData("sc", 0);
            }

            foreach (BotPlayer b in BotManager.allBotPlayers)
            {
                if (b == null)
                {
                    continue;
                }

                b.botStats.kills = 0;
                b.botStats.deaths = 0;
                b.botStats.headshots = 0;
                b.botStats.score = 0;

                Topan.Network.SetServerInfo("bS" + b.index.ToString(), BotManager.ParseToBotFormat(b.botStats));
            }

            Topan.Network.SetServerInfo("rTK", (UInt16)0);
            Topan.Network.SetServerInfo("bTK", (UInt16)0);
            Topan.Network.SetServerInfo("rTD", (UInt16)0);
            Topan.Network.SetServerInfo("bTD", (UInt16)0);
        }

        GeneralVariables.Networking.playerInstances = new Transform[Topan.Network.MaxPlayers];
        isRoundEnded = false;

        yield return null;
        Destroy(gameObject);
    }
}