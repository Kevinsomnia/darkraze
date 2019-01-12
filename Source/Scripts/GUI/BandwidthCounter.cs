using UnityEngine;
using System.Collections;

public class BandwidthCounter : MonoBehaviour {
	private UILabel label;
	private bool mShowBandwidth;
	
	void Awake() {
		GeneralVariables.showBandwidth = (PlayerPrefs.GetInt("Bandwidth", 0) == 1) ? true : false;
	}

    void OnGUI() {
        if(!GeneralVariables.showBandwidth || !Topan.Network.isConnected) {
            return;
        }

        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUI.skin.label.fontSize = 10;
        GUILayout.Label(" Bandwidth: " + (Topan.Network.bytesInPerSecond + Topan.Network.bytesOutPerSecond).ToString() + " bytes/sec");
    }
	
	void Update() {
		if(mShowBandwidth != GeneralVariables.showBandwidth) {
			mShowBandwidth = GeneralVariables.showBandwidth;
			PlayerPrefs.SetInt("Bandwidth", (mShowBandwidth) ? 1 : 0);
		}
	}
}