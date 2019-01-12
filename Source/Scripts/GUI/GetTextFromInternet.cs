using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UILabel))]
public class GetTextFromInternet : MonoBehaviour
{
    public string url = "www.google.com/robots.txt";
    public string defaultString = "Getting information...";
    public string errorString = "Error retrieving info";
    public float repeatTime = -1f;

    private UILabel label;

    void Start()
    {
        label = GetComponent<UILabel>();
        label.text = defaultString;

        StartCoroutine(GetInfo());
    }

    private IEnumerator GetInfo()
    {
        while (repeatTime >= 0f)
        {
            WWW requestText = new WWW(url);

            yield return requestText;

            if (requestText.error != null)
            {
                label.text = errorString;
                yield break;
            }

            if (requestText.text != null)
            {
                if (requestText.text.StartsWith("<?") || requestText.text.StartsWith("<!"))
                {
                    label.text = errorString;
                    yield break;
                }

                label.text = requestText.text;
            }

            yield return new WaitForSeconds(repeatTime);
        }
    }
}