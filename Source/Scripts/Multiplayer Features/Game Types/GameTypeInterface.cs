using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameType
{
    public enum SettingType { TextField, EnumPopup, Slider, Checkbox };

    public class GameTypeSetting
    {
        public SettingType settingType;
        public string[] possibleValues;
        public string currentValue;

        public GameTypeSetting(SettingType s, string dV, params string[] possibles)
        {
            settingType = s;
            currentValue = dV;
            possibleValues = possibles;
        }
    }

    public static object GetSettingValue(GameTypeSetting setting)
    {
        if (setting.settingType == SettingType.Slider)
        {
            return int.Parse(setting.currentValue);
        }
        else if (setting.settingType == SettingType.Checkbox)
        {
            return DarkRef.ConvertStringToBool(setting.currentValue);
        }
        else if (setting.settingType == SettingType.EnumPopup || setting.settingType == SettingType.TextField)
        {
            return setting.currentValue;
        }

        return null;
    }
}

public enum SortPlayersBy
{
    Kills = 0,
    Score = 1
}

public interface GameTypeInterface
{
    string typeName { get; }
    Dictionary<string, GameType.GameTypeSetting> customSettings { get; }
    Dictionary<int, int> killsPerTeam { get; }
    SortPlayersBy sortPlayersBy { get; }
    void InitializeSettings();
    int GetWinner();
    int GetWinnerDefinite();
    int GetTeamAssign(int pID);
    void AddPlayer(int pID, int team);
    void RemovePlayer(int pID);
    void ClearPlayerList();
    bool ValidTeamSwitch(int team);
}