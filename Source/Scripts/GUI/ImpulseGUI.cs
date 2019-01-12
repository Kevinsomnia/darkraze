using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UIWidget))]
public class ImpulseGUI : MonoBehaviour
{
    public float impulseInterval = 1f;
    public float impulseAmount = 0.1f;
    public float restoreSpeed = 2f;
    public float impulseAlpha = 0f;
    public bool ignoreTimescale = false;

    [HideInInspector] public float baseAlpha;
    [HideInInspector] public Color curColor;

    private UIWidget widget;
    private Vector3 defaultSize;
    private float extAlpha;
    private float impulseValue = 1f;
    private float impulseTimer;

    void Awake()
    {
        widget = GetComponent<UIWidget>();
        defaultSize = widget.transform.localScale;
        baseAlpha = widget.alpha;
        curColor = widget.color;
    }

    void Start()
    {
        impulseValue = 1f;
        impulseTimer = impulseInterval;
    }

    void OnDisable()
    {
        widget.transform.localScale = defaultSize;
        widget.color = curColor;
        widget.alpha = baseAlpha;
        extAlpha = 0f;
    }

    void Update()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        float delta = (ignoreTimescale) ? Time.unscaledDeltaTime : Time.deltaTime;

        impulseTimer += delta;
        if (impulseTimer >= impulseInterval)
        {
            DoImpulse();
        }

        impulseValue = Mathf.MoveTowards(impulseValue, 1f, delta * restoreSpeed * impulseAmount);
        widget.transform.localScale = defaultSize * impulseValue;

        if (impulseAlpha > 0f)
        {
            widget.color = curColor;
            extAlpha = Mathf.MoveTowards(extAlpha, 0f, delta * restoreSpeed * impulseAlpha);
            widget.alpha = baseAlpha + extAlpha;
        }
    }

    public void DoImpulse()
    {
        impulseValue += impulseAmount;
        extAlpha += impulseAlpha;
        impulseTimer -= impulseInterval;
    }
}