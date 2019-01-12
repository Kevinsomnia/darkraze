using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ButtonAction))]
public class ButtonActionInspector : Editor {
	//Sections
	private static bool isOpen1;
	private static bool isOpen2;
	private static bool isOpen3;
	private static bool isOpen4;
	private static bool isOpen5;
	private static bool isOpen6;
	private static bool isOpen7;
	private static bool isOpen8;
	private static bool isOpen9;
    private static bool isOpen10;

	//Message sub-sections.
	private static bool open1;
	private static bool open2;
	private static bool open3;
	private static bool open4;
	private static bool open5;
	
	public override void OnInspectorGUI() {
		ButtonAction ba = target as ButtonAction;
				
		GUILayout.Space(5);
		
		string openText1 = (ba.moveObject.enabled) ? "[ENABLED]" : "[DISABLED]";
		isOpen1 = EditorGUILayout.Foldout(isOpen1, " Move Object          " + openText1);
		if(isOpen1) {
			EditorGUI.indentLevel += 1;
			ba.moveObject.enabled = EditorGUILayout.Toggle("Enabled:", ba.moveObject.enabled);
			if(ba.moveObject.enabled) {
				EditorGUI.indentLevel += 1;
				ba.moveObject.onHover = EditorGUILayout.Toggle("On Hover", ba.moveObject.onHover);
				ba.moveObject.moveGO = (GameObject)EditorGUILayout.ObjectField("Target Object:", ba.moveObject.moveGO, typeof(GameObject), true);
				ba.moveObject.targetPos = EditorGUILayout.Vector3Field("Target Position:", ba.moveObject.targetPos);

				if(ba.moveObject.moveGO == null || (ba.moveObject.moveGO != null && ba.moveObject.moveGO.GetComponent<CameraMove>() == null)) {
					GUILayout.Box("The target must have a CameraMove component attached to it!");
				}

				EditorGUI.indentLevel -= 1;
			}
			EditorGUI.indentLevel -= 1;
		}
		
		string openText2 = (ba.activateObject.enabled) ? "[ENABLED]" : "[DISABLED]";
		isOpen2 = EditorGUILayout.Foldout(isOpen2, " Activate Object      " + openText2);
		if(isOpen2) {
			EditorGUI.indentLevel += 1;
			ba.activateObject.enabled = EditorGUILayout.Toggle("Enabled:", ba.activateObject.enabled);
			if(ba.activateObject.enabled) {
				EditorGUI.indentLevel += 1;
				EditorGUIUtility.labelWidth = 120f;
				ba.activateObject.activateObject = (GameObject)EditorGUILayout.ObjectField("Target Object:", ba.activateObject.activateObject, typeof(GameObject), true);
				EditorGUIUtility.LookLikeControls();
				EditorGUI.indentLevel -= 1;
			}
			EditorGUI.indentLevel -= 1;
		}
		
		string openText3 = (ba.deactivateObject.enabled) ? "[ENABLED]" : "[DISABLED]";
		isOpen3 = EditorGUILayout.Foldout(isOpen3, " Deactivate Object  " + openText3);
		if(isOpen3) {
			EditorGUI.indentLevel += 1;
			ba.deactivateObject.enabled = EditorGUILayout.Toggle("Enabled:", ba.deactivateObject.enabled);
			if(ba.deactivateObject.enabled) {
				EditorGUI.indentLevel += 1;
                EditorGUIUtility.labelWidth = 120f;
				ba.deactivateObject.deactivateObject = (GameObject)EditorGUILayout.ObjectField("Target Object:", ba.deactivateObject.deactivateObject, typeof(GameObject), true);
				EditorGUIUtility.LookLikeControls();
				EditorGUI.indentLevel -= 1;
			}
			EditorGUI.indentLevel -= 1;
		}
		
		string openText4 = (ba.fadeGUI.enabled) ? "[ENABLED]" : "[DISABLED]";
		isOpen4 = EditorGUILayout.Foldout(isOpen4, " Fade GUI Element  " + openText4);
		if(isOpen4) {
			EditorGUI.indentLevel += 1;
			ba.fadeGUI.enabled = EditorGUILayout.Toggle("Enabled:", ba.fadeGUI.enabled);
			if(ba.fadeGUI.enabled) {
				EditorGUI.indentLevel += 1;
				ba.fadeGUI.fadeDirection = (ButtonAction.FadeDirection)EditorGUILayout.EnumPopup("Fade Direction:", ba.fadeGUI.fadeDirection);
				ba.fadeGUI.fadePanel = (UIPanel)EditorGUILayout.ObjectField("Fade Panel:", ba.fadeGUI.fadePanel, typeof(UIPanel), true);
				ba.fadeGUI.fadeWidget = (UIWidget)EditorGUILayout.ObjectField("Fade Widget:", ba.fadeGUI.fadeWidget, typeof(UIWidget), true);
				ba.fadeGUI.fadeSpeed = EditorGUILayout.FloatField("Fade Speed:", Mathf.Clamp(ba.fadeGUI.fadeSpeed, 0.05f, 20f));
				GUILayout.Space(5f);
				ba.fadeGUI.blurEffect = (BlurEffect)EditorGUILayout.ObjectField("Blur Effect:", ba.fadeGUI.blurEffect, typeof(BlurEffect), true);
				ba.fadeGUI.blurFactor = EditorGUILayout.FloatField("Blur Factor:", Mathf.Clamp(ba.fadeGUI.blurFactor, 0.01f, 5f));
				EditorGUI.indentLevel -= 1;
			}
			EditorGUI.indentLevel -= 1;
		}
		
		string openText5 = (ba.loadLevel.enabled) ? "[ENABLED]" : "[DISABLED]";
		isOpen5 = EditorGUILayout.Foldout(isOpen5, " Load Level             " + openText5);
		if(isOpen5) {
			EditorGUI.indentLevel += 1;
			ba.loadLevel.enabled = EditorGUILayout.Toggle("Enabled:", ba.loadLevel.enabled);
			if(ba.loadLevel.enabled) {
				EditorGUI.indentLevel += 1;
				ba.loadLevel.levelName = EditorGUILayout.TextField("Level Name:", ba.loadLevel.levelName);
				ba.loadLevel.MpDisconnect = EditorGUILayout.Toggle("Disconnect from MP", ba.loadLevel.MpDisconnect);
				EditorGUI.indentLevel -= 1;
			}
			EditorGUI.indentLevel -= 1;
		}
		
		string openText6 = (ba.loadWebsite.enabled) ? "[ENABLED]" : "[DISABLED]";
		isOpen6 = EditorGUILayout.Foldout(isOpen6, " Load Website         " + openText6);
		if(isOpen6) {
			EditorGUI.indentLevel += 1;
			ba.loadWebsite.enabled = EditorGUILayout.Toggle("Enabled:", ba.loadWebsite.enabled);
			if(ba.loadWebsite.enabled) {
				EditorGUI.indentLevel += 1;
                EditorGUIUtility.labelWidth = 100f;
				ba.loadWebsite.url = EditorGUILayout.TextField("URL Link:", ba.loadWebsite.url);
				EditorGUIUtility.LookLikeControls();
				EditorGUI.indentLevel -= 1;
			}
			EditorGUI.indentLevel -= 1;
		}

		string openText9 = (ba.guestNotification.enabled) ? "[ENABLED]" : "[DISABLED]";
		isOpen9 = EditorGUILayout.Foldout(isOpen9, " Guest Notification   " + openText9);
		if(isOpen9) {
			EditorGUI.indentLevel += 1;
			ba.guestNotification.enabled = EditorGUILayout.Toggle("Enabled:", ba.guestNotification.enabled);
			if(ba.guestNotification.enabled) {
				EditorGUI.indentLevel += 1;
				EditorGUIUtility.labelWidth = 100f;
				ba.guestNotification.okayButton = (ButtonAction)EditorGUILayout.ObjectField("Okay Button:", ba.guestNotification.okayButton, typeof(ButtonAction), true);
                ba.guestNotification.dontShowAgain = (UIToggle)EditorGUILayout.ObjectField("Don't Show:", ba.guestNotification.dontShowAgain, typeof(UIToggle), true);
				EditorGUIUtility.LookLikeControls();
				EditorGUI.indentLevel -= 1;
			}
			EditorGUI.indentLevel -= 1;
		}
		int amount = 0;
		if(ba.sendMessage.genericMessage != null) {
		amount = BoolToInt(ba.sendMessage.genericMessage.enabled) +
				 BoolToInt(ba.sendMessage.gameObjectMessage.enabled) +
				 BoolToInt(ba.sendMessage.booleanMessage.enabled) +
				 BoolToInt(ba.sendMessage.stringMessage.enabled) +
				 BoolToInt(ba.sendMessage.numericalMessage.enabled);
		}

		isOpen7 = EditorGUILayout.Foldout(isOpen7, " Send Message Actions (" + amount.ToString() + ")");
		if(isOpen7) {
			EditorGUI.indentLevel += 1;
			
			string text1 = (ba.sendMessage.genericMessage.enabled) ? "[ENABLED]" : "[DISABLED]";
			open1 = EditorGUILayout.Foldout(open1, " Generic Message  " + text1);
			if(open1) {
				EditorGUI.indentLevel += 1;
				ba.sendMessage.genericMessage.enabled = EditorGUILayout.Toggle("Enabled:", ba.sendMessage.genericMessage.enabled);
				if(ba.sendMessage.genericMessage.enabled) {
					GUI.color = new Color(0.85f, 0.85f, 0.85f);
					ba.sendMessage.genericMessage.receiverEnum = (ButtonAction.ReceiverEnum)EditorGUILayout.EnumPopup("Receiver Type:", ba.sendMessage.genericMessage.receiverEnum);
					ba.sendMessage.genericMessage.broadcastMessage = EditorGUILayout.Toggle("Broadcast Message:", ba.sendMessage.genericMessage.broadcastMessage);
					if(ba.sendMessage.genericMessage.receiverEnum == ButtonAction.ReceiverEnum.None) {
						ba.sendMessage.genericMessage.messageReceiver = (GameObject)EditorGUILayout.ObjectField("Message Receiver:", ba.sendMessage.genericMessage.messageReceiver, typeof(GameObject), true);
					}
					ba.sendMessage.genericMessage.messageName = EditorGUILayout.TextField("Function Name:", ba.sendMessage.genericMessage.messageName);
					GUI.color = Color.white;
				}
				EditorGUI.indentLevel -= 1;
			}
			
			string text2 = (ba.sendMessage.gameObjectMessage.enabled) ? "[ENABLED]" : "[DISABLED]";
			open2 = EditorGUILayout.Foldout(open2, " GameObject Message  " + text2);
			if(open2) {
				EditorGUI.indentLevel += 1;
				ba.sendMessage.gameObjectMessage.enabled = EditorGUILayout.Toggle("Enabled:", ba.sendMessage.gameObjectMessage.enabled);
				if(ba.sendMessage.gameObjectMessage.enabled) {
					GUI.color = new Color(0.85f, 0.85f, 0.85f);
					ba.sendMessage.genericMessage.receiverEnum = (ButtonAction.ReceiverEnum)EditorGUILayout.EnumPopup("Receiver Type:", ba.sendMessage.genericMessage.receiverEnum);
					ba.sendMessage.gameObjectMessage.broadcastMessage = EditorGUILayout.Toggle("Broadcast Message:", ba.sendMessage.gameObjectMessage.broadcastMessage);
					if(ba.sendMessage.gameObjectMessage.receiverEnum == ButtonAction.ReceiverEnum.None) {
						ba.sendMessage.gameObjectMessage.messageReceiver = (GameObject)EditorGUILayout.ObjectField("Message Receiver:", ba.sendMessage.gameObjectMessage.messageReceiver, typeof(GameObject), true);
					}
					ba.sendMessage.gameObjectMessage.messageName = EditorGUILayout.TextField("Function Name:", ba.sendMessage.gameObjectMessage.messageName);
					ba.sendMessage.gameObjectMessage.gameobjectValue = (GameObject)EditorGUILayout.ObjectField("Function Parameter:", ba.sendMessage.gameObjectMessage.gameobjectValue, typeof(GameObject), true);
					GUI.color = Color.white;
				}
				EditorGUI.indentLevel -= 1;
			}
			
			string text3 = (ba.sendMessage.booleanMessage.enabled) ? "[ENABLED]" : "[DISABLED]";
			open3 = EditorGUILayout.Foldout(open3, " Boolean Message  " + text3);
			if(open3) {
				EditorGUI.indentLevel += 1;
				ba.sendMessage.booleanMessage.enabled = EditorGUILayout.Toggle("Enabled:", ba.sendMessage.booleanMessage.enabled);
				if(ba.sendMessage.booleanMessage.enabled) {
					GUI.color = new Color(0.85f, 0.85f, 0.85f);
					ba.sendMessage.booleanMessage.receiverEnum = (ButtonAction.ReceiverEnum)EditorGUILayout.EnumPopup("Receiver Type:", ba.sendMessage.booleanMessage.receiverEnum);
					ba.sendMessage.booleanMessage.broadcastMessage = EditorGUILayout.Toggle("Broadcast Message:", ba.sendMessage.booleanMessage.broadcastMessage);
					if(ba.sendMessage.booleanMessage.receiverEnum == ButtonAction.ReceiverEnum.None) {
						ba.sendMessage.booleanMessage.messageReceiver = (GameObject)EditorGUILayout.ObjectField("Message Receiver:", ba.sendMessage.booleanMessage.messageReceiver, typeof(GameObject), true);
					}
					ba.sendMessage.booleanMessage.messageName = EditorGUILayout.TextField("Function Name:", ba.sendMessage.booleanMessage.messageName);
					ba.sendMessage.booleanMessage.booleanValue = EditorGUILayout.Toggle("Function Parameter:", ba.sendMessage.booleanMessage.booleanValue);
					GUI.color = Color.white;
				}
				EditorGUI.indentLevel -= 1;
			}
			
			string text4 = (ba.sendMessage.stringMessage.enabled) ? "[ENABLED]" : "[DISABLED]";
			open4 = EditorGUILayout.Foldout(open4, " String Message  " + text4);
			if(open4) {
				EditorGUI.indentLevel += 1;
				ba.sendMessage.stringMessage.enabled = EditorGUILayout.Toggle("Enabled:", ba.sendMessage.stringMessage.enabled);
				if(ba.sendMessage.stringMessage.enabled) {
					GUI.color = new Color(0.85f, 0.85f, 0.85f);
					ba.sendMessage.stringMessage.receiverEnum = (ButtonAction.ReceiverEnum)EditorGUILayout.EnumPopup("Receiver Type:", ba.sendMessage.stringMessage.receiverEnum);
					ba.sendMessage.stringMessage.broadcastMessage = EditorGUILayout.Toggle("Broadcast Message:", ba.sendMessage.stringMessage.broadcastMessage);
					if(ba.sendMessage.stringMessage.receiverEnum == ButtonAction.ReceiverEnum.None) {
						ba.sendMessage.stringMessage.messageReceiver = (GameObject)EditorGUILayout.ObjectField("Message Receiver:", ba.sendMessage.stringMessage.messageReceiver, typeof(GameObject), true);
					}
					ba.sendMessage.stringMessage.messageName = EditorGUILayout.TextField("Function Name:", ba.sendMessage.stringMessage.messageName);
					ba.sendMessage.stringMessage.stringValue = EditorGUILayout.TextField("Function Parameter:", ba.sendMessage.stringMessage.stringValue);
					GUI.color = Color.white;
				}
				EditorGUI.indentLevel -= 1;
			}
			
			string text5 = (ba.sendMessage.numericalMessage.enabled) ? "[ENABLED]" : "[DISABLED]";
			open5 = EditorGUILayout.Foldout(open5, " Numerical Message  " + text5);
			if(open5) {
				EditorGUI.indentLevel += 1;
				ba.sendMessage.numericalMessage.enabled = EditorGUILayout.Toggle("Enabled:", ba.sendMessage.numericalMessage.enabled);
				if(ba.sendMessage.numericalMessage.enabled) {
					GUI.color = new Color(0.85f, 0.85f, 0.85f);
                    EditorGUIUtility.labelWidth = 175f;
					ba.sendMessage.numericalMessage.receiverEnum = (ButtonAction.ReceiverEnum)EditorGUILayout.EnumPopup("Receiver Type:", ba.sendMessage.numericalMessage.receiverEnum);
					ba.sendMessage.numericalMessage.broadcastMessage = EditorGUILayout.Toggle("Broadcast Message:", ba.sendMessage.numericalMessage.broadcastMessage);
					if(ba.sendMessage.numericalMessage.receiverEnum == ButtonAction.ReceiverEnum.None) {
						ba.sendMessage.numericalMessage.messageReceiver = (GameObject)EditorGUILayout.ObjectField("Message Receiver:", ba.sendMessage.numericalMessage.messageReceiver, typeof(GameObject), true);
					}
					ba.sendMessage.numericalMessage.messageName = EditorGUILayout.TextField("Function Name:", ba.sendMessage.numericalMessage.messageName);
					ba.sendMessage.numericalMessage.isInt = EditorGUILayout.Toggle("Parameter Is Integer:", ba.sendMessage.numericalMessage.isInt);
                    EditorGUIUtility.labelWidth = 185f;
					if(ba.sendMessage.numericalMessage.isInt) {
						ba.sendMessage.numericalMessage.valueToSend = EditorGUILayout.IntField("Function Parameter (Int):", Mathf.RoundToInt(ba.sendMessage.numericalMessage.valueToSend));
					}
					else {
						ba.sendMessage.numericalMessage.valueToSend = EditorGUILayout.FloatField("Function Parameter (Float):", ba.sendMessage.numericalMessage.valueToSend);
					}
					GUI.color = Color.white;
					EditorGUIUtility.LookLikeControls();
				}
				EditorGUI.indentLevel -= 1;
			}
			
			EditorGUI.indentLevel -= 1;
		}
		
		string openText8 = (ba.quitApplication.enabled) ? "[ENABLED]" : "[DISABLED]";
		isOpen8 = EditorGUILayout.Foldout(isOpen8, " Quit Application      " + openText8);
		if(isOpen8) {
			EditorGUI.indentLevel += 1;
			ba.quitApplication.enabled = EditorGUILayout.Toggle("Enabled", ba.quitApplication.enabled);
			EditorGUI.indentLevel -= 1;
		}

        isOpen10 = EditorGUILayout.Foldout(isOpen10, " Easter Egg");
        if(isOpen10) {
            EditorGUI.indentLevel += 1;
            ba.easterEgg.label = (UILabel)EditorGUILayout.ObjectField("Label:", ba.easterEgg.label, typeof(UILabel));
            EditorGUILayout.HelpBox("Change the cycle strings using Debug Inspector", MessageType.Info);
            EditorGUI.indentLevel -= 1;
        }
		
		if(GUI.changed) {
			EditorUtility.SetDirty(ba);
		}
	}
	
	private int BoolToInt(bool t) {
		return (t) ? 1 : 0;
	}
}