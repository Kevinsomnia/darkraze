using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KillFeedManager : MonoBehaviour {
    [System.Serializable]
    public class KillContainer {
        public KillContainer(string klr, string vctm, int wepIndex) {
            killer = klr;
            victim = vctm;
            weaponIndex = wepIndex;
        }

        public string killer = "Player1";
        public string victim = "Player2";
        public int weaponIndex = 0;
    }

    public Transform killFeedParent;
    public UILabel labelPrefab;
    public float feedSpacing = 16f;
    public int fontSize = 14;
    public float feedDuration = 5f;
    public int queueBuffer = 10;

    [HideInInspector] public List<UILabel> feedList = new List<UILabel>();
    [HideInInspector] public List<KillContainer> feedQueue = new List<KillContainer>();

    void OnDisable() {
        ClearAllItems();
    }

    void Update() {
        if(feedQueue.Count > 0 && feedList.Count < queueBuffer) {
            AddToFeed(feedQueue[0].killer, feedQueue[0].victim, feedQueue[0].weaponIndex, true);
            feedQueue.RemoveAt(0);
        }
    }

    public void AddToFeed(string killerName, string victimName, int weaponIndex, bool queued = false) {
        if(feedList.Count >= queueBuffer) {
            feedQueue.Add(new KillContainer(killerName, victimName, weaponIndex));
            return;
        }

        UILabel newFeedInstance = (UILabel)Instantiate(labelPrefab);
        newFeedInstance.transform.parent = killFeedParent;
        newFeedInstance.transform.localScale = Vector3.one;
        newFeedInstance.fontSize = fontSize;

        if(weaponIndex >= 0) {
            string killedBy = (weaponIndex >= 200) ? GrenadeDatabase.GetGrenadeByID(weaponIndex - 200).grenadeName : WeaponDatabase.GetWeaponByID(weaponIndex).gunName;
            newFeedInstance.text = killerName + " [" + killedBy + "] " + victimName;
        }
        else {
            newFeedInstance.text = killerName + " killed " + victimName;
        }

        KillFeedItem kfi = newFeedInstance.GetComponent<KillFeedItem>();
        kfi.manager = this;
        kfi.targetPos = -Vector3.up * feedList.Count * feedSpacing;
        kfi.Initialize((queued) ? feedDuration * Random.Range(0.75f, 0.85f) : feedDuration); //yay for variety

        feedList.Add(newFeedInstance);
    }

    public void RebuildFeedList() {
        for(int i = 0; i < feedList.Count; i++) {
            if(feedList[i] == null) {
                continue;
            }

            feedList[i].GetComponent<KillFeedItem>().targetPos = -Vector3.up * i * feedSpacing;
        }
    }

    public void ClearAllItems() {
        if(killFeedParent != null) {
            for(int i = 0; i < killFeedParent.childCount; i++) {
                Destroy(killFeedParent.GetChild(i).gameObject);
            }
        }

        feedList.Clear();
        feedQueue.Clear();
    }
}