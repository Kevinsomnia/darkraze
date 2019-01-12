using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionFeedManager : MonoBehaviour
{
    [System.Serializable]
    public class ActionContainer
    {
        public ActionContainer(string name, int award, bool bonus)
        {
            actionName = name;
            expAward = award;
            isBonus = bonus;
        }

        public void IncrementStack()
        {
            if (label == null)
            {
                return;
            }

            currentStack++;
            suffix = (currentStack >= 1) ? ("[" + DarkRef.RGBtoHex(new Color32(255, (byte)(255 - (Mathf.Clamp(currentStack - 1, 0, 6) * 38)), (byte)(255 - (Mathf.Clamp(currentStack - 1, 0, 6) * 45)), 255)) + "] (x" + currentStack.ToString() + ")[-]") : "";
            lastActionTime = Time.time;

            afi.SetDuration(duration);
            afi.defaultSize = 1f + Mathf.Clamp((currentStack - 1) * 0.01f, 0f, 0.2f);
            afi.ImpulseAnimation();
        }

        public UILabel label;

        private ActionFeedItem _afi;
        public ActionFeedItem afi
        {
            get
            {
                if (label == null)
                {
                    return null;
                }

                _afi = label.GetComponent<ActionFeedItem>();
                return _afi;
            }
        }

        public float duration = 5f;

        public string actionName = "Kill";
        public string suffix = "";

        public string fullName
        {
            get
            {
                return actionName.ToUpper() + suffix;
            }
        }

        public int expAward = 100;
        public bool isBonus = false;
        public int currentStack = 1;

        public float lastActionTime = 0f;
    }

    public Transform actionFeedParent;
    public UILabel labelPrefab;
    public float feedSpacing = 16f;
    public float feedDuration = 5f;
    public int queueBuffer = 5;
    public float stackingThreshold = 2f;

    [HideInInspector] public List<ActionContainer> feedList = new List<ActionContainer>();
    [HideInInspector] public List<ActionContainer> feedQueue = new List<ActionContainer>();

    private int currentStack;

    void Update()
    {
        if (feedQueue.Count > 0 && feedList.Count <= queueBuffer)
        {
            AddToFeed(feedQueue[0].actionName, feedQueue[0].expAward, feedQueue[0].isBonus);
            feedQueue.RemoveAt(0);
        }
    }

    public void AddToFeed(string action, int expEarned, bool isBonus = false)
    {
        if (feedList.Count > queueBuffer - 1)
        {
            feedQueue.Add(new ActionContainer(action, expEarned, isBonus));
            return;
        }

        ActionContainer currentAC = GetContainerByName(action);

        if (currentAC != null)
        {
            currentAC.expAward += expEarned;
            currentAC.afi.targetReward = currentAC.expAward;
            currentAC.IncrementStack();
        }
        else
        {
            UILabel newFeedInstance = (UILabel)Instantiate(labelPrefab);
            newFeedInstance.transform.parent = actionFeedParent;
            newFeedInstance.transform.localScale = Vector3.one;
            newFeedInstance.fontSize = labelPrefab.fontSize - ((isBonus) ? 2 : 0);

            ActionFeedItem afItem = newFeedInstance.GetComponent<ActionFeedItem>();
            afItem.manager = this;
            afItem.targetPos = -Vector3.up * feedList.Count * feedSpacing;
            afItem.SetDuration(feedDuration);
            afItem.Initialize(feedList.Count);

            currentAC = new ActionContainer(action, expEarned, isBonus);
            currentAC.label = newFeedInstance;
            currentAC.afi.targetReward = expEarned;
            currentAC.duration = feedDuration;
            currentAC.lastActionTime = Time.time;
            feedList.Add(currentAC);
        }

        if (isBonus)
        {
            Color col = currentAC.label.defaultColor * 0.7f;
            col.g *= 0.9f;
            col.b *= 0.9f;
            currentAC.label.color = col;
        }

        currentAC.afi.baseText = ((isBonus) ? "+" : "") + currentAC.fullName;
    }

    public void RebuildFeedList()
    {
        for (int i = 0; i < feedList.Count; i++)
        {
            feedList[i].afi.targetPos = -Vector3.up * i * feedSpacing;
        }
    }

    public ActionContainer GetContainerByName(string action)
    {
        for (int i = 0; i < feedList.Count; i++)
        {
            if (feedList[i].actionName.ToLower() == action.ToLower() && feedList[i].afi != null && !feedList[i].afi.isFadingOut)
            {
                return feedList[i];
            }
        }

        return null;
    }
}