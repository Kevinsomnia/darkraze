using UnityEngine;
using System.Collections;

public class KillFeedItem : MonoBehaviour {
    public UILabel thisLabel;
    public float fadeInSpeed = 6f;
    public float fadeOutSpeed = 3f;
    public float shiftSpeed = 8f;

    [HideInInspector] public KillFeedManager manager;
    [HideInInspector] public Vector3 targetPos;

    private Transform tr;
    private float defAlpha;
    private float alphaMod;

    public void Initialize(float duration = 5f) {
        tr = transform;
        defAlpha = thisLabel.alpha;
		thisLabel.alpha = 0f;
        tr.localPosition = targetPos - (Vector3.up * manager.feedSpacing * 0.4f);

        if(gameObject.activeInHierarchy) {
            StartCoroutine(RemoveFromFeed(duration));
        }
    }

    void Update() {
        tr.localPosition = Vector3.Lerp(tr.localPosition, targetPos, Time.unscaledDeltaTime * shiftSpeed);
        tr.localScale = Vector3.one * (0.95f + (thisLabel.alpha * 0.05f));
    }

    public IEnumerator RemoveFromFeed(float dur) {
        while(thisLabel.alpha < thisLabel.defaultAlpha) {
            thisLabel.alpha += Time.unscaledDeltaTime * fadeInSpeed;
            yield return null;
        }

        yield return new WaitForSeconds(dur);

        if(manager.feedList.Count > 0 && thisLabel == manager.feedList[0]) {
            targetPos += Vector3.up * manager.feedSpacing * 0.25f;
        }

        while(thisLabel.alpha > 0f) {
            thisLabel.alpha -= Time.unscaledDeltaTime * fadeOutSpeed;
            yield return null;
        }

        if(manager.feedList.Count > 0) {
            manager.feedList.RemoveAt(0);
            manager.RebuildFeedList();
        }

        Destroy(gameObject);
    }
}