using UnityEngine;
using System.Collections;

public class PauseConfirmGUI : MonoBehaviour {
    public UILabel title;
    public UILabel body;
    public ButtonAction confirmButton;

	public void MessageType(int type) {
        if(type == 0) {
            title.text = "EXIT TO MAIN MENU";
            body.text = "Are you sure that you want to quit to the main menu?";
            confirmButton.loadLevel.enabled = true;
            confirmButton.quitApplication.enabled = false;
        }
        else if(type == 1) {
            title.text = "CONFIRM EXIT";
            body.text = "Are you sure that you want to exit to the desktop?";
            confirmButton.loadLevel.enabled = false;
            confirmButton.quitApplication.enabled = true;
        }
    }
}