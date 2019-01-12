using UnityEngine;
using System.Collections;

public class AnchorControl : MonoBehaviour
{
    public UIAnchor[] anchorsToControl;
    public Vector2 wideOffset = new Vector2(0f, 0f);

    private UIAnchor anchor;
    private Vector2 defOffset;

    void Start()
    {
        anchor = GetComponent<UIAnchor>();

        if (anchor != null)
        {
            defOffset = anchor.relativeOffset;
        }
    }

    void Update()
    {
        if (anchor != null)
        {
            anchor.relativeOffset = (DarkRef.isModernWidescreen || DarkRef.isOldWidescreen) ? wideOffset : defOffset;
        }
    }

    public void ControlAnchors(bool control)
    {
        foreach (UIAnchor anchor in anchorsToControl)
        {
            //anchor.anchorControl = control;
        }
    }
}