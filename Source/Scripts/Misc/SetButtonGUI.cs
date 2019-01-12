using UnityEngine;
using System.Collections;

public class SetButtonGUI : MonoBehaviour {
	public string buttonName = "Button";
	public UILabel buttonLabel;
	public ButtonAction PrimaryButton;
	public UILabel PrimaryLabel;
	public ButtonAction SecondaryButton;
	public UILabel SecondaryLabel;
	
	[HideInInspector] public bool allowMouseAxis = false;
	[HideInInspector] public bool allowJoystickAxis = false;
}