using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlsChange : MonoBehaviour
{
    public class ButtonClass
    {
        public string buttonName;
        public string primaryKey;
        public string secondaryKey;
    }

    public UILabel controlsMessage;
    public Transform parentPoint;
    public SetButtonGUI setButtonPrefab;
    public float horizontalSpacing = 2f;
    public float verticalSpacing = 0.6f;
    public int rows = 9;
    public AudioClip messagePopup;
    public UIPanel existingKeyPanel;
    public BlurEffect blurBackground;
    public UILabel existingKeyLabel;

    private int inputType;
    private float vertSpace = 0;
    private float horizSpace = 0;
    private SetButtonGUI waitingForInput;
    private string currentButtonName;
    private string oldButtonInfo;
    private string existButtonInfo;
    private int waitIndex;
    private bool waitForNextKeyPress = false;

    private List<ButtonClass> buttonList;
    private Dictionary<string, SetButtonGUI> controlButtons = new Dictionary<string, SetButtonGUI>();

    private void AddButtonForKey(string key, bool allowMouse = false)
    {
        if (vertSpace >= rows * verticalSpacing)
        {
            horizSpace += horizontalSpacing;
            vertSpace = 0f;
        }

        SetButtonGUI sbg = (SetButtonGUI)Instantiate(setButtonPrefab);
        sbg.buttonName = key;
        sbg.buttonLabel.text = key + ":";
        sbg.PrimaryLabel.text = FormatButtonText(cInput.GetText(key, 1));
        sbg.SecondaryLabel.text = FormatButtonText(cInput.GetText(key, 2));
        sbg.transform.parent = parentPoint;
        sbg.transform.localPosition = new Vector3(horizSpace, -vertSpace, 0f);
        sbg.transform.localScale = Vector3.one;

        sbg.PrimaryButton.sendMessage.gameObjectMessage.messageReceiver = gameObject;
        sbg.SecondaryButton.sendMessage.gameObjectMessage.messageReceiver = gameObject;

        if (allowMouse)
        {
            sbg.allowMouseAxis = true;
            sbg.allowJoystickAxis = true;
        }

        ButtonClass newButtonClass = new ButtonClass();
        newButtonClass.buttonName = key;
        buttonList.Add(newButtonClass);

        controlButtons.Add(sbg.buttonName, sbg);
        vertSpace += verticalSpacing;
    }

    public void ResetControls()
    {
        cInput.ResetInputs();
        RefreshAllButtons();
    }

    public void RefreshAllButtons()
    {
        foreach (KeyValuePair<string, SetButtonGUI> pair in controlButtons)
        {
            pair.Value.PrimaryLabel.text = FormatButtonText(cInput.GetText(pair.Key, 1));
            pair.Value.SecondaryLabel.text = FormatButtonText(cInput.GetText(pair.Key, 2));
        }

        for (int i = 0; i < buttonList.Count; i++)
        {
            buttonList[i].primaryKey = cInput.GetText(buttonList[i].buttonName, 1);
            buttonList[i].secondaryKey = cInput.GetText(buttonList[i].buttonName, 2);
        }
    }

    private string FormatButtonText(string buttonName)
    {
        string finalString = "";

        int upCount = 0;
        foreach (char c in buttonName)
        {
            bool addSpace = (char.IsUpper(c) || char.IsNumber(c));
            if (addSpace)
            {
                upCount++;
            }

            finalString += (upCount > 1 && addSpace) ? " " + c.ToString().ToUpper() : c.ToString();
        }

        return finalString;
    }

    void Awake()
    {
        buttonList = new List<ButtonClass>();

        AddButtonForKey("Forward");
        AddButtonForKey("Backward");
        AddButtonForKey("Strafe Left");
        AddButtonForKey("Strafe Right");

        vertSpace += 10f;

        AddButtonForKey("Run");
        AddButtonForKey("Jump");
        AddButtonForKey("Crouch");
        AddButtonForKey("Walk");

        vertSpace += 10f;

        AddButtonForKey("Fire Weapon");
        AddButtonForKey("Aim");

        vertSpace += 5f;

        AddButtonForKey("Inventory");
        AddButtonForKey("Drop Weapon");
        AddButtonForKey("Melee");
        AddButtonForKey("Flashlight");
        AddButtonForKey("Use");
        AddButtonForKey("Reload");
        AddButtonForKey("Fire Mode");

        vertSpace += 10f;

        AddButtonForKey("Leaderboard");
        AddButtonForKey("General Chat");
        AddButtonForKey("Team Chat");

        cInput.updateKeyDelegate = UpdatedKey;
        RefreshAllButtons();
    }

    public void UpdatedKey(string keyName, string prim, string sec)
    {
        int duplicateIndex = CheckDuplicateKey(prim, sec, keyName, waitIndex);
        if (duplicateIndex > -1)
        {
            return; //Wait for confirmation or denial.
        }

        waitingForInput = null;
        waitForNextKeyPress = false;

        RefreshAllButtons();
    }

    public void SetPrimary(GameObject button)
    {
        if (waitForNextKeyPress)
        {
            return;
        }

        waitingForInput = button.GetComponent<SetButtonGUI>();
        currentButtonName = waitingForInput.buttonName;
        oldButtonInfo = cInput.GetText(currentButtonName, 1) + "||" + cInput.GetText(currentButtonName, 2);
        waitingForInput.PrimaryLabel.text = "Press any key";
        cInput.ChangeKey(waitingForInput.buttonName, 1, waitingForInput.allowMouseAxis, true, waitingForInput.allowJoystickAxis, true);
        waitIndex = 1;
        waitForNextKeyPress = true;
    }

    public void SetSecondary(GameObject button)
    {
        if (waitForNextKeyPress)
        {
            return;
        }

        waitingForInput = button.GetComponent<SetButtonGUI>();
        currentButtonName = waitingForInput.buttonName;
        oldButtonInfo = cInput.GetText(currentButtonName, 1) + "||" + cInput.GetText(currentButtonName, 2);
        waitingForInput.SecondaryLabel.text = "Press any key";
        cInput.ChangeKey(waitingForInput.buttonName, 2, waitingForInput.allowMouseAxis, true, waitingForInput.allowJoystickAxis, true);
        waitIndex = 2;
        waitForNextKeyPress = true;
    }

    public void ConfirmKeyChange()
    {
        waitingForInput = null;
        waitForNextKeyPress = false;

        RefreshAllButtons();
    }

    public void RevertKeyChange()
    {
        string[] oldString = oldButtonInfo.Split(new string[] { "||" }, System.StringSplitOptions.None);
        string[] existString = existButtonInfo.Split(new string[] { "||" }, System.StringSplitOptions.None);
        cInput.ChangeKey(currentButtonName, oldString[0], oldString[1]);
        cInput.ChangeKey(existString[0], existString[1], existString[2]);

        waitingForInput = null;
        waitForNextKeyPress = false;

        RefreshAllButtons();
    }

    private int CheckDuplicateKey(string pKey, string sKey, string assignTo, int index)
    {
        string key = (index == 1) ? pKey : sKey;

        if (key == "None")
        {
            return -1;
        }

        for (int i = 0; i < buttonList.Count; i++)
        {
            bool matchPrimary = (buttonList[i].primaryKey == key);
            bool matchSecondary = (buttonList[i].secondaryKey == key);
            if (buttonList[i].buttonName != currentButtonName && (matchPrimary || matchSecondary))
            {
                existButtonInfo = buttonList[i].buttonName + "||" + buttonList[i].primaryKey + "||" + buttonList[i].secondaryKey;
                existingKeyLabel.text = "The key '" + FormatButtonText(key) + "' already exists for '" + buttonList[i].buttonName + "'. Are you sure that you want to assign it to '" + assignTo + "'?";
                NGUITools.PlaySound(messagePopup);
                StartCoroutine(FadeMessage());
                return i;
            }
        }

        return -1;
    }

    private IEnumerator FadeMessage()
    {
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha = Mathf.MoveTowards(alpha, 1f, Time.deltaTime * 7.5f);
            existingKeyPanel.alpha = alpha;
            blurBackground.blurSpread = alpha * 0.6f;
            yield return null;
        }
    }
}