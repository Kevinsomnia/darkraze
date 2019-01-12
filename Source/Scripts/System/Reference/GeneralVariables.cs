using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneralVariables
{
    public static float lightingFactor = 1.0f;

    public static GameObject player;
    public static Camera mainPlayerCamera;
    private static PlayerReference _playerRef;
    public static PlayerReference playerRef
    {
        get
        {
            return _playerRef;
        }
        set
        {
            _playerRef = value;

            if (_uiController != null)
            {
                _uiController.crosshairs.SetReferenceVars();
            }
        }
    }

    public static WaveManager waveManager;
    public static MultiplayerGUI multiplayerGUI
    {
        get
        {
            if (_uiController && Topan.Network.isConnected)
            {
                return _uiController.mpGUI;
            }

            return null;
        }
    }

    private static SpectatorCamera _spectatorCamera;
    public static GameObject cachedSpectCam;
    public static SpectatorCamera spectatorCamera
    {
        get
        {
            if (_spectatorCamera == null)
            {
                if (cachedSpectCam != null)
                {
                    _spectatorCamera = ((GameObject)GameObject.Instantiate(cachedSpectCam)).GetComponent<SpectatorCamera>();
                }
                else
                {
                    _spectatorCamera = (SpectatorCamera)GameObject.Instantiate(Resources.Load("SpectatorCamera", typeof(SpectatorCamera)));
                }
            }

            return _spectatorCamera;
        }
    }

    public static NetworkingGeneral Networking;

    private static Topan.NetworkView _nv;
    public static Topan.NetworkView connectionView
    {
        get
        {
            if (Topan.Network.isConnected && _nv == null)
            {
                _nv = Topan.Network.GetNetworkView(Networking.gameObject);
            }

            return _nv;
        }
    }

    private static Server _server;
    public static Server server
    {
        get
        {
            if (_server == null && Networking != null)
            {
                _server = Networking.GetComponent<Server>();
            }

            return _server;
        }
    }

    private static Client _client;
    public static Client client
    {
        get
        {
            if (_client == null && Networking != null)
            {
                _client = Networking.GetComponent<Client>();
            }

            return _client;
        }
    }

    public static bool showBandwidth = false;
    public static ServerLobby lobbyManager;
    public static void ChangeLayerRecursively(GameObject go, int layerIndex)
    {
        go.layer = layerIndex;

        foreach (Transform child in go.transform)
        {
            ChangeLayerRecursively(child.gameObject, layerIndex);
        }
    }

    public static bool uicIsActive
    {
        get
        {
            return (_uiController != null);
        }
    }

    public static bool gameModeHasTeams
    {
        get
        {
            return (Topan.Network.isConnected && NetworkingGeneral.currentGameType != null && NetworkingGeneral.currentGameType.typeName != "Deathmatch");
        }
    }

    public static UIController cachedUI;
    private static UIController _uiController;
    public static UIController uiController
    {
        get
        {
            if (_uiController == null)
            {
                UIController currentUI = (UIController)MonoBehaviour.FindObjectOfType(typeof(UIController));

                if (currentUI)
                {
                    _uiController = currentUI;
                }
                else
                {
                    Object prefabSrc = (cachedUI != null) ? cachedUI : Resources.Load("GUI/Local UI", typeof(UIController));
                    _uiController = (UIController)GameObject.Instantiate(prefabSrc);
                }
            }

            return _uiController;
        }
    }

    public static SpawnController_MP spawnController;

    private static AudioListener _listener;
    public static AudioListener curListener
    {
        get
        {
            if (_listener == null)
            {
                _listener = (AudioListener)Object.FindObjectOfType(typeof(AudioListener));
            }

            return _listener;
        }
    }
}