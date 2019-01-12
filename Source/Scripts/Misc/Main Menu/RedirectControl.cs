using UnityEngine;
using System.Collections;

public class RedirectControl : MonoBehaviour {
    public UILabel message;
    public ButtonAction confirmButton;

    public void UpdateRedirection(string newWebsite) {
        confirmButton.loadWebsite.url = newWebsite;
    }

    public void MessageType(int type) {
        if(type == 0) {
            message.text = "Registering an account will redirect you to the official website. Continue?";
        }
    }
}