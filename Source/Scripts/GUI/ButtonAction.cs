using UnityEngine;
using System.Collections;

public class ButtonAction : MonoBehaviour
{
    public enum ReceiverEnum { None, Player }
    public enum FadeDirection { FadeIn, FadeOut }

    #region Classes
    [System.Serializable]
    public class MoveObject
    {
        public bool enabled = false;
        public bool onHover = false;
        public GameObject moveGO;
        public Vector3 targetPos;
    }

    [System.Serializable]
    public class ActivateObject
    {
        public bool enabled = false;
        public GameObject activateObject;
    }

    [System.Serializable]
    public class DeactivateObject
    {
        public bool enabled = false;
        public GameObject deactivateObject;
    }

    [System.Serializable]
    public class FadeGUI
    {
        public bool enabled = false;
        public FadeDirection fadeDirection = FadeDirection.FadeIn;
        public UIPanel fadePanel;
        public UIWidget fadeWidget;
        public float fadeSpeed = 1f;
        public BlurEffect blurEffect;
        public float blurFactor = 1f;
    }

    [System.Serializable]
    public class LoadLevel
    {
        public bool enabled = false;
        public string levelName = "level";
        public bool MpDisconnect = false;
    }

    [System.Serializable]
    public class SendMessage
    {
        public GenericMessage genericMessage = new GenericMessage();
        public GameObjectMessage gameObjectMessage = new GameObjectMessage();
        public BooleanMessage booleanMessage = new BooleanMessage();
        public StringMessage stringMessage = new StringMessage();
        public NumericalMessage numericalMessage = new NumericalMessage();
    }

    [System.Serializable]
    public class EasterEgg
    {
        public UILabel label;
        public int currentIndex = 0;
        public string[] cycleString;
    }

    #region Message Types
    [System.Serializable]
    public class GenericMessage
    {
        public ReceiverEnum receiverEnum = ReceiverEnum.None;
        public bool enabled = false;
        public bool broadcastMessage = false;
        public string messageName = "Message";
        public GameObject messageReceiver;
    }


    [System.Serializable]
    public class GameObjectMessage
    {
        public ReceiverEnum receiverEnum = ReceiverEnum.None;
        public bool enabled = false;
        public bool broadcastMessage = false;
        public string messageName = "Message";
        public GameObject gameobjectValue = null;
        public GameObject messageReceiver;
    }

    [System.Serializable]
    public class BooleanMessage
    {
        public ReceiverEnum receiverEnum = ReceiverEnum.None;
        public bool enabled = false;
        public bool broadcastMessage = false;
        public string messageName = "Message";
        public bool booleanValue = false;
        public GameObject messageReceiver;
    }

    [System.Serializable]
    public class StringMessage
    {
        public ReceiverEnum receiverEnum = ReceiverEnum.None;
        public bool enabled = false;
        public bool broadcastMessage = false;
        public string messageName = "Message";
        public string stringValue = "";
        public GameObject messageReceiver;
    }

    [System.Serializable]
    public class NumericalMessage
    {
        public ReceiverEnum receiverEnum = ReceiverEnum.None;
        public bool enabled = false;
        public bool broadcastMessage = false;
        public bool isInt = true; //Is it a integer? If not, then it's a float.
        public string messageName = "Message";
        public float valueToSend = 1;
        public GameObject messageReceiver;
    }
    #endregion

    [System.Serializable]
    public class QuitApplication
    {
        public bool enabled = false;
    }

    [System.Serializable]
    public class LoadWebsite
    {
        public bool enabled = false;
        public string url = "www.google.com";
    }

    [System.Serializable]
    public class GuestNotification
    {
        public bool enabled = false;
        public ButtonAction okayButton;
        public UIToggle dontShowAgain;
    }
    #endregion

    public MoveObject moveObject = new MoveObject();
    public ActivateObject activateObject = new ActivateObject();
    public DeactivateObject deactivateObject = new DeactivateObject();
    public FadeGUI fadeGUI = new FadeGUI();
    public LoadLevel loadLevel = new LoadLevel();
    public LoadWebsite loadWebsite = new LoadWebsite();
    public GuestNotification guestNotification = new GuestNotification();
    public SendMessage sendMessage = new SendMessage();
    public QuitApplication quitApplication = new QuitApplication();
    public EasterEgg easterEgg = new EasterEgg();

    private bool skipMessage; //For the guest notification;

    void Start()
    {
        if (guestNotification.enabled && guestNotification.dontShowAgain != null)
        {
            guestNotification.dontShowAgain.value = (PlayerPrefs.GetInt("SkipGuestNotification", 0) == 1) ? true : false;
        }
    }

    void OnHover(bool hover)
    {
        if (hover && moveObject.enabled && moveObject.onHover)
        {
            moveObject.moveGO.transform.SendMessage("TargetPos", moveObject.targetPos, SendMessageOptions.RequireReceiver);
        }
    }

    void Update()
    {
        if (guestNotification.enabled && guestNotification.dontShowAgain != null)
        {
            skipMessage = guestNotification.dontShowAgain.value;
        }
    }

    public void OnClick()
    {
        if (guestNotification.enabled && !skipMessage && AccountManager.isGuestAccount)
        {
            if (fadeGUI.enabled && (fadeGUI.fadePanel || fadeGUI.fadeWidget))
            {
                StopCoroutine(StartFade());
                StartCoroutine(StartFade());
            }

            guestNotification.okayButton.sendMessage.genericMessage.messageReceiver = this.gameObject;
            return;
        }

        DoAction();
    }

    public void DoAction()
    {
        if (quitApplication.enabled)
        {
            Application.Quit();
            return;
        }

        if (loadLevel.enabled)
        {
            if (loadLevel.MpDisconnect && Topan.Network.isConnected)
            {
                Topan.Network.Disconnect();
            }
            Loader.LoadLevel(loadLevel.levelName);
        }

        if (moveObject.enabled && !moveObject.onHover)
        {
            moveObject.moveGO.transform.SendMessage("TargetPos", moveObject.targetPos, SendMessageOptions.RequireReceiver);
        }

        if (activateObject.enabled)
        {
            activateObject.activateObject.SetActive(true);
        }

        if (deactivateObject.enabled)
        {
            deactivateObject.deactivateObject.SetActive(false);
        }

        if (loadWebsite.enabled)
        {
            Application.OpenURL(loadWebsite.url);
        }

        if (guestNotification.enabled && guestNotification.dontShowAgain != null)
        {
            PlayerPrefs.SetInt("SkipGuestNotification", (guestNotification.dontShowAgain.value) ? 1 : 0);
        }

        if (fadeGUI.enabled && (fadeGUI.fadePanel || fadeGUI.fadeWidget) && !(guestNotification.enabled && ((guestNotification.enabled && skipMessage) || !AccountManager.isGuestAccount)))
        {
            StartCoroutine(StartFade());
        }

        GameObject player = GeneralVariables.player;
        if (sendMessage.genericMessage.enabled)
        {
            GameObject receiver = sendMessage.genericMessage.messageReceiver;

            if (sendMessage.genericMessage.receiverEnum == ReceiverEnum.Player)
            {
                receiver = player;
                if (player == null)
                {
                    return;
                }
            }

            if (sendMessage.genericMessage.broadcastMessage)
            {
                receiver.BroadcastMessage(sendMessage.genericMessage.messageName, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                receiver.SendMessage(sendMessage.genericMessage.messageName, SendMessageOptions.DontRequireReceiver);
            }
        }

        if (sendMessage.gameObjectMessage.enabled)
        {
            GameObject receiver = sendMessage.gameObjectMessage.messageReceiver;

            if (sendMessage.gameObjectMessage.receiverEnum == ReceiverEnum.Player)
            {
                receiver = player;
                if (player == null)
                {
                    return;
                }
            }

            if (sendMessage.gameObjectMessage.broadcastMessage)
            {
                receiver.BroadcastMessage(sendMessage.gameObjectMessage.messageName, sendMessage.gameObjectMessage.gameobjectValue, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                receiver.SendMessage(sendMessage.gameObjectMessage.messageName, sendMessage.gameObjectMessage.gameobjectValue, SendMessageOptions.RequireReceiver);
            }
        }

        if (sendMessage.booleanMessage.enabled)
        {
            GameObject receiver = sendMessage.booleanMessage.messageReceiver;

            if (sendMessage.booleanMessage.receiverEnum == ReceiverEnum.Player)
            {
                receiver = player;
                if (player == null)
                {
                    return;
                }
            }

            if (sendMessage.booleanMessage.broadcastMessage)
            {
                receiver.BroadcastMessage(sendMessage.booleanMessage.messageName, sendMessage.booleanMessage.booleanValue, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                receiver.SendMessage(sendMessage.booleanMessage.messageName, sendMessage.booleanMessage.booleanValue, SendMessageOptions.RequireReceiver);
            }
        }

        if (sendMessage.stringMessage.enabled)
        {
            GameObject receiver = sendMessage.stringMessage.messageReceiver;

            if (sendMessage.stringMessage.receiverEnum == ReceiverEnum.Player)
            {
                receiver = player;
                if (player == null)
                {
                    return;
                }
            }

            if (sendMessage.stringMessage.broadcastMessage)
            {
                receiver.BroadcastMessage(sendMessage.stringMessage.messageName, sendMessage.stringMessage.stringValue, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                receiver.SendMessage(sendMessage.stringMessage.messageName, sendMessage.stringMessage.stringValue, SendMessageOptions.RequireReceiver);
            }
        }

        if (sendMessage.numericalMessage.enabled)
        {
            GameObject receiver = sendMessage.numericalMessage.messageReceiver;

            if (sendMessage.numericalMessage.receiverEnum == ReceiverEnum.Player)
            {
                receiver = player;
                if (player == null)
                {
                    return;
                }
            }

            if (sendMessage.numericalMessage.isInt)
            {
                if (sendMessage.numericalMessage.broadcastMessage)
                {
                    receiver.BroadcastMessage(sendMessage.numericalMessage.messageName, (int)sendMessage.numericalMessage.valueToSend, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    receiver.SendMessage(sendMessage.numericalMessage.messageName, (int)sendMessage.numericalMessage.valueToSend, SendMessageOptions.RequireReceiver);
                }
            }
            else
            {
                if (sendMessage.numericalMessage.broadcastMessage)
                {
                    receiver.BroadcastMessage(sendMessage.numericalMessage.messageName, sendMessage.numericalMessage.valueToSend, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    receiver.SendMessage(sendMessage.numericalMessage.messageName, sendMessage.numericalMessage.valueToSend, SendMessageOptions.RequireReceiver);
                }
            }
        }

        if (easterEgg.cycleString.Length > 0)
        {
            easterEgg.currentIndex++;
            if (easterEgg.currentIndex >= easterEgg.cycleString.Length)
            {
                easterEgg.currentIndex = 0;
            }

            easterEgg.label.text = easterEgg.cycleString[easterEgg.currentIndex];
        }
    }

    private IEnumerator StartFade()
    {
        float time = fadeGUI.fadePanel.alpha;
        if (fadeGUI.fadeDirection == FadeDirection.FadeIn)
        {
            while (time < 1f)
            {
                time += Time.unscaledDeltaTime * fadeGUI.fadeSpeed;
                if (fadeGUI.fadePanel)
                {
                    fadeGUI.fadePanel.alpha = Mathf.Clamp01(time);
                }
                if (fadeGUI.fadeWidget)
                {
                    fadeGUI.fadePanel.alpha = Mathf.Clamp01(time);
                }
                if (fadeGUI.blurEffect)
                {
                    fadeGUI.blurEffect.blurSpread = Mathf.Clamp01(time) * fadeGUI.blurFactor;
                }
                yield return null;
            }
        }
        else if (fadeGUI.fadeDirection == FadeDirection.FadeOut)
        {
            while (time > 0f)
            {
                time -= Time.unscaledDeltaTime * fadeGUI.fadeSpeed;
                if (fadeGUI.fadePanel)
                {
                    fadeGUI.fadePanel.alpha = Mathf.Clamp01(time);
                }
                if (fadeGUI.fadeWidget)
                {
                    fadeGUI.fadeWidget.alpha = Mathf.Clamp01(time);
                }
                if (fadeGUI.blurEffect)
                {
                    fadeGUI.blurEffect.blurSpread = Mathf.Clamp01(time) * fadeGUI.blurFactor;
                }
                yield return null;
            }

            if (fadeGUI.blurEffect)
            {
                fadeGUI.blurEffect.blurSpread = 0f;
            }
        }
    }
}