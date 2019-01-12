using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TeamDeathmatch : GameTypeInterface {
	public string typeName {
		get {
			return "Team Deathmatch";
		}
	}

	private Dictionary<string, GameType.GameTypeSetting> _customSettings = new Dictionary<string, GameType.GameTypeSetting>();
	public Dictionary<string, GameType.GameTypeSetting> customSettings {
		get {
			return _customSettings;
		}
	}

    private SortPlayersBy _spb = SortPlayersBy.Kills;
    public SortPlayersBy sortPlayersBy {
        get {
            return _spb;
        }
    }

	//List of player IDs
	public List<int> redPlayers = new List<int>();
	public List<int> bluePlayers = new List<int>();

	public Dictionary<int, int> _killsPerTeam = new Dictionary<int, int>();
	public Dictionary<int, int> killsPerTeam {
		get {
			GetTeamTotal();
			return _killsPerTeam;
		}
	}

    private bool initialized;

	public void InitializeSettings() {
        if(initialized) {
            return;
        }

		_killsPerTeam = new Dictionary<int, int>();
		_killsPerTeam.Add(0, 0);
		_killsPerTeam.Add(1, 0);

		_customSettings.Add("Kill Limit", new GameType.GameTypeSetting(GameType.SettingType.Slider, "100"));
		_customSettings.Add("Friendly Fire", new GameType.GameTypeSetting(GameType.SettingType.Checkbox, "False"));
        _customSettings.Add("Team Auto-Balance", new GameType.GameTypeSetting(GameType.SettingType.Checkbox, "True"));

        initialized = true;
	}

    public void AddPlayer(int playerID, int team) {
        if(team == 0) {
            redPlayers.Add(playerID);
        }
        else if(team == 1) {
            bluePlayers.Add(playerID);
        }
    }
	
	public void RemovePlayer(int playerID) {
		bluePlayers.Remove(playerID);
		redPlayers.Remove(playerID);
	}

	public void ClearPlayerList() {
		redPlayers.Clear();
		bluePlayers.Clear();
	}
	
	public int GetTeamAssign(int playerID) {
        if(redPlayers.Count <= bluePlayers.Count) {
            redPlayers.Add(playerID);
            return 0;
        }

        bluePlayers.Add(playerID);
        return 1;
	}
	
	public int GetWinner() {
		GetTeamTotal();

		foreach(KeyValuePair<int, int> teamK in killsPerTeam) {
			if(teamK.Value >= (int)GameType.GetSettingValue(customSettings["Kill Limit"])) {
				return teamK.Key;	
			}
		}
		
		return -1;
	}
	
	public int GetWinnerDefinite() {
		GetTeamTotal();
		
		int highestTeam = -1;
		int equalCheck = 0;
		bool equalCheckAssigned = false;
		bool allEqual = true;
		foreach(KeyValuePair<int, int> currentTeam in killsPerTeam) {
			if(!equalCheckAssigned) {
				equalCheck = currentTeam.Value;
                equalCheckAssigned = true;
				continue;
			}
			
			if(currentTeam.Value != equalCheck) {
				allEqual = false;
				break;	
			}
		}
		
		if(allEqual) {
			return -1;	
		}

        List<int> kptKeys = new List<int>(killsPerTeam.Keys);
		foreach(int key in kptKeys) {
			if(highestTeam == -1) {
				highestTeam = key;
				continue;
			}
			
			if(killsPerTeam[key] > killsPerTeam[highestTeam]) {
				highestTeam = key;	
			}
		}
		
		return highestTeam;		
	}

	private void GetTeamTotal() {
        if(!Topan.Network.HasServerInfo("rTK") || !Topan.Network.HasServerInfo("bTK")) {
            return;
        }

		_killsPerTeam[0] = (UInt16)Topan.Network.GetServerInfo("rTK");
        _killsPerTeam[1] = (UInt16)Topan.Network.GetServerInfo("bTK");
	}

    public bool ValidTeamSwitch(int team) {
        if(team == 0 && redPlayers.Count >= bluePlayers.Count && redPlayers.Count < 8) {
            return false;
        }
        if(team == 1 && bluePlayers.Count >= redPlayers.Count && bluePlayers.Count < 8) {
            return false;
        }

        return true;
    }
}