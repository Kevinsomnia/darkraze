using UnityEngine;
using System.Collections;

public class AlphaGroupUI : MonoBehaviour
{
    public UIWidget[] widgets;

    private float alphaFactor = 1f;
    public float alpha
    {
        get
        {
            return alphaFactor;
        }
        set
        {
            alphaFactor = Mathf.Clamp01(value);

            foreach (UIWidget wgt in widgets)
            {
                wgt.alpha = wgt.defaultAlpha * alphaFactor;
            }
        }
    }

    void Awake()
    {
        if (widgets.Length <= 0)
        {
            widgets = (UIWidget[])GetComponentsInChildren<UIWidget>();
        }
    }
}