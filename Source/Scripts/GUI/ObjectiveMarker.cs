using UnityEngine;
using System.Collections;

public class ObjectiveMarker : MonoBehaviour
{
    public bool isEnabled = true;

    public Transform target;

    public Vector2 baseScale = new Vector2(1, 1);

    public UITexture markerTexture;
    public UILabel distanceLabel;
    public UILabel descriptionLabel;
    public float distanceRefreshRate = 0.4f; //Just added this to make it look cool somehow... Plus, a slight performance increase

    public Vector2 edgeOffset = new Vector2(16, 16);

    public bool scalingEnabled = false;
    public float nearScale = 1f;
    public float farScale = 0.8f;
    public float nearDistance = 15f;
    public float farDistance = 250f;

    public Vector3 textBorder = new Vector4(0.02f, 0.98f, 0.04f);

    private Transform tr;
    private Camera mainCamera;

    private int normalWidth;
    private int modernWideWidth; //16:9
    private int oldWideWidth; //16:10
    private int normalHeight;

    public bool setByExternal = false;

    private int curWidth;
    private Transform player;
    private Vector3 pos;
    private Vector2 markerPos;
    private float curScale;
    private float distance;
    private float rotation;

    private float xText;
    private float yText;
    private float yText2;
    private Vector2 curOffset;
    private Vector2 offsetReal;
    private Vector2 positionReal;
    private float alphaMod;
    private float osMod;
    private UIWidget.Pivot pivotPoint;
    private Texture2D curTexture;
    private bool targetDead;
    private bool aimingAt;

    private AimController ac;
    private UIWidget[] widgets;
    private Vector3 m_LastPos;

    public Texture2D[] GUITextures;

    void Awake()
    {
        if (GeneralVariables.player == null)
        {
            this.enabled = false;
            return;
        }

        tr = transform;

        mainCamera = GeneralVariables.mainPlayerCamera;
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            enabled = false;
            return;
        }

        alphaMod = 0f;

        normalWidth = DarkRef.NormalNGUIWidth;
        modernWideWidth = DarkRef.ModernWideNGUIWidth;
        oldWideWidth = DarkRef.OldWideNGUIWidth;
        normalHeight = DarkRef.NGUIHeight;

        InvokeRepeating("CalculateDistance", 0f, distanceRefreshRate);

        player = GeneralVariables.player.transform;
        ac = GeneralVariables.playerRef.ac;
        widgets = new UIWidget[3] { markerTexture, distanceLabel, descriptionLabel };
        isEnabled = true;
    }

    void Update()
    {
        alphaMod = Mathf.MoveTowards(alphaMod, (isEnabled && !aimingAt) ? 1f : 0f, Time.unscaledDeltaTime * 5f);
        foreach (UIWidget widget in widgets)
        {
            widget.alpha = widget.defaultAlpha * alphaMod * osMod;
        }

        if (target != null)
        {
            BaseStats bs = target.GetComponent<BaseStats>();
            targetDead = (bs != null && bs.curHealth <= 0);
        }

        if (target == null || targetDead)
        {
            isEnabled = false;
        }

        if (!isEnabled && alphaMod <= 0f)
        {
            return;
        }

        curWidth = normalWidth;
        if (DarkRef.isModernWidescreen)
        {
            curWidth = modernWideWidth;
        }
        else if (DarkRef.isOldWidescreen)
        {
            curWidth = oldWideWidth;
        }

        curScale = (scalingEnabled) ? DarkRef.ScaleWithDistance(distance, nearDistance, farDistance, nearScale, farScale) : 1f;

        if (mainCamera == null)
        {
            return;
        }

        if (target != null)
        {
            m_LastPos = target.position;
            pos = GetBasicViewportPosition(target.position);
        }
        else
        {
            pos = GetBasicViewportPosition(m_LastPos);
        }

        Vector3 currentTarget = (target == null) ? m_LastPos : target.position;

        if (pos.z < 0f)
        {
            pos = GetAdvancedViewportPosition(currentTarget, 1.0f, pos);
        }
        else if (pos.x < 0f)
        {
            if (pos.x <= -1.0f)
            {
                pos = GetAdvancedViewportPosition(currentTarget, 1.0f, pos);
            }
            else
            {
                pos = GetAdvancedViewportPosition(currentTarget, 1.0f - (1.0f - Mathf.Abs(pos.x)), pos);
            }
        }
        else if (pos.x > 1.0f)
        {
            if (pos.x >= 2.0f)
            {
                pos = GetAdvancedViewportPosition(currentTarget, 1.0f, pos);
            }
            else
            {
                pos = GetAdvancedViewportPosition(currentTarget, 1.0f - (1.0f - (pos.x - 1.0f)), pos);
            }
        }
        else if (pos.y < 0f)
        {
            if (pos.y <= -1.0f)
            {
                pos = GetAdvancedViewportPosition(currentTarget, 1.0f, pos);
            }
            else
            {
                pos = GetAdvancedViewportPosition(currentTarget, 1.0f - (1.0f - Mathf.Abs(pos.x)), pos);
            }
        }
        else if (pos.y > 2.0f)
        {
            if (pos.y >= 2.0f)
            {
                pos = GetAdvancedViewportPosition(currentTarget, 1.0f, pos);
            }
            else
            {
                pos = GetAdvancedViewportPosition(currentTarget, 1.0f - (1.0f - (pos.y - 1.0f)), pos);
            }
        }

        int markerX = Mathf.RoundToInt(-curWidth + (pos.x * curWidth * 2f));
        int markerY = Mathf.RoundToInt(-normalHeight + (pos.y * normalHeight * 2f));

        rotation = 0f;
        if (pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f)
        {
            markerPos = new Vector2(markerX, markerY);
            curOffset = Vector2.zero;
            curTexture = GUITextures[0];
        }
        else if (pos.x <= 0f)
        {
            if (pos.y <= 0f)
            {
                markerPos = new Vector2(-curWidth, -normalHeight);
                curOffset = new Vector2(edgeOffset.x, edgeOffset.y);
                curTexture = GUITextures[3];
                rotation = 135f;
            }
            else if (pos.y >= 1f)
            {
                markerPos = new Vector2(-curWidth, normalHeight);
                curOffset = new Vector2(edgeOffset.x, -edgeOffset.y);
                curTexture = GUITextures[3];
                rotation = 45f;
            }
            else
            {
                markerPos = new Vector2(-curWidth, Mathf.Clamp(markerY, -normalHeight, normalHeight));
                curOffset = new Vector2(edgeOffset.x, 0f);
                curTexture = GUITextures[1];
            }
        }
        else if (pos.x >= 1f)
        {
            if (pos.y <= 0f)
            {
                markerPos = new Vector2(curWidth, -normalHeight);
                curOffset = new Vector2(-edgeOffset.x, edgeOffset.y);
                curTexture = GUITextures[3];
                rotation = -135f;
            }
            else if (pos.y >= 1f)
            {
                markerPos = new Vector2(curWidth, normalHeight);
                curOffset = new Vector2(-edgeOffset.x, -edgeOffset.y);
                curTexture = GUITextures[3];
                rotation = -45f;
            }
            else
            {
                markerPos = new Vector2(curWidth, Mathf.Clamp(markerY, -normalHeight, normalHeight));
                curOffset = new Vector2(-edgeOffset.x, 0f);
                curTexture = GUITextures[2];
            }
        }
        else if (pos.y <= 0f)
        {
            markerPos = new Vector2(markerX, -normalHeight);
            curOffset = new Vector2(0f, edgeOffset.y);
            curTexture = GUITextures[4];
        }
        else if (pos.y >= 1f)
        {
            markerPos = new Vector2(markerX, normalHeight);
            curOffset = new Vector2(0f, -edgeOffset.y);
            curTexture = GUITextures[3];
        }

        osMod = (pos.x <= 0f || pos.x >= 1f || pos.y <= 0f || pos.y >= 1f) ? 0.35f : 1f;

        if (markerTexture.mainTexture != curTexture)
        {
            markerTexture.mainTexture = curTexture;
        }

        if (pos.x <= textBorder.x)
        {
            pivotPoint = UIWidget.Pivot.Left;
            xText = -12f;
        }
        else if (pos.x >= textBorder.y)
        {
            pivotPoint = UIWidget.Pivot.Right;
            xText = 12f;
        }
        else
        {
            pivotPoint = UIWidget.Pivot.Center;
            xText = 0f;
        }

        if (distanceLabel.pivot != pivotPoint)
        {
            distanceLabel.pivot = pivotPoint;
            descriptionLabel.pivot = pivotPoint;
        }

        if (pos.y <= textBorder.z)
        {
            yText = 32f;
            yText2 = 20f;
        }
        else
        {
            yText = -20f;
            yText2 = -33f;
        }

        offsetReal = Vector2.Lerp(offsetReal, curOffset, Time.unscaledDeltaTime * 5f);

        distanceLabel.cachedTrans.localPosition = Vector3.Lerp(distanceLabel.cachedTrans.localPosition, new Vector3(xText, yText, 0f), Time.unscaledDeltaTime * 10f);
        descriptionLabel.cachedTrans.localPosition = Vector3.Lerp(descriptionLabel.cachedTrans.localPosition, new Vector3(xText, yText2, 0f), Time.unscaledDeltaTime * 10f);

        tr.localPosition = markerPos + offsetReal;

        markerTexture.cachedTrans.localRotation = Quaternion.Slerp(markerTexture.cachedTrans.localRotation, Quaternion.Euler(0f, 0f, rotation), Time.unscaledDeltaTime * 8f);
        aimingAt = (ac != null && ac.isAiming && Mathf.Abs(pos.x - 0.5f) < 0.2f && Mathf.Abs(pos.y - 0.5f) < 0.2f);
    }

    private Vector3 GetBasicViewportPosition(Vector3 target)
    {
        return mainCamera.WorldToViewportPoint(target);
    }

    private Vector3 GetAdvancedViewportPosition(Vector3 targetPos, float interpolationAmount, Vector3 curViewportPos)
    {
        Quaternion dirQuaternion = Quaternion.LookRotation((targetPos - mainCamera.transform.position).normalized);
        float horizAngle = Mathf.DeltaAngle(mainCamera.transform.eulerAngles.y, dirQuaternion.eulerAngles.y);
        float vertAngle = -Mathf.DeltaAngle(mainCamera.transform.eulerAngles.x, dirQuaternion.eulerAngles.x);

        float horizPerc = 0f;
        if (horizAngle < (mainCamera.fieldOfView * mainCamera.aspect) * -0.5f)
        {
            horizPerc = 0f;
        }
        if (horizAngle > (mainCamera.fieldOfView * mainCamera.aspect) * 0.5f)
        {
            horizPerc = 1f;
        }
        else
        {
            horizPerc = (horizAngle + (mainCamera.fieldOfView * mainCamera.aspect) * 0.5f) / (mainCamera.fieldOfView * mainCamera.aspect);
        }

        float vertPerc = 0f;
        if (vertAngle < mainCamera.fieldOfView * -0.5f)
        {
            vertPerc = 0f;
        }
        else if (vertAngle > mainCamera.fieldOfView * 0.5f)
        {
            vertPerc = 1.0f;
        }
        else
        {
            vertPerc = (vertAngle + mainCamera.fieldOfView * 0.5f) / mainCamera.fieldOfView;
        }

        return Vector3.Lerp(curViewportPos, new Vector3(horizPerc, vertPerc, curViewportPos.z), interpolationAmount);
    }

    private void CalculateDistance()
    {
        if (!isEnabled && alphaMod <= 0f || target == null || player == null)
        {
            return;
        }

        Vector3 targetPos = (target == null) ? m_LastPos : target.position;
        distance = Vector3.Distance(player.position, targetPos);
        distanceLabel.text = distance.ToString("F0") + "m";
        tr.localScale = baseScale * curScale;
    }

    public void SetDescription(string text)
    {
        descriptionLabel.text = text;
    }
}