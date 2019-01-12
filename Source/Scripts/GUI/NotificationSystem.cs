using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NotificationSystem : MonoBehaviour {
	private static NotificationSystem _instance;
	public static NotificationSystem Instance {
		get {
			if(_instance == null) {
				GameObject go = (GameObject)GameObject.Instantiate(Resources.Load("GUI/[Notification System]"));
				_instance = go.GetComponent<NotificationSystem>();
			}
			
			return _instance;
		}
	}
	
	public UIPanel notificationPanel;
	public UISprite background;
	public UILabel label;
	
	[HideInInspector] public bool queueIsRunning = false;
	
	private List<string> titleQueue = new List<string>();
	private List<string> bodyQueue = new List<string>();
	private List<float> durationQueue = new List<float>();
	
	public void CreateNotification(string title, string body, float duration) {
		titleQueue.Add(title);
		bodyQueue.Add(body);
		durationQueue.Add(duration);
		
		StartQueue();
	}
	
	private void StartQueue() {
		if(queueIsRunning || titleQueue.Count <= 0) {
			return;
		}
		
		queueIsRunning = true;
		StartCoroutine(RepeatAction());		
	}
	
	private IEnumerator RepeatAction() {
		notificationPanel.alpha = 0f;
		label.text = "";
		
		if(titleQueue[0] != "") {
			label.text += titleQueue[0] + "\n" + "[7E7E7E]" + bodyQueue[0] + "[-]";
		}
		else {
			label.text += bodyQueue[0];
		}
		
		while(notificationPanel.alpha < 1f) {
			notificationPanel.alpha += Time.unscaledDeltaTime * 3.75f;
			yield return null;
		}
		
		yield return new WaitForSeconds(durationQueue[0]);
		
		while(notificationPanel.alpha > 0f) {
			notificationPanel.alpha -= Time.unscaledDeltaTime * 3.75f;
			yield return null;
		}
		
		titleQueue.RemoveAt(0);
		bodyQueue.RemoveAt(0);
		durationQueue.RemoveAt(0);
		
		if(titleQueue.Count > 0) {
			StartCoroutine(RepeatAction());
		}
		else {
			queueIsRunning = false;
			yield break;
		}
	}
}