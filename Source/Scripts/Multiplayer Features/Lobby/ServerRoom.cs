using UnityEngine;
using System.Collections;

public class ServerRoom : MonoBehaviour {
	public UILabel roomName;
	public UILabel hostName;
    public UILabel playerCount;
    public UILabel mapName;
    public UILabel gameMode;
    public UISprite backgroundSprite;
    public ShowTooltip fullTooltip;
	
    [HideInInspector] public int hostID;
	[HideInInspector] public int buttonNumber;
	[HideInInspector] public bool selected;
	
	private ServerList sl;
	private UIButton button;
	private Color defaultColor;
	private Color selectedColor;
	private bool isHovering;
	private float lClickTime;
	
	void Start() {
		button = GetComponent<UIButton>();
        button.ignoreColoring = true;
		sl = transform.parent.parent.GetComponent<ServerList>();
		ToggleServerButton(false);
		
		if(buttonNumber % 2 == 0) {
			backgroundSprite.defaultColor = new Color(0f, 0f, 0f, 0.25f);
		}
		else if(buttonNumber % 2 == 1) {
            backgroundSprite.defaultColor = new Color(0f, 0f, 0f, 0.325f);
		}

        selectedColor = new Color(0.6f, 0.27f, 0.136f, 0.8f);
	}
	
	void Update() {
        if(selected) {
            backgroundSprite.color = Color.Lerp(backgroundSprite.color, selectedColor, Time.unscaledDeltaTime * 8f);
        }
        else if(isHovering) {
            backgroundSprite.color = Color.Lerp(backgroundSprite.color, button.hover, Time.unscaledDeltaTime * 8f);
        }
        else {
            backgroundSprite.color = Color.Lerp(backgroundSprite.color, backgroundSprite.defaultColor, Time.unscaledDeltaTime * 8f);
        }
	}
	
	public void OnClick() {
        sl.curSelection = this;

		if(Time.unscaledTime - lClickTime <= 0.25f) {
			hostID = ((sl.pageNumber - 1) * sl.serversPerPage) + buttonNumber;
            GeneralVariables.lobbyManager.OnJoin(hostID);
		}

		lClickTime = Time.unscaledTime;
	}

    public void OnHover(bool hover) {
        isHovering = hover;
    }
	
	public void AssignHostDetails(int pageNumber, bool isOnline) {
        hostID = ((pageNumber - 1) * sl.serversPerPage) + buttonNumber;

		if(isOnline) {
            if(sl.displayHostedServers != null && hostID < sl.displayHostedServers.Count) {
				ToggleServerButton(true);
                HostInfo curHost = sl.displayHostedServers[hostID];
				roomName.text = curHost.gameName;
				hostName.text = curHost.hostName;
                playerCount.text = curHost.playerCount + "/" + curHost.maxPlayers;
                mapName.text = ((curHost.mapIndex >= 255) ? "Custom Map" : StaticMapsList.mapsArraySorted[curHost.mapIndex].mapName);
                gameMode.text = MultiplayerMenu.gameTypeNames[curHost.gameModeIndex];
                fullTooltip.text = (curHost.playerCount >= curHost.maxPlayers) ? "Server is full" : "";
			}
			else {
				ToggleServerButton(false);
			}
		}
		else {
            if(Topan.Network.foundLocalGames != null && hostID < Topan.Network.foundLocalGames.Count) {
				ToggleServerButton(true);
                roomName.text = Topan.Network.foundLocalGames[hostID].GameName;
				hostName.text = "Local";
                playerCount.text = "1/16";
                mapName.text = "Map";
                gameMode.text = "Game Mode";
//              fullTooltip.text = (curHost.playerCount >= curHost.maxPlayers) ? "Server is full" : "";
                fullTooltip.text = "";
			}
			else {
				ToggleServerButton(false);
			}
		}
	}
	
	public void ToggleServerButton(bool toggle) {
		gameObject.SetActive(toggle);
	}
}