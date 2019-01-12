using UnityEngine;
using System.Collections;

public class TabManager : MonoBehaviour
{
    [System.Serializable]
    public class TabProperties
    {
        public GameObject[] tabContents;
        public UIButton tabButton;
    }

    public TabProperties[] tabs = new TabProperties[0];
    public float tabSelectionAlpha = 1f;
    public int startingIndex = 0;
    public float fadeSpeed = 0f;

    [HideInInspector] public int selectedTab;

    void Start()
    {
        if (tabs.Length <= 0)
        {
            Debug.Log("WARNING: You do not have any tabs for this object!", this);
            this.enabled = false;
            return;
        }

        startingIndex = Mathf.Clamp(startingIndex, 0, tabs.Length - 1);
        selectedTab = startingIndex;

        if (tabs[selectedTab].tabButton != null)
        {
            Color defCCol = tabs[selectedTab].tabButton.defaultColor;
            defCCol.a *= tabSelectionAlpha;
            tabs[selectedTab].tabButton.defaultColor = defCCol;
        }

        for (int i = 0; i < tabs.Length; i++)
        {
            foreach (GameObject content in tabs[i].tabContents)
            {
                content.SetActive(i == selectedTab);
            }
        }
    }

    public void SelectTab(int index)
    {
        if (selectedTab == index)
        {
            return;
        }

        StartCoroutine(TabTransition(selectedTab, index));
    }

    private IEnumerator TabTransition(int oldTab, int curTab)
    {
        tabSelectionAlpha = Mathf.Max(0.01f, tabSelectionAlpha);

        if (tabs[oldTab].tabButton != null)
        {
            Color defOCol = tabs[oldTab].tabButton.defaultColor;
            defOCol.a /= tabSelectionAlpha;
            tabs[oldTab].tabButton.defaultColor = defOCol;
        }

        if (tabs[curTab].tabButton != null)
        {
            Color defCCol = tabs[curTab].tabButton.defaultColor;
            defCCol.a *= tabSelectionAlpha;
            tabs[curTab].tabButton.defaultColor = defCCol;
        }

        foreach (GameObject content in tabs[oldTab].tabContents)
        {
            if (fadeSpeed > 0f)
            {
                UIPanel oldTabPanel = content.GetComponent<UIPanel>();
                if (oldTabPanel != null)
                {
                    float fadeAlpha = oldTabPanel.alpha;
                    while (fadeAlpha > 0f)
                    {
                        fadeAlpha -= Time.deltaTime * fadeSpeed;
                        oldTabPanel.alpha = Mathf.Clamp01(fadeAlpha);
                        yield return null;
                    }
                }
            }

            content.SetActive(false);
        }

        foreach (GameObject content in tabs[curTab].tabContents)
        {
            content.SetActive(true);

            if (fadeSpeed > 0f)
            {
                UIPanel newTabPanel = content.GetComponent<UIPanel>();
                if (newTabPanel != null)
                {
                    float fadeAlpha = 0f;
                    while (fadeAlpha < 1f)
                    {
                        fadeAlpha += Time.deltaTime * fadeSpeed;
                        newTabPanel.alpha = Mathf.Clamp01(fadeAlpha);
                        yield return null;
                    }
                }
            }
        }

        selectedTab = curTab;
    }
}