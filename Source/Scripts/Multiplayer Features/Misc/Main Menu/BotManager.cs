using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class BotPlayer
{
    public byte team = 0;
    public CombatantInfo botInfo = null;

    [System.NonSerialized]
    public bool isParticipating = true;

    [System.NonSerialized]
    public int index = 0;

    [System.NonSerialized]
    public BotStats botStats = new BotStats(); //stats for server
}

[System.Serializable]
public class BotStats
{
    public int kills = 0;
    public int deaths = 0;
    public int headshots = 0;
    public int score = 0;
}

public class BotManager : MonoBehaviour
{
    public static BotPlayer[] _sBots;
    public static BotPlayer[] allBotPlayers
    {
        get
        {
            if (!Topan.Network.isConnected)
            {
                return null;
            }

            if (Topan.Network.isServer)
            {
                return _sBots;
            }

            if (!Topan.Network.HasServerInfo("bots"))
            {
                return null;
            }

            return (BotPlayer[])Topan.Network.GetServerInfo("bots");
        }
    }

    public static BotPlayer[] redBotPlayers
    {
        get
        {
            if (!GeneralVariables.gameModeHasTeams)
            {
                return null;
            }

            return allBotPlayers.Where(bot => bot.team == 0).ToArray();
        }
    }

    public static BotPlayer[] blueBotPlayers
    {
        get
        {
            if (!GeneralVariables.gameModeHasTeams)
            {
                return null;
            }

            return allBotPlayers.Where(bot => bot.team == 1).ToArray();
        }
    }

    private static List<BotStats> lastBotStats = new List<BotStats>();

    public BlurEffect blurGUI;
    public SliderAction amountSlider;
    public UILabel lagNote;

    private UIPanel panel;

    void Start()
    {
        panel = GetComponent<UIPanel>();
        panel.alpha = 0f;
    }

    void Update()
    {
        lagNote.alpha = Mathf.Lerp(lagNote.alpha, (amountSlider.currentValue > 8) ? lagNote.defaultAlpha : 0f, Time.unscaledDeltaTime * 14f);
    }

    public void DisplayWindow(bool disp)
    {
        StartCoroutine(FadeAnimation(disp));
    }

    private IEnumerator FadeAnimation(bool f)
    {
        if (f)
        {
            float fade = 0f;
            while (fade < 1f)
            {
                fade += Time.deltaTime * 8f;
                panel.alpha = Mathf.Clamp01(fade);
                blurGUI.blurSpread = Mathf.Clamp01(fade) * 0.9f;
                yield return null;
            }
        }
        else
        {
            float fade = 1f;
            while (fade > 0f)
            {
                fade -= Time.deltaTime * 8f;
                panel.alpha = Mathf.Clamp01(fade);
                blurGUI.blurSpread = Mathf.Clamp01(fade) * 0.9f;
                yield return null;
            }

            PlayerPrefs.SetInt("SavedBotCount", (int)amountSlider.currentValue);
        }
    }

    public static BotStats GetBotStats(int index)
    {
        if (!Topan.Network.HasServerInfo("bS" + index.ToString()))
        {
            return null;
        }

        BotStats newStats = new BotStats();
        string botInfo = Topan.Network.GetServerInfo("bS" + index.ToString()).ToString();
        string[] thisBotInfo = botInfo.Split(new string[] { "," }, StringSplitOptions.None);

        newStats.kills = int.Parse(thisBotInfo[0]);
        newStats.deaths = int.Parse(thisBotInfo[1]);
        newStats.headshots = int.Parse(thisBotInfo[2]);
        newStats.score = int.Parse(thisBotInfo[3]);
        return newStats;
    }

    public static string ParseToBotFormat(BotStats bStats)
    {
        return bStats.kills.ToString() + "," + bStats.deaths.ToString() + "," + bStats.headshots.ToString() + "," + bStats.score.ToString();
    }
}