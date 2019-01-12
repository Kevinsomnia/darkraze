using UnityEngine;
using System.Collections;

public class MapGUI : MonoBehaviour {
	public UILabel nameLabel;
	public UITexture mapScreenshot;
	public string description = "Map description";
	public ShowTooltip showTooltip;

	void Start() {
		if(showTooltip) {
			showTooltip.text = description;
		}
	}
}