using UnityEngine;
using System.Collections;

public class GeneralChat : MonoBehaviour {
    public UIInput chatInput;
	public ChatListGUI chatOutput;
    public bool useTeamChat = false;
    public bool spamProtection = true;
    public float antiFloodTime = 0.4f;
    public AlphaGroupUI chatIndicator;

    private Topan.NetworkView cng;
    private Topan.NetworkView chatNetGeneral {
        get {
            if(cng == null && GeneralVariables.Networking != null) {
                cng = Topan.Network.GetNetworkView(GeneralVariables.Networking.gameObject);
            }

            return cng;
        }
    }

	private int chatLength;
    private bool isTeamChat;
    private string primaryChatKey;
    private string primaryTeamKey;
    private float startChatTime;
    private float floodTime;
    private UILabel chatIndLabel;

	void Awake() {
        primaryChatKey = cInput.GetText("General Chat", 1);
        primaryTeamKey = cInput.GetText("Team Chat", 1);
        chatInput.defaultText = primaryChatKey + ((useTeamChat && GeneralVariables.gameModeHasTeams) ? " or " + primaryTeamKey : " or " + cInput.GetText("General Chat", 2)) + " to chat";
        chatIndicator.alpha = 0f;
        chatIndLabel = chatIndicator.widgets[1].GetComponent<UILabel>();
	}
	
	void Update() {
		if(!Topan.Network.isConnected) {
			return;
		}

        if(chatLength != chatOutput.chatList.Count) {
			UpdateChatList();
		}

        bool allChatPressed = cInput.GetButtonDown("General Chat");
        bool teamChatPressed = GeneralVariables.gameModeHasTeams && cInput.GetButtonDown("Team Chat");
        if(!(RestrictionManager.restricted && !RestrictionManager.allInput) && !RoundEndManager.isRoundEnded && allChatPressed || (useTeamChat && teamChatPressed)) {
            if(!chatInput.isSelected) {
                if(Time.unscaledTime - floodTime >= antiFloodTime) {
//                    chatInput.restrictFrames = 1;
                    chatInput.isSelected = true;
                    RestrictionManager.allInput = true;

                    if(teamChatPressed) {
                        isTeamChat = true;
                    }

                    if(chatIndicator != null) {
                        if(teamChatPressed) {
                            chatIndLabel.text = "TEAM";
                        }
                        else {
                            chatIndLabel.text = "ALL";
                        }

                        if(useTeamChat) {
                            chatIndicator.alpha = 1f;
                        }
                    }

                    startChatTime = Time.unscaledTime;
                }
            }
            else if(Input.GetKeyDown(primaryChatKey.ToLower())) {
                bool sameAsLastMsg = (chatOutput.chatList.Count > 0) ? DarkRef.RemoveSpaces(chatOutput.chatList[chatOutput.chatList.Count - 1].ToLower()) == DarkRef.RemoveSpaces(chatInput.value.ToLower()) : false;
                if(chatNetGeneral != null && !string.IsNullOrEmpty(DarkRef.RemoveSpaces(chatInput.value)) && !(spamProtection && sameAsLastMsg)) {
                    if(isTeamChat) {
                        int myTeam = (int)((byte)Topan.Network.player.GetPlayerData("team"));
                        string message = "[TEAM] [DAA314]" + AccountManager.profileData.username + "[-]: " + chatInput.value;
                        NetworkingGeneral.gameChatList.Add(message);
                        chatNetGeneral.RPC(DarkRef.SendTeamMessage(myTeam), "ChatMessage", message);
                    }
                    else {
                        chatNetGeneral.RPC(Topan.RPCMode.All, "ChatMessage", "[DAA314]" + AccountManager.profileData.username + "[-]: " + chatInput.value);
                    }
                }
                if(!string.IsNullOrEmpty(chatInput.value)) {
                    floodTime = Time.unscaledTime;
                }

                chatInput.value = "";
                chatInput.isSelected = false;
                isTeamChat = false;
                RestrictionManager.restricted = false;
            }            
        }

        if(Time.unscaledTime - startChatTime >= 0.6f) {
            chatIndicator.alpha -= Time.unscaledDeltaTime * 2.2f;
        }
	}

    public void UpdateChatList() {
        chatOutput.CopyList(NetworkingGeneral.gameChatList);
        chatLength = chatOutput.chatList.Count;
    }
}