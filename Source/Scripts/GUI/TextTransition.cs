using UnityEngine;
using System.Collections;

/// <summary>
/// Pretty neat text transition! It pretty much slides up to reveal another set of text.
/// Great for multi-lined paragraphs to be revealed one line at a time, or just for coolness.
/// </summary>
public class TextTransition : MonoBehaviour {
    public float shiftAmount = 3f;
    public float shiftSpeed = 4f;
    public float shrinkAmount = 0.05f;
    public float animationPause = 0.1f;

    private UILabel label;

    void Awake() {
        label = GetComponent<UILabel>();
    }

    public void UpdateText(string newText) {
        StartCoroutine(TextStuff(newText));
    }

    private IEnumerator TextStuff(string t) {
        Vector3 anchorPosition = transform.localPosition;

        float startTime = 0f;
        while(startTime < 1f) {
            startTime += Time.deltaTime * shiftSpeed;
            startTime = Mathf.Clamp01(startTime);
            transform.localPosition = anchorPosition + (Vector3.up * startTime * shiftAmount);
            transform.localScale = Vector3.one * (1f - (startTime * shrinkAmount));
            label.alpha = 1f - startTime;
            yield return null;
        }

        transform.localPosition = transform.localPosition = anchorPosition + (Vector3.up * -shiftAmount);
        label.text = t;
        yield return new WaitForSeconds(animationPause);

        float endTime = 1f;
        while(endTime > 0f) {
            endTime -= Time.deltaTime * shiftSpeed;
            endTime = Mathf.Clamp01(endTime);
            transform.localPosition = anchorPosition + (Vector3.up * endTime * -shiftAmount);
            transform.localScale = Vector3.one * (1f - (endTime * shrinkAmount));
            label.alpha = 1f - endTime;
            yield return null;
        }
    }
}