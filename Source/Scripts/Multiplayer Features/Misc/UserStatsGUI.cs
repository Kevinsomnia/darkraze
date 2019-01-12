using UnityEngine;
using System.Collections;

public class UserStatsGUI : MonoBehaviour
{
    public UILabel rank;
    public UITexture rankTexture;
    public UILabel username;
    public UILabel kills;
    public UILabel deaths;
    public UILabel headshots;
    public UILabel kdRatio;
    public UILabel score;
    public UILabel ping;
    public UISprite highlight;

    public void SetInfo(string rankNum, string playerName, string kill, string death, string kdr, string headshot, string scoreNum, string pingTime, bool isLocalPlayer, bool darken)
    {
        rank.text = rankNum;
        //		rankTexture.mainTexture = null; Placeholder until we get rank icons.
        username.text = playerName;
        kills.text = kill;
        deaths.text = death;
        kdRatio.text = kdr;
        score.text = scoreNum;
        headshots.text = headshot;
        highlight.enabled = (isLocalPlayer || darken);

        if (isLocalPlayer)
        {
            highlight.color = new Color(1f, 0.7f, 0.4f, 0.02f);
        }
        else if (darken)
        {
            highlight.color = new Color(1f, 1f, 1f, 0.007f);
        }

        if (ping != null)
        {
            int mPing = int.Parse(pingTime);
            ping.text = ((mPing <= 0 || mPing > 999) ? "---" : mPing.ToString()) + " ms";
        }
    }
}