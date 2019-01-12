using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public enum DebugType {Normal, Warning, Error}
public class DeveloperConsole : MonoBehaviour {
	[System.Serializable]
	public class DevConsoleGUI {
		public UILabel title;
		public UIPanel mainPanel;
        public UIScrollView scrollView;
		public UIInput inputField;
		public UILabel outputField;
        public UILabel autoCompleteLabel;
	}
	
	[System.Serializable]
	public class SpawnablePrefab {
		public string prefabName = "object";
		public GameObject prefabObject;
		public Vector3 offset;
	}

    [System.Serializable]
    public class TestMap {
        public string sceneName = "TestMap";
    }
	
	public DevConsoleGUI devConsoleGUI = new DevConsoleGUI();
	public string startupString = "--- Developer console initialized. ---";
    public int maximumCharacters = 15000;
	public UIScrollBar scrollBar;
    public UILabel versionLabel;

	private List<string> _rCom;
    public List<string> restrictedCommands {
		get {
			if(_rCom == null) {
				_rCom = new List<string>();
				_rCom.Add("addplayerweapon");
				_rCom.Add("availablespawns");
				_rCom.Add("createobjective");
				_rCom.Add("delete");
				_rCom.Add("destroy");
                _rCom.Add("undodelete");
				_rCom.Add("falldamage");
				_rCom.Add("flymode");
				_rCom.Add("listweaponids");
				_rCom.Add("notify");
				_rCom.Add("restockammo");
				_rCom.Add("restockgrenades");
				_rCom.Add("setbulletdropfactor");
				_rCom.Add("setmuzzlevelocity");
				_rCom.Add("setricochetamount");
                _rCom.Add("setbulletforce");
				_rCom.Add("setroundtime");
				_rCom.Add("setspawnpos");
                _rCom.Add("settimescale");
                _rCom.Add("shiftposition");
                _rCom.Add("spawn");
                _rCom.Add("spawnatpos");
                _rCom.Add("networkspawn");
                _rCom.Add("toggleambience");
                _rCom.Add("bypassmp");
                _rCom.Add("godmode");
			}

			return _rCom;
		}
	}

	public List<string> ignoreKeywords = new List<string>();

	public static bool consoleIsOpen = false;
	public static string[] commandText;
	public static int selectedObjID;
	
    public TestMap[] testMaps = new TestMap[0];
	public SpawnablePrefab[] spawnablePrefabs = new SpawnablePrefab[0];
	public Dictionary<string, int> spawnablePrefabKeys;
	
	public static string lastCommand = "";

	public static Transform _csp;
	public static Transform curSpawnPos {
		get {
			if(_csp == null) {
				_csp = new GameObject("(curSpawn)").transform;
			}
			
			return _csp;
		}
	}

	private static int mDebug = -1;
	public static bool debuggingEnabled {
		get {
			if(mDebug <= -1) {
				mDebug = PlayerPrefs.GetInt("Debugging", 1);
			}

			return (mDebug == 1) ? true : false;
		}
		set {
			int val = (value) ? 1 : 0;

			if(val != mDebug) {
				PlayerPrefs.SetInt("Debugging", val);
				mDebug = val;
			}
		}
	}

	private static int mStacktrace = -1;
	public static bool enableStacktrace {
		get {
			if(mStacktrace <= -1) {
				mStacktrace = PlayerPrefs.GetInt("Stacktrace", 0);
			}

			return (mStacktrace == 1) ? true : false;
		}
		set {
			int val = (value) ? 1 : 0;
			
			if(val != mStacktrace) {
				PlayerPrefs.SetInt("Stacktrace", val);
				mStacktrace = val;
			}
		}
	}

    private static List<string> loggedList;
    private bool isTransition = false;
    private string oldInput;
	
	private static Camera _fCam;
	public static Camera flythoughCamera {
		get {
			if(_fCam == null) {
				Transform playerCamTr = GeneralVariables.mainPlayerCamera.transform;
				GameObject cam = new GameObject("(FTHROUGHCAM)");
				Camera camComp = cam.AddComponent<Camera>();
				FreeFlyCamera ffc = cam.AddComponent<FreeFlyCamera>();
				ffc.ignoreTimeScale = true;
				ffc.isDevFTCam = true;
				ffc.flySpeed = 10f;
				ffc.flySpeedSlow = 5f;
				ffc.flySpeedFast = 25f;
				camComp.backgroundColor = new Color(0.125f, 0.125f, 0.125f, 1f);
				camComp.nearClipPlane = 0.02f;
				camComp.farClipPlane = 10000f;
				camComp.depth = 1;
				
				if(playerCamTr) {
					cam.transform.position = playerCamTr.position;
					cam.transform.eulerAngles = playerCamTr.eulerAngles;
				}
				
				_fCam = camComp;				
			}
			
			return _fCam;
		}
	}
	
    private static DeveloperConsole cachedPrefab;
	private static DeveloperConsole m_Instance;
	public static DeveloperConsole Instance {
		get {
			if(m_Instance == null && Application.isPlaying) {
                if(!Application.isEditor) {
                    Application.RegisterLogCallback(HandleLog);
                }

                if(cachedPrefab != null) {
                    m_Instance = (DeveloperConsole)Instantiate(cachedPrefab);
                }
                else {
				    m_Instance = (DeveloperConsole)Instantiate(Resources.Load("Developer Console", typeof(DeveloperConsole)));
                }

                if(loggedList == null) {
                    loggedList = new List<string>();
                    m_Instance.ClearOutput();

                    if(debuggingEnabled && Application.isEditor) {
                        loggedList.Add("  - NOTE: [A33D17]Debugging (and stacktrace) are disabled. Please use the built-in editor console instead[-]");
                    }
                }
			}

			return m_Instance;
		}
	}

	private static string lastHandleLog = "";
	public static void HandleLog(string logString, string stackTrace, LogType type) {
		for(int i = 0; i < Instance.ignoreKeywords.Count; i++) {
			if(logString.ToLower().Contains(Instance.ignoreKeywords[i].ToLower())) {
				return;
			}
		}

		string appendStackTrace = (enableStacktrace) ? " |>| " + stackTrace : "";

        if((logString + appendStackTrace) == lastHandleLog) {
            return;
        }

		if(type == LogType.Assert || type == LogType.Error || type == LogType.Exception) {
            DebugError(logString + appendStackTrace);	
		}
        else if(type == LogType.Warning) {
            DebugWarning(logString + appendStackTrace);	
        }
		else if(type == LogType.Log) {
            DebugLog(logString + appendStackTrace);	
		}

        lastHandleLog = logString + appendStackTrace;
	}
	
	public static void ManualInit(DeveloperConsole cachedConsole) {
        if(cachedConsole != null) {
            cachedPrefab = cachedConsole;
        }

		Instance.SetEnable(false);
	}
					
	public static void ToggleConsole() {
		Instance.SetEnable(!consoleIsOpen);
	}
	
	public void SetEnable(bool e) {
        if(isTransition) {
            return;
        }

		StartCoroutine(FadeConsole(e));
	}
	
	public IEnumerator FadeConsole(bool enable) {	
		consoleIsOpen = enable;
		RestrictionManager.devConsole = enable;
		devConsoleGUI.inputField.enabled = enable;
				
		float alpha = devConsoleGUI.mainPanel.alpha;
        isTransition = true;
		if(enable) {
			Screen.lockCursor = false;
			devConsoleGUI.mainPanel.gameObject.SetActive(true);
            RebuildLogList(true);

			while(devConsoleGUI.mainPanel.alpha < 1f) {
				alpha += Time.unscaledDeltaTime * 6f;
				devConsoleGUI.mainPanel.alpha = Mathf.Clamp01(alpha);
				yield return null;
			}
		}
		else {
            if(GeneralVariables.player != null) {
                yield return null;
                Screen.lockCursor = true;
            }

			while(devConsoleGUI.mainPanel.alpha > 0f) {
				alpha -= Time.unscaledDeltaTime * 6f;
				devConsoleGUI.mainPanel.alpha = Mathf.Clamp01(alpha);
				yield return null;
			}

			devConsoleGUI.mainPanel.gameObject.SetActive(false);
		}

        isTransition = false;
	}
	
	public void ClearOutput() {
		loggedList.Clear();
        loggedList.Add(startupString);
        RebuildLogList(true);
        scrollBar.value = 0f;
	}

	void Start() {
		spawnablePrefabKeys = new Dictionary<string, int>();
		for(int i = 0; i < spawnablePrefabs.Length; i++) {
			spawnablePrefabKeys.Add(spawnablePrefabs[i].prefabName, i);
		}

        versionLabel.text = "DRDC " + DarkRef.GetBuildVersion(false);
		devConsoleGUI.mainPanel.alpha = 0f;
	}
	
	void Update() {        
		if(consoleIsOpen) {
			if(DeveloperConsole.selectedObjID != 0) {
                devConsoleGUI.title.text = "DEVELOPER CONSOLE [" + DeveloperConsole.selectedObjID.ToString() + "]";
			}
			else {
                devConsoleGUI.title.text = "DEVELOPER CONSOLE";
			}

            if(devConsoleGUI.inputField.value != oldInput) {
                if(devConsoleGUI.inputField.value != "") {
                    devConsoleGUI.autoCompleteLabel.text = AutoCompleteResult(devConsoleGUI.inputField.value);
                }
                else {
                    devConsoleGUI.autoCompleteLabel.text = "";
                }

                oldInput = devConsoleGUI.inputField.value;
            }

            if(Input.GetKeyDown(KeyCode.Space) && devConsoleGUI.autoCompleteLabel.text != "") {
                devConsoleGUI.inputField.value = devConsoleGUI.autoCompleteLabel.text;
            }

            if(Input.GetKeyDown(KeyCode.Return) && devConsoleGUI.inputField.isSelected && devConsoleGUI.inputField.value != "") {
                string toExecute = (devConsoleGUI.autoCompleteLabel.text != "") ? devConsoleGUI.autoCompleteLabel.text : devConsoleGUI.inputField.value;
				loggedList.Add(toExecute);
                RebuildLogList();
                StartCoroutineAction(toExecute);
			}

			if(Camera.main != null && Input.GetMouseButtonDown(0) && DarkRef.CheckAccess()) {
				Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(screenRay, out hit, Camera.main.farClipPlane)) {
					DeveloperConsole.selectedObjID = hit.collider.gameObject.GetInstanceID();
				}
				else {
					DeveloperConsole.selectedObjID = 0;
				}
			}
		}
	}

	private IEnumerator ExecuteActionCoroutine(string Value) {
		ExecuteAction(Value);

        yield return null;

        if(devConsoleGUI.scrollView.shouldMoveVertically) {
            scrollBar.value = 1f;
        }
	}
	
	public void ExecuteAction(string Value) {
        if(Value.StartsWith("www") || Value.StartsWith("http")) {
            Application.OpenURL(Value);
            Log("Opening URL in default browser: '" + Value + "'");
            devConsoleGUI.inputField.value = "";
            return;
        }

		string functionName = "";
		object[] arguments = new object[0];
		
		if(Value.IndexOf(' ') != -1) {
			functionName = Value.Substring(0, Value.IndexOf(' '));
			string functionParams = Value.Substring(Value.IndexOf(' ') + 1);
			string[] args = functionParams.Split(new char[]{','}, System.StringSplitOptions.None);
			
			arguments = new object[args.Length];		
			
			for(int i = 0; i < args.Length; i++){
				arguments[i] = (object)args[i];
			}
		}
		else {
			functionName = Value;	
		}
		
		MethodInfo toCall = typeof(DeveloperConsoleCommands).GetMethod(functionName, BindingFlags.Static | BindingFlags.Public);
		if(toCall != null && !(restrictedCommands.Contains(functionName) && !DarkRef.CheckAccess())) {
			try {
				toCall.Invoke(null, arguments);
				if(Value != "re" && Value != "") {
					DeveloperConsole.lastCommand = Value;
				}
			}
			catch {
				Log("Incorrect usage of '" + functionName + "'. This function has " + toCall.GetParameters().Length + " parameter" + ((toCall.GetParameters().Length > 1) ? "s" : ""), DebugType.Error); 	
			}
		}
		else{
            Log("Invalid command: '" + devConsoleGUI.inputField.value + "' does not exist!", DebugType.Error);	
		}

        devConsoleGUI.inputField.value = "";
	}
		
	public void Log(string message, DebugType debugType = DebugType.Normal) {
		if(debugType == DebugType.Normal) {
            loggedList.Add("  - [8A8A8A]" + message + "[-]");
		}
		else if(debugType == DebugType.Warning) {
            loggedList.Add("  - [C8B419]" + message + "[-]");
		}
		else if(debugType == DebugType.Error) {
            loggedList.Add("  - [B41414]" + message + "[-]");
		}

        RebuildLogList();
	}

    public void RebuildLogList(bool forceUpdate = false) {
        string output = "";
        for(int i = 0; i < loggedList.Count; i++) {
            if(i > 0) {
                output += "\n";
            }

            output += loggedList[i];
        }

        string extraAppend = (output.Length > maximumCharacters) ? "... (clearoutput for more room)" : "";
        if(!forceUpdate) {
            if(devConsoleGUI.mainPanel.alpha > 0.001f) {
                devConsoleGUI.outputField.text = output.Substring(0, Mathf.Min(output.Length, maximumCharacters)) + extraAppend;
            }
        }
        else {
            devConsoleGUI.outputField.text = output.Substring(0, Mathf.Min(output.Length, maximumCharacters)) + extraAppend;
        }
    }

    public string AutoCompleteResult(string input) {
        List<MethodInfo> methods = new List<MethodInfo>(typeof(DeveloperConsoleCommands).GetMethods(BindingFlags.Static | BindingFlags.Public));
        methods.Sort((m1, m2) => m1.Name.Length.CompareTo(m2.Name.Length));

        for(int i = 0; i < methods.Count; i++) {
            string funcName = methods[i].Name;

            if(funcName == "showcommands" || funcName == "getsystemid" || funcName == "google" || (!DarkRef.CheckAccess() && DeveloperConsole.Instance.restrictedCommands.Contains(funcName))) {
                continue;
            }

            if(input[0] != ' ' && funcName.StartsWith(DarkRef.RemoveSpaces(input))) {
                return funcName;
            }
        }

        return "";
    }
	
	//These functions are for use in other scripts.
	public static void DebugLog(string message) {
		if(!debuggingEnabled) {
			return;
		}
		
		Instance.Log(message, DebugType.Normal);
	}
	public static void DebugWarning(string message) {
		if(!debuggingEnabled) {
			return;
		}
		
		Instance.Log(message, DebugType.Warning);
	}
	public static void DebugError(string message) {
		if(!debuggingEnabled) {
			return;
		}
		
		Instance.Log(message, DebugType.Error);
	}
	
	public void StartCoroutineAction(string Value) {
        StopCoroutine("ExecuteActionCoroutine");
		StartCoroutine(ExecuteActionCoroutine(Value));
	}
}

public class DeveloperConsoleCommands {
    private static Dictionary<AudioSource, float> ambienceSounds;
    private static List<GameObject> recentDeletes = new List<GameObject>();

	public static void showcommands() {
		MethodInfo[] info = typeof(DeveloperConsoleCommands).GetMethods(BindingFlags.Static | BindingFlags.Public);
				
        List<MethodInfo> sortedInfo = new List<MethodInfo>(info);
        sortedInfo.Sort(delegate(MethodInfo mi1, MethodInfo mi2) {
            return mi1.Name.CompareTo(mi2.Name);
        });

		foreach(MethodInfo i in sortedInfo) {
			string funcName = i.Name;
			if(funcName != "showcommands" && funcName != "getsystemid" && !(!DarkRef.CheckAccess() && DeveloperConsole.Instance.restrictedCommands.Contains(funcName))) {
				int paramLength = i.GetParameters().Length;
				DeveloperConsole.Instance.Log(funcName + "  [" + paramLength + " parameter" + ((paramLength == 1) ? "" : "s").ToString() + "]");
			}
		}
	}

    public static void google(string q) {
        DeveloperConsole.Instance.Log("Googling term with default browser: '" + q + "'");
        Application.OpenURL("www.google.com/search?&q=" + q.Replace(" ", "%20"));
    }
	
	public static void showbandwidth(string t) {
		GeneralVariables.showBandwidth = DarkRef.ConvertStringToBool(t);
		DeveloperConsole.Instance.Log("Bandwidth Meter Toggled: " + DarkRef.ConvertStringToBool(t));
	}
	
	public static void enabledebugging(string ed) {
		DeveloperConsole.debuggingEnabled = DarkRef.ConvertStringToBool(ed);
		DeveloperConsole.Instance.Log("Debugging Toggled: " + DarkRef.ConvertStringToBool(ed));
	}
	
	public static void clearoutput() {
		DeveloperConsole.Instance.ClearOutput();
	}
	
	public static void falldamage(string height) {
		float hgt = 0f;
		
        PlayerReference pr = GeneralVariables.playerRef;
		if(pr != null) {
            if(float.TryParse(height, out hgt)) {
			    pr.GetComponent<PlayerVitals>().FallDamage(hgt);
			    DeveloperConsole.Instance.Log("Applied Fall Damage to Player: " + hgt.ToString("F2") + " meters");
            }
            else {
                DeveloperConsole.Instance.Log("Value is not in correct format!", DebugType.Error);
                return;
            }
		}
		else {
			DeveloperConsole.Instance.Log("Invalid request! An instance of player does not exist.", DebugType.Warning);
		}
	}

    public static void shiftposition(string x, string y, string z) {
        if(GeneralVariables.player == null) {
            DeveloperConsole.Instance.Log("Invalid request! An instance of player does not exist.", DebugType.Warning);
            return;
        }

        float vX, vY, vZ;
        if(float.TryParse(x, out vX) && float.TryParse(y, out vY) && float.TryParse(z, out vZ)) {
            Transform playerTr = GeneralVariables.player.transform;
            Vector3 modVector = new Vector3(vX, vY, vZ);
            modVector = playerTr.TransformDirection(modVector);
            playerTr.transform.position += modVector;

            DeveloperConsole.Instance.Log("Player transform's position shifted: (" + vX.ToString() + ", " + vY.ToString() + ", " + vZ.ToString() + ")");
        }
        else {
            DeveloperConsole.Instance.Log("Value(s) are not a number!", DebugType.Error);
            return;
        }
    }

    public static void setmuzzlevelocity(string velocity) {
        PlayerReference pr = GeneralVariables.playerRef;
        if(pr == null) {
            DeveloperConsole.Instance.Log("Invalid request! An instance of player does not exist.", DebugType.Warning);
            return;
        }

        if(pr.wm.currentGC == null) {
            DeveloperConsole.Instance.Log("Invalid request! No weapon is equipped.", DebugType.Warning);
            return;
        }

        float velo = 0f;
        if(float.TryParse(velocity, out velo)) {
            velo = Mathf.Clamp(velo, 0f, 10000f);
            pr.wm.currentGC.bulletInfo.muzzleVelocity = velo;
            DeveloperConsole.Instance.Log("Setting muzzle velocity for current weapon to: " + velo.ToString() + " m/s");
        }
        else {
            if(velocity == "default") {
                velo = Mathf.Clamp(WeaponDatabase.GetWeaponByID(pr.wm.currentGC.weaponID).bulletInfo.muzzleVelocity, 0f, 10000f);
                pr.wm.currentGC.bulletInfo.muzzleVelocity = velo;
                DeveloperConsole.Instance.Log("Setting muzzle velocity for current weapon to: " + velo.ToString() + " m/s");
                return;
            }

            DeveloperConsole.Instance.Log("Input value is not a number!", DebugType.Error);
        }
    }

    public static void setbulletdropfactor(string factor) {
        PlayerReference pr = GeneralVariables.playerRef;
        if(pr == null) {
            DeveloperConsole.Instance.Log("Invalid request! An instance of player does not exist.", DebugType.Warning);
            return;
        }

        if(pr.wm.currentGC == null) {
            DeveloperConsole.Instance.Log("Invalid request! No weapon is equipped.", DebugType.Warning);
            return;
        }

        float fac = 0f;
        if(float.TryParse(factor, out fac)) {
            fac = Mathf.Clamp(fac, 0f, 5f);
            pr.wm.currentGC.bulletInfo.gravityFactor = fac;
            DeveloperConsole.Instance.Log("Setting bullet drop factor for current weapon to: " + (fac * 100f).ToString("F0") + "% (" + (-Physics.gravity.y * fac).ToString("F2") + " m/s)");
        }
        else {
            DeveloperConsole.Instance.Log("Input value is not a number!", DebugType.Error);
            return;
        }
    }

    public static void setbulletforce(string amt) {
        PlayerReference pr = GeneralVariables.playerRef;
        if(pr == null) {
            DeveloperConsole.Instance.Log("Invalid request! An instance of player does not exist.", DebugType.Warning);
            return;
        }

        if(pr.wm.currentGC == null) {
            DeveloperConsole.Instance.Log("Invalid request! No weapon is equipped.", DebugType.Warning);
            return;
        }

        float force = 0f;
        if(float.TryParse(amt, out force)) {
            force = Mathf.Clamp(force, 0f, 1000000f);
            pr.wm.currentGC.bulletInfo.force = force;
            DeveloperConsole.Instance.Log("Setting bullet impact force to: " + force.ToString("F2"));
        }
        else {
            DeveloperConsole.Instance.Log("Input value is not a number!", DebugType.Error);
            return;
        }
    }
	
	public static void spawn(string prefabName, string quantity) {
		Transform targetTransform = (GeneralVariables.player != null) ? GeneralVariables.player.transform : Camera.main.transform;
		SpawnFunction(prefabName, quantity, targetTransform);
	}
	
	public static void spawnatpos(string prefabName, string quantity) {
		if(DeveloperConsole._csp == null) {
			DeveloperConsole.Instance.Log("Invalid request, you must set the spawn position first!", DebugType.Warning);
			return;
		}

		SpawnFunction(prefabName, quantity, DeveloperConsole.curSpawnPos);
	}

    public static void networkspawn(string prefabName) {
        if(!Topan.Network.isConnected) {
            DeveloperConsole.Instance.Log("Invalid request! You are not connected to a network!", DebugType.Error);
            return;
        }

        if(GeneralVariables.player == null) {
            DeveloperConsole.Instance.Log("Invalid request! A player instance must be available.", DebugType.Warning);
            return;
        }

        if(!DeveloperConsole.Instance.spawnablePrefabKeys.ContainsKey(prefabName)) {
            DeveloperConsole.Instance.Log("Prefab named '" + prefabName + "' does not exist!", DebugType.Error);
            return;
        }

        Transform origin = GeneralVariables.player.transform;
        DeveloperConsole.SpawnablePrefab toSpawn = DeveloperConsole.Instance.spawnablePrefabs[DeveloperConsole.Instance.spawnablePrefabKeys[prefabName]];

        RaycastHit hit;
        Vector3 posToSpawn = origin.position + (origin.forward * 2f) + (Vector3.up * 0.5f);
        if(Physics.Raycast(posToSpawn + (Vector3.down * 0.5f), Vector3.down, out hit, 3f)) {
            posToSpawn = hit.point + toSpawn.offset;
        }

        Quaternion someRot = Quaternion.Euler(toSpawn.prefabObject.transform.eulerAngles.x, origin.eulerAngles.y, toSpawn.prefabObject.transform.eulerAngles.z);

        try {
            Topan.Network.Instantiate(toSpawn.prefabObject, posToSpawn, someRot);
            DeveloperConsole.Instance.Log("Spawned '" + toSpawn.prefabName + "' over the network");
        }
        catch {
            DeveloperConsole.Instance.Log("Error occurred when attempting to instantiate prefab", DebugType.Error);
        }
    }

    public static void addplayerweapon(string index) {
        if(GeneralVariables.playerRef == null) {
            DeveloperConsole.Instance.Log("Invalid request! An instance of player does not exist.", DebugType.Warning);
            return;
        }

        int finalIndex = 0;
        if(int.TryParse(index, out finalIndex)) {
            finalIndex = WeaponDatabase.GetAvailableID(finalIndex);
            WeaponManager playerWM = GeneralVariables.playerRef.wm;
            playerWM.ignoreWeightDelayOnce = true;
            playerWM.AddWeapon(WeaponDatabase.GetAvailableID(finalIndex));
            DeveloperConsole.Instance.Log("Adding and switching to weapon: " + WeaponDatabase.GetWeaponByID(finalIndex).gunName);
        }
        else {
            DeveloperConsole.Instance.Log("Value is not an integer!", DebugType.Error);
            return;
        }
    }

    public static void listweaponids() {
        DeveloperConsole.Instance.Log("Listing all weapon IDs...");
        foreach(GunController gc in WeaponDatabase.publicGunControllers) {
            DeveloperConsole.Instance.Log("[" + gc.weaponID + "] - " + gc.gunName);
        }
    }

    public static void restockammo() {
        PlayerReference pr = GeneralVariables.playerRef;
        if(pr == null) {
            DeveloperConsole.Instance.Log("Invalid request! An instance of player does not exist.", DebugType.Warning);
            return;
        }

        if(pr.wm.currentGC == null) {
            DeveloperConsole.Instance.Log("Invalid request! No weapon is equipped.", DebugType.Warning);
            return;
        }

        AntiHackSystem.ProtectInt("currentAmmo", AntiHackSystem.RetrieveInt("clipSize"));
        AntiHackSystem.ProtectInt("ammoLeft", AntiHackSystem.RetrieveInt("ammoLeftCap"));
        DeveloperConsole.Instance.Log("Restocking ammo for the current weapon...");
    }

    public static void restockgrenades() {
        PlayerReference pr = GeneralVariables.playerRef;
        if(pr == null) {
            DeveloperConsole.Instance.Log("Invalid request! An instance of player does not exist.", DebugType.Warning);
            return;
        }

        AntiHackSystem.ProtectInt("t1Grenade", AntiHackSystem.RetrieveInt("t1GrenadeMax"));
        AntiHackSystem.ProtectInt("t2Grenade", AntiHackSystem.RetrieveInt("t2GrenadeMax"));
        DeveloperConsole.Instance.Log("Restocking grenades...");
    }
	
	public static void re() {
		if(DeveloperConsole.lastCommand != "") {
			DeveloperConsole.Instance.Log("Repeating last command: '" + DeveloperConsole.lastCommand + "'");
			DeveloperConsole.Instance.StartCoroutineAction(DeveloperConsole.lastCommand);
		}
		else {
			DeveloperConsole.Instance.Log("Invalid repeat! Last command is empty", DebugType.Warning);
		}
	}
	
	public static void availablespawns(){
		foreach (KeyValuePair<string, int> pair in DeveloperConsole.Instance.spawnablePrefabKeys){
			DeveloperConsole.Instance.Log(pair.Key);	
		}
	}
	
	public static void setspawnpos() {
		Transform target = (GeneralVariables.player != null) ? GeneralVariables.player.transform : Camera.main.transform;

		if(target == null) {
			DeveloperConsole.Instance.Log("You really messed something up...", DebugType.Error);
			return;
		}

		DeveloperConsole.curSpawnPos.position = target.position;
		DeveloperConsole.curSpawnPos.rotation = target.rotation;
		DeveloperConsole.Instance.Log("Setting spawn position " + DarkRef.PreciseStringVector3(DeveloperConsole.curSpawnPos.position) + " and rotation " + DarkRef.PreciseStringVector3(DeveloperConsole.curSpawnPos.eulerAngles) + " to current player's transform.", DebugType.Normal);

		if(target == Camera.main.transform) {
			DeveloperConsole.Instance.Log("NOTE: The spawn position is set to the main camera since no player instance was found.", DebugType.Warning);
		}
	}
	
	public static void settimescale(string scale) {
        if(Application.loadedLevelName == "Main Menu") {
            DeveloperConsole.Instance.Log("Time-scale cannot be used while in the main menu.", DebugType.Error);
			return;
        }
		
		float time = 0f;
		if(float.TryParse(scale, out time)) {
			if(time >= 0f && time <= 3f) {
				DarkRef.SetTimeScale(time);
				DeveloperConsole.Instance.Log("Time-scale set to: " + scale.ToString());
			}
			else {
                DarkRef.SetTimeScale(Mathf.Clamp(time, 0f, 3f));
				DeveloperConsole.Instance.Log("Value must be within the acceptable range (0.0 - 3.0). Clamping value automatically...", DebugType.Warning);
			}
		}
		else {
			DeveloperConsole.Instance.Log("Value is not a number!", DebugType.Error);
		}
	}

	public static void targetfps(string fps) {
		int finalFPS = 0;
		if(int.TryParse(fps, out finalFPS)) {
			finalFPS = Mathf.Clamp(finalFPS, 1, 300);
			Application.targetFrameRate = finalFPS;
            PlayerPrefs.SetInt("TargetFPS", finalFPS);
			DeveloperConsole.Instance.Log("Setting the target to " + finalFPS + " FPS", DebugType.Normal);
		}
		else {
			DeveloperConsole.Instance.Log("Value is not a number!", DebugType.Error);
		}
	}

	public static void delete() {
		if(DeveloperConsole.selectedObjID == 0) {
			DeveloperConsole.Instance.Log("You must select an object to delete first!", DebugType.Warning);
			return;
		}

		GameObject[] allObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
		GameObject sel = null;

		foreach(GameObject go in allObjects) {
			if(go.GetInstanceID() == DeveloperConsole.selectedObjID) {
				sel = go;
				DeveloperConsole.selectedObjID = 0;
				break;
			}
		}

		if(sel != null) {
            recentDeletes.Add(sel);
			sel.SetActive(false);
			DeveloperConsole.Instance.Log("Deleted object: " + sel.name);
		}
		else {
			DeveloperConsole.Instance.Log("GameObject with ID of '" + DeveloperConsole.selectedObjID.ToString() + "' does not exist!", DebugType.Error);
		}
		
		DeveloperConsole.selectedObjID = 0;
	}
	
	public static void destroy(string name) {
		GameObject sel = GameObject.Find(name);

		if(sel != null) {
            recentDeletes.Add(sel);
            sel.SetActive(false);
			DeveloperConsole.Instance.Log("Deleted object: " + sel.name);
		}
		else {
			DeveloperConsole.Instance.Log("GameObject named '" + name + "' does not exist!", DebugType.Error);
		}
	}

    public static void undodelete() {
        if(recentDeletes != null && recentDeletes.Count > 0) {
            GameObject lastDelete = recentDeletes[recentDeletes.Count - 1];

            if(lastDelete != null) {
                lastDelete.SetActive(true);
                DeveloperConsole.Instance.Log("Undoing last deleted object: " + lastDelete.name);
                lastDelete = null;
            }
            else {
                DeveloperConsole.Instance.Log("You are attempting to undo a null object! Removing automatically...", DebugType.Warning);
            }

            recentDeletes.RemoveAt(recentDeletes.Count - 1);
        }
        else {
            DeveloperConsole.Instance.Log("You haven't deleted an object recently", DebugType.Warning);
        }
    }
	
	public static void enablestacktrace(string t) {
		DeveloperConsole.enableStacktrace = DarkRef.ConvertStringToBool(t);
		string s = (DarkRef.ConvertStringToBool(t)) ? "enabled" : "disabled";
		DeveloperConsole.Instance.Log("Debugging stacktrace " + s);
	}
	
	public static void mapeditor() {
		DeveloperConsole.Instance.Log("Loading map editor...", DebugType.Warning);
		RestrictionManager.restricted = false;
		DeveloperConsole.Instance.SetEnable(false);
		Application.LoadLevel("MapEditor");	
	}

	public static void loadtestmap(string map) {
		int m = 0;
		
		if(int.TryParse(map, out m)) {
            bool throwError = true;
            for(int i = 0; i < DeveloperConsole.Instance.testMaps.Length; i++) {
                if(m == i) {
                    DeveloperConsole.Instance.Log("Loading Test Map #" + i.ToString() + " (" + DeveloperConsole.Instance.testMaps[i].sceneName + ")...", DebugType.Warning);
                    Application.LoadLevel(DeveloperConsole.Instance.testMaps[i].sceneName);
                    throwError = false;
                    break;
                }
            }

            if(throwError) {
                DeveloperConsole.Instance.Log("Test Map #" + map + " (Unknown) does not exist!", DebugType.Error);
                return;
            }
			
			RestrictionManager.restricted = false;
			DeveloperConsole.Instance.SetEnable(false);
		}
		else {
			DeveloperConsole.Instance.Log("Value is not an integer!", DebugType.Error);
		}		
	}
	
	public static void flymode(string enable) {
		Camera playerCam = GeneralVariables.mainPlayerCamera;
		bool boolValue = DarkRef.ConvertStringToBool(enable);
		if(boolValue) {
			RestrictionManager.allInput = true;
			DeveloperConsole.flythoughCamera.enabled = true;
			if(playerCam) {
				playerCam.enabled = false;
			}
		}
		else {
			RestrictionManager.allInput = false;
			DeveloperConsole.flythoughCamera.enabled = false;
			if(playerCam) {
				playerCam.enabled = true;
			}
		}
		
		DeveloperConsole.Instance.Log("Toggling flythrough mode: " + boolValue);
	}
	
	public static void notify(string title, string body) {
		NotificationSystem.Instance.CreateNotification(title, body, 6f);
		DeveloperConsole.Instance.Log("Generating notification...");
	}
	
	public static void createobjective(string name) {
		GameObject foundGO = GameObject.Find(name);
		if(foundGO == null) {
			DeveloperConsole.Instance.Log("Failed to create an objective! There is no such gameObject with the name of '" + name + "'", DebugType.Error);
			return;
		}
		
		DarkRef.CreateObjectiveMarker(foundGO.transform, "Destroy");
	}

	public static void clearobjectives() {
		DarkRef.ClearObjectiveMarkers();
		DeveloperConsole.Instance.Log("Cleared all current objectives");
	}
	
	public static void myviewid() {
		GameObject player = GeneralVariables.player;
		if(player == null) {
			DeveloperConsole.Instance.Log("Invalid request! An instance of player does not exist.", DebugType.Error);
			return;
		}
		
		if(!Topan.Network.isConnected) {
			DeveloperConsole.Instance.Log("You must be connected to a network.", DebugType.Warning);
			return;
		}
		
		Topan.NetworkView playerNetView = player.GetComponent<Topan.NetworkView>();
		if(playerNetView == null) {
			DeveloperConsole.Instance.Log("The player object does not have a network view.", DebugType.Error);
			return;
		}
		
		DeveloperConsole.Instance.Log("Your network ID for local player is: " + playerNetView.m_viewID.ToString());
	}

    public static void systeminfo() {
        DeveloperConsole.Instance.Log("--System Information--");
        DeveloperConsole.Instance.Log("Operating System: " + SystemInfo.operatingSystem);
		int coreCount = SystemInfo.processorCount;
        DeveloperConsole.Instance.Log("CPU: " + SystemInfo.processorType + " (" + coreCount + " core" + ((coreCount == 1) ? "" : "s") + ")");
        DeveloperConsole.Instance.Log("Memory [RAM]: " + SystemInfo.systemMemorySize.ToString() + " MB");
        DeveloperConsole.Instance.Log("Graphics Card: " + SystemInfo.graphicsDeviceName + " (" + SystemInfo.graphicsMemorySize + " MB)");
		DeveloperConsole.Instance.Log("  *- Type 'gpuinfo' for more GPU information -*");
    }

    public static void gpuinfo() {
        DeveloperConsole.Instance.Log("--GPU Support/Additional Information--");
		DeveloperConsole.Instance.Log("Pixel Fillrate: " + Mathf.Abs(SystemInfo.graphicsPixelFillrate * 1000000).ToString("#,#") + " pixels/second");
        DeveloperConsole.Instance.Log("Real-time Shadows: " + SystemInfo.supportsShadows.ToString());

		bool renderTex = SystemInfo.supportsRenderTextures;
        DeveloperConsole.Instance.Log("Render Textures: " + renderTex.ToString(), (renderTex) ? DebugType.Normal : DebugType.Warning);

        DeveloperConsole.Instance.Log("3D Textures: " + SystemInfo.supports3DTextures.ToString());
        DeveloperConsole.Instance.Log("Sparse Textures (Mega-textures): " + SystemInfo.supportsSparseTextures.ToString());
        DeveloperConsole.Instance.Log("Stencil Buffers: " + (SystemInfo.supportsStencil > 0).ToString());
        DeveloperConsole.Instance.Log("Image Effects: " + SystemInfo.supportsImageEffects.ToString());
        DeveloperConsole.Instance.Log("Compute Shaders: " + SystemInfo.supportsShadows.ToString());
		DeveloperConsole.Instance.Log("NPOT Support: " + SystemInfo.npotSupport.ToString());
    }

    public static void dofdebug() {
        if(GameSettings.settingsController.wDepthOfField == 0) {
            DeveloperConsole.Instance.Log("Cannot toggle Depth of Field debugging since it's disabled.", DebugType.Error);
            return;
        }

        WeaponDepthOfField wdof = (WeaponDepthOfField)Object.FindObjectOfType(typeof(WeaponDepthOfField));
        if(wdof == null) {
            DeveloperConsole.Instance.Log("Cannot find the depth of field component.", DebugType.Warning);
            return;
        }

        wdof.visualizeFocus = !wdof.visualizeFocus;
        DeveloperConsole.Instance.Log("Depth of Field Debugging Mode: " + wdof.visualizeFocus);
    }
	
	public static void getsystemid() {
		DeveloperConsole.Instance.Log("Your System ID: " + DarkRef.GetSystemID);
	}

	private static void SpawnFunction(string prefabName, string quantity, Transform origin) {
		try {
			int quantityInt = 0;
			if(int.TryParse(quantity, out quantityInt)) {
				DeveloperConsole.SpawnablePrefab toSpawn = DeveloperConsole.Instance.spawnablePrefabs[DeveloperConsole.Instance.spawnablePrefabKeys[prefabName]];
				GameObject toInstantiate = toSpawn.prefabObject;
				
				MeshFilter mf = toInstantiate.GetComponent<MeshFilter>();
				if(mf == null) {
					mf = toInstantiate.GetComponentInChildren<MeshFilter>();
				}
				
				float spacing = 1f;
				if(mf != null) {
					spacing = Vector3.Scale(mf.sharedMesh.bounds.extents, toInstantiate.transform.lossyScale).magnitude + 0.05f;
				}
				
				Vector3 pos = origin.position + (origin.forward * 2f) + (Vector3.up * 0.5f) + (-origin.right * (quantityInt * 0.5f));
				
				float faceAngle = origin.eulerAngles.y;
				for(int i = 0; i < quantityInt; i++) {
					RaycastHit hit;
					Vector3 posToSpawn = Vector3.zero;
					if(Physics.Raycast(pos, Vector3.down, out hit, 3f)) {
						posToSpawn = hit.point + toSpawn.offset;
					}
					else {
						posToSpawn = pos;
					}

					bool isSetSpawn = (DeveloperConsole.curSpawnPos != null && origin == DeveloperConsole.curSpawnPos);
					
					Transform spawnTr = ((GameObject)GameObject.Instantiate(toInstantiate, posToSpawn, (isSetSpawn) ? origin.rotation : Quaternion.identity)).transform;

					if(!isSetSpawn) {
						Vector3 sEuler = spawnTr.eulerAngles;
						sEuler.y = faceAngle;
						spawnTr.eulerAngles = sEuler;
					}
					
					pos += origin.right * spacing;
				}
				
				DeveloperConsole.Instance.Log("Spawned " + quantity + " instances of '" + prefabName + "'");
			}
			else {
				DeveloperConsole.Instance.Log("Quantity parameter is not an integer!", DebugType.Error);
				return;
			}
		}
		catch {
			DeveloperConsole.Instance.Log("Instantiation failed: " + prefabName, DebugType.Error);	
		}
	}

    public static void toggleambience(string e) {
        if(ambienceSounds == null) {
            ambienceSounds = new Dictionary<AudioSource, float>();

            AudioSource[] allSources = (AudioSource[])Object.FindObjectsOfType(typeof(AudioSource));
            foreach(AudioSource source in allSources) {
                if(source.name.ToLower().Contains("ambience")) {
                    ambienceSounds.Add(source, source.volume);
                }
            }
        }

        bool enable = DarkRef.ConvertStringToBool(e);
        foreach(KeyValuePair<AudioSource, float> sourcePair in ambienceSounds) {
            sourcePair.Key.volume = ((enable) ? sourcePair.Value : 0f);
        }

        DeveloperConsole.Instance.Log("Ambience sounds: " + ((enable) ? "Enabled" : "Disabled"));
    }

	public static void networkinfo() {
		if(!Topan.Network.isConnected) {
            DeveloperConsole.Instance.Log("You are not connected to a server!", DebugType.Error);	
			return;
		}

		DeveloperConsole.Instance.Log("Player ID: " + Topan.Network.player.id);
	}

    public static void showplayerlist() {
        if(!Topan.Network.isConnected) {
            DeveloperConsole.Instance.Log("You are not connected to a server!", DebugType.Error);
            return;
        }

        DeveloperConsole.Instance.Log("List of Players:");
        for(int i = 0; i < Topan.Network.connectedPlayers.Length; i++) {
            CombatantInfo theirInfo = (CombatantInfo)Topan.Network.connectedPlayers[i].GetInitialData("dat");
            DeveloperConsole.Instance.Log(i + " - " + theirInfo.username);
        }
    }

    public static void kick(string id) {
        if(!Topan.Network.isConnected) {
            DeveloperConsole.Instance.Log("You are not connected to a server!", DebugType.Error);
            return;
        }

        if(!Topan.Network.isServer) {
            DeveloperConsole.Instance.Log("You are not the host of this server!", DebugType.Error);
            return;
        }

        int theirID = 0;
        if(int.TryParse(id, out theirID)) {
            theirID = Mathf.Clamp(theirID, 0, Topan.Network.connectedPlayers.Length - 1);

            if(theirID == 0) {
                DeveloperConsole.Instance.Log("You cannot kick yourself!", DebugType.Error);
                return;
            }

            CombatantInfo theirInfo = (CombatantInfo)Topan.Network.connectedPlayers[theirID].GetInitialData("dat");
            DeveloperConsole.Instance.Log("Kicked player: " + theirInfo.username);
            GeneralVariables.connectionView.RPC(new int[1]{theirID}, "KickPlayer", (byte)1);
        }
        else {
            if(id == "all") {
                DeveloperConsole.Instance.Log("Kicking all players on the server...");
                GeneralVariables.connectionView.RPC(Topan.RPCMode.Others, "KickPlayer", (byte)1);
                return;
            }

            for(int i = 1; i < Topan.Network.connectedPlayers.Length; i++) {
                Topan.NetworkPlayer player = Topan.Network.connectedPlayers[i];
                CombatantInfo theirInfo = (CombatantInfo)player.GetInitialData("dat");
                if(theirInfo.username.ToLower() == id.ToLower()) {
                    DeveloperConsole.Instance.Log("Kicked player: " + theirInfo.username);
                    GeneralVariables.connectionView.RPC(new int[1]{player.id}, "KickPlayer", (byte)1);
                    return;
                }
            }

            DeveloperConsole.Instance.Log("The value that you have entered is not a valid ID or name", DebugType.Warning);
            return;
        }
    }

    public static void ban(string id) {
        if(!Topan.Network.isConnected) {
            DeveloperConsole.Instance.Log("You are not connected to a server!", DebugType.Error);
            return;
        }

        if(!Topan.Network.isServer) {
            DeveloperConsole.Instance.Log("You are not the host of this server!", DebugType.Error);
            return;
        }

        int theirID = 0;
        if(int.TryParse(id, out theirID)) {
            theirID = Mathf.Clamp(theirID, 0, Topan.Network.connectedPlayers.Length - 1);
            Topan.NetworkPlayer player = Topan.Network.connectedPlayers[theirID];

            if(theirID == 0) {
                DeveloperConsole.Instance.Log("You cannot ban yourself!", DebugType.Error);
                return;
            }

            if(Server.connectionBanList.Contains(player.peerObject.UniqueIdentifier.ToString())) {
                DeveloperConsole.Instance.Log("This player is already banned!", DebugType.Warning);
                return;
            }

            CombatantInfo theirInfo = (CombatantInfo)player.GetInitialData("dat");
            DeveloperConsole.Instance.Log("Banned player: " + theirInfo.username);
            Server.connectionBanList.Add(player.peerObject.UniqueIdentifier.ToString());
            GeneralVariables.connectionView.RPC(new int[1]{theirID}, "KickPlayer", (byte)1);
        }
        else {
            for(int i = 1; i < Topan.Network.connectedPlayers.Length; i++) {
                Topan.NetworkPlayer player = Topan.Network.connectedPlayers[i];
                CombatantInfo theirInfo = (CombatantInfo)player.GetInitialData("dat");
                if(theirInfo.username.ToLower() == id.ToLower()) {
                    if(Server.connectionBanList.Contains(player.peerObject.UniqueIdentifier.ToString())) {
                        DeveloperConsole.Instance.Log("This player is already banned!", DebugType.Warning);
                        return;
                    }

                    DeveloperConsole.Instance.Log("Banning player: " + theirInfo.username);
                    Server.connectionBanList.Add(player.peerObject.UniqueIdentifier.ToString());
                    GeneralVariables.connectionView.RPC(new int[1]{player.id}, "KickPlayer", (byte)1);
                    return;
                }
            }

            DeveloperConsole.Instance.Log("The value that you have entered is not a valid ID or name", DebugType.Warning);
            return;
        }
    }

    public static void godmode(string t) {
        bool enable = DarkRef.ConvertStringToBool(t);
        PlayerVitals.godmode = enable;
        DeveloperConsole.Instance.Log("God mode: " + ((enable) ? "Enabled" : "Disabled"));
    }

    public static void setplayername(string name) {
        if(Topan.Network.isConnected) {
            DeveloperConsole.Instance.Log("You cannot change your name while connected to a server!", DebugType.Error);
            return;
        }

        if(!AccountManager.isGuestAccount) {
            DeveloperConsole.Instance.Log("You cannot change your name while logged into an account!", DebugType.Error);
            return;
        }

        name = DarkRef.RemoveSpaces(name);
        if(name.Length > 26) {
            DeveloperConsole.Instance.Log("The name requested is too long! (26 characters maximum)", DebugType.Error);
            return;
        }

        string validated = "";
        for(int i = 0; i < name.Length; i++) {
            char c = name[i];
            if((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-') {
                validated += c;
            }
        }

        DeveloperConsole.Instance.Log("Setting your name to: " + "'" + validated + "'");
        AccountManager.defaultName = validated;
        AccountManager.profileData.username = validated;
    }

    public static void setroundtime(string amt) {
        if(!Topan.Network.isConnected) {
            DeveloperConsole.Instance.Log("You are not connected to a server!", DebugType.Error);
            return;
        }

        if(!Topan.Network.isServer) {
            DeveloperConsole.Instance.Log("You are not the host of this server!", DebugType.Error);
            return;
        }

        int newTime = 0;
        if(int.TryParse(amt, out newTime)) {
            newTime = Mathf.Clamp(newTime, 0, 3600);
            GeneralVariables.server.gameTime = newTime;
            DeveloperConsole.Instance.Log("Setting match timer to: " + amt);
        }
        else {
            DeveloperConsole.Instance.Log("The value that you have entered is not valid", DebugType.Warning);
            return;
        }
    }

    public static void bypassmp() {
        MultiplayerMenu.multiplayerEnabled = true;
        DeveloperConsole.Instance.Log("Success!");
    }
}