using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AntiHackSystem : MonoBehaviour
{
    public float checkInterval = 0.05f; //compare game/system speed every X seconds
    public int speedDiffThreshold = 4; //in millisecond; by default: 4.
    public int violationLimit = 4; //Maximum violations before kicking you out.
    public int randomizeSeedInterval = 120; //Interval in frames. 0 to disable.

    private int gameMilliseconds = 0;
    private int lastGameMilliseconds = -1;
    private long systemMilliseconds = 0;
    private long lastSystemMilliseconds = -1;
    private int speedViolations = 0;
    private float lastSysCheck = 0f;
    private float lastCheck;

    private static DateTime startDate = new DateTime(2014, 1, 1);
    private static DateTime appInitializeTime = DateTime.MinValue;

    private static bool initializedSeeds = false;
    private static int protectionIntSeed = -1;
    private static Dictionary<string, string> protectedInt = new Dictionary<string, string>();

    private static float protectionFloatSeed = -1f;
    private static Dictionary<string, string> protectedFloat = new Dictionary<string, string>();

    private static float protectionVector3Seed = -1f;
    private static Dictionary<string, string> protectedVector3 = new Dictionary<string, string>();

    private static float protectionQuaternionSeed = -1f;
    private static Dictionary<string, string> protectedQuaternion = new Dictionary<string, string>();

    void Awake()
    {
        if (appInitializeTime == DateTime.MinValue)
        {
            appInitializeTime = DateTime.UtcNow;
        }

        Initialize();
    }

    public static void Initialize()
    {
        if (initializedSeeds)
        {
            return;
        }

        RandomizeSeeds();
        initializedSeeds = true;
    }

    void Update()
    {
#if !UNITY_EDITOR
        SpeedHackDetector();
#endif

        if (initializedSeeds && randomizeSeedInterval > 0 && Time.frameCount % Mathf.Max(1, randomizeSeedInterval) == 0)
        {
            RandomizeSeeds();
        }

        if (Time.unscaledTime - lastSysCheck >= 1f)
        {
            SystemTimeCheck();
            lastSysCheck = Time.unscaledTime;
        }
    }

    private void SpeedHackDetector()
    {
        if (Time.unscaledTime - lastCheck >= checkInterval)
        {
            gameMilliseconds = (int)(Time.realtimeSinceStartup * 1000f);
            systemMilliseconds = (long)(DateTime.UtcNow.Ticks / 10000);

            if (lastGameMilliseconds > -1 && lastSystemMilliseconds > -1)
            {
                int difference = Mathf.Abs((gameMilliseconds - lastGameMilliseconds) - (int)(systemMilliseconds - lastSystemMilliseconds));

                if (difference > speedDiffThreshold)
                {
                    speedViolations++;
                    Debug.Log("Speed violation: " + speedViolations + " (" + difference + " ticks)" + " [time: " + Time.time + "]");
                }
                else
                {
                    speedViolations = 0;
                }

                if ((Topan.Network.isConnected && !Mathf.Approximately(Time.timeScale, 1f)) || speedViolations >= violationLimit || speedViolations < 0)
                {
                    if (Topan.Network.isConnected)
                    {
                        Topan.Network.Disconnect();
                    }
                    else
                    {
                        Application.Quit();
                    }
                }
            }

            lastGameMilliseconds = gameMilliseconds;
            lastSystemMilliseconds = systemMilliseconds;
            lastCheck = Time.unscaledTime;
        }
    }

    public static void RandomizeSeeds()
    {
        List<string> iKeys = new List<string>(protectedInt.Keys);
        List<string> fKeys = new List<string>(protectedFloat.Keys);
        List<string> v3Keys = new List<string>(protectedVector3.Keys);
        List<string> qKeys = new List<string>(protectedQuaternion.Keys);

        int decryptInt = 0;
        float decryptFloat = 0f;
        Vector3 decryptVector = Vector3.zero;
        Vector4 decryptQuaternion = Vector4.zero;

        int oldIntSeed = protectionIntSeed;
        float oldFloatSeed = protectionFloatSeed;
        float oldVector3Seed = protectionVector3Seed;
        float oldQuaternionSeed = protectionQuaternionSeed;

        UnityEngine.Random.seed = Mathf.RoundToInt((float)(DateTime.UtcNow.Subtract(startDate).TotalSeconds * UnityEngine.Random.value));
        protectionIntSeed = UnityEngine.Random.Range(2, 999);
        protectionFloatSeed = Mathf.Round(UnityEngine.Random.Range(2f, 999f) * 100f) / 100f;
        protectionVector3Seed = Mathf.Round(UnityEngine.Random.Range(2f, 999f) * 80f) / 100f;
        protectionQuaternionSeed = Mathf.Round(UnityEngine.Random.Range(2f, 999f) * 110f) / 100f;

        if (initializedSeeds)
        {
            foreach (string key in iKeys)
            {
                decryptInt = int.Parse(protectedInt[key]) - (oldIntSeed * 3);
                decryptInt /= oldIntSeed;
                decryptInt *= protectionIntSeed;
                protectedInt[key] = (decryptInt + (protectionIntSeed * 3)).ToString();
            }
            foreach (string key in fKeys)
            {
                decryptFloat = float.Parse(protectedFloat[key]) - (oldFloatSeed * 2.4f);
                decryptFloat /= oldFloatSeed;
                decryptFloat *= protectionFloatSeed;
                protectedFloat[key] = (decryptFloat + (protectionFloatSeed * 2.4f)).ToString();
            }
            foreach (string key in v3Keys)
            {
                decryptVector = ReadVector3String(protectedVector3[key]) - (Vector3.one * oldVector3Seed);
                decryptVector /= oldVector3Seed;
                decryptVector *= protectionVector3Seed;
                protectedVector3[key] = FormatVector3ToString(decryptVector + (Vector3.one * protectionVector3Seed));
            }
            foreach (string key in qKeys)
            {
                decryptQuaternion = ReadVector4String(protectedQuaternion[key]) - (Vector4.one * oldQuaternionSeed * 1.2f);
                decryptQuaternion /= oldQuaternionSeed;
                decryptQuaternion *= protectionQuaternionSeed;
                protectedQuaternion[key] = FormatVector4ToString(decryptQuaternion + (Vector4.one * protectionQuaternionSeed * 1.2f));
            }
        }
    }

    private void SystemTimeCheck()
    {
        float sessionTimeSystem = (float)(DateTime.UtcNow.Subtract(appInitializeTime).TotalMilliseconds / 1000);

        if (Mathf.Abs(Time.unscaledTime - sessionTimeSystem) >= 10f)
        {
            if (Topan.Network.isConnected)
            {
                Topan.Network.Disconnect();
            }

            Application.Quit();
        }
    }

    #region PROTECT FUNCTIONS
    public static void ProtectInt(string parameterName, int val)
    {
        Initialize();
        protectedInt[parameterName] = ((val * protectionIntSeed) + (protectionIntSeed * 3)).ToString();
    }

    public static void ProtectFloat(string parameterName, float val)
    {
        Initialize();
        protectedFloat[parameterName] = ((val * protectionFloatSeed) + (protectionFloatSeed * 2.4f)).ToString();
    }

    public static void ProtectVector3(string parameterName, Vector3 val)
    {
        Initialize();
        protectedVector3[parameterName] = FormatVector3ToString((val * protectionVector3Seed) + (Vector3.one * protectionVector3Seed));
    }

    public static void ProtectQuaternion(string parameterName, Quaternion val)
    {
        Initialize();
        protectedQuaternion[parameterName] = FormatVector4ToString((QuaternionToVector4(val) * protectionQuaternionSeed) + (Vector4.one * protectionQuaternionSeed * 1.2f));
    }
    #endregion

    #region RETRIEVE FUNCTIONS
    public static int RetrieveInt(string parameterName)
    {
        Initialize();

        if (!protectedInt.ContainsKey(parameterName))
        {
            Debug.Log("Failed to retrieve the protected integer! [" + parameterName + "]");
        }
        else
        {
            return (int.Parse(protectedInt[parameterName]) - (protectionIntSeed * 3)) / protectionIntSeed;
        }

        return -1;
    }

    public static float RetrieveFloat(string parameterName)
    {
        Initialize();

        if (!protectedFloat.ContainsKey(parameterName))
        {
            Debug.Log("Failed to retrieve the protected float! [" + parameterName + "]");
        }
        else
        {
            return (float.Parse(protectedFloat[parameterName]) - (protectionFloatSeed * 2.4f)) / protectionFloatSeed;
        }

        return -1f;
    }

    public static Vector3 RetrieveVector3(string parameterName)
    {
        Initialize();

        if (!protectedVector3.ContainsKey(parameterName))
        {
            Debug.Log("Failed to retrieve the protected Vector3! [" + parameterName + "]");
        }
        else
        {
            return (ReadVector3String(protectedVector3[parameterName]) - (Vector3.one * protectionVector3Seed)) / protectionVector3Seed;
        }

        return Vector3.zero;
    }

    public static Quaternion RetrieveQuaternion(string parameterName)
    {
        Initialize();

        if (!protectedQuaternion.ContainsKey(parameterName))
        {
            Debug.Log("Failed to retrieve the protected quaternion! [" + parameterName + "]");
        }
        else
        {
            return Vector4ToQuaternion((ReadVector4String(protectedQuaternion[parameterName]) - (Vector4.one * protectionQuaternionSeed * 1.2f)) / protectionQuaternionSeed);
        }

        return Quaternion.identity;
    }
    #endregion

    private static Vector4 QuaternionToVector4(Quaternion toConvert)
    {
        return new Vector4(toConvert.x, toConvert.y, toConvert.z, toConvert.w);
    }

    private static Quaternion Vector4ToQuaternion(Vector4 toConvert)
    {
        return new Quaternion(toConvert.x, toConvert.y, toConvert.z, toConvert.w);
    }

    private static string FormatVector3ToString(Vector3 input)
    {
        return input.x.ToString() + "|" + input.y.ToString() + "|" + input.z.ToString();
    }

    private static string FormatVector4ToString(Vector4 input)
    {
        return input.x.ToString() + "|" + input.y.ToString() + "|" + input.z.ToString() + "|" + input.w.ToString();
    }

    private static Vector3 ReadVector3String(string toRead)
    {
        string[] vec3Val = toRead.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
        if (vec3Val.Length == 3)
        {
            return new Vector3(float.Parse(vec3Val[0]), float.Parse(vec3Val[1]), float.Parse(vec3Val[2]));
        }

        return Vector3.zero;
    }

    private static Vector4 ReadVector4String(string toRead)
    {
        string[] vec4Val = toRead.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
        if (vec4Val.Length == 4)
        {
            return new Vector4(float.Parse(vec4Val[0]), float.Parse(vec4Val[1]), float.Parse(vec4Val[2]), float.Parse(vec4Val[3]));
        }

        return Vector4.zero;
    }
}