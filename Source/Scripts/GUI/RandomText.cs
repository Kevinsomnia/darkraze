using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UILabel))]
public class RandomText : MonoBehaviour {
    public float waitTime = 5f;
    public string[] availableText = new string[1]{"Text"};

    private UILabel label;
    private float timer;
    private int newIndex;
    private int oldIndex;

    void Awake() {
        label = GetComponent<UILabel>();
        oldIndex = -1;
        DisplayNewText();
    }

    void Update() {
        timer += Time.unscaledDeltaTime;
        if(timer >= waitTime) {
            DisplayNewText();
        }
    }

    private void DisplayNewText() {
        do {
            newIndex = Random.Range(0, availableText.Length);
        }
        while(availableText.Length > 1 && oldIndex == newIndex);

        label.text = availableText[newIndex];

        timer -= waitTime;
        timer = Mathf.Max(0f, timer);
        oldIndex = newIndex;
    }
}