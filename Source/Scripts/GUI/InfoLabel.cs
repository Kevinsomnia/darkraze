using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(UILabel))]
public class InfoLabel : MonoBehaviour {
    public bool isBuildVersion = false;
    public bool includeTag = false;
    public bool isDateAndTime = false;

    private UILabel label;

	void Awake() {
        label = GetComponent<UILabel>();
        if(isBuildVersion) {
            label.text = DarkRef.GetBuildVersion(includeTag);
        }
	}

    void Update() {
        if(isDateAndTime) {
            label.text = DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString();
        }
    }
}