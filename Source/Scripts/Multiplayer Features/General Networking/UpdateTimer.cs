using UnityEngine;
using System.Collections;

public class UpdateTimer : Topan.TopanMonoBehaviour
{
    private UILabel timerMin;
    private UILabel timerSec;
    private UILabel colon;

    private int lastSyncTime = -1;
    private bool dontFuckUpInternet = false;
    private float clientSyncDifferential;

    private Color normalColor = new Color(1f, 1f, 1f, 0.75f);
    private Color timeLowColor = new Color(1f, 0.2f, 0f, 0.75f);
    private Color timeColonColor = new Color(0.84f, 0.14f, 0f, 0.7f);

    void Start()
    {
        if (!GeneralVariables.multiplayerGUI)
        {
            return;
        }

        lastSyncTime = -1;
        clientSyncDifferential = 0f;
        timerMin = GeneralVariables.multiplayerGUI.timerLabels[0];
        timerSec = GeneralVariables.multiplayerGUI.timerLabels[1];
        colon = GeneralVariables.multiplayerGUI.timerLabels[2];
    }

    void Update()
    {
        if (Topan.Network.isConnected && !Topan.Network.isServer && lastSyncTime > -1 && !RoundEndManager.isRoundEnded)
        {
            clientSyncDifferential += Time.unscaledDeltaTime;

            if (clientSyncDifferential >= 6f)
            {
                MultiplayerMenu.disconnectMsg = 4;
                Topan.Network.Disconnect();
                Loader.LoadLevel("Main Menu");
            }
        }
    }

    [RPC]
    public void SyncServerTime(int newTime)
    {
        if (timerMin == null || timerSec == null)
        {
            if (!GeneralVariables.multiplayerGUI)
            {
                return;
            }

            timerMin = GeneralVariables.multiplayerGUI.timerLabels[0];
            timerSec = GeneralVariables.multiplayerGUI.timerLabels[1];
            colon = GeneralVariables.multiplayerGUI.timerLabels[2];

            if (timerMin == null || timerSec == null)
            {
                return;
            }
        }

        if (lastSyncTime <= -1)
        {
            lastSyncTime = newTime;
        }

        float timeValue = (float)newTime;
        float lerpValue = (Mathf.Clamp(timeValue - 10f, 0f, 20f)) * 0.05f;
        Color timeCol = Color.Lerp(timeLowColor, normalColor, lerpValue);
        Color colonCol = Color.Lerp(timeColonColor, normalColor, lerpValue);

        timerMin.color = timeCol;
        timerMin.text = (((newTime / 60) < 10) ? " " : "") + (newTime / 60).ToString();
        timerSec.color = timeCol;
        timerSec.text = (newTime % 60).ToString("00");
        colon.color = colonCol;

        if (Topan.Network.isServer || newTime == lastSyncTime || clientSyncDifferential <= 0f)
        {
            return;
        }

        float differential = Mathf.Abs((float)(lastSyncTime - newTime) - clientSyncDifferential);
        lastSyncTime = newTime;

        if (differential >= 2f)
        {
            if (dontFuckUpInternet)
            {
                MultiplayerMenu.disconnectMsg = 4;
                Topan.Network.Disconnect();
                Loader.LoadLevel("Main Menu");
            }

            dontFuckUpInternet = true;
        }
        else
        {
            dontFuckUpInternet = false;
        }

        clientSyncDifferential = 0f;
    }
}