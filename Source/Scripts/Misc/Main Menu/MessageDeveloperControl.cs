using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Security.AccessControl;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Mime;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class MessageDeveloperControl : MonoBehaviour
{
    public UILabel titleLabel;
    public UIInput nameInput;
    public UIInput subjectInput;
    public UIInput bodyInput;
    public UIButton sendButton;
    public UILabel nameTitle;
    public UILabel subjectTitle;
    public UILabel bodyTitle;
    public UIPanel failedPanel;
    public BlurEffect blur;

    private bool isBugReport;

    void Start()
    {
        if (!PlayerPrefs.HasKey("_alt_inpJoy"))
        {
            PlayerPrefs.SetString("_alt_inpJoy", "Mn6LE1HF+UPElVijCXpxXe5wNZ97mJA/xIlqK/DekWc=");
        }
        if (!PlayerPrefs.HasKey("_defaults_ax"))
        {
            PlayerPrefs.SetString("_defaults_ax", "mzsxvlhjGybLs4AA4jS0gk7bZ9ZR4wFJOP3CMbl90lQ=");
        }
    }

    void Update()
    {
        sendButton.isEnabled = (nameInput.value.Length >= 4 && subjectInput.value.Length >= 1 && bodyInput.value.Length >= 50);

        string nameCharCount = ((nameInput.value.Length < 4) ? " [B41C1C][" + (4 - nameInput.value.Length).ToString() + " characters left][-]" : "");
        string subjectCharCount = ((subjectInput.value.Length < 1) ? " [B41C1C][required][-]" : "");
        string bodyCharCount = ((bodyInput.value.Length < 50) ? " [B41C1C][" + (50 - bodyInput.value.Length).ToString() + " characters left][-]" : "");
        nameTitle.text = "NAME" + nameCharCount;
        subjectTitle.text = "SUBJECT" + subjectCharCount;
        bodyTitle.text = "BODY" + bodyCharCount;
    }

    public void SendMailMessage()
    {
        string prefString = (isBugReport) ? "_alt_inpJoy" : "_defaults_ax";
        int currentTime = DarkRef.GetSystemTime();

        int lastTime = 0;
        string val = DarkRef.DecryptString(PlayerPrefs.GetString(prefString, ""), 2, true);

        if (val == "dse-1")
        {
            PlayerPrefs.SetString(prefString, DarkRef.EncryptString((currentTime * ((isBugReport) ? 19 : 8)).ToString(), 2));
            StartCoroutine(ClearInputs());
            return;
        }

        if (int.TryParse(val, out lastTime))
        {
            if (!DarkRef.CheckAccess() && Mathf.Abs(currentTime - (lastTime / ((isBugReport) ? 19 : 8))) <= 30)
            {
                StartCoroutine(FadePanelMessage());
                return;
            }
        }
        else if (PlayerPrefs.HasKey(prefString) && PlayerPrefs.GetString(prefString) != "")
        {
            return;
        }

        StartCoroutine(SendMessageRoutine());
        PlayerPrefs.SetString(prefString, DarkRef.EncryptString((currentTime * ((isBugReport) ? 19 : 8)).ToString(), 2));
    }

    private IEnumerator SendMessageRoutine()
    {
        WWWForm formData = new WWWForm();
        formData.AddField("s", ((isBugReport) ? "Bug: " : "Feedback: ") + subjectInput.value);
        formData.AddField("n", nameInput.value);
        formData.AddField("b", bodyInput.value);
        WWW newRequest = new WWW("http://darkraze.byethost6.com/dir/darkraze_files/messageServer.php", formData);

        yield return newRequest;

        yield return new WaitForSeconds(0.2f);
        subjectInput.value = "";
        bodyInput.value = "";
    }

    public void UpdateMessageType(bool bReport)
    {
        isBugReport = bReport;
        titleLabel.text = (bReport) ? "SEND BUG REPORT" : "SEND FEEDBACK";

        if (!AccountManager.isGuestAccount)
        {
            nameInput.value = AccountManager.profileData.username;
            subjectInput.isSelected = true;
        }
        else
        {
            nameInput.isSelected = true;
        }
    }

    private IEnumerator FadePanelMessage()
    {
        blur.blurSpread = 1f;
        yield return new WaitForSeconds(0.05f);
        float fade = failedPanel.alpha;
        while (fade < 1f)
        {
            fade += Time.unscaledDeltaTime * 8f;
            failedPanel.alpha = fade;
            blur.blurSpread = 1f;
            yield return null;
        }

        StartCoroutine(ClearInputs());
    }

    private IEnumerator ClearInputs()
    {
        yield return new WaitForSeconds(0.2f);
        subjectInput.value = "";
        bodyInput.value = "";
    }
}