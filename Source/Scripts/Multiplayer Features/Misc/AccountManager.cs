using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;
using System.Text;

public class AccountManager : MonoBehaviour
{
    [System.Serializable]
    public class ProfileInfoGUI
    {
        public UILabel username;
        public UILabel profileStats;
        public UILabel combatStats;
        public UILabel matchStats;
        public UISlider xpBar;
        public UILabel xpText;
    }

    [System.Serializable]
    public class ProfileInfo
    {
        public string username = "Guest101";
        public string clan = "";
        public int rank = 1;
        public int curXP = 0;
        public int kills = 0;
        public int deaths = 0;
        public int assists = 0;
        public int matchesWon = 0;
        public int matchesLost = 0;
        public int currency = 2500;
        public int premCurrency = 0;
    }

    public static int[] targetXP;

    public int startingExp = 500;
    public float linearExpGrowth = 200f;
    public float exponentialExpGrowth = 1.08f;
    public int expRoundingInterval = 25;
    public bool debugTargetXp = false;

    public ProfileInfoGUI profileInfo;

    public static ProfileInfo profileData = new ProfileInfo();
    public static int databaseID = -1;
    public static bool isGuestAccount = true;
    public static string defaultName = "";
    public static string defaultClan = "";

    public UILabel signinResult;
    public UIInput usernameInput;
    public UIInput passwordInput;
    public UIToggle rememberMeCheck;
    public GameObject signInLoading;
    public UIButton loginButton;
    public UIButton logoutButton;
    public GameObject profilePanel;
    public TabManager tabManager;
    public AudioClip loginSuccess;
    public AudioClip loginFailure;

    private bool logging = false;
    private bool ignoreSound = false;
    private bool ignoreWait = false;

    private string loginURL = "http://darkraze.byethost6.com/dir/darkraze_files/accounts/login_request.php";
    private string infoRequest = "http://darkraze.byethost6.com/dir/darkraze_files/accounts/info_request.php";

    void Awake()
    {
        targetXP = new int[75];
        for (int i = 0; i < targetXP.Length; i++)
        {
            targetXP[i] = Mathf.RoundToInt(((startingExp + (linearExpGrowth * i)) * Mathf.Pow(exponentialExpGrowth, i)) / expRoundingInterval) * expRoundingInterval;
        }
    }

    void Start()
    {
        if (debugTargetXp)
        {
            string stringAppend = "array(";
            for (int i = 0; i < targetXP.Length; i++)
            {
                stringAppend += "@" + (i + 1).ToString() + "@ => @" + targetXP[i] + "@";

                if (i < targetXP.Length - 1)
                {
                    stringAppend += ", ";
                }
                else
                {
                    stringAppend += ");";
                }
            }

            stringAppend = stringAppend.Replace('@', '"');
            Debug.Log(stringAppend);
        }

        signInLoading.SetActive(false);
        rememberMeCheck.value = (PlayerPrefs.GetInt("RememberMe", 0) == 1);

        if (string.IsNullOrEmpty(defaultName))
        {
            defaultName = "Guest" + UnityEngine.Random.Range(100, 99999);
        }

        tabManager.SelectTab(1);
        profileData.username = defaultName;
        profileData.clan = defaultClan;

        if (rememberMeCheck.value)
        {
            usernameInput.value = PlayerPrefs.GetString("Username", "");
            passwordInput.value = DarkRef.DecryptString(PlayerPrefs.GetString("Pwd", ""));
            ignoreSound = ignoreWait = true;
            StartLogin();
        }

        StartCoroutine(UpdateProfileInfo(-1));
    }

    void Update()
    {
        loginButton.isEnabled = (usernameInput.value.Length >= 4 && passwordInput.value.Length >= 3 && isGuestAccount && !logging);
        logoutButton.isEnabled = !isGuestAccount;
    }

    private IEnumerator UpdateProfileInfo(int dbid)
    {
        string[] data = new string[12] { defaultName, "", "1", "0", "0", "0", "0", "0", "0", "0", "2500", "0" };
        if (dbid > -1)
        {
            WWWForm newForm = new WWWForm();
            newForm.AddField("id", dbid);
            WWW request = new WWW(infoRequest, newForm);

            yield return request;

            if (string.IsNullOrEmpty(request.error))
            {
                data = request.text.Split(new string[] { "-" }, StringSplitOptions.None);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        if (data.Length < 12)
        {
            Logout();
            yield break;
        }

        profileData.username = data[0];
        profileData.clan = data[1];
        profileData.rank = Mathf.Clamp(int.Parse(data[2]), 1, 75);
        profileData.curXP = int.Parse(data[3]);
        float targXP = (float)AccountManager.GetTargetExperienceForRank(profileData.rank);

        float kills = float.Parse(data[4]);
        float deaths = float.Parse(data[5]);

        profileData.deaths = (int)deaths;
        profileData.kills = (int)kills;
        profileData.assists = int.Parse(data[6]);
        profileData.matchesWon = int.Parse(data[8]);
        profileData.matchesLost = int.Parse(data[9]);

        float xpPercent = (float)profileData.curXP / targXP;
        profileInfo.username.text = "Welcome, " + ((profileData.clan != "") ? DarkRef.ClanColor(false) + "[" + profileData.clan + "] [-][D99677]" : "[D99677]") + profileData.username + "[-]";
        profileInfo.profileStats.text = "GENERAL" + "\n" + "  [BC8F76]Rank:[-] " + profileData.rank.ToString() + " [A0A0A0][Rookie]"; //Placeholder for rank names.
        profileInfo.xpText.text = "[BC8F76]XP:[-] " + profileData.curXP.ToString() + ((dbid <= -1) ? "" : "/" + targXP.ToString() + " (" + (xpPercent * 100f).ToString("F1") + "%)");
        profileInfo.xpBar.value = (dbid <= -1) ? 1f : xpPercent;
        profileInfo.combatStats.text = "COMBAT" + "\n" + "  [BC8F76]Kills:[-] " + kills.ToString() + "\n" + "  [BC8F76]Deaths:[-] " + deaths.ToString() + "\n" + "  [BC8F76]Assists:[-] " + profileData.assists.ToString() + "\n" + "  [BC8F76]K/D Ratio:[-] " + (deaths > 0f ? (kills / deaths) : kills).ToString("F2") + "\n" + "  [BC8F76]Headshots:[-] " + int.Parse(data[7]);
        profileInfo.matchStats.text = "MATCHES" + "\n" + "  [BC8F76]Matches Played:[-] " + (profileData.matchesWon + profileData.matchesLost).ToString() + "\n" + "  [BC8F76]Matches Won:[-] " + profileData.matchesWon.ToString() + "\n" + "  [BC8F76]Matches Lost:[-] " + profileData.matchesLost.ToString();

        profileData.currency = int.Parse(data[10]);
        profileData.premCurrency = int.Parse(data[11]);
    }

    public void StartLogin()
    {
        if (logging)
        {
            return;
        }

        StartCoroutine(StartLoginRoutine());
    }

    public void Logout()
    {
        PlayerPrefs.SetString("Pwd", "");
        PlayerPrefs.SetInt("RememberMe", 0);
        databaseID = -1;
        signinResult.text = "";
        signinResult.enabled = false;
        profileData.username = defaultName;
        profileData.clan = defaultClan;
        isGuestAccount = true;
        StartCoroutine(UpdateProfileInfo(databaseID));

        passwordInput.value = "";
        rememberMeCheck.value = false;
        UICamera.selectedObject = null;

        tabManager.SelectTab(1);
    }

    private IEnumerator StartLoginRoutine()
    {
        logging = true;

        signInLoading.SetActive(true);
        signinResult.enabled = false;

        WWWForm loginForm = new WWWForm();
        loginForm.AddField("user", usernameInput.value);
        loginForm.AddField("pwd", CalculateSHA1(passwordInput.value, Encoding.Default));
        WWW loginWWW = new WWW(loginURL, loginForm);

        yield return loginWWW;

        databaseID = -1;
        if (string.IsNullOrEmpty(loginWWW.error) && loginWWW.text != "-1")
        {
            databaseID = int.Parse(loginWWW.text);
            isGuestAccount = false;

            if (rememberMeCheck.value)
            {
                PlayerPrefs.SetString("Username", usernameInput.value);
                PlayerPrefs.SetString("Pwd", DarkRef.EncryptString(passwordInput.value));
            }

            PlayerPrefs.SetInt("RememberMe", (rememberMeCheck.value) ? 1 : 0);

            signinResult.enabled = true;
            signinResult.text = "[89B01B]Login successful[-]";

            if (!ignoreSound)
            {
                NGUITools.PlaySound(loginSuccess);
            }

            signInLoading.SetActive(false);

            yield return StartCoroutine(UpdateProfileInfo(databaseID));

            if (!ignoreWait)
            {
                float wait = 0f;
                while (wait < 0.8f)
                {
                    wait += Time.deltaTime;
                    yield return null;
                }
            }

            if (tabManager.selectedTab != 0)
            {
                tabManager.SelectTab(0);
            }

            yield return new WaitForSeconds(0.2f);
            signinResult.enabled = false;
        }
        else
        {
            signinResult.enabled = true;

            if (!ignoreSound)
            {
                NGUITools.PlaySound(loginFailure, 0.3f);
            }

            signInLoading.SetActive(false);

            if (MultiplayerMenu.mServerIsOnline)
            {
                signinResult.text = "[FF2000]Invalid username or password[-]";
            }
            else
            {
                signinResult.text = "[FF2000]Authentication failed[-]";
            }
        }

        logging = false;
        ignoreWait = false;
        ignoreSound = false;
    }

    public static string CalculateSHA1(string text, Encoding enc)
    {
        SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
        return BitConverter.ToString(cryptoTransformSHA1.ComputeHash(enc.GetBytes(text))).Replace("-", "").ToLower();
    }

    public static int GetTargetExperienceForRank(int rank)
    {
        return targetXP[Mathf.Clamp(rank - 1, 0, targetXP.Length)];
    }
}