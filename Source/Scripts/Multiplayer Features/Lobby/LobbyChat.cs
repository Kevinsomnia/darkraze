using UnityEngine;
using System.Collections;

public class LobbyChat : MonoBehaviour
{
    public UIInput chatInput;
    public ChatListGUI chatOutput;
    public float antiFloodTime = 0.4f;

    private static Topan.NetworkView _nv;
    public static Topan.NetworkView netView
    {
        get
        {
            if (_nv == null && GeneralVariables.Networking != null)
            {
                _nv = Topan.Network.GetNetworkView(GeneralVariables.Networking.gameObject);
            }

            return _nv;
        }
    }

    private int chatLength;
    private float chatActionTime;

    public void Start()
    {
        chatLength = 0;
    }

    void Update()
    {
        if (!Topan.Network.isConnected)
        {
            return;
        }

        if (chatLength != NetworkingGeneral.gameChatList.Count)
        {
            chatOutput.CopyList(NetworkingGeneral.gameChatList);
            chatLength = NetworkingGeneral.gameChatList.Count;
        }

        if (Time.unscaledTime - chatActionTime >= antiFloodTime && Input.GetKeyDown(KeyCode.Return))
        {
            if (!chatInput.isSelected)
            {
                chatInput.isSelected = true;
            }
            else
            {
                bool sameAsLastMsg = (chatOutput.chatList.Count > 0) ? DarkRef.RemoveSpaces(chatOutput.chatList[chatOutput.chatList.Count - 1].ToLower()) == DarkRef.RemoveSpaces(chatInput.value.ToLower()) : false;
                if (netView != null && !string.IsNullOrEmpty(DarkRef.RemoveSpaces(chatInput.value)) && !sameAsLastMsg)
                {
                    string message = "[DAA314]" + AccountManager.profileData.username + "[-]: " + chatInput.value;
                    netView.RPC(Topan.RPCMode.All, "ChatMessage", message);
                }

                if (!string.IsNullOrEmpty(chatInput.value))
                {
                    chatActionTime = Time.unscaledTime;
                }

                chatInput.value = "";
                chatInput.isSelected = false;
            }
        }
    }
}