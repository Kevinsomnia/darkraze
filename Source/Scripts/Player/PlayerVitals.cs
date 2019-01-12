using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerVitals : BaseStats
{
    [System.Serializable]
    public class HitIndicatorInfo
    {
        public GameObject instance;
        public float lifetime = 1.5f;
        public Vector3 hitFromPosition;

        [HideInInspector] public float initTime = 0f;
        [HideInInspector] public bool queueDestroy = false;
        [HideInInspector] public float impulseCurrent = 1f;
        [HideInInspector] public float impulseTarget = 1f;
    }

    public GameObject cam;

    private int _curshield = 40;
    public int curShield
    {
        get
        {
            if (Application.isPlaying)
            {
                _curshield = AntiHackSystem.RetrieveInt("curShield");
            }

            return _curshield;
        }
        set
        {
            _curshield = value;

            if (Application.isPlaying)
            {
                AntiHackSystem.ProtectInt("curShield", _curshield);
            }
        }
    }

    public int maxShield = 40;
    public float healthRecoverDelay = 6f;
    public float healthRecoverySpeed = 3f;
    public float healthRecoverInfluence = 2.5f;
    public int healthRecoverAmount = 1;
    public float shieldRecoverDelay = 7f;
    public float shieldRecoverySpeed = 0.2f;
    public int shieldRecoverAmount = 1;
    public float effectIntensity = 4f;
    public float fallDamageTolerance = 8f;
    public float fallDamageCurve = 2f;
    public float staminaRecoverySpeed = 0.3f;
    public float staminaDepletionRate = 0.2f;
    public GameObject arms;
    public GameObject deathReplacement;
    public AudioLowPassFilter healthLowPass;
    public AudioSource breathingSound;
    public AudioSource heartbeatSound;
    public AudioSource fallDamageSource;
    public AudioSource shieldAlarmSource;
    public AudioSource equipmentRattleSource;
    public AudioSource noiseSource;

    //Access these through debug inspector.
    public Vector2 muffleRange = new Vector2(3000f, 5000f);
    public float empMuffle = 3000f;
    public float rattleVolumeSprint = 0.125f;
    public float rattleVolumeNormal = 0.08f;
    public float dmgBlurRestore = 2.5f;
    public Color hurtColor = new Color(1f, 0.2f, 0f, 1f);
    public PlayerEffects pe;
    public GameObject cachedSpectCam;
    public GameObject hitIndicatorPrefab;
    public float hitIndicatorRotRound = 15f;
    public Material dissolveMat;

    public AudioClip[] painSounds;
    public AudioClip deathSound;
    public AudioClip fallDamageSound;
    public AudioClip noBreathExhale;
    public AudioClip healthDamage;
    public AudioClip shieldDamage;
    public AudioClip shieldFailure;
    public AudioClip shieldRegen;

    [HideInInspector] public int curStamina = 100;
    [HideInInspector] public bool canSprint;
    [HideInInspector] public float aimEffect;
    [HideInInspector] public bool dead;
    [HideInInspector] public bool jumpRattleEquip = false;
    [HideInInspector] public float distortEMP;
    [HideInInspector] public float grainEMP;
    [HideInInspector] public float vignetteEMP;
    [HideInInspector] public float rTimer = 0f;
    [HideInInspector] public float shrTimer = 0f;
    [HideInInspector] public float hearingPenalty = 1f;
    [HideInInspector] public bool shRecovering;
    [HideInInspector] public bool startShRecoveryTimer;

    private PlayerLook playerLook;
    private WeightController wc;
    private VignettingC vignetting;
    private ScreenAdjustment sa;
    private NoiseEffect ne;
    private DistortionEffect disE;
    private bool staminaBlinking = false;
    private bool recovering;

    private int fallDamageTotal;
    private float damageBreathBoost;
    private float dBreath;
    private float percent;
    private float shPercent;
    private float imageEffect;
    private float timer;
    private float shTimer;
    private float dTimer;
    private float damageEffect;
    private float chromAbb;
    private float sTimer;
    private float lastDe;
    private float bloodEffect;
    private float lastHitIndicate;
    private float standardFreq;

    private float guiSizeModH;
    private float guiSizeModS;
    private float lastUpdateTime;

    private bool staminaCooldown;
    private bool startRecoveryTimer;
    private bool shGlowRecover;
    private bool grenade;
    private float painTimer;
    private float lastDamage;
    private int oldHealth;
    private int oldShield;

    private float lastHeartTime;
    private float hbEffectTarget;
    private float heartbeatEffect;
    private float sBlinkTimer;
    private float sBlinkValue;
    private float alpha;
    private float rattleTimer;
    private float shieldAlpha;
    private float finalShAlpha;
    private float initTime;
    private float flickerIntensity;
    private float breathFactor;

    private PlayerMovement pm;
    private WeaponManager wm;
    private TimeScaleSound rattleTSS;

    private UIController uic;
    private UISlider healthBar;
    private UILabel healthText;
    private UISlider shieldBar;
    private UILabel shieldText;
    private UISlider staminaBar;
    private UIWidget staminaBackground;
    private UITexture shieldTexture;
    private Color defStaminaBGCol;
    private MeshRenderer bloodyScreen;
    private FlickeringGUI[] flickeringGUI;
    private List<HitIndicatorInfo> hitIndicators = new List<HitIndicatorInfo>();

    private string builtData = "";
    private List<byte> damageIDs;
    private List<int> damageInflicted;
    private int killerID = -1;
    private int headID = -1;
    private int lastWeaponID = -1;

    public static bool godmode = false;

    void Start()
    {
        base.isLocalPlayer = true;
        GeneralVariables.cachedSpectCam = cachedSpectCam;
        uic = GeneralVariables.uiController;
        rattleTSS = equipmentRattleSource.GetComponent<TimeScaleSound>();

        healthBar = uic.healthBar;
        healthText = uic.healthText;
        shieldBar = uic.shieldBar;
        shieldText = uic.shieldText;
        staminaBar = uic.staminaBar;
        shieldTexture = uic.shieldTexture;
        ne = uic.guiCamera.GetComponent<NoiseEffect>();
        disE = uic.guiCamera.GetComponent<DistortionEffect>();
        staminaBackground = staminaBar.backgroundWidget;
        defStaminaBGCol = staminaBackground.color;
        bloodyScreen = uic.bloodyScreen;
        flickeringGUI = uic.flickeringPanels;
        sa = uic.screenAdjustment;

        bloodyScreen.material.color = DarkRef.SetAlpha(bloodyScreen.material.color, 0f);

        PlayerReference pr = GeneralVariables.playerRef;
        pm = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
        vignetting = cam.GetComponent<VignettingC>();

        damageBreathBoost = 0f;
        ne.grainIntensity = 0f;
        disE.baseIntensity = 0f;
        guiSizeModH = guiSizeModS = 1f;
        healthBar.value = 0f;
        shieldBar.value = 0f;
        breathFactor = 1f;
        hearingPenalty = 1f;
        standardFreq = 5000f;

        wm = pr.wm;
        wc = pr.wc;

        shieldAlpha = 0f;
        finalShAlpha = 0f;
        flickerIntensity = 0.5f;
        percent = 1f;
        shPercent = 1f;
        recovering = true;
        canSprint = true;
        initTime = Time.time;

        damageIDs = new List<byte>();
        damageInflicted = new List<int>();
        killerID = -1;
        headID = -1;
        lastWeaponID = -1;
        builtData = "";

        //Initialize values.
        curHealth = maxHealth;
        curShield = maxShield;
        AntiHackSystem.ProtectInt("maxHealth", maxHealth);
        AntiHackSystem.ProtectInt("maxShield", maxShield);

        oldHealth = curHealth;
        oldShield = curShield;
    }

    void Update()
    {
        int retrievedMaxHealth = AntiHackSystem.RetrieveInt("maxHealth");
        int retrievedMaxShield = AntiHackSystem.RetrieveInt("maxShield");

        if (GameManager.boundarySettings != null)
        {
            bool inMapBounds = GameManager.boundarySettings.mapBounds.Contains(transform.position);
            if (!inMapBounds)
            {
                ApplyDamageMain((curHealth + curShield) * 2, false);
            }
        }

        if (Input.GetKey(KeyCode.X))
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                ApplyDamageMain(Random.Range(10, 15), true);
                HitIndicator(transform.position + DarkRef.RandomVector3(Vector3.one * -5f, Vector3.one * 5f));
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                ApplyDamageMain(retrievedMaxHealth + retrievedMaxShield + 1, false);
            }
        }

        AdjustGUISize();
        ManageIndicatorGUI();

        if (recovering && curHealth < retrievedMaxHealth)
        {
            timer += Time.deltaTime;
        }

        if (shRecovering)
        {
            if (curShield < retrievedMaxShield)
            {
                shTimer += Time.deltaTime;
            }

            shieldAlarmSource.volume = 0f;

            if (!shGlowRecover)
            {
                shieldTexture.color = shieldTexture.defaultColor;
                shieldAlpha = 0.21f;
                shGlowRecover = true;
            }
        }
        else
        {
            shGlowRecover = false;
            if (retrievedMaxShield > 0f)
            {
                if (pe.hasEMP)
                {
                    shieldAlarmSource.volume = 0f;
                }
                else
                {
                    shieldAlarmSource.volume = Mathf.Clamp01(0.5f - shPercent) * 0.12f;
                }
            }
        }

        if (Time.time - lastDamage > 0.3f)
        {
            Color lerpHurtColor = Color.Lerp(hurtColor, Color.white, Mathf.Clamp01((percent - 0.2f) * 3.5f));
            healthText.color = Color.Lerp(healthText.color, lerpHurtColor, Time.unscaledDeltaTime * 5f);
            Color lerpShieldColor = Color.Lerp(new Color(0.8f, 0.8f, 0.8f, 1f), Color.white, Mathf.Clamp01(shPercent * 5f));
            shieldText.color = Color.Lerp(shieldText.color, lerpShieldColor, Time.unscaledDeltaTime * 5f);
        }

        curShield = Mathf.Clamp(curShield, 0, retrievedMaxShield);
        float fallDmgMod = (fallDamageTotal > 0) ? 0.8f : 1f;

        if (retrievedMaxShield > 0)
        {
            shPercent = (float)curShield / (float)retrievedMaxShield;

            float shRecoverRate = (shieldRecoverySpeed * fallDmgMod);
            if (shTimer >= shRecoverRate && (curShield < retrievedMaxShield))
            {
                curShield += shieldRecoverAmount;

                if (fallDamageTotal > 0)
                {
                    fallDamageTotal--;
                }

                shTimer -= shRecoverRate;
            }
        }
        else
        {
            shPercent = 0f;
        }

        curHealth = Mathf.Clamp(curHealth, 0, retrievedMaxHealth);
        maxHealth = retrievedMaxHealth;
        maxShield = retrievedMaxShield;
        curStamina = Mathf.Clamp(curStamina, 0, 100);

        float recoverRate = (healthRecoverySpeed * (1f + (percent * healthRecoverInfluence)) * fallDmgMod);
        if (timer >= recoverRate && (curHealth < retrievedMaxHealth))
        {
            curHealth += healthRecoverAmount;

            if (fallDamageTotal > 0)
            {
                fallDamageTotal--;
            }

            timer -= recoverRate;
        }

        if (!dead)
        {
            if (startRecoveryTimer)
            {
                rTimer += Time.deltaTime;
            }

            if (startShRecoveryTimer)
            {
                shrTimer += Time.deltaTime;
            }
        }

        if (rTimer >= healthRecoverDelay && !dead)
        {
            startRecoveryTimer = false;
            rTimer = 0f;
            recovering = true;
        }

        if (maxShield > 0 && shrTimer >= shieldRecoverDelay && !dead)
        {
            startShRecoveryTimer = false;
            shrTimer = 0f;
            shRecovering = true;

            fallDamageSource.GetComponent<TimeScaleSound>().pitchMod = 1f;
            fallDamageSource.PlayOneShot(shieldRegen, 0.1f);
        }

        float steepSlopeFactor = (pm.grounded) ? Mathf.Clamp01(pm.controller.velocity.normalized.y) : 0f;
        if (pm.grounded && (pm.sprinting || pm.sprintReloadBoost > 1f) && pm.xyVelocity >= 0.75f)
        {
            rattleTSS.pitchMod = 1f;
            equipmentRattleSource.volume = Mathf.Lerp(equipmentRattleSource.volume, rattleVolumeSprint, Time.deltaTime * 9f);
            dTimer += Time.deltaTime * (pm.controllerVeloMagn / pm.movement.sprintSpeed);

            float requirement = staminaDepletionRate * (1f - (wc.weightPercentage * 0.8f)) * (1f - (steepSlopeFactor * 0.22f));
            if (dTimer >= requirement && curStamina > 0f)
            {
                curStamina -= 1 + Mathf.RoundToInt((1f / requirement) * Time.deltaTime);
                dTimer = 0f;
            }
        }
        else
        {
            float velocityFactor = Mathf.Clamp01(pm.xyVelocity / pm.movement.runSpeed);
            if (jumpRattleEquip)
            {
                rattleTimer += Time.deltaTime;

                if (pm.xyVelocity < 0.75f)
                {
                    rattleTSS.pitchMod = 1f;
                    equipmentRattleSource.volume = Mathf.Lerp(equipmentRattleSource.volume, Mathf.Lerp(rattleVolumeNormal, rattleVolumeSprint, 0.5f), Time.deltaTime * 11f);
                }

                if (rattleTimer >= 0.4f)
                {
                    rattleTimer = 0f;
                    jumpRattleEquip = false;
                }
            }
            else
            {
                if (pm.grounded)
                {
                    if (pm.xyVelocity >= 0.75f)
                    {
                        rattleTSS.pitchMod = (pm.crouching) ? 0.8f : 0.96f;
                        equipmentRattleSource.volume = Mathf.Lerp(equipmentRattleSource.volume, rattleVolumeNormal * velocityFactor, Time.deltaTime * 9f);
                    }
                    else
                    {
                        equipmentRattleSource.volume = Mathf.Lerp(equipmentRattleSource.volume, 0f, Time.deltaTime * 9f);
                    }
                }
                else
                {
                    equipmentRattleSource.volume = Mathf.Lerp(equipmentRattleSource.volume, 0f, Time.deltaTime * 9f);
                }
            }

            sTimer += Time.deltaTime;

            float requirement = (staminaRecoverySpeed * (2f - Mathf.Clamp(Time.time - lastDe, 0f, 1f)) * (1f + Mathf.Clamp01((pm.xyVelocity * 0.17f) / pm.movement.runSpeed)) + (steepSlopeFactor * 0.16f));
            if (sTimer >= requirement && curStamina < 100 && !staminaCooldown)
            {
                curStamina += 1 + Mathf.RoundToInt((1f / requirement) * Time.deltaTime);
                sTimer = 0f;
            }
        }

        curStamina = Mathf.Clamp(curStamina, 0, 100);

        if (curStamina <= 1f && canSprint)
        {
            StartCoroutine(CalmingStage());
        }

        shieldAlpha = Mathf.Clamp(shieldAlpha, 0f, 0.35f);
        shieldAlpha = Mathf.MoveTowards(shieldAlpha, 0f, Time.deltaTime * 0.6f);
        finalShAlpha = Mathf.Lerp(finalShAlpha, shieldAlpha, Time.deltaTime * 8.5f);

        shieldTexture.alpha = finalShAlpha * ((shGlowRecover) ? 1f : (0.3f + (Mathf.PerlinNoise(Mathf.PingPong(Time.time * 22f, 250f), 0f) * 0.7f)));
        damageEffect = Mathf.Lerp(damageEffect, 0f, (Time.time - lastDe) * 5f);
        chromAbb = Mathf.Lerp(chromAbb, Mathf.Clamp(damageEffect, 0f, 30f) + ((0.3f - Mathf.Min(percent, 0.3f)) * 25f), Time.deltaTime * 5f);

        bloodEffect = Mathf.Clamp01(Mathf.Lerp(bloodEffect, 0f, Time.deltaTime * 0.068f));
        uic.gManager.damageBlur = Mathf.Clamp(Mathf.Lerp(uic.gManager.damageBlur, 0f, Time.deltaTime * dmgBlurRestore) + ((percent <= 0.2f) ? 0.001f : 0f), 0f, 0.85f);

        foreach (FlickeringGUI fg in flickeringGUI)
        {
            if (pe.hasEMP)
            {
                fg.dimAlpha = Random.Range(0.1f, 1f);
                fg.updateFrequency = 0.03f;
                fg.flickerFrequency = 0.8f;
            }
            else
            {
                fg.dimAlpha = Mathf.Clamp01(0.3f + flickerIntensity * 2f);
                fg.updateFrequency = 0.05f;
                fg.flickerFrequency = 0.85f;
            }
        }

        float stPercent = Mathf.Clamp01((float)curStamina / 100f);
        float hbVolume = Mathf.Clamp(0.5f - percent, 0f, 0.5f) * 0.3f;
        heartbeatSound.volume = hbVolume + ((1f - stPercent) * 0.05f);

        float nVolume = Mathf.Clamp(0.25f - percent, 0f, 0.25f) / 0.25f;
        noiseSource.volume = nVolume * 0.2f;

        if (Time.time - lastHeartTime >= 1f)
        {
            hbEffectTarget = 3.1f;
            lastHeartTime = Time.time;
        }

        if (pe.hasEMP)
        {
            grainEMP = Mathf.Lerp(grainEMP, 0.035f, Time.deltaTime * 1.3f);
            distortEMP = Mathf.Lerp(distortEMP, 0.15f, Time.deltaTime * 1.3f);
        }
        else
        {
            grainEMP = Mathf.Lerp(grainEMP, 0f, Time.deltaTime * 3f);
            distortEMP = Mathf.Lerp(distortEMP, 0f, Time.deltaTime * 3f);
        }

        vignetteEMP = Mathf.Lerp(vignetteEMP, 0f, Time.deltaTime * 4f);
        ne.empEffect = grainEMP;

        hbEffectTarget = Mathf.MoveTowards(hbEffectTarget, 0f, Time.deltaTime * 4.3f);
        heartbeatEffect = Mathf.Lerp(heartbeatEffect, hbEffectTarget + 0.05f, Time.deltaTime * 11f);

        percent = curHealth / (float)retrievedMaxHealth;
        flickerIntensity = Mathf.Lerp(flickerIntensity, percent, Time.deltaTime * 0.5f * Mathf.Clamp((Time.time - initTime) * 0.5f, 0f, 4f));
        hearingPenalty = Mathf.Lerp(hearingPenalty, 1f, Time.deltaTime * 0.2f);

        if (percent <= 0.25f)
        {
            standardFreq = Mathf.Lerp(standardFreq, (pe.hasEMP) ? empMuffle : Mathf.Lerp(muffleRange.x, muffleRange.y, Mathf.Clamp01(percent * 4f)), Time.deltaTime * 9f);
            ne.grainIntensity = Mathf.Lerp(0f, 0.0075f, 1f - Mathf.Clamp01(percent * 4f));
        }
        else
        {
            standardFreq = Mathf.Lerp(standardFreq, (pe.hasEMP) ? empMuffle : 20000f, Time.deltaTime * ((pe.hasEMP) ? 3.5f : 0.7f));
            ne.grainIntensity = Mathf.Lerp(ne.grainIntensity, 0f, Time.deltaTime * 5f);
        }

        healthLowPass.cutoffFrequency = standardFreq * hearingPenalty;

        ne.enabled = (ne.grainIntensity + grainEMP > 0f);
        disE.enabled = (ne.enabled || distortEMP > 0f);
        disE.baseIntensity = (ne.grainIntensity * 4f) + distortEMP;
        disE.splitOffset = (pe.hasEMP) ? 0.05f : 0f;

        if (staminaBlinking)
        {
            sBlinkTimer += Time.deltaTime * 6.75f;
            sBlinkValue = Mathf.Sin(sBlinkTimer);

            Color redCol = defStaminaBGCol;
            redCol.r *= 3f;
            staminaBackground.color = Color.Lerp(defStaminaBGCol, redCol, sBlinkValue);
        }
        else
        {
            sBlinkTimer = 0f;
            sBlinkValue = 0f;

            staminaBackground.color = defStaminaBGCol;
        }

        damageBreathBoost = Mathf.Clamp(Mathf.MoveTowards(damageBreathBoost, 0f, Time.deltaTime * 0.05f), 0f, 0.42f);
        dBreath = Mathf.Lerp(dBreath, damageBreathBoost, Time.deltaTime * 7f);
        breathingSound.volume = (0.05f + (hbVolume * 0.11f) + (0.148f * (1f - stPercent)) + dBreath) * breathFactor;
        breathingSound.GetComponent<TimeScaleSound>().pitchMod = 1f + (hbVolume * 0.03f) + (0.145f * (1f - stPercent));

        imageEffect = Mathf.Lerp(imageEffect, Mathf.Clamp01(1f - (percent + 0.45f)) * effectIntensity, Time.deltaTime * 3f);
        vignetting.intensity = imageEffect + vignetteEMP + (heartbeatEffect * Mathf.Clamp01(1f - (percent * 2f)));
        vignetting.blur = (imageEffect * 0.2f) + aimEffect;
        vignetting.blurSpread = aimEffect * 0.5f;
        vignetting.heartbeatBlur = heartbeatEffect * Mathf.Clamp01(1f - (percent * 2f)) * 0.45f;
        vignetting.chromaticAberration = chromAbb * (1f + ((1 - percent) * 0.3f));

        float saturation = Mathf.Clamp01(0.45f + (percent * 0.88f));
        sa.saturationAmount = Mathf.Lerp(sa.saturationAmount, (pe.hasEMP) ? Random.value * Random.value : saturation, Time.deltaTime * 4f);
        sa.colorTint = Vector4.Lerp(new Vector4(1.1f, 0.95f, 0.95f, 1f), Vector4.one, Mathf.Clamp01(percent * 2.5f));

        if (!dead)
        {
            alpha = Mathf.Lerp(alpha, bloodEffect + ((1 - percent) * 0.15f), Time.deltaTime * 8f);
            bloodyScreen.material.color = DarkRef.SetAlpha(bloodyScreen.material.color, alpha * 0.82f);
        }

        healthBar.value = Mathf.Lerp(healthBar.value, (pe.hasEMP) ? Random.value : percent, Time.unscaledDeltaTime * 7.5f);
        shieldBar.value = Mathf.Lerp(shieldBar.value, (pe.hasEMP) ? Random.value : shPercent, Time.unscaledDeltaTime * 7.5f);

        if (pe.hasEMP)
        {
            if (Time.time - lastUpdateTime >= 0.1f)
            {
                healthText.text = (dead) ? "INACTIVE" : (Random.Range(0, 999) + "/" + Random.Range(0, 999));
                lastUpdateTime = Time.time;
            }
        }
        else
        {
            healthText.text = (dead) ? "INACTIVE" : (curHealth + "/" + retrievedMaxHealth);
        }

        if (shrTimer < 0f || maxShield <= 0)
        {
            shieldText.color = new Color(1f, 0.4f, 0.2f);
            shieldText.text = "DISABLED";
        }
        else
        {
            shieldText.color = shieldText.defaultColor;
            shieldText.text = curShield + "/" + retrievedMaxShield;
        }

        staminaBar.value = Mathf.Lerp(staminaBar.value, (pe.hasEMP) ? Random.value : stPercent, Time.deltaTime * 8f);
    }

    [RPC]
    public override void NetworkEMP()
    {
        pe.StartPhase_EMP();
    }

    [RPC]
    public override void ApplyDamageNetwork(byte damage, byte senderID, byte weaponID, byte bodyPart)
    {
        if (Time.time - initTime <= 2f || GeneralVariables.Networking.finishedGame)
        {
            return;
        }

        Limb.LimbType limb = (Limb.LimbType)Mathf.Clamp(bodyPart, 0, 4);
        ApplyDamage(damage, senderID, true, limb, weaponID + ((bodyPart > 4) ? 1000 : 0));
    }

    public override void ApplyDamageMain(int damage, bool showBlood, Limb.LimbType bodyPart)
    {
        ApplyDamage(damage, -1, showBlood, bodyPart);
    }

    private void ApplyDamage(int damage, int senderID, bool showBlood, Limb.LimbType bodyPart, int weaponID = -1)
    {
        if (dead || damage <= 0)
        {
            return;
        }

        if (Topan.Network.isConnected && (!Topan.Network.HasServerInfo("sm") || !((bool)Topan.Network.GetServerInfo("sm"))))
        {
            return;
        }

        if (GeneralVariables.gameModeHasTeams && senderID > -1)
        {
            byte teamNum = (senderID >= 64) ? BotManager.allBotPlayers[senderID - 64].team : (byte)Topan.Network.GetPlayerByID(senderID).GetPlayerData("team", (byte)0);

            if (teamNum != (byte)Topan.Network.player.GetPlayerData("team", (byte)0))
            {
                if (!damageIDs.Contains((byte)senderID))
                {
                    damageIDs.Add((byte)senderID);
                    damageInflicted.Add(Mathf.Clamp(damage, 0, curHealth));
                }
                else
                {
                    int theIndex = GetDamageIndex(senderID);
                    if (theIndex >= 0)
                    {
                        damageInflicted[theIndex] += Mathf.Clamp(damage, 0, curHealth);
                    }
                }
            }
        }

        float fltDamage = (float)damage;

        if (playerLook != null)
        {
            playerLook.Flinch(fltDamage);
        }

        int damageToHealth = Mathf.Clamp(damage - curShield, 0, curHealth);
        if (damageToHealth > 0)
        {
            base.headshot = (bodyPart == Limb.LimbType.Head);
            shieldAlpha += Mathf.Clamp(damageToHealth, 0, 15) * 0.006f;

            if (showBlood)
            {
                fallDamageSource.GetComponent<TimeScaleSound>().pitchMod = Random.Range(0.75f, 1f);
                fallDamageSource.PlayOneShot(healthDamage, 0.15f + (0.006f * Mathf.Clamp(damageToHealth, 0f, 15f)));
            }
        }

        int damageToShield = Mathf.Clamp(damage, 0, curShield);
        if (damageToShield > 0)
        {
            Color hurtColor = shieldTexture.defaultColor;
            hurtColor *= 0.8f;
            hurtColor.r *= 1.6f;
            hurtColor.b /= 1.6f;
            shieldTexture.color = hurtColor;
            shieldAlpha += 0.021f + (Mathf.Clamp(damageToShield, 0, 10) * 0.039f);
        }

        if (damage > curShield)
        {
            int temp = damage - curShield;

            if (curShield > 0)
            {
                fallDamageSource.GetComponent<TimeScaleSound>().pitchMod = Random.Range(0.8f, 0.85f);
                fallDamageSource.PlayOneShot(shieldDamage, 0.08f);
                fallDamageSource.PlayOneShot(shieldFailure, 0.35f);
            }

            curShield = 0;

            healthText.color = new Color(1f, 0.2f, 0f, 1f);
            curHealth -= temp * ((godmode) ? -1 : 1);

            uic.gManager.damageBlur += Mathf.Clamp(fltDamage * 0.032f, 0f, 0.37f);
            uic.gManager.damageBlur = Mathf.Clamp01(uic.gManager.damageBlur);
            damageBreathBoost = Mathf.Clamp(fltDamage * 0.04f, 0f, 0.22f);

            if (showBlood)
            {
                bloodEffect += 0.003f + (temp * 0.028f);
            }
            if (Time.time - painTimer >= 0.5f)
            {
                fallDamageSource.GetComponent<TimeScaleSound>().pitchMod = Random.Range(0.8f, 1f);
                fallDamageSource.PlayOneShot(painSounds[Random.Range(0, painSounds.Length)], (1f - Mathf.Clamp01(curHealth / (maxHealth * 0.5f))) * 0.18f);
                painTimer = Time.time;
            }

            recovering = false;
            startRecoveryTimer = true;
            rTimer = 0f;
        }
        else
        {
            curShield -= damage;
            uic.gManager.damageBlur += Mathf.Clamp(fltDamage * 0.01f, 0f, 0.11f);
            uic.gManager.damageBlur = Mathf.Clamp01(uic.gManager.damageBlur);
            shieldText.color = new Color(1f, 0.2f, 0f, 1f);

            if (damageToShield > 0)
            {
                fallDamageSource.GetComponent<TimeScaleSound>().pitchMod = Random.Range(0.77f, 0.84f);
                fallDamageSource.PlayOneShot(shieldDamage, 0.03f + (0.009f * Mathf.Clamp(damageToShield, 0, 15)));
            }
        }

        lastDe = Time.time;
        damageEffect += fltDamage * 0.5f;
        lastDamage = lastDe;
        shRecovering = false;
        shTimer = 0f;
        startShRecoveryTimer = true;

        if (shrTimer > 0f)
        {
            shrTimer = 0f;
        }

        if (curHealth <= 0)
        {
            if (!damageIDs.Contains((byte)senderID))
            {
                damageIDs.Add((byte)senderID);
                damageInflicted.Add(9);
            }

            lastWeaponID = weaponID;
            killerID = senderID;
            headID = (base.headshot) ? senderID : -1;
            Die();
        }
    }

    public void FallDamage(float hgt)
    {
        if (hgt > fallDamageTolerance)
        {
            int finalDmg = Mathf.RoundToInt(Mathf.Pow(hgt - fallDamageTolerance, fallDamageCurve));

            if (finalDmg > 0)
            {
                fallDamageSource.transform.localPosition = new Vector3(Random.Range(-1f, 1f), -1f, 0f);
                fallDamageSource.PlayOneShot(fallDamageSound, finalDmg * 0.002f);

                pm.fDmgSpeedMult -= (Mathf.Clamp01((finalDmg - curShield) / (maxHealth * 0.6f)) * 0.4f);
                fallDamageTotal += finalDmg;

                ApplyDamageMain(finalDmg, false);
            }
        }
    }

    private void Die()
    {
        Transform specTarget = null;

        if (Topan.Network.isConnected)
        {
            bool isSuicide = (killerID <= -1);
            builtData = Topan.Network.player.id.ToString() + ((!isSuicide) ? "," : "");

            if (!isSuicide)
            {
                for (int i = 0; i < damageIDs.Count; i++)
                {
                    builtData += damageIDs[i].ToString();

                    if (damageIDs.Count > 1 && damageIDs[i] == killerID)
                    {
                        builtData += "k";
                    }

                    if (damageIDs[i] == headID)
                    {
                        builtData += "!";
                    }

                    if (i < damageIDs.Count - 1)
                    {
                        builtData += ",";
                    }
                }

                builtData += ".";

                if (damageIDs.Count > 1)
                {
                    for (int i = 0; i < damageInflicted.Count; i++)
                    {
                        builtData += damageInflicted[i].ToString();

                        if (i < damageIDs.Count - 1)
                        {
                            builtData += ",";
                        }
                    }
                }

                if (lastWeaponID > -1)
                {
                    grenade = false;
                    if (lastWeaponID >= 1000)
                    {
                        lastWeaponID -= 1000;
                        grenade = true;
                    }

                    builtData += "." + lastWeaponID.ToString() + ((grenade) ? "*" : "");
                }
            }

            if (wm.currentGC != null)
            {
                string gunName = wm.currentGC.gunName;
                Dictionary<string, object> dropData = new Dictionary<string, object>();
                dropData.Add("chamber", wm.currentGC.bulletInChamber);

                Vector3 playerVelo = Vector3.zero;
                if (pm != null)
                {
                    playerVelo = (pm.controller.velocity * 0.25f);
                }

                dropData.Add("force", wm.transform.forward + playerVelo);
                dropData.Add("curammo", wm.currentGC.currentAmmo);
                dropData.Add("ammoleft", wm.currentGC.ammoLeft);
                Topan.Network.Instantiate(Topan.Network.server, "Weapons/" + gunName, wm.transform.position + (wm.transform.forward * 0.5f), wm.currentGC.transform.rotation, 0, dropData);
            }

            GetComponent<Topan.NetworkView>().Deallocate();

            if (killerID > -1)
            {
                if (killerID >= 64)
                {
                    specTarget = GeneralVariables.Networking.botInstances[killerID - 64];
                }
                else if (killerID >= 0)
                {
                    specTarget = ((Topan.NetworkView)Topan.Network.GetPlayerByID(killerID).GetLocalData("currentView")).transform;
                }

                if (specTarget != null)
                {
                    GeneralVariables.spectatorCamera.playerFollow.startingPosition = transform.position;
                    GeneralVariables.spectatorCamera.playerFollow.offset = Vector3.up * 0.55f;
                    GeneralVariables.spectatorCamera.target = specTarget.GetChild(1);
                }
            }

            GeneralVariables.spectatorCamera.playerFollow.followRotation = true;
            GeneralVariables.Networking.GetComponent<Topan.NetworkView>().RPC(Topan.RPCMode.Server, "KilledPlayer", builtData);
            GeneralVariables.Networking.StartSpectating();
        }

        uic.gManager.damageBlur = 0f;
        bloodyScreen.material.color = DarkRef.SetAlpha(bloodyScreen.material.color, 0f);
        staminaBackground.color = defStaminaBGCol;
        healthText.text = "INACTIVE";
        healthText.transform.localScale = Vector3.one;
        healthBar.value = 0f;
        shieldBar.value = 0f;
        pe.ClearExplosionEffect();

        for (int i = 0; i < hitIndicators.Count; i++)
        {
            if (hitIndicators[i].instance == null)
            {
                continue;
            }

            Destroy(hitIndicators[i].instance);
        }

        if (deathReplacement != null)
        {
            GameObject go = (GameObject)Instantiate(deathReplacement, transform.position, transform.rotation);
            go.GetComponent<AudioSource>().pitch = Random.Range(0.85f, 0.94f);
            go.GetComponent<AudioSource>().PlayOneShot(deathSound, Random.Range(0.3f, 0.35f));
            go.GetComponentInChildren<SkinnedMeshRenderer>().gameObject.AddComponent<DissolveEffect>().Dissolve(dissolveMat, GameSettings.settingsController.ragdollDestroyTimer, 0.18f, new Color(1f, 0.3f, 0f), DissolveEffect.DissolveDirection.DissolveOut, true);

            if ((Topan.Network.isConnected && killerID == -1) || (Topan.Network.isConnected && specTarget == null))
            {
                GeneralVariables.spectatorCamera.playerFollow.startingPosition = transform.position;
                GeneralVariables.spectatorCamera.playerFollow.offset = Vector3.up * 0.55f;
                GeneralVariables.spectatorCamera.target = go.transform.GetChild(1);
            }
        }

        dead = true;
        base.headshot = false;
        Destroy(gameObject);

        if (!Topan.Network.isConnected)
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }

    private IEnumerator CalmingStage()
    {
        canSprint = false;
        staminaBlinking = true;

        //Play the exhausted out of breath exhale sound.
        staminaCooldown = true;

        float timer = 0f;
        while (timer < 0.24f)
        {
            timer += Time.deltaTime;
            breathFactor = Mathf.Clamp01(1f - (timer / 0.18f));
            yield return null;
        }

        breathingSound.Stop();
        fallDamageSource.GetComponent<TimeScaleSound>().pitchMod = Random.Range(0.9f, 1.05f);
        fallDamageSource.PlayOneShot(noBreathExhale, 0.37f);
        yield return new WaitForSeconds(noBreathExhale.length + 0.07f);
        breathFactor = 1f;
        breathingSound.Play();
        yield return new WaitForSeconds(0.28f);
        staminaCooldown = false;

        while (curStamina < 16f)
        {
            yield return null;
        }

        staminaBlinking = false;
        canSprint = true;
    }

    private void AdjustGUISize()
    {
        if (curHealth != oldHealth)
        {
            guiSizeModH += 0.03f + Mathf.Clamp(Mathf.Abs(curHealth - oldHealth) * 0.006f, 0f, 0.1f);
            oldHealth = curHealth;
        }
        else
        {
            guiSizeModH = Mathf.Lerp(guiSizeModH, 1f, Time.unscaledDeltaTime * 7f);
        }

        if (curShield != oldShield)
        {
            guiSizeModS += 0.03f + Mathf.Clamp(Mathf.Abs(curShield - oldShield) * 0.006f, 0f, 0.1f);
            oldShield = curShield;
        }
        else
        {
            guiSizeModS = Mathf.Lerp(guiSizeModS, 1f, Time.unscaledDeltaTime * 7f);
        }

        healthText.transform.localScale = Vector3.one * Mathf.Clamp(guiSizeModH, 1f, 1.15f) * (1f + (Mathf.Abs(Mathf.Sin(Time.time * 3f)) * 0.125f * Mathf.Clamp(0.5f - percent, 0f, 0.5f)));
        shieldText.cachedTrans.localScale = Vector3.one * Mathf.Clamp(guiSizeModS, 1f, 1.15f);
    }

    public void ReplenishHealth(int amount)
    {
        curHealth += amount;
    }

    public void HitIndicator(Vector3 hitFromPos)
    {
        if (dead)
        {
            return;
        }

        if (pe.hasEMP)
        {
            hitFromPos += DarkRef.RandomVector3(-Vector3.one, Vector3.one) * (hitFromPos - transform.position).magnitude * 0.75f;
        }

        hitFromPos.y = 0f;

        for (int i = 0; i < hitIndicators.Count; i++)
        {
            if (Time.time - lastHitIndicate < 0.05f && (hitIndicators[i].hitFromPosition - hitFromPos).sqrMagnitude < 0.01f)
            {
                hitIndicators[i].initTime = Time.time;
                return;
            }
        }

        GameObject newInstance = (GameObject)Instantiate(hitIndicatorPrefab);
        newInstance.transform.parent = GeneralVariables.uiController.hitIndicatorRoot;
        newInstance.transform.localPosition = Vector3.zero;
        newInstance.transform.localScale = Vector3.one;

        Vector3 rot = newInstance.transform.localEulerAngles;
        rot.z = Mathf.Round(-CalculateIndicatorRotation(hitFromPos - transform.position) / hitIndicatorRotRound) * hitIndicatorRotRound;
        newInstance.transform.localRotation = Quaternion.Euler(rot);

        HitIndicatorInfo hii = new HitIndicatorInfo();
        hii.instance = newInstance;
        hii.lifetime = 0.3f;
        hii.hitFromPosition = hitFromPos;
        hii.initTime = Time.time;
        hii.impulseTarget = 1.5f;
        hii.instance.transform.GetChild(0).localPosition = new Vector3(-1.5f, uic.crosshairs.hitIndicatorOffset + 40f, 0f);

        hitIndicators.Add(hii);
        lastHitIndicate = Time.time;
    }

    private IEnumerator HandleIndicatorRemoval(HitIndicatorInfo hii)
    {
        AlphaGroupUI agu = hii.instance.GetComponent<AlphaGroupUI>();
        while (agu.alpha > 0f)
        {
            agu.alpha -= Time.deltaTime * 5f;
            agu.alpha = Mathf.Clamp01(agu.alpha);

            hii.instance.transform.GetChild(0).localScale = new Vector3(0.85f + (agu.alpha * 0.15f), 0.7f + (agu.alpha * 0.3f), 0.85f + (agu.alpha * 0.15f)) * hii.impulseTarget;
            Vector3 rot = hii.instance.transform.localEulerAngles;
            rot.z = Mathf.Round(-CalculateIndicatorRotation(hii.hitFromPosition - transform.position) / hitIndicatorRotRound) * hitIndicatorRotRound;
            hii.instance.transform.localRotation = Quaternion.Slerp(hii.instance.transform.localRotation, Quaternion.Euler(rot), Time.deltaTime * 10f);
            hii.instance.transform.GetChild(0).localPosition = new Vector3(-1.5f, uic.crosshairs.hitIndicatorOffset + 37f + (agu.alpha * 2f), 0f);

            yield return null;
        }

        Destroy(hii.instance);
        hitIndicators.Remove(hii);
    }

    private void ManageIndicatorGUI()
    {
        for (int i = 0; i < hitIndicators.Count; i++)
        {
            if (hitIndicators[i].instance == null)
            {
                continue;
            }

            float elapsed = Time.time - hitIndicators[i].initTime;
            Vector3 rot = hitIndicators[i].instance.transform.localEulerAngles;
            rot.z = Mathf.Round(-CalculateIndicatorRotation(hitIndicators[i].hitFromPosition - transform.position) / hitIndicatorRotRound) * hitIndicatorRotRound;
            hitIndicators[i].instance.transform.localRotation = Quaternion.Slerp(hitIndicators[i].instance.transform.localRotation, Quaternion.Euler(rot), Time.deltaTime * 10f);
            hitIndicators[i].instance.transform.GetChild(0).localPosition = new Vector3(-1.5f, uic.crosshairs.hitIndicatorOffset + 39f, 0f);

            hitIndicators[i].impulseCurrent = Mathf.Lerp(hitIndicators[i].impulseCurrent, hitIndicators[i].impulseTarget, Time.deltaTime * 15f);
            hitIndicators[i].instance.transform.GetChild(0).localScale = Vector3.one * (1f + (Mathf.PingPong(elapsed, hitIndicators[i].lifetime) * 0.5f)) * hitIndicators[i].impulseCurrent;
            hitIndicators[i].impulseTarget = Mathf.MoveTowards(hitIndicators[i].impulseTarget, 1f, Time.deltaTime * 3f);

            if (elapsed >= hitIndicators[i].lifetime && !hitIndicators[i].queueDestroy)
            {
                StartCoroutine(HandleIndicatorRemoval(hitIndicators[i]));
                hitIndicators[i].queueDestroy = true;
            }
        }
    }

    private float CalculateIndicatorRotation(Vector3 direction)
    {
        direction.Normalize();
        direction.y = 0f;

        float angle = Vector3.Angle(direction, transform.forward);
        Vector3 perp = Vector3.Cross(transform.forward, direction);
        float faceDir = -Vector3.Dot(perp, transform.up);

        if (faceDir < 0f)
        {
            angle = 360f - angle;
        }

        return angle;
    }

    private int GetDamageIndex(int pID)
    {
        for (int i = 0; i < damageIDs.Count; i++)
        {
            if (pID == damageIDs[i])
            {
                return i;
            }
        }

        return -1;
    }
}