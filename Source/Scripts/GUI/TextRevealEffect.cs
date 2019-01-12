using UnityEngine;
using System.Collections;

public class TextRevealEffect : MonoBehaviour
{
    public string text = "";
    public float intervalTime = 0.1f; //10 times per second.

    private UILabel label;
    private int curOffset = 0;
    private float nextTime;
    private string normalString;
    private string jumbledString;

    void Awake()
    {
        label = GetComponent<UILabel>();
        nextTime = 0f;
        normalString = "";
        jumbledString = "";

        if (text == "")
        {
            text = label.text;
        }

        label.text = "";
    }

    void Update()
    {
        if (Time.time >= nextTime)
        {
            curOffset++;
            curOffset = Mathf.Clamp(curOffset, 0, text.Length);
            normalString = text.Substring(0, curOffset);
            nextTime = Time.time + intervalTime;
        }

        jumbledString = "[969696]";

        for (int i = curOffset; i < text.Length; i++)
        {
            if (text[i] == ' ')
            {
                jumbledString += " ";
            }
            else
            {
                jumbledString += DarkRef.GetRandomLetter();
            }
        }

        jumbledString += "[-]";

        label.text = normalString + jumbledString;
    }
}