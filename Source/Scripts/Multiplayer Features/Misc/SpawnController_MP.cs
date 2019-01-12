using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SpawnController_MP : MonoBehaviour {
    public bool inSpawnScreen = false;

    public Camera spectatorCamera;
    public Camera uiCamera;
    public UISprite blackSprite;
    public UILabel chatOutput;
    public float fadeSpeed = 1f;
    public UILabel matchStatus;
    public UILabel matchName;
    public UILabel matchDetails;
    public UILabel spectateLabel;
    public UILabel spectateNote;
    public SpectatorFollow spectateFollow;
    public UIPopupList primarySelection;
    public UIPopupList secondarySelection;
    public UILabel timeLabel;
    public KillFeedManager killFeed;
    public UILabel redTitle;
    public UISlider redTeamProgress;
    public UILabel redTeamValue;
    public UILabel blueTitle;
    public UISlider blueTeamProgress;
    public UILabel blueTeamValue;
    public ShowTooltip primaryTooltip;
    public ShowTooltip secondaryTooltip;
    public GeneralChat chatBox;
    public UIInput chatInput;
    public float spectateCycleInterval = 10f;

    [HideInInspector] public float blurREM = 0f;

    private Topan.NetworkView _snv;
    private Topan.NetworkView spawnNetView {
        get {
            if(_snv == null && GeneralVariables.Networking != null) {
                _snv = Topan.Network.GetNetworkView(GeneralVariables.Networking.gameObject);
            }

            return _snv;
        }
    }

    private MultiplayerGUI mpGUI;
    private UICamera uiCam;
    private Transform specCamTr;
    private GUISway specCamSway;
    private DistortionEffect distortTransition;
    private Transform spectatorRoot;
    private List<Transform> spectatorPositions;
    private List<Transform> myAvailableTeamList;
    private bool isTransitioning;
    private bool isFading;
    private bool autoSpectate;
    private float autoSpectateTimer;
    private int specIndex = 0;

    void Awake() {
        GeneralVariables.spawnController = this;
        GeneralVariables.uiController.Awake();

        specCamTr = spectatorCamera.transform;

        uiCam = uiCamera.GetComponent<UICamera>();
        specCamSway = spectatorCamera.GetComponent<GUISway>();
        distortTransition = spectatorCamera.GetComponent<DistortionEffect>();

        GameObject specRoot = GameObject.FindGameObjectWithTag("SpectatorRoot");
        if(specRoot != null) {
            spectatorRoot = specRoot.transform;

            spectatorPositions = new List<Transform>();
            foreach(Transform t in spectatorRoot) {
                spectatorPositions.Add(t);
            }

            specIndex = UnityEngine.Random.Range(0, spectatorPositions.Count);
        }

        myAvailableTeamList = new List<Transform>();
        blackSprite.alpha = 1f;
        FadeInOut(true, 0.7f);
        isTransitioning = false;
        distortTransition.baseIntensity = 0f;
        distortTransition.enabled = false;
        autoSpectate = true;
        autoSpectateTimer = Time.time;
    }

    void Start() {
        if(Topan.Network.isConnected) {
            GeneralVariables.Networking.spawnControl = this;
        }

        primarySelection.items.Clear();
        secondarySelection.items.Clear();
        for(int i = 0; i < WeaponDatabase.publicGunControllers.Length; i++) {
            GunController sGun = WeaponDatabase.publicGunControllers[i];
            primarySelection.items.Add(sGun.gunName);
            secondarySelection.items.Add(sGun.gunName);
        }

        primarySelection.value = "NR-94";
        secondarySelection.value = "Beretta 92";

        mpGUI = GeneralVariables.multiplayerGUI;
    }

    void Update() {
        uiCam.enabled = !GameManager.isPaused && !RoundEndManager.isRoundEnded;
        chatOutput.alpha = 1f - blackSprite.alpha;
        uiCam.GetComponent<BlurEffect>().blurSpread = blurREM;

        GunController primaryWep = WeaponDatabase.GetWeaponByID(primarySelection.selectionIndex);
        primaryTooltip.textSize = 0.8f;
        primaryTooltip.text = "Weapon Statistics:[C6BCB6]" + "||" + "Damage: " + (primaryWep.bulletInfo.damage * primaryWep.bulletsPerShot)
                                                  + "||" + "Rate of Fire: " + primaryWep.firstRPM.ToString("F0") + " RPM"
                                                  + "||" + "Vertical Recoil: " + primaryWep.upKickAmount
                                                  + "||" + "Horizontal Recoil: " + primaryWep.sideKickAmount
                                                  + "||" + "Effective Range: " + primaryWep.GetMinimumRange() + "-" + primaryWep.GetMaximumRange() + "m"
                                                  + "||" + "Weight: " + primaryWep.weaponWeight.ToString("F2") + " kg";

        GunController secondaryWep = WeaponDatabase.GetWeaponByID(secondarySelection.selectionIndex);
        secondaryTooltip.textSize = 0.8f;
        secondaryTooltip.text = "Weapon Statistics:[C6BCB6]" + "||" + "Damage: " + (secondaryWep.bulletInfo.damage * secondaryWep.bulletsPerShot)
                                                    + "||" + "Rate of Fire: " + secondaryWep.firstRPM.ToString("F0") + " RPM"
                                                    + "||" + "Vertical Recoil: " + secondaryWep.upKickAmount
                                                    + "||" + "Horizontal Recoil: " + secondaryWep.sideKickAmount
                                                    + "||" + "Effective Range: " + secondaryWep.GetMinimumRange() + "-" + secondaryWep.GetMaximumRange() + "m"
                                                    + "||" + "Weight: " + secondaryWep.weaponWeight.ToString("F2") + " kg";

        if(GeneralVariables.gameModeHasTeams) {
            if(spectateFollow.target == null) {
                GetAllTeamInstances();
            }
            else {
                Rigidbody theRigid = spectateFollow.target.GetComponent<Rigidbody>();
                if(theRigid != null && !theRigid.isKinematic) {
                    spectateFollow.target = null;
                    GetAllTeamInstances();
                }
            }
        }

        if(mpGUI != null) {
            timeLabel.color = mpGUI.timerLabels[0].color;
            if(GeneralVariables.Networking.countingDown) {
                timeLabel.text = "MATCH STARTING: " + GeneralVariables.Networking.countdown.ToString();
            }
            else {
                if(mpGUI.timerLabels[0].text == "--") {
                    timeLabel.text = "WAITING FOR: " + mpGUI.serverPL.ToString() + " player" + ((mpGUI.serverPL == 1) ? "" : "s");
                }
                else {
                    timeLabel.text = mpGUI.timerLabels[0].text + ":" + mpGUI.timerLabels[1].text;
                }
            }

            if(GeneralVariables.gameModeHasTeams) {
                redTitle.text = "RED TEAM";
                blueTitle.text = "BLUE TEAM";
            }
            else {
                redTitle.text = (mpGUI.youAreWinning) ? "YOU" : mpGUI.winningPlayerName.ToUpper();
                blueTitle.text = (mpGUI.youAreWinning) ? mpGUI.runnerUpName.ToUpper() : "YOU"; 
            }

            redTeamProgress.value = mpGUI.redTeamProgress.value;
            redTeamProgress.foregroundWidget.color = mpGUI.redTeamProgress.foregroundWidget.color;
            blueTeamProgress.value = mpGUI.blueTeamProgress.value;
            blueTeamProgress.foregroundWidget.color = mpGUI.blueTeamProgress.foregroundWidget.color;
            redTeamValue.text = mpGUI.redTeamValue.text;
            blueTeamValue.text = mpGUI.blueTeamValue.text;
        }

        if(Topan.Network.isConnected && GeneralVariables.Networking != null) {
            string suffix = "ready";
            int playersWaiting = Topan.Network.connectedPlayers.Length;
            if(GeneralVariables.Networking.matchStarted) {
                matchStatus.text = "BATTLE INITIATED";
                matchStatus.alpha = matchStatus.defaultAlpha;
                suffix = "in battle";
            }
            else {
                matchStatus.text = "BATTLE AWAITING";
                matchStatus.alpha = (matchStatus.defaultAlpha - 0.2f) + Mathf.PingPong(Time.time * 2f * 0.2f, 0.2f);
                playersWaiting = Mathf.CeilToInt(playersWaiting * 0.25f);

                if(Topan.Network.HasServerInfo("pl")) {
                    playersWaiting -= (int)((byte)Topan.Network.GetServerInfo("pl"));
                }
            }

            matchName.text = Topan.Network.GameName;
            matchDetails.text = StaticMapsList.mapsArraySorted[(byte)Topan.Network.GetServerInfo("m")].mapName + "\n" + "> [5C5C5C]" + NetworkingGeneral.currentGameType.typeName + "[-]" + "\n" + "\n" +
                                Mathf.Max(0, playersWaiting).ToString() + " combatant" + ((playersWaiting == 0 || playersWaiting > 1) ? "s " : " ") + suffix;
        }

        if(spectatorPositions != null && spectatorPositions.Count > 0) {
            if(Input.GetKeyDown(KeyCode.Space) && !RestrictionManager.restricted && UICamera.selectedObject == null && !isTransitioning) {
                autoSpectate = false;
                StartCoroutine(TransitionToIndex(specIndex + 1));
            }

            if(autoSpectate && Time.time - autoSpectateTimer >= spectateCycleInterval && specIndex < spectatorPositions.Count && spectatorPositions.Count > 1 && !isTransitioning) {
                int newRandomPoint = 0;

                do {
                    newRandomPoint = Random.Range(0, spectatorPositions.Count);
                }
                while(newRandomPoint == specIndex);

                autoSpectateTimer = Time.time;
                StartCoroutine(TransitionToIndex(newRandomPoint));
            }

            specIndex = Mathf.Clamp(specIndex, 0, spectatorPositions.Count + myAvailableTeamList.Count - 1);

            if(specIndex >= spectatorPositions.Count && myAvailableTeamList.Count > 0) {
                spectateFollow.enabled = true;
                spectateFollow.offset = Vector3.up * 0.8f;
                spectateFollow.target = myAvailableTeamList[specIndex - spectatorPositions.Count];
                spectateLabel.text = "Spectating: " + myAvailableTeamList[specIndex - spectatorPositions.Count].root.name;
                spectateNote.text = "[SPACE] to switch cameras" + "\n" + "[RMB] to rotate camera";
            }
            else {
                spectateFollow.enabled = false;
                spectateFollow.target = null;

                //Regular spectator cameras
                specCamTr.position = spectatorPositions[specIndex].position;
                specCamSway.focusRot = spectatorPositions[specIndex].eulerAngles;
                spectateLabel.text = "Spectating: Camera_" + (specIndex + 1).ToString("000");
                spectateNote.text = "[SPACE] to switch cameras";
            }
        }
    }

    public void SpawnPlayer() {
        if(Topan.Network.isConnected && spawnNetView != null && !isFading) {
            StartCoroutine(SpawnCoroutine());
        }
    }

    private IEnumerator SpawnCoroutine() {
        WeaponManager.mpSpawnStartPrimary = primarySelection.selectionIndex;
        WeaponManager.mpSpawnStartSecondary = secondarySelection.selectionIndex;

        isFading = true;
        while(blackSprite.alpha < 1f) {
            blackSprite.alpha = Mathf.MoveTowards(blackSprite.alpha, 1f, Time.deltaTime * 3f);
            yield return null;
        }

        inSpawnScreen = false;
        spawnNetView.RPC(Topan.RPCMode.Server, "RequestInstantiate", (byte)Topan.Network.player.id);
        while(GeneralVariables.player == null) {
            yield return null;
        }

        RestrictionManager.restricted = false;
        isFading = false;
        blackSprite.alpha = 1f;
        gameObject.SetActive(false);
    }

    private void GetAllTeamInstances() {
        myAvailableTeamList.Clear();

        byte myTeam = (byte)Topan.Network.player.GetPlayerData("team");
        for(int i = 0; i < GeneralVariables.Networking.availablePlayers.Length; i++) {
            Transform thisPlayer = GeneralVariables.Networking.availablePlayers[i];
            if(thisPlayer.childCount >= 2 && thisPlayer.GetComponent<MovementSync_Proxy>() != null && thisPlayer.GetComponent<MovementSync_Proxy>().playerTeam == myTeam) {
                Rigidbody rigid = thisPlayer.GetChild(1).GetComponent<Rigidbody>();
                if(rigid != null && rigid.isKinematic) {
                    myAvailableTeamList.Add(thisPlayer.GetChild(1));
                }
            }
        }

        /*
        for(int i = 0; i < GeneralVariables.Networking.availableBots.Length; i++) {
            Transform thisBot = GeneralVariables.Networking.availableBots[i];
            if(thisBot.childCount >= 2 && thisBot.GetComponent<BotMovement>() != null && thisBot.GetComponent<BotMovement>().myBotPlayer.team == myTeam) {
                Rigidbody rigid = thisBot.GetChild(1).GetComponent<Rigidbody>();
                if(rigid != null && rigid.isKinematic) {
                    myAvailableTeamList.Add(thisBot.GetChild(1));
                }
            }
        }*/
    }

    private IEnumerator TransitionToIndex(int index) {
        isTransitioning = true;
        distortTransition.enabled = true;

        float timer = 0f;
        while(timer < 0.25f) {
            timer += Time.deltaTime;
            distortTransition.baseIntensity = (timer / 0.25f) * 1.2f;
            yield return null;
        }

        if(GeneralVariables.gameModeHasTeams) {
            GetAllTeamInstances();
        }

        specIndex = index;

        if(specIndex > spectatorPositions.Count + myAvailableTeamList.Count - 1) {
            specIndex = 0;
        }

        while(timer < 0.5f) {
            timer += Time.deltaTime;
            distortTransition.baseIntensity = Mathf.Max(0f, ((0.5f - timer) / 0.25f) * 1.2f);
            yield return null;
        }

        distortTransition.enabled = false;
        yield return new WaitForSeconds(0.05f);
        isTransitioning = false;
    }

    public void FadeInOut(bool fadeIn, float delay = 0f) {
        if(fadeIn) {
            if(Topan.Network.isConnected && myAvailableTeamList.Count > 0) {
                specIndex = spectatorPositions.Count;
            }

            if(specIndex < spectatorPositions.Count) {
                autoSpectate = true;
            }

            chatBox.UpdateChatList();
        }
        else {
            autoSpectate = false;
        }

        autoSpectateTimer = Time.time;
        StopAllCoroutines();
        StartCoroutine(StartFade(fadeIn, delay));
    }

    private IEnumerator StartFade(bool e, float d) {
        inSpawnScreen = e;

        blackSprite.alpha = (e) ? 1f : 0f;

        if(d > 0f) {
            yield return new WaitForSeconds(d);
        }

        Screen.lockCursor = false;

        if(e) {
            GeneralVariables.uiController.guiCamera.enabled = false;
            UICamera.selectedObject = null;

            while(blackSprite.alpha > 0f) {
                blackSprite.alpha -= Time.unscaledDeltaTime * fadeSpeed;

                if(isFading) {
                    yield break;
                }

                yield return null;
            }
        }
        else {
            while(blackSprite.alpha < 1f) {
                blackSprite.alpha += Time.unscaledDeltaTime * fadeSpeed;

                if(isFading) {
                    yield break;
                }

                yield return null;
            }
        }
    }

    public void AddToFeed(string killerName, string victimName, int weaponIndex) {
        killFeed.AddToFeed(killerName, victimName, weaponIndex);
    }
}