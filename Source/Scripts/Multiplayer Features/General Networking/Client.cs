using UnityEngine;
using System.Collections;
using Lidgren.Network;
using System.Threading;

public class Client : Topan.TopanMonoBehaviour
{
    public GameObject spawnInterfacePrefab;

    private bool firstFunctionCalled = false;

    public void Awake()
    {
        if (!Topan.Network.isServer)
        {
            Topan.Network.AddNetworkEventListener(this);
            topanNetworkView.observedComponents.Add(this);
        }
        else
        {
            Destroy(this);
        }
    }

    void CheckInit()
    {
        if (!firstFunctionCalled)
        {
            Topan.Network.AskForAllNetworkViews();
            firstFunctionCalled = true;
        }
    }

    [RPC]
    public void LoadGame(string mapHash)
    {
        Map toLoad = StaticMapsList.mapsArraySorted[(byte)Topan.Network.GetServerInfo("m")];

        CheckInit();
        Topan.Network.isMessageQueueRunning = false;

        Loader.finished = () =>
        {
            Topan.Network.isMessageQueueRunning = true;
            Instantiate(spawnInterfacePrefab);
        };

        Loader.LoadLevel(toLoad.sceneName);
    }

    [RPC]
    public void DestroySpectatorCamera()
    {
        if (GeneralVariables.spectatorCamera)
        {
            Destroy(GeneralVariables.spectatorCamera.gameObject);
        }
    }

    [RPC]
    public void LoadLobby()
    {
        CheckInit();

        if (Application.loadedLevelName != "Main Menu")
        {
            Loader.finished = () =>
            {
                GameObject.FindWithTag("MainCamera").GetComponent<CameraMove>().TargetPos(new Vector3(3840f, -800f, -700f));
                GeneralVariables.lobbyManager.lobbyChat.Start();
            };
            Loader.LoadLevel("Main Menu");
        }
        else
        {
            GameObject.FindWithTag("MainCamera").GetComponent<CameraMove>().TargetPos(new Vector3(3840f, -800f, -700f));
        }
    }
}