using UnityEngine;
using System.Collections;

public class ActionFeedItem : MonoBehaviour {
	public UILabel thisLabel;
	public float fadeInSpeed = 6f;
	public float fadeOutSpeed = 3f;
	public float shiftSpeed = 8f;
    public float indexBonusTime = 0.5f;
	
	[HideInInspector] public ActionFeedManager manager;
	[HideInInspector] public Vector3 targetPos;
	[HideInInspector] public int indx;
    [HideInInspector] public float curDur = 0f;
    [HideInInspector] public float defaultSize = 1f;
    [HideInInspector] public string baseText = "";
    [HideInInspector] public int targetReward = 0;
    [HideInInspector] public bool isFadingOut = false;
	
	private Transform tr;
	private float alphaMod;
    private float initTime = 0f;
    private float sizeFactor = 1f;
    private float displayReward;
	
	public void Initialize(int index) {
		tr = transform;
		indx = index;
		thisLabel.alpha = 0f;
        isFadingOut = false;
		tr.localPosition = targetPos - (Vector3.up * manager.feedSpacing * 0.2f);
		StartCoroutine(RemoveFromFeed());
	}

    public void SetDuration(float duration) {
        curDur = duration;
        isFadingOut = false;
        initTime = Time.time;
    }
	
	void Update() {
		tr.localPosition = Vector3.Lerp(tr.localPosition, targetPos, Time.unscaledDeltaTime * shiftSpeed);
        sizeFactor = Mathf.Lerp(sizeFactor, defaultSize, Time.unscaledDeltaTime * 3f);
        thisLabel.alpha = thisLabel.defaultAlpha * alphaMod;
        thisLabel.transform.localScale = Vector3.one * sizeFactor * (0.94f + (alphaMod * 0.06f));
        displayReward = Mathf.Lerp(displayReward, (float)targetReward, Time.unscaledDeltaTime * 8f);
        thisLabel.text = baseText + " - " + displayReward.ToString("F0") + " XP";
	}

    public void ImpulseAnimation() {
        sizeFactor += 0.075f;
    }
	
	private IEnumerator RemoveFromFeed() {
		while(alphaMod < 1f) {
			alphaMod += Time.deltaTime * fadeInSpeed;
            alphaMod = Mathf.Clamp01(alphaMod);
			yield return null;
		}

        while(Time.time - initTime <= curDur + (indx * indexBonusTime)) {
            yield return null;
        }

        isFadingOut = true;

		while(alphaMod > 0f) {
			alphaMod -= Time.deltaTime * fadeOutSpeed;
            alphaMod = Mathf.Clamp01(alphaMod);
			yield return null;
		}
		
		manager.feedList.RemoveAt(0);
		manager.RebuildFeedList();
		Destroy(gameObject);
	}
}