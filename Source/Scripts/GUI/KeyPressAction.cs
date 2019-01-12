using UnityEngine;
using System.Collections;

public class KeyPressAction : MonoBehaviour {
	#region Classes
	[System.Serializable]
	public class SendMessage {
		public GenericMessage genericMessage;
		public GameObjectMessage gameObjectMessage;
		public BooleanMessage booleanMessage;
		public StringMessage stringMessage;
		public NumericalMessage numericalMessage;
	}
	
	#region Message Types
	[System.Serializable]
	public class GenericMessage {
		public bool enabled = false;
		public bool broadcastMessage = false;
		public string messageName = "Message";
		public GameObject messageReceiver;
	}
	
	
	[System.Serializable]
	public class GameObjectMessage {
		public bool enabled = false;
		public bool broadcastMessage = false;
		public string messageName = "Message";
		public GameObject gameobjectValue = null;
		public GameObject messageReceiver;
	}
	
	[System.Serializable]
	public class BooleanMessage {
		public bool enabled = false;
		public bool broadcastMessage = false;
		public string messageName = "Message";
		public bool booleanValue = false;
		public GameObject messageReceiver;
	}
	
	[System.Serializable]
	public class StringMessage {
		public bool enabled = false;
		public bool broadcastMessage = false;
		public string messageName = "Message";
		public string stringValue = "";
		public GameObject messageReceiver;
	}
	
	[System.Serializable]
	public class NumericalMessage {
		public bool enabled = false;
		public bool broadcastMessage = false;
		public bool isInt = true; //Is it a integer? If not, then it's a float.
		public string messageName = "Message";
		public float valueToSend = 1;
		public GameObject messageReceiver;
	}
	#endregion

	#endregion

	public KeyCode keyToPress = KeyCode.None;
	public SendMessage sendMessage = new SendMessage();

	void Update() {
		if(Input.GetKeyDown(keyToPress)) {
			OnKeyPressed();
		}
	}

	private void OnKeyPressed() {
		if(sendMessage.genericMessage.enabled) {
			GameObject receiver = sendMessage.genericMessage.messageReceiver;
			
			if(sendMessage.genericMessage.broadcastMessage) {
				receiver.BroadcastMessage(sendMessage.genericMessage.messageName, SendMessageOptions.DontRequireReceiver);
			}
			else {
				receiver.SendMessage(sendMessage.genericMessage.messageName, SendMessageOptions.DontRequireReceiver);
			}
		}
		
		if(sendMessage.gameObjectMessage.enabled) {
			GameObject receiver = sendMessage.gameObjectMessage.messageReceiver;

			if(sendMessage.gameObjectMessage.broadcastMessage) {
				receiver.BroadcastMessage(sendMessage.gameObjectMessage.messageName, sendMessage.gameObjectMessage.gameobjectValue, SendMessageOptions.DontRequireReceiver);
			}
			else {
				receiver.SendMessage(sendMessage.gameObjectMessage.messageName, sendMessage.gameObjectMessage.gameobjectValue, SendMessageOptions.RequireReceiver);
			}
		}
		
		if(sendMessage.booleanMessage.enabled) {
			GameObject receiver = sendMessage.booleanMessage.messageReceiver;

			if(sendMessage.booleanMessage.broadcastMessage) {
				receiver.BroadcastMessage(sendMessage.booleanMessage.messageName, sendMessage.booleanMessage.booleanValue, SendMessageOptions.DontRequireReceiver);
			}
			else {
				receiver.SendMessage(sendMessage.booleanMessage.messageName, sendMessage.booleanMessage.booleanValue, SendMessageOptions.RequireReceiver);
			}
		}
		
		if(sendMessage.stringMessage.enabled) {
			GameObject receiver = sendMessage.stringMessage.messageReceiver;
			
			if(sendMessage.stringMessage.broadcastMessage) {
				receiver.BroadcastMessage(sendMessage.stringMessage.messageName, sendMessage.stringMessage.stringValue, SendMessageOptions.DontRequireReceiver);
			}
			else {
				receiver.SendMessage(sendMessage.stringMessage.messageName, sendMessage.stringMessage.stringValue, SendMessageOptions.RequireReceiver);
			}
		}
		
		if(sendMessage.numericalMessage.enabled) {
			GameObject receiver = sendMessage.numericalMessage.messageReceiver;

			if(sendMessage.numericalMessage.isInt) {
				if(sendMessage.numericalMessage.broadcastMessage) {
					receiver.BroadcastMessage(sendMessage.numericalMessage.messageName, (int)sendMessage.numericalMessage.valueToSend, SendMessageOptions.DontRequireReceiver);
				}
				else {
					receiver.SendMessage(sendMessage.numericalMessage.messageName, (int)sendMessage.numericalMessage.valueToSend, SendMessageOptions.RequireReceiver);
				}
			}
			else {
				if(sendMessage.numericalMessage.broadcastMessage) {
					receiver.BroadcastMessage(sendMessage.numericalMessage.messageName, sendMessage.numericalMessage.valueToSend, SendMessageOptions.DontRequireReceiver);
				}
				else {
					receiver.SendMessage(sendMessage.numericalMessage.messageName, sendMessage.numericalMessage.valueToSend, SendMessageOptions.RequireReceiver);
				}
			}
		}
	}
}