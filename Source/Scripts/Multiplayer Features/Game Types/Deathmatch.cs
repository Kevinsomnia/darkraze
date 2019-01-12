using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Deathmatch : GameTypeInterface
{
    public class KillGroup
    {
        public KillGroup(int i, int k)
        {
            id = i;
            kills = k;
        }

        public int id = 0;
        public int kills = 0;
    }

    public string typeName
    {
        get
        {
            return "Deathmatch";
        }
    }

    private Dictionary<string, GameType.GameTypeSetting> _customSettings = new Dictionary<string, GameType.GameTypeSetting>();
    public Dictionary<string, GameType.GameTypeSetting> customSettings
    {
        get
        {
            return _customSettings;
        }
    }

    private SortPlayersBy _spb = SortPlayersBy.Kills;
    public SortPlayersBy sortPlayersBy
    {
        get
        {
            return _spb;
        }
    }

    //List of player IDs (no bots!)
    public List<int> unassignedPlayers = new List<int>();

    public Dictionary<int, int> _killsPerPlayer = new Dictionary<int, int>();
    public Dictionary<int, int> killsPerTeam
    {
        get
        {
            _killsPerPlayer.Clear();
            for (int i = 0; i < Topan.Network.connectedPlayers.Length; i++)
            {
                int kills = (int)((UInt16)Topan.Network.GetPlayerByID(i).GetPlayerData("k", (UInt16)0));
                _killsPerPlayer.Add(i, kills);
            }

            for (int i = 0; i < BotManager.allBotPlayers.Length && i < GeneralVariables.Networking.botCount; i++)
            {
                _killsPerPlayer.Add(i + 64, BotManager.allBotPlayers[i].botStats.kills);
            }

            return _killsPerPlayer;
        }
    }

    private bool initialized;

    public void InitializeSettings()
    {
        if (initialized)
        {
            return;
        }

        _customSettings.Add("Kill Limit", new GameType.GameTypeSetting(GameType.SettingType.Slider, "50"));

        initialized = true;
    }

    public void AddPlayer(int playerID, int team)
    {
        unassignedPlayers.Add(playerID);
    }

    public void RemovePlayer(int playerID)
    {
        unassignedPlayers.Remove(playerID);
    }

    public void ClearPlayerList()
    {
        unassignedPlayers.Clear();
    }

    public int GetTeamAssign(int playerID)
    {
        if (playerID > -1)
        {
            unassignedPlayers.Add(playerID); //-1 is a bot, and you don't want to add bots to the player list.
        }

        return 2;
    }

    public int GetWinner()
    {
        foreach (KeyValuePair<int, int> kills in killsPerTeam)
        {
            if (kills.Value >= (int)GameType.GetSettingValue(customSettings["Kill Limit"]))
            {
                return kills.Key;
            }
        }

        return -1;
    }

    public int GetWinnerDefinite()
    {
        List<KillGroup> killers = new List<KillGroup>();
        foreach (KeyValuePair<int, int> kvp in killsPerTeam)
        {
            killers.Add(new KillGroup(kvp.Key, kvp.Value));
        }

        killers.Sort((k1, k2) => k2.kills.CompareTo(k1.kills));
        return killers[0].id;
    }

    private void GetTeamTotal()
    {
        //No teams in this game mode
    }

    public bool ValidTeamSwitch(int t)
    {
        return true; //No teams, no point...
    }
}