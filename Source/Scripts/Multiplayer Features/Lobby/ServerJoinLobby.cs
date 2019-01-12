using UnityEngine;
using System.Collections;

public class ServerJoinLobby : MonoBehaviour {
	public ServerList serverList;
	public ServerLobby serverLobby;
	
	void OnClick() {
		serverLobby.ServerDetails(serverList.pageNumber, serverList.curSelection.buttonNumber, serverList.isOnlineList);
	}
}