using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerList : MonoBehaviour
{
    [System.Serializable]
    public class ServerSortSettings
    {
        public UIButton sortButton;
        public UISprite sortArrow; //ascending/descending sort
        public int sortDirection = -1; //-1 = not sorted.
    }

    public Transform listContainer;
    public GameObject roomGUIPrefab;
    public UISprite serverMsgBackground;
    public UILabel serverMessage;
    public UIButton joinButton;
    public HostInfo[] hostInfoArray;
    public int serversPerPage = 19;
    public float spacing = 30f;
    public float autoRefreshInterval = 30f;
    public UISprite selectionOnline;
    public UISprite selectionLocal;
    public UILabel pageLabel;
    public UIButton refreshButton;
    public UIButton prevPageButton;
    public UIButton nextPageButton;
    public CameraMove menuCamera;
    public ServerLobby sLobby;

    //IMPORTANT, THIS MUST BE IN ORDER! LEFT TO RIGHT FROM SERVER LIST
    public ServerSortSettings[] sortListItems = new ServerSortSettings[6];

    public int pageNumber
    {
        get
        {
            return (isOnlineList) ? onlinePageNumber : localPageNumber;
        }
        set
        {
            if (isOnlineList)
            {
                onlinePageNumber = value;
            }
            else
            {
                localPageNumber = value;
            }
        }
    }

    private UIButton[] listOfServerButtons;
    private ServerRoom[] listOfServerRooms;
    private float alpha;
    private bool refreshing = false;
    private bool runOnce = true;
    private bool quickJoin = false;
    private float refreshTime = -1f;

    [HideInInspector] public bool isOnlineList = true;
    [HideInInspector] public int maximumPages;
    [HideInInspector] public ServerRoom curSelection;

    [HideInInspector] public List<HostInfo> displayHostedServers;

    private ServerRoom oldSelection;
    private int onlinePageNumber = 1;
    private int localPageNumber = 1;

    void Start()
    {
        foreach (ServerSortSettings sss in sortListItems)
        {
            sss.sortDirection = -1;
            sss.sortArrow.enabled = false;
        }

        onlinePageNumber = 1;
        localPageNumber = 1;
        maximumPages = 1;
        curSelection = null; oldSelection = null;
        isOnlineList = true;
        runOnce = true;
        quickJoin = false;
        refreshTime = -1;
        PopulateList(serversPerPage);
    }

    void Update()
    {
        if (curSelection == null && Time.time - refreshTime >= autoRefreshInterval)
        {
            RefreshList();
        }

        refreshButton.isEnabled = !refreshing;

        serverMessage.text = "";
        if (refreshing)
        {
            serverMessage.text = "Refreshing list...";
        }
        else if (!MultiplayerMenu.mServerIsOnline)
        {
            serverMessage.text = "Cannot access server list at this time." + "\n" + "Master server is offline.";
        }
        else
        {
            if (isOnlineList)
            {
                if (!(Topan.MasterServer.hosts != null && Topan.MasterServer.hosts.Count > 0) || refreshTime < 0f)
                {
                    serverMessage.text = "No active servers were found." + "\n" + "Try refreshing the list or create your own.";
                }
            }
            else
            {
                if (!(Topan.Network.foundLocalGames != null && Topan.Network.foundLocalGames.Count > 0) || refreshTime < 0f)
                {
                    serverMessage.text = "No local servers were found." + "\n" + "Try refreshing the list or create your own.";
                }
            }
        }

        serverMsgBackground.alpha = Mathf.Lerp(serverMsgBackground.alpha, (serverMessage.text != "") ? 0.4f : 0f, Time.deltaTime * 10f);

        maximumPages = 1;
        if (isOnlineList)
        {
            if (Topan.MasterServer.hosts != null && Topan.MasterServer.hosts.Count > 0 && refreshTime > 0f)
            {
                maximumPages = CustomCeiling((float)Topan.MasterServer.hosts.Count / (float)serversPerPage);
            }
        }
        else
        {
            if (Topan.Network.foundLocalGames != null && Topan.MasterServer.hosts.Count > 0 && refreshTime > 0f)
            {
                maximumPages = CustomCeiling((float)Topan.Network.foundLocalGames.Count / (float)serversPerPage);
            }
        }

        pageNumber = Mathf.Clamp(pageNumber, 1, maximumPages);
        prevPageButton.isEnabled = (pageNumber > 1);
        nextPageButton.isEnabled = (pageNumber < maximumPages);

        pageLabel.text = "Page: " + pageNumber.ToString() + "/" + maximumPages.ToString();

        if (curSelection != oldSelection || runOnce)
        {
            bool serverIsFull = false;
            if (isOnlineList)
            {
                if (curSelection != null && Topan.MasterServer.hosts != null && Topan.MasterServer.hosts.Count > 0)
                {
                    serverIsFull = (Topan.MasterServer.hosts[curSelection.hostID].playerCount == Topan.MasterServer.hosts[curSelection.hostID].maxPlayers);
                }
            }
            else
            {
                //serverIsFull = (Topan.Network.foundLocalGames[curSelection.hostID].playerCount == Topan.Network.foundLocalGames[curSelection.hostID].maxPlayers);
            }

            if (curSelection == null)
            {
                joinButton.isEnabled = false;
                joinButton.GetComponent<Collider>().enabled = false;

                foreach (ServerRoom sr in listOfServerRooms)
                {
                    sr.selected = false;
                }
            }
            else
            {
                joinButton.isEnabled = !serverIsFull;
                joinButton.GetComponent<Collider>().enabled = !serverIsFull;
                joinButton.GetComponent<ButtonAction>().sendMessage.numericalMessage.valueToSend = curSelection.buttonNumber;

                for (int i = 0; i < listOfServerRooms.Length; i++)
                {
                    listOfServerRooms[i].selected = (curSelection == listOfServerRooms[i]);
                }
            }

            if (serverIsFull)
            {
                //joinButton.disabledTooltip.text = "This server is full";
            }
            else
            {
                //joinButton.disabledTooltip.text = "";
            }

            oldSelection = curSelection;
            runOnce = false;
        }
    }

    public void PopulateList(int amount)
    {
        listOfServerButtons = new UIButton[amount];
        listOfServerRooms = new ServerRoom[amount];
        for (int i = 0; i < amount; i++)
        {
            Transform inst = ((GameObject)Instantiate(roomGUIPrefab)).transform;
            inst.parent = listContainer;
            inst.localPosition = Vector3.down * (i * spacing);
            inst.localScale = Vector3.one;
            listOfServerButtons[i] = inst.GetComponent<UIButton>();
            listOfServerRooms[i] = inst.GetComponent<ServerRoom>();
            listOfServerRooms[i].buttonNumber = i;
            inst.GetComponent<AlphaGroupUI>().alpha = 0f;
        }
    }

    public void ChangePage(int amount)
    {
        int oldNum = pageNumber;
        pageNumber += amount;
        pageNumber = Mathf.Clamp(pageNumber, 1, maximumPages);

        if (pageNumber != oldNum)
        {
            RefreshList();
        }
    }

    public void RefreshList()
    {
        if (Mathf.Abs(menuCamera.transform.localPosition.x - 3840f) > 1200f || refreshing)
        {
            return;
        }

        ForceRefreshServer();
    }

    public void ForceRefreshServer()
    {
        refreshTime = Time.time;

        if (isOnlineList)
        {
            Topan.MasterServer.RefreshHostsList(this);
        }
        else
        {
            Topan.Network.DiscoverLocalServers(this);
        }

        StopAllCoroutines();
        StartCoroutine(FadeOutServerElements());

        refreshing = true;
        curSelection = null;
    }

    public void RefreshFailed()
    {
        MultiplayerMenu.mServerIsOnline = false;
        MultiplayerMenu.mServerCheckTime = System.DateTime.UtcNow;
        refreshing = false;
    }

    public void MakeRefresh()
    {
        if (GetSortMethod() > -1)
        {
            SortServerList(GetSortMethod());
        }
        else
        {
            displayHostedServers = Topan.MasterServer.hosts;
        }

        foreach (UIButton btn in listOfServerButtons)
        {
            btn.GetComponent<ServerRoom>().AssignHostDetails(pageNumber, isOnlineList);
        }

        StartCoroutine(FadeInServerElements());

        MultiplayerMenu.mServerIsOnline = true;
        MultiplayerMenu.mServerCheckTime = System.DateTime.UtcNow;
        refreshing = false;
    }

    public void QuickMatchAction()
    {
        quickJoin = true;
        ForceRefreshServer();
    }

    private IEnumerator FadeInServerElements()
    {
        if (quickJoin)
        {
            if (Topan.MasterServer.hosts.Count > 0)
            {
                sLobby.OnJoin(Random.Range(0, Topan.MasterServer.hosts.Count));
            }

            quickJoin = false;
        }

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * 10.5f;

            for (int i = 0; i < listOfServerButtons.Length; i++)
            {
                listOfServerButtons[i].GetComponent<AlphaGroupUI>().alpha = time;
            }

            yield return null;
        }
    }

    private IEnumerator FadeOutServerElements()
    {
        float time = 1f;
        while (time > 0f)
        {
            time -= Time.deltaTime * 12f;

            foreach (UIButton btn in listOfServerButtons)
            {
                btn.GetComponent<AlphaGroupUI>().alpha = time;
            }

            yield return null;
        }

        foreach (UIButton btn in listOfServerButtons)
        {
            btn.GetComponent<ServerRoom>().ToggleServerButton(false);
        }
    }

    public void SelectedList(bool isOnline)
    {
        if (isOnlineList == isOnline)
        {
            return;
        }

        isOnlineList = isOnline;
        if (isOnlineList)
        {
            selectionOnline.color = new Color(1f, 0.8f, 0.6f, 0.5f);
            selectionLocal.color = new Color(0f, 0f, 0f, 0.3f);
        }
        else
        {
            selectionOnline.color = new Color(0f, 0f, 0f, 0.3f);
            selectionLocal.color = new Color(1f, 0.8f, 0.6f, 0.5f);
        }

        RefreshList();
    }

    private int CustomCeiling(float val)
    {
        return Mathf.Max(1, Mathf.CeilToInt(val));
    }

    public void SortButtonPress(int sortIndex)
    {
        for (int i = 0; i < sortListItems.Length; i++)
        {
            if (i != sortIndex)
            {
                sortListItems[i].sortDirection = -1;
            }

            sortListItems[i].sortArrow.enabled = (i == sortIndex);
        }

        ServerSortSettings sortSetting = sortListItems[sortIndex];
        if (sortSetting.sortDirection <= -1)
        {
            sortSetting.sortDirection = 1;
        }
        else
        {
            sortSetting.sortDirection = ((sortSetting.sortDirection == 1) ? 0 : 1);
        }

        Vector3 euler = sortSetting.sortArrow.transform.localEulerAngles;
        euler.z = ((sortSetting.sortDirection == 1) ? 0f : 180f);
        sortSetting.sortArrow.transform.localEulerAngles = euler;

        SortServerList(sortIndex);
    }

    private void SortServerList(int methodIndex)
    {
        if (displayHostedServers != Topan.MasterServer.hosts)
        {
            ForceRefreshServer();
            displayHostedServers = Topan.MasterServer.hosts;
        }

        if (displayHostedServers != null && displayHostedServers.Count > 0)
        {
            if (methodIndex == 0)
            {
                if (sortListItems[methodIndex].sortDirection == 1)
                {
                    displayHostedServers.Sort((s1, s2) => (s1.gameName).CompareTo(s2.gameName));
                }
                else
                {
                    displayHostedServers.Sort((s1, s2) => (s2.gameName).CompareTo(s1.gameName));
                }
            }
            else if (methodIndex == 1)
            {
                if (sortListItems[methodIndex].sortDirection == 1)
                {
                    displayHostedServers.Sort((s1, s2) => (s1.hostName).CompareTo(s2.hostName));
                }
                else
                {
                    displayHostedServers.Sort((s1, s2) => (s2.hostName).CompareTo(s1.hostName));
                }
            }
            else if (methodIndex == 2)
            {
                if (sortListItems[methodIndex].sortDirection == 1)
                {
                    displayHostedServers.Sort((s1, s2) => (s1.playerCount).CompareTo(s2.playerCount));
                }
                else
                {
                    displayHostedServers.Sort((s1, s2) => (s2.playerCount).CompareTo(s1.playerCount));
                }
            }
            else if (methodIndex == 3)
            {
                if (sortListItems[methodIndex].sortDirection == 1)
                {
                    displayHostedServers.Sort((s1, s2) => (s1.mapIndex).CompareTo(s2.mapIndex));
                }
                else
                {
                    displayHostedServers.Sort((s1, s2) => (s2.mapIndex).CompareTo(s1.mapIndex));
                }
            }
            else if (methodIndex == 4)
            {
                if (sortListItems[methodIndex].sortDirection == 1)
                {
                    displayHostedServers.Sort((s1, s2) => (s1.gameModeIndex).CompareTo(s2.gameModeIndex));
                }
                else
                {
                    displayHostedServers.Sort((s1, s2) => (s2.gameModeIndex).CompareTo(s1.gameModeIndex));
                }
            }
            else if (methodIndex == 5)
            {
                if (sortListItems[methodIndex].sortDirection == 1)
                {
                    displayHostedServers.Sort((s1, s2) => (s1.gameName).CompareTo(s2.gameName)); //Nothing for ping yet!
                }
                else
                {
                    displayHostedServers.Sort((s1, s2) => (s2.gameName).CompareTo(s1.gameName)); //Nothing for ping yet!
                }
            }
        }

        foreach (UIButton btn in listOfServerButtons)
        {
            btn.GetComponent<ServerRoom>().AssignHostDetails(pageNumber, isOnlineList);
        }

        StartCoroutine(FadeInServerElements());
    }

    private int GetSortMethod()
    {
        for (int i = 0; i < sortListItems.Length; i++)
        {
            if (sortListItems[i].sortDirection > -1)
            {
                return i;
            }
        }

        return -1;
    }
}