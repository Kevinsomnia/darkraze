using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UILabel))]
public class ChatListGUI : MonoBehaviour
{
    public int maximumEntries = 100;

    private UILabel _l;
    public UILabel label
    {
        get
        {
            if (_l == null)
            {
                _l = GetComponent<UILabel>();
            }

            return _l;
        }
    }

    [HideInInspector] public List<string> chatList;

    void Start()
    {
        chatList = new List<string>();
        RebuildChatList();
        maximumEntries = Mathf.Clamp(maximumEntries, 3, 1000);
    }

    public void AppendNewLine(string toAppend)
    {
        while (chatList.Count > maximumEntries)
        {
            chatList.RemoveAt(0);
        }
        chatList.Add(toAppend);

        RebuildChatList();
    }

    public void CopyList(List<string> cList)
    {
        while (cList.Count > maximumEntries)
        {
            cList.RemoveAt(0);
        }

        chatList = cList;
        RebuildChatList();
    }

    public void RebuildChatList()
    {
        label.text = "";
        for (int i = 0; i < chatList.Count; i++)
        {
            if (i > 0)
            {
                label.text += "\n";
            }
            label.text += chatList[i];
        }
    }

    public void ClearChatList()
    {
        if (chatList != null)
        {
            chatList.Clear();
        }

        if (label != null)
        {
            label.text = "";
        }
    }
}