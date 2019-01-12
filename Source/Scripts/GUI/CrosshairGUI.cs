using UnityEngine;
using System.Collections;

public class CrosshairGUI : MonoBehaviour
{
    public UISprite leftCross;
    public UISprite rightCross;
    public UISprite upCross;
    public UISprite downCross;
    public AudioClip hitSound;

    public int defaultSpacing = 30;
    public float guiBaseOffset = 8f;
    public float opacity = 0.75f;
    public float offsetMultiplier = 5f;

    [HideInInspector] public float realSpacing;
    [HideInInspector] public float hitIndicatorOffset;

    private bool isEnabled;
    private UISprite[] crossSprites;

    private PlayerMovement pm;
    private AimController ac;

    private WeaponManager wm;
    private DynamicMovement dm;
    private AntiClipSystem acs;
    private PlayerEffects pe;

    private CrosshairStyle crossStyle;
    private float baseSpread;
    private float growthSpreadAnim;
    private float joltAnim;

    void Start()
    {
        isEnabled = true;

        crossSprites = new UISprite[4] { leftCross, rightCross, upCross, downCross };
        SetReferenceVars();
        baseSpread = defaultSpacing;
    }

    public void SetReferenceVars()
    {
        PlayerReference pRef = GeneralVariables.playerRef;

        if (pRef != null)
        {
            pe = pRef.GetComponent<PlayerEffects>();
            wm = pRef.wm;
            dm = pRef.dm;
            pm = pRef.GetComponent<PlayerMovement>();
            ac = pRef.ac;
            acs = pRef.acs;
        }
    }

    void Update()
    {
        if (GeneralVariables.playerRef == null)
        {
            foreach (UISprite sprite in crossSprites)
            {
                sprite.alpha = 0f;
            }

            return;
        }

        crossStyle = GameSettings.settingsController.crossStyle;
        float empDistortCrosshair = (pe.hasEMP) ? (Mathf.PerlinNoise(Mathf.PingPong(Time.time * 25f, 200f), 0f) * 35f) : 0f;

        if (crossStyle == CrosshairStyle.Dynamic)
        {
            GunController curGun = wm.currentGC;

            if (curGun != null)
            {
                if (curGun.crosshairsEnabled && !pm.sprinting && !dm.terminalVelocity)
                {
                    isEnabled = (!ac.isAiming && !curGun.reloading && !acs.clipping && !pm.onLadder);

                    if (!ac.isTransitioning && !ac.isAiming)
                    {
                        baseSpread = curGun.spreadReal * offsetMultiplier;
                        baseSpread *= (70f / GameSettings.settingsController.FOV);
                    }
                }
                else
                {
                    isEnabled = false;
                }
            }
            else
            {
                isEnabled = !pm.sprinting;
                baseSpread = defaultSpacing;
            }

            if (isEnabled && !pm.sprinting && !dm.animationIsPlaying)
            {
                growthSpreadAnim = Mathf.Lerp(growthSpreadAnim, 0f, Time.deltaTime * 9f);
            }
            else
            {
                growthSpreadAnim = Mathf.Lerp(growthSpreadAnim, 30f, Time.deltaTime * 9f);
            }

            joltAnim = Mathf.Lerp(joltAnim, 0f, Time.deltaTime * 7f);
        }
        else if (crossStyle == CrosshairStyle.Static)
        {
            isEnabled = !pm.sprinting && !ac.isAiming;
            baseSpread = defaultSpacing;
            growthSpreadAnim = 0f;
            joltAnim = 0f;
        }
        else
        {
            foreach (UISprite sprite in crossSprites)
            {
                sprite.alpha = 0f;
            }

            baseSpread = defaultSpacing;
            growthSpreadAnim = 0f;
            joltAnim = 0f;
            return;
        }

        realSpacing = (baseSpread + guiBaseOffset) + joltAnim + growthSpreadAnim + empDistortCrosshair;
        hitIndicatorOffset = realSpacing - ((!ac.isAiming) ? growthSpreadAnim : 0f);

        float xEMP = ((pe.hasEMP) ? (1f - Random.value * 2f) * 75f * Random.value : 0f);
        float yEMP = ((pe.hasEMP) ? (1f - Random.value * 2f) * 75f * Random.value : 0f);
        leftCross.cachedTrans.localPosition = new Vector3(-realSpacing + xEMP, yEMP, 0f);
        rightCross.cachedTrans.localPosition = new Vector3(realSpacing + xEMP, yEMP, 0f);
        upCross.cachedTrans.localPosition = new Vector3(xEMP, realSpacing + yEMP, 0f);
        downCross.cachedTrans.localPosition = new Vector3(xEMP, -realSpacing + yEMP, 0f);

        float alpha = (isEnabled && !dm.animationIsPlaying && !RestrictionManager.restricted) ? opacity : 0f;
        foreach (UISprite sprite in crossSprites)
        {
            if (pe.hasEMP)
            {
                sprite.alpha = Random.value * alpha;
            }
            else
            {
                sprite.alpha = Mathf.MoveTowards(sprite.alpha, alpha, Time.unscaledDeltaTime * opacity * 10f);
            }
        }
    }

    public void JoltAnimation(float amount)
    {
        joltAnim += amount * 8f;
    }

    public void ShowHitMarker(bool targetDead)
    {
        if (pe.hasEMP)
            return;

        NGUITools.PlaySound(hitSound, (targetDead) ? 1f : 0.4f, Random.Range(0.9f, 1f) * ((targetDead) ? 0.8f : 1f));
    }
}