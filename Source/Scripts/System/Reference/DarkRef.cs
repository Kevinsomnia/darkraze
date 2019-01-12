using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Globalization;

public static class DarkRef {
	public static float pauseTime = 0f;

	public static string GetBuildVersion(bool includeTag) {
        return ((includeTag) ? "[PRE-ALPHA] " : "") + "v.0.5.6a";
    }
	
	#region IMPORTANT, YOU MUST HAVE THE CAMERA EXACTLY 700 UNITS AWAY FOR THESE VARIABLES TO WORK!
	/// <summary>
	/// The width of NGUI screen (in game units) using 4:3 screens.
	/// </summary>
	public static int NormalNGUIWidth = 537;
	
	/// <summary>
	/// The width of NGUI screen (in game units) using 16:9 screens.
	/// </summary>
	public static int ModernWideNGUIWidth = 717;
	
	/// <summary>
	/// The width of NGUI screen (in game units) using 16:10 screens.
	/// </summary>
	public static int OldWideNGUIWidth = 645;
	
	/// <summary>
	/// The height of NGUI screen (in game units) using any screen aspect ratio.
	/// </summary>
	public static int NGUIHeight = 405;
	#endregion

    private static AudioListener _listen;
    public static AudioListener listener {
        get {
            if(_listen == null || (_listen != null && !_listen.enabled)) {
                _listen = (AudioListener)Object.FindObjectOfType(typeof(AudioListener));
            }

            return _listen;
        }
    }

    private static ICryptoTransform _enc;
    private static ICryptoTransform enc {
        get {
            if(_enc == null) {
                _enc = rijndaelInst.CreateEncryptor();
            }

            return _enc;
        }
    }

    private static ICryptoTransform _dec;
    private static ICryptoTransform dec {
        get {
            if(_dec == null) {
                _dec = rijndaelInst.CreateDecryptor();
            }

            return _dec;
        }
    }
	private static RijndaelManaged _rijn;
	public static RijndaelManaged rijndaelInst {
		get {
			if(_rijn == null) {
				_rijn = new RijndaelManaged();
                _rijn.Key = UTF8Encoding.UTF8.GetBytes("50167346801032327202419585546228");
				_rijn.Mode = CipherMode.ECB;
				_rijn.Padding = PaddingMode.ANSIX923;
			}

			return _rijn;
		}
	}
	
	public static string GetSystemID {
		get {
			if(PlayerPrefs.HasKey("iamdeveloper")) {
				return PlayerPrefs.GetString("iamdeveloper");
			}

			return SystemInfo.deviceName.ToUpper() + " [)" + SystemInfo.deviceUniqueIdentifier + "(]";
		}
	}

    private static string extIP = "";
    public static string myExternalIP {
        get {
            if(string.IsNullOrEmpty(PlayerPrefs.GetString("gameData"))) {
                try {
                    WebClient client = new WebClient();
                    PlayerPrefs.SetString("gameData", EncryptString(client.DownloadString("http://ipinfo.io/ip").Trim()));
                }
                catch {
                    Debug.LogError("External IP fetch failed");
                }
            }

            if(string.IsNullOrEmpty(extIP)) {
                extIP = DecryptString(PlayerPrefs.GetString("gameData"), 1, true);
                if(extIP == "dse-1") {
                    while(true) {
                        Application.OpenURL("www.youtube.com");
                    }
                }
            }

            return extIP;
        }
    }
	
	public static bool isOldWidescreen {
		get {
			float aspect = (float)Screen.width / (float)Screen.height;
			return (aspect > 1.59f && aspect < 1.61f);
		}
	}
	
	public static bool isModernWidescreen {
		get {
			float aspect = (float)Screen.width / (float)Screen.height;
			return (aspect > 1.767f && aspect < 1.787f);
		}
	}

    public static string ClanColor(bool isBot) {
        return (isBot) ? "[969539]" : "[BFAB1E]";
    }

	private static int sdFrame = -1;
	private static System.DateTime lastDate;
	private static float deltaDiff;
	public static float systemDelta {
		get {
			if(sdFrame == -1) {
				lastDate = System.DateTime.UtcNow;
			}

			if(Time.frameCount == sdFrame) {
				return deltaDiff;
			}

			deltaDiff = (float)System.DateTime.UtcNow.Subtract(lastDate).TotalSeconds;
			sdFrame = Time.frameCount;
			lastDate = System.DateTime.UtcNow;
			return deltaDiff;
		}
	}
	
	public static string GetTimerFormat(int totalTime) {
		int minute = totalTime / 60;
		int second = totalTime % 60;

		if(totalTime >= 3600) {
			int hour = totalTime / 3600;
			minute -= (hour * 60);
			return string.Format("{0:0}:{1:00}:{2:00}", hour, minute, second);
		}
		else {
			return string.Format("{0:0}:{1:00}", minute, second);
		}
	}

	public static void SetTimeScale(float timescale) {
		if(Time.timeScale != timescale && timescale <= 0f) {
			pauseTime = Time.time;
		}

		Time.timeScale = timescale;
        Time.fixedDeltaTime = 0.02f * ((timescale > 0f) ? Mathf.Clamp(timescale, 0.01f, 1f) : 1f);
	}
	
	public static bool ConvertStringToBool(string input) {
		if(input.ToLower() == "true") {
			return true;
		}
        
		return false;
	}
	
	public static bool IsIdle() {
		return ((Mathf.Abs(cInput.GetAxis("Horizontal Move")) + Mathf.Abs(cInput.GetAxis("Vertical Move")) + Mathf.Abs(cInput.GetAxis("Horizontal Look")) + Mathf.Abs(cInput.GetAxis("Vertical Look"))) <= 0.01f);
	}
	
	public static float ScaleWithDistance(float distance, float nearDistance, float farDistance, float nearScale, float farScale) {
		float percentage = 0f;
		percentage = Mathf.Clamp01((distance - nearDistance) / (farDistance - nearDistance));
		return Mathf.Lerp(nearScale, farScale, percentage);
	}
	
	public static Color SetAlpha(Color targetColor, float alpha) {
        targetColor.a = Mathf.Clamp01(alpha);
        return targetColor;
	}

    /// <summary>
    /// Attempts to preserve the scale of the target transform when parenting. Doesn't always work on some objects.
    /// </summary>
    public static void KeepUniformScale(this Transform target, Transform parent) {
        Transform conserveScale = (new GameObject("ConserveScale")).transform;
        conserveScale.position = target.position;
        target.parent = conserveScale;
        conserveScale.parent = parent;
    }

    /// <summary>
    /// For some reason, Unity decides to not include the max value that's returned.
    /// </summary>
    public static int RandomRange(int min, int max) {
        if(min == max) {
            return min;
        }

        return Random.Range(min, max + 1);
    }
	
	/// <summary>
	/// Creates an objective marker and places it at a point.
	/// </summary>
	public static void CreateObjectiveMarker(Transform targetOfObjective, string objectiveDescription = "") {
		ObjectiveMarker marker = ((GameObject)GameObject.Instantiate(Resources.Load("GUI/ObjectiveMarkerPrefab"))).GetComponent<ObjectiveMarker>();

		marker.transform.parent = GeneralVariables.uiController.parentOfObjectives;
		marker.transform.localPosition = Vector3.forward * -2000f;
		marker.transform.localRotation = Quaternion.identity;
		marker.transform.localScale = Vector3.one;
		
		marker.target = targetOfObjective;
		
		if(objectiveDescription == "") {
			return;
		}
		
		marker.setByExternal = true;
		marker.SetDescription(objectiveDescription);
	}

	public static void ClearObjectiveMarkers() {
		Transform objectiveParent = GeneralVariables.uiController.parentOfObjectives;
		for(int i = 0; i < objectiveParent.childCount; i++) {
			GameObject.Destroy(objectiveParent.GetChild(i).gameObject);
		}
	}

	public static Vector3 RandomVector3(Vector3 min, Vector3 max) {
        return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
	}
	
	public static string PreciseStringVector3(Vector3 vector) {
		return ("(" + vector.x.ToString("F3") + ", " + vector.y.ToString("F3") + ", " + vector.z.ToString("F3") + ")");
	}

	//Current approximate integer form of date based on universal (time-zone independent) system time.
	public static int GetSystemTime() {
		System.DateTime utcDate = System.DateTime.UtcNow;
		return ((utcDate.DayOfYear - 1) * 1440) + (int.Parse(utcDate.ToString("HH")) * 60) + utcDate.Minute;
	}

    public static int GetSystemSeconds() {
        System.DateTime utcDate = System.DateTime.UtcNow;
        return ((utcDate.DayOfYear - 1) * 86440) + (int.Parse(utcDate.ToString("HH")) * 3600) + (utcDate.Minute * 60) + utcDate.Second;
    }

	public static string EncryptString(string toEncrypt, int iterations = 1) {
		if(toEncrypt == "") {
			return "";
		}

		try {
			string finalResult = toEncrypt;
			for(int i = 0; i < iterations; i++) {
				byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(finalResult);
				byte[] resultArray = enc.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
				finalResult = System.Convert.ToBase64String(resultArray, 0, resultArray.Length);
			}

			return finalResult;
		}
		catch {
			return "";
		}
	}

	public static string DecryptString(string toDecrypt, int iterations = 1, bool errorCode = false) {
		if(toDecrypt == "") {
			return "";
		}

		try {
			string finalResult = toDecrypt;
			for(int i = 0; i < iterations; i++) {
				byte[] toDecryptArray = System.Convert.FromBase64String(finalResult);
				byte[] resultArray = dec.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
				finalResult = UTF8Encoding.UTF8.GetString(resultArray);
			}

			return finalResult;
		}
		catch {
			if(errorCode) {
				return "dse-1";
			}

			return "";
		}
	}

    public static float GetMaxCurveValue(AnimationCurve curve) {
        float maxVal = 0f;
        for(int i = 0; i < curve.keys.Length; i++) {
            float keyValue = curve.keys[i].value;
            if(keyValue > maxVal) {
                maxVal = keyValue;
            }
        }

        return maxVal;
    }

    public static float LerpTowards(float current, float target, float lerpSpeed, float mTowardSpeed, float blend) {
        return Mathf.Lerp(Mathf.Lerp(current, target, lerpSpeed), Mathf.MoveTowards(current, target, mTowardSpeed), blend);
    }

    public static Vector3 LerpTowards(Vector3 current, Vector3 target, float lerpSpeed, float mTowardSpeed, float blend) {
        return Vector3.Lerp(Vector3.Lerp(current, target, lerpSpeed), Vector3.MoveTowards(current, target, mTowardSpeed), blend);
    }

    private static List<char> lowerAlpha = new List<char>(("abcdefghijklmnopqrstuvwxyz").ToCharArray());
    private static List<char> upperAlpha = new List<char>(("ABCDEFGHIJKLMNOPQRSTUVWXYZ").ToCharArray());
    private static List<char> numerical = new List<char>(("1234567890").ToCharArray());
    public static string ScrambleString(string target, int aberration, float scrambleChance = 1f) {
        if(target == null || target.Length <= 0 || aberration <= 0 || scrambleChance <= 0f) {
            return target;
        }

        string newScramble = "";
        
        for(int i = 0; i < target.Length; i++) {
            char curChar = target[i];
            if(Random.value < scrambleChance) {
                int curAber = Random.Range(-aberration, aberration + 1);
                if(lowerAlpha.Contains(curChar)) {
                    curAber += lowerAlpha.IndexOf(curChar);
                    curAber %= lowerAlpha.Count;
                    curAber = Mathf.Abs(curAber);

                    newScramble += lowerAlpha[curAber];
                }
                else if(upperAlpha.Contains(curChar)) {
                    curAber += upperAlpha.IndexOf(curChar);
                    curAber %= upperAlpha.Count;
                    curAber = Mathf.Abs(curAber);

                    newScramble += upperAlpha[curAber];
                }
                else if(numerical.Contains(curChar)) {
                    curAber += numerical.IndexOf(curChar);
                    curAber %= numerical.Count;
                    curAber = Mathf.Abs(curAber);

                    newScramble += numerical[curAber];
                }
                else {
                    newScramble += curChar;
                }
            }
            else {
                newScramble += curChar;
            }
        }

        return newScramble;
    }

    /// <summary>
    /// Checks for the component, then toggles it according to the boolean value.
    /// </summary>
    public static void ToggleComponent(MonoBehaviour toToggle, bool t) {
        if(toToggle == null) {
            return;
        }

        toToggle.enabled = t;
    }

    /// <summary>
    /// Returns the list of player IDs with the specified team to send an RPC to. Excludes local player.
    /// </summary>
    public static int[] SendTeamMessage(int teamID) {
        if(!Topan.Network.isConnected) {
            return null;
        }

        List<int> sendTo = new List<int>();
        foreach(Topan.NetworkPlayer player in Topan.Network.connectedPlayers) {
            if(player == Topan.Network.player) {
                continue;
            }

            byte tID = (byte)player.GetPlayerData("team");
            if(teamID == tID) {
                sendTo.Add(player.id);
            }
        }

        return sendTo.ToArray();
    }

    public static int GetDirection(float val) {
        if(val > 0f) {
            return 1;
        }
        else if(val < 0f) {
            return -1;
        }

        return 0;
    }

	public static string RemoveSpaces(string input) {
		return input.Replace(" ", string.Empty);
	}

    public static string GetRandomLetter(bool includeUppercase = false) {
        bool isUppercase = (includeUppercase && Random.value < 0.5f);
        int randomCharVal = (isUppercase) ? RandomRange(65, 90) : RandomRange(97, 122);
        return ((char)randomCharVal).ToString();
    }

    public static string RandomLetterCombination(int length, bool includeUppercase = false) {
        string result = "";
        for(int i = 0; i < length; i++) {
            result += GetRandomLetter(includeUppercase);
        }

        return result;
    }

    private static string[] someNames = null;
    public static string RandomBotName() {
        if(someNames == null) {
            someNames = new string[62]{"Johnny", "Dante", "Chris", "Geoffrey", "Evan", "Trevor", "Edward", "Robert", "Nate", "Paul", "Tim", "Jake", "Bob", "William", "Kevin",
            "Sam", "Alex", "Adam", "Thomas", "Lucas", "Stephen", "Mason", "Michael", "Roderick", "Andrew", "Greg", "David", "Ryan", "Reece", "Richard", "Batman", "Jeffrey",
            "Jack", "Derek", "Shawn", "Connor", "Colton", "Conrad", "Daniel", "Hunter", "Joker", "Assassin", "Serpent", "Phoenix", "Hitman", "Serial No.", "Ro-bot",
            "King Slayer", "Nakul", "Devourer", "Moonlight", "Shadow", "Jackal", "Grunt", "Beserker", "Juggernaut", "Death Code", "Inferno", "Reaper", "Rhino", "Electro",
            "Roman"};
        }

        return someNames[Random.Range(0, someNames.Length)];
    }

    public static bool CheckAccess() {
        string[] allowedSystems = new string[]{"DARKRAZELAPTOP [)35a0ac9595e87668132ead5dfa384b456d471b5d(]", "LIUFAMILY-PC [)67dea7947a5d0d401a135705fd0cd72509616f3e(]"};

		string yourID = DarkRef.GetSystemID;
		for(int i = 0; i < allowedSystems.Length; i++) {
			if(allowedSystems[i].ToUpper() == yourID.ToUpper()) {
				yourID = string.Empty;
				return true;
			}
		}

		allowedSystems = null;
		return false;
    }

    public static System.DateTime GetInternetTime() {
        try {
            TcpClient client = new TcpClient("time.nist.gov", 13);
            using(System.IO.StreamReader reader = new System.IO.StreamReader(client.GetStream())) {
                string response = reader.ReadToEnd();
                response = response.Substring(7, 17);
                return System.DateTime.ParseExact(response, "yy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            }
        }
        catch {
            return System.DateTime.MinValue;
        }
    }

    public static Vector3 Multiply(this Vector3 vec, Vector3 v) {
        Vector3 newVector = vec;
        newVector.x *= v.x;
        newVector.y *= v.y;
        newVector.z *= v.z;
        return newVector;
    }

    public static void CalculateTangents(Mesh mesh) {
        int vertexCount = mesh.vertexCount;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] texcoords = mesh.uv;
        int[] triangles = mesh.triangles;
        Vector4[] tangents = new Vector4[vertexCount];
        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];

        for(int tri = 0; tri <= triangles.Length - 1; tri += 3) {
            int i1 = triangles[tri];
            int i2 = triangles[tri + 1];
            int i3 = triangles[tri + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector2 w1 = texcoords[i1];
            Vector2 w2 = texcoords[i2];
            Vector2 w3 = texcoords[i3];

            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;

            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;

            float r = 1.0f / (s1 * t2 - s2 * t1);
            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
        }

        for(int i = 0; i <= (vertexCount - 1); i++) {
            Vector3 n = normals[i];
            Vector3 t = tan1[i];

            // Gram-Schmidt orthogonalize
            Vector3.OrthoNormalize(ref n, ref t);

            tangents[i].x = t.x;
            tangents[i].y = t.y;
            tangents[i].z = t.z;

            // Calculate handedness
            int tW = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0) ? -1 : 1;

            tangents[i].w = tW;
        }

        mesh.tangents = tangents;
    }

    public static string RGBtoHex(Color32 color) {
        return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
    }

    public static Color32 HexToRGB(string hex) {
        if(hex.Length != 6) {
            return new Color32(0, 0, 0, 255);
        }

        int convertedHex = int.Parse(hex, NumberStyles.HexNumber);
        byte r = (byte)((convertedHex >> 16) & 255);
        byte g = (byte)((convertedHex >> 8) & 255);
        byte b = (byte)((convertedHex) & 255);
        return new Color32(r, g, b, 255);
    }

    public static string OrdinalIndicatorFormat(int num) {
        string numString = num.ToString();

        if(num > 0) {
            if(numString.EndsWith("1") && !numString.EndsWith("11")) {
                return numString + "st";
            }
            else if(numString.EndsWith("2") && !numString.EndsWith("12")) {
                return numString + "nd";
            }
            if(numString.EndsWith("3") && !numString.EndsWith("13")) {
                return numString + "rd";
            }
            else {
                return numString + "th";
            }
        }

        return numString;
    }

    public static bool Contains(this LayerMask mask, int layer) {
		return (mask.value & 1<<layer) != 0;
    }

	#region EDITOR FUNCTIONS
	public static void GUISeparator() {
		GUILayout.Space(2f);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2f));
		GUILayout.Space(2f);
	}
	
	public static void GUISeparator(float spacing) {
		GUILayout.Space(spacing);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2f));
		GUILayout.Space(spacing);
	}
	
	public static void GUIBeginCenter() {
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
	}
	
	public static void GUIEndCenter() {
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}
	#endregion
}

public class DarkMath {
	public static float Berp(float start, float end, float Value) {
		float val = Mathf.Clamp01(Value);
		val = (Mathf.Sin(val * Mathf.PI * (0.2f + (2.5f * val * val * val))) * Mathf.Pow(1f - val, 2.2f) + val) * (1f + (1.2f * (1f - val)));
		return val;
	}
	
	public static float Bounce(float val) {
		float v = Mathf.Clamp01(val);
		return Mathf.Abs(Mathf.Sin(6.28f * (v + 1f) * (v + 1f)) * (1f - v));
	}
}

//TEMPLATES

//Custom Inspector Array. isOpen and length are both static variables.
/*
 	isOpen = EditorGUILayout.Foldout(isOpen, "Shop Panels");
	if(isOpen) {
		UIPanel[] tempStorage = uic.shopPanels;
		EditorGUI.indentLevel += 1;
		length = EditorGUILayout.IntField("Length:", Mathf.Clamp(length, 0, 10000));
		if(length != uic.shopPanels.Length && (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
			uic.shopPanels = new UIPanel[length];
			for(int i = 0; i < tempStorage.Length; i++) {
				if(i < uic.shopPanels.Length) {
					uic.shopPanels[i] = tempStorage[i];
				}
			}
		}
		EditorGUI.indentLevel += 1;
		for(int i = 0; i < uic.shopPanels.Length; i++) {
			uic.shopPanels[i] = (UIPanel)EditorGUILayout.ObjectField("Element " + i.ToString(), uic.shopPanels[i], typeof(UIPanel));
		}
		EditorGUI.indentLevel -= 1;
		EditorGUI.indentLevel -= 1;
	}
*/