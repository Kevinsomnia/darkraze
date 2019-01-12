using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UIInput))]
public class HostRoomName : MonoBehaviour {
	private UIInput input;
	private string oldText;
	private bool canUpdate;

	void Start() {
		input = GetComponent<UIInput>();
		canUpdate = false;
		StartCoroutine(LateStart());
	}
	
	private IEnumerator LateStart() {
		yield return null;
		RefreshRoomName();
		oldText = input.value;
		canUpdate = true;
	}
	
	void Update() {
		if(!canUpdate) {
			return;
		}

        if(oldText != input.value && input.value != AccountManager.profileData.username + "'s Game") {
            PlayerPrefs.SetString("SavedHostRoomName", input.value);
            oldText = input.value;
		}
	}

	public void RefreshRoomName() {
        input.value = PlayerPrefs.GetString("SavedHostRoomName", AccountManager.profileData.username + "'s Game");
	}
}