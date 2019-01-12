using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CurrentWeaponMode { GenericWeapon = 0, AlternateWeapon = 1 }
public enum WeaponSlot { Primary, Secondary, Melee }

public class WeaponManager : MonoBehaviour
{
    public static int mpSpawnStartPrimary = -1;
    public static int mpSpawnStartSecondary = -1;

    public Transform weaponsParent;
    public Transform currentWeaponTransform;
    public int startingPrimary;
    public int startingSecondary;
    public MeleeController meleeController;
    public GrenadeManager grenadeManager;
    public GameObject hands;
    public float drawTime = 1.1f;
    public AudioClip drawSound;
    public AudioClip dropSound;

    //Through debug inspector
    public WeaponList wepList;
    public PoolingList poolList;
    public float muzzleGlowBright = 0.2f;
    public LayerMask layersToMelee = -1;

    [HideInInspector] public GunController[] heldWeapons = new GunController[2];
    [HideInInspector] public bool ignoreWeightDelayOnce;
    [HideInInspector] public bool isReloading;
    [HideInInspector] public bool queuedChamber;
    [HideInInspector] public bool isQuickThrowState = false;
    [HideInInspector] public bool prepareToSwitch = false;
    [HideInInspector] public int curWeaponNum;
    [HideInInspector] public DynamicMovement dm;
    [HideInInspector] public PlayerEffects pe;
    [HideInInspector] public CurrentWeaponMode curWeaponMode = CurrentWeaponMode.GenericWeapon;
    [HideInInspector] public int queuedAmmo;
    [HideInInspector] public int queuedReserve;
    [HideInInspector] public int displayCurAmmo;
    [HideInInspector] public int displayAmmoLeft;
    [HideInInspector] public float dca;

    private float initTime;
    private bool switching;
    private bool pressedQuickThrow;
    private int primaryDropIndex;
    private int secondaryDropIndex;
    private GrenadeAmmoManager gam;
    private PlayerMovement pm;
    private AimController ac;
    private AntiClipSystem acs;
    private PlayerVitals pv;
    private PlayerLook pl;

    private GameObject player;
    private float lastClick;

    private Vector3 grenOnePos = new Vector3(-35f, 83.5f, 0f);
    private Vector3 grenTwoPos = new Vector3(1f, 83.5f, 0f);

    private Vector3 defaultIconPos;
    private UILabel weaponName;
    private UILabel curAmmoDisplay;
    private UILabel ammoLeftDisplay;
    private UITexture weaponIcon;
    private UILabel weaponSlot;
    private UISlider ammoBar;
    private UILabel fireModeDisplay;
    private UISprite grenadeSelection;
    private UILabel grenadeSelectionLabel;
    private ImpulseGUI reloadIndImpulse;
    private Renderer muzGlow;
    private string curWepName;
    private string empName;

    private bool friendlyFire;
    private RaycastHit melee;
    private int particleMeleeIndex;
    private float startDropKeyTime;
    private float lastRandomizeTime;
    private float lastQuickMeleeTime;
    private float dal;
    private float scramble = 0f;
    private float lastScramble;

    public GunController currentGC
    {
        get
        {
            if (curWeaponNum == 0 || curWeaponNum == 1)
            {
                return heldWeapons[curWeaponNum];
            }
            else
            {
                return null;
            }
        }
    }

    [HideInInspector] public Topan.NetworkView rootNetView;

    void Awake()
    {
        WeaponDatabase.savedWeaponList = wepList;
        PoolManager.cachedList = poolList;
    }

    void Start()
    {
        player = GeneralVariables.player;
        dm = GetComponent<DynamicMovement>();
        pm = player.GetComponent<PlayerMovement>();
        pv = player.GetComponent<PlayerVitals>();
        pl = player.GetComponent<PlayerLook>();
        pe = player.GetComponent<PlayerEffects>();
        gam = GeneralVariables.playerRef.gam;
        ac = GeneralVariables.playerRef.ac;
        acs = GeneralVariables.playerRef.acs;

        UIController uiController = GeneralVariables.uiController;
        curAmmoDisplay = uiController.curAmmoDisplay;
        ammoLeftDisplay = uiController.ammoLeftDisplay;
        weaponName = uiController.weaponName;
        weaponIcon = uiController.weaponIcon;
        weaponSlot = uiController.weaponSlot;
        ammoBar = uiController.ammoBar;
        muzGlow = uiController.muzzleHelmetGlow;
        fireModeDisplay = uiController.fireModeLabel;
        reloadIndImpulse = uiController.reloadIndicatorLabel.GetComponent<ImpulseGUI>();
        grenadeSelection = uiController.grenadeSelectionSprite;
        grenadeSelectionLabel = uiController.grenadeSelectionLabel;
        defaultIconPos = new Vector3(-172f, 14f, 2f);

        grenadeSelectionLabel.alpha = 0.2f;
        grenadeSelection.transform.localPosition = new Vector3(-35f, 83f, 0f);

        grenadeManager.gameObject.SetActive(true);
        grenadeManager.Initialize();
        queuedAmmo = -1;
        queuedReserve = -1;
        queuedChamber = false;
        curWeaponNum = -1;

        if (mpSpawnStartPrimary > -1)
        {
            AddWeapon(mpSpawnStartPrimary, false);
            mpSpawnStartPrimary = -1;
        }
        else if (startingPrimary > -1)
        {
            AddWeapon(startingPrimary, false);
        }

        if (mpSpawnStartSecondary > -1)
        {
            AddWeapon(mpSpawnStartSecondary, false);
            mpSpawnStartSecondary = -1;
        }
        else if (startingSecondary > -1)
        {
            AddWeapon(startingSecondary, false);
        }

        if (Topan.Network.isConnected)
        {
            StartCoroutine(WaitForNetworkInit());
        }
        else
        {
            FindWeaponToUse(true, 0);
        }

        dca = 0f;
        dal = 0f;
        displayCurAmmo = 0;
        displayAmmoLeft = 0;
        friendlyFire = NetworkingGeneral.friendlyFire;
        initTime = Time.time;
    }

    private IEnumerator WaitForNetworkInit()
    {
        while (rootNetView == null)
        {
            rootNetView = player.GetComponent<Topan.NetworkView>();
            yield return null;
        }

        FindWeaponToUse(true, 0);
        rootNetView.RPC(Topan.RPCMode.OthersBuffered, "RefreshWeapon", (byte)currentGC.weaponID);
    }

    void Update()
    {
        CheckText();

        grenadeSelection.transform.localPosition = Vector3.Lerp(grenadeSelection.transform.localPosition, (grenadeManager.grenadeIndex == 0) ? grenOnePos : grenTwoPos, Time.deltaTime * 18f);
        grenadeSelection.alpha = Mathf.Lerp(grenadeSelection.alpha, (currentWeaponTransform == grenadeManager.transform) ? grenadeSelection.defaultAlpha : grenadeSelection.defaultAlpha * 0.35f, Time.deltaTime * 16f);
        grenadeSelectionLabel.text = grenadeManager.grenadeInventory[grenadeManager.grenadeIndex].grenadeName;

        if (currentWeaponTransform == grenadeManager.transform || !grenadeManager.canSwitchToGrenade)
        {
            grenadeSelectionLabel.alpha = Mathf.Lerp(grenadeSelectionLabel.alpha, 0f, Time.deltaTime * 15f);
        }
        else if (!dm.animationIsPlaying)
        {
            grenadeSelectionLabel.alpha = Mathf.Lerp(grenadeSelectionLabel.alpha, grenadeSelection.defaultAlpha * 0.4f, Time.deltaTime * 15f);
        }

        if (pe.hasEMP)
        {
            if (PlayerEffects.onFinishEMP == null)
            {
                PlayerEffects.onFinishEMP = FinishedEMP;
            }

            ammoBar.value = Mathf.Lerp(ammoBar.value, Random.value, Time.deltaTime * 10f);
        }

        if (currentGC != null)
        {
            dca = Mathf.Lerp(dca, currentGC.currentAmmo, Time.deltaTime * 14f * Mathf.Clamp01(Time.time - initTime));
            dal = Mathf.Lerp(dal, currentGC.ammoLeft, Time.deltaTime * 15f * Mathf.Clamp01(Time.time - initTime));

            muzGlow.enabled = currentGC.muzzlePercent > 0f;
            muzGlow.material.SetColor("_TintColor", DarkRef.SetAlpha(muzGlow.material.GetColor("_TintColor"), currentGC.muzzlePercent * muzzleGlowBright));
        }

        if (!pe.hasEMP)
        {
            displayCurAmmo = Mathf.RoundToInt(dca);
            displayAmmoLeft = Mathf.RoundToInt(dal);
        }

        if (Time.timeScale <= 0f || RestrictionManager.restricted)
        {
            return;
        }

        if (!dm.animationIsPlaying)
        {
            if (currentGC != null)
            {
                if (cInput.GetButtonDown("Flashlight") && (Time.time - lastClick) >= 0.3f)
                {
                    currentGC.ToggleFlashlight();
                    lastClick = Time.time;
                }

                if (!RestrictionManager.restricted && !acs.clipping && !ac.isAiming && !pm.sprinting && cInput.GetButtonDown("Melee") && Time.time - lastQuickMeleeTime >= 0.6f)
                {
                    QuickMeleeWithWeapon();
                    lastQuickMeleeTime = Time.time;
                }

                if (currentGC.percent < 0.33f && !pe.hasEMP)
                {
                    reloadIndImpulse.curColor = new Color(1f, 0.2f, 0f);
                    reloadIndImpulse.baseAlpha = 0.5f;
                    reloadIndImpulse.enabled = true;
                }
                else
                {
                    reloadIndImpulse.curColor = new Color(0.25f, 0.25f, 0.25f);
                    reloadIndImpulse.baseAlpha = 0.5f;
                    reloadIndImpulse.enabled = false;
                }
            }
            else
            {
                reloadIndImpulse.curColor = new Color(0.25f, 0.25f, 0.25f);
                reloadIndImpulse.baseAlpha = 0.5f;
                reloadIndImpulse.enabled = false;
            }

            bool grenCanSwitch = true;
            if (grenadeManager != null && grenadeManager.curGrenade != null && grenadeManager.curGrenade.cannotSwitch)
            {
                grenCanSwitch = false;
            }

            if (grenCanSwitch && !dm.terminalVelocity && !pm.onLadder)
            {
                if (!ac.isAiming && !acs.clipping)
                {
                    int scrollSelect = curWeaponNum;
                    if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.08f)
                    {
                        scrollSelect -= (int)Mathf.Sign(Input.GetAxis("Mouse ScrollWheel"));
                        if (scrollSelect > 1)
                        {
                            scrollSelect = 0;
                        }
                        else if (scrollSelect < 0)
                        {
                            scrollSelect = 1;
                        }

                        SelectWeapon(scrollSelect);
                        return;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha0) && hands != null && currentWeaponTransform != hands.transform && !switching)
                {
                    SelectWeapon(-1);
                }
                if (Input.GetKeyDown(KeyCode.Alpha1) && heldWeapons[0] != null && currentGC != heldWeapons[0] && !switching)
                {
                    SelectWeapon(0);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2) && heldWeapons[1] != null && currentGC != heldWeapons[1] && !switching)
                {
                    SelectWeapon(1);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3) && meleeController != null && currentWeaponTransform != meleeController.transform && !switching)
                {
                    SelectWeapon(2);
                }
                if (grenadeManager != null && currentWeaponTransform != grenadeManager.transform && !switching && grenadeManager.canSwitchToGrenade)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        SelectWeapon(3);
                    }
                    else if (cInput.GetButtonDown("Throw Grenade"))
                    {
                        SelectWeapon(3);
                        pressedQuickThrow = true;
                        isQuickThrowState = true;
                    }
                }

                if (currentGC != null)
                {
                    if (cInput.GetButtonDown("Drop Weapon"))
                    {
                        startDropKeyTime = Time.time;
                    }

                    if (cInput.GetButtonUp("Drop Weapon"))
                    {
                        if (Time.time - startDropKeyTime < 0.2f)
                        {
                            DropWeapon(curWeaponNum);
                            FindWeaponToUse(false);
                        }
                    }
                }
            }
        }
    }

    public void CheckText()
    {
        if (pe.hasEMP)
        {
            weaponName.text = empName;
        }
        else if (Time.time - lastScramble >= 0.025f)
        {
            weaponName.text = DarkRef.ScrambleString(curWepName, 5, scramble);
            lastScramble = Time.time;
        }

        if (pe.hasEMP)
        {
            if (Time.time - lastRandomizeTime >= 0.1f)
            {
                weaponSlot.text = DarkRef.RandomLetterCombination(DarkRef.RandomRange(7, 8)).ToUpper();
                fireModeDisplay.text = "[" + DarkRef.RandomLetterCombination(DarkRef.RandomRange(4, 5)).ToUpper() + "]";
                displayCurAmmo = DarkRef.RandomRange(0, 99);
                displayAmmoLeft = DarkRef.RandomRange(0, 999);
                curAmmoDisplay.text = displayCurAmmo.ToString();
                ammoLeftDisplay.text = displayAmmoLeft.ToString();

                empName = DarkRef.RandomLetterCombination(Random.Range(6, 9), true);

                lastRandomizeTime = Time.time;
            }

            return;
        }

        if (currentGC != null)
        {
            if (curWeaponNum == 0)
            {
                weaponSlot.text = "PRIMARY";
            }
            else if (curWeaponNum == 1)
            {
                weaponSlot.text = "SECONDARY";
            }

            if (currentGC.currentFireMode == GunController.FireMode.FullAuto)
            {
                fireModeDisplay.text = "[AUTO]";
            }
            else if (currentGC.currentFireMode == GunController.FireMode.SemiAuto)
            {
                fireModeDisplay.text = "[SEMI]";
            }
            else if (currentGC.currentFireMode == GunController.FireMode.BurstFire)
            {
                fireModeDisplay.text = "[BURST]";
            }
            else
            {
                fireModeDisplay.text = "[SAFE]";
            }
        }
        else if (curWeaponNum == 3)
        {
            weaponSlot.text = "GRENADE";
            fireModeDisplay.text = "[NONE]";
        }
        else if (curWeaponNum == 2)
        {
            weaponSlot.text = "MELEE";
            fireModeDisplay.text = "[NONE]";
        }
        else if (curWeaponNum <= -1)
        {
            weaponSlot.text = "NONE";
            fireModeDisplay.text = "[NONE]";
        }
    }

    public void SelectWeaponImmediate(int weaponIndex)
    {
        StopCoroutine("SelectWeaponCoroutine");
        StartCoroutine(SelectWeaponCoroutine(weaponIndex, true));
    }

    public void SelectWeapon(int weaponSlot)
    {
        if (pm.sprinting)
        {
            return;
        }

        StopCoroutine("SelectWeaponCoroutine");
        StartCoroutine(SelectWeaponCoroutine(weaponSlot, false));
    }

    private IEnumerator SelectWeaponCoroutine(int weaponIndex, bool immediate)
    {
        prepareToSwitch = true;

        while (ac.aimTransition > 0.05f)
        {
            yield return 0;
        }

        prepareToSwitch = false;

        if (currentGC != null && currentGC.reloading)
        {
            currentGC.CancelReload();
        }

        int prevWeaponNum = curWeaponNum;
        if (Topan.Network.isConnected && rootNetView != null)
        {
            if (weaponIndex == 0 || weaponIndex == 1)
            {
                rootNetView.RPC(Topan.RPCMode.OthersBuffered, "RefreshWeapon", (byte)heldWeapons[weaponIndex].weaponID);

                if (heldWeapons[weaponIndex].gunVisuals != null && heldWeapons[weaponIndex].gunVisuals.flashlight != null)
                {
                    rootNetView.RPC(Topan.RPCMode.OthersBuffered, "SetFlashlight", heldWeapons[weaponIndex].flashlightOn);
                }
            }
            else
            {
                if (weaponIndex <= -1)
                {
                    rootNetView.RPC(Topan.RPCMode.OthersBuffered, "SetSpecialActive", (byte)0); //Hands
                }
                else if (weaponIndex == 2)
                {
                    rootNetView.RPC(Topan.RPCMode.OthersBuffered, "SetSpecialActive", (byte)1); //Melee
                }
                else if (weaponIndex == 3)
                {
                    grenadeManager.OnSelect(true);
                }
            }
        }

        if (!immediate)
        {
            if (!isQuickThrowState)
            {
                curAmmoDisplay.text = "--";
                ammoLeftDisplay.text = "---";
            }

            float weightFactorTime = 0f;
            if (weaponIndex == 0 || weaponIndex == 1)
            {
                if (ignoreWeightDelayOnce)
                {
                    ignoreWeightDelayOnce = false;
                }
                else
                {
                    weightFactorTime = (heldWeapons[weaponIndex].weaponWeight * 0.05f);
                }
            }

            dm.Draw(drawTime + weightFactorTime);
            switching = true;
            pv.jumpRattleEquip = true;

            float waitPeriod = 0f;
            bool cyclingGrenades = false;
            bool ignoreFirstPress = true;
            while (waitPeriod < drawTime + weightFactorTime)
            {
                if (weaponIndex == 3 && Input.GetKeyDown(KeyCode.Alpha4) && grenadeManager.availableGrenades.Count > 1)
                {
                    if (!ignoreFirstPress)
                    {
                        cyclingGrenades = true;

                        if (grenadeManager.grenadeIndex == 0 && gam.typeTwoGrenades > 0)
                        {
                            grenadeManager.grenadeIndex = 1;
                        }
                        else if (grenadeManager.grenadeIndex == 1 && gam.typeOneGrenades > 0)
                        {
                            grenadeManager.grenadeIndex = 0;
                        }

                        grenadeSelection.alpha = grenadeSelection.defaultAlpha;
                        grenadeSelectionLabel.text = grenadeManager.grenadeInventory[grenadeManager.grenadeIndex].grenadeName;

                        dm.ExtendDrawTime(waitPeriod);
                        waitPeriod = 0f;
                    }
                    else
                    {
                        grenadeSelectionLabel.alpha = 0.4f;
                    }

                    ignoreFirstPress = false;
                }

                if (cyclingGrenades)
                {
                    grenadeSelectionLabel.alpha = Mathf.Lerp(grenadeSelectionLabel.alpha, 0.8f, Time.deltaTime * 18f);
                }

                waitPeriod += Time.deltaTime;
                yield return null;
            }

            switching = false;
        }

        bool sameWeapon = false;
        GunController gc1 = null;
        GunController gc2 = null;

        if (prevWeaponNum > -1 && prevWeaponNum >= 0 && prevWeaponNum <= 1 && weaponIndex >= 0 && weaponIndex <= 1)
        {
            gc1 = heldWeapons[prevWeaponNum];
            gc2 = heldWeapons[weaponIndex];

            if (gc1 != null && gc2 != null && gc1.weaponID == gc2.weaponID)
            {
                sameWeapon = true;
            }
        }

        if (weaponIndex > -2)
        {
            pv.jumpRattleEquip = true;
            GetComponent<AudioSource>().PlayOneShot(drawSound, 0.2f);
        }

        if (currentWeaponTransform == grenadeManager.transform)
        {
            grenadeManager.OnDeselect();
        }

        DeselectAll();

        curWeaponNum = weaponIndex;
        curWeaponMode = (curWeaponNum == 0 || curWeaponNum == 1) ? CurrentWeaponMode.GenericWeapon : CurrentWeaponMode.AlternateWeapon;

        if (curWeaponMode == CurrentWeaponMode.GenericWeapon)
        {
            currentGC.gameObject.SetActive(true);
            currentGC.OnWeaponDraw();
            currentWeaponTransform = currentGC.transform;

            if (queuedAmmo > -1)
            {
                AntiHackSystem.ProtectInt("currentAmmo", queuedAmmo);
                queuedAmmo = -1;
            }

            if (queuedReserve > -1)
            {
                AntiHackSystem.ProtectInt("ammoLeft", queuedReserve);
                queuedReserve = -1;
            }

            currentGC.bulletInChamber = queuedChamber;
            queuedChamber = false;

            curWepName = currentGC.gunName;

            if (!pe.hasEMP)
            {
                weaponIcon.mainTexture = currentGC.iconTexture;
                weaponIcon.transform.localPosition = defaultIconPos + new Vector3(currentGC.iconOffset.x, currentGC.iconOffset.y, 0f);
                weaponIcon.SetDimensions((int)currentGC.iconScale.x, (int)currentGC.iconScale.y);
            }
        }
        else
        {
            if (curWeaponNum == 3)
            {
                grenadeManager.gameObject.SetActive(true);
                grenadeManager.OnSelect(false);

                if (pressedQuickThrow)
                {
                    grenadeManager.QuickThrow(prevWeaponNum);
                }

                currentWeaponTransform = grenadeManager.transform;
                pressedQuickThrow = false;
            }
            else if (curWeaponNum == 2)
            {
                meleeController.gameObject.SetActive(true);
                currentWeaponTransform = meleeController.transform;

                weaponIcon.mainTexture = meleeController.iconTexture;
                weaponIcon.transform.localPosition = defaultIconPos + new Vector3(meleeController.iconOffset.x, meleeController.iconOffset.y, 0f);
                weaponIcon.SetDimensions((int)meleeController.iconSize.x, (int)meleeController.iconSize.y);
            }
            else if (curWeaponNum <= -1)
            {
                if (curWeaponNum == -1)
                {
                    hands.SetActive(true);
                    currentWeaponTransform = hands.transform;
                }
                else
                {
                    currentWeaponTransform = null;
                }

                curWepName = "Hands";
                weaponIcon.mainTexture = null;
                weaponIcon.SetDimensions(100, 50);

                curAmmoDisplay.color = Color.white;
                curAmmoDisplay.text = "---";
                ammoLeftDisplay.text = "---";

                ammoBar.value = 1f;
            }
        }

        StartCoroutine(WeaponNameTransition());

        ReflectionUpdateGroup foundRUG = currentWeaponTransform.GetComponent<ReflectionUpdateGroup>();
        if (foundRUG != null)
        {
            foreach (Renderer r in foundRUG.allRenderers)
            {
                UpdateReflection(r);
            }
        }

        if (sameWeapon)
        {
            gc2.ammoLeft = gc1.ammoLeft;
            AntiHackSystem.ProtectInt("ammoLeft", gc1.ammoLeft);
        }

        if (curWeaponNum < 3 && curWeaponNum != -2)
        {
            dm.sao = currentWeaponTransform.GetComponent<SprintAnimOverride>();
        }

        grenadeSelectionLabel.alpha = 0.2f;
    }

    public void DeselectAll()
    {
        foreach (GunController gc in heldWeapons)
        {
            if (gc != null)
            {
                gc.gameObject.SetActive(false);
            }
        }

        hands.SetActive(false);
        meleeController.gameObject.SetActive(false);
        grenadeManager.gameObject.SetActive(false);
    }

    public void FindWeaponToUse(bool immediate, int priority = 0)
    {
        if (heldWeapons[priority] != null && priority < heldWeapons.Length)
        {
            StartCoroutine(SelectWeaponCoroutine(priority, immediate));
            return;
        }

        for (int i = 0; i < heldWeapons.Length; i++)
        {
            if (i != priority && heldWeapons[i] != null)
            {
                StartCoroutine(SelectWeaponCoroutine(i, immediate));
                return;
            }
        }

        StartCoroutine(SelectWeaponCoroutine(-1, immediate));
    }

    public void DropWeapon(int index, bool findWeaponToUse = true)
    {
        if (curWeaponNum == index)
        {
            currentWeaponTransform = null;
        }

        GetComponent<AudioSource>().PlayOneShot(dropSound);

        GunController dropGC = heldWeapons[index];
        GameObject dropping = dropGC.gameObject;

        Vector3 playerVelo = Vector3.zero;
        if (pm != null)
        {
            playerVelo = (pm.controller.velocity * 0.4f);
        }

        if (Topan.Network.isConnected)
        {
            if (dropGC != null)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                data.Add("chamber", dropGC.bulletInChamber);
                data.Add("force", transform.forward + playerVelo);
                data.Add("curammo", dropGC.currentAmmo);
                data.Add("ammoleft", dropGC.ammoLeft);
                string gunName = dropGC.gunName;
                Topan.Network.Instantiate(Topan.Network.server, "Weapons/" + gunName, dropping.transform.position, dropping.transform.rotation, 0, data);
                Destroy(dropping);
            }
        }
        else
        {
            dropGC.MakePickup(transform.forward + playerVelo);
        }

        heldWeapons[index] = null;
        pl.magnificationFactor = 1f;

        if (findWeaponToUse)
        {
            FindWeaponToUse(false);
        }
    }

    public void AddWeapon(int weaponID, bool autoSwitch = true)
    {
        GunController toInstantiate = WeaponDatabase.GetWeaponByID(weaponID);
        GunController instantiated = null;

        int switchTo = 0;
        if (heldWeapons[0] == null)
        {
            heldWeapons[0] = (GunController)Instantiate(toInstantiate);
            instantiated = heldWeapons[0];
        }
        else if (heldWeapons[1] == null)
        {
            switchTo = 1;
            heldWeapons[1] = (GunController)Instantiate(toInstantiate);
            instantiated = heldWeapons[1];
        }
        else if (curWeaponMode == CurrentWeaponMode.GenericWeapon)
        {
            switchTo = curWeaponNum;
            DropWeapon(curWeaponNum, false);
            heldWeapons[curWeaponNum] = (GunController)Instantiate(toInstantiate);
            instantiated = heldWeapons[curWeaponNum];
        }
        else if (curWeaponMode == CurrentWeaponMode.AlternateWeapon)
        {
        }

        if (instantiated)
        {
            instantiated.transform.parent = weaponsParent;
            instantiated.transform.localPosition = toInstantiate.firstPersonPosition;
            instantiated.transform.localRotation = toInstantiate.firstPersonRotation;

            instantiated.RunStart();
            instantiated.gameObject.SetActive(false);
        }

        if (autoSwitch)
        {
            SelectWeapon(switchTo);
        }
    }

    public void AddAmmo(int weaponID, int amount)
    {
        for (int i = 0; i < heldWeapons.Length; i++)
        {
            if (heldWeapons[i] != null && heldWeapons[i].weaponID == weaponID)
            {
                heldWeapons[i].ammoLeft += amount;
                return;
            }
        }
    }

    public bool HasWeapon(int weaponID)
    {
        for (int i = 0; i < heldWeapons.Length; i++)
        {
            if (heldWeapons[i] != null && heldWeapons[i].weaponID == weaponID)
            {
                return true;
            }
        }

        return false;
    }

    public void HitTarget(bool targetDied)
    {
        GeneralVariables.uiController.crosshairs.ShowHitMarker(targetDied);
    }

    public void FinishedEMP()
    {
        if (currentGC != null)
        {
            UITexture wepIcon = GeneralVariables.uiController.weaponIcon;
            wepIcon.mainTexture = currentGC.iconTexture;
            wepIcon.transform.localPosition = defaultIconPos + new Vector3(currentGC.iconOffset.x, currentGC.iconOffset.y, 0f);
            wepIcon.SetDimensions((int)currentGC.iconScale.x, (int)currentGC.iconScale.y);
        }
    }

    //Slam the stock into the enemy's face.
    private void QuickMeleeWithWeapon()
    {
        dm.DoMeleeAnimation();

        Transform theCamera = GeneralVariables.mainPlayerCamera.transform;
        if (Physics.Raycast(theCamera.position, theCamera.forward, out melee, 2f, layersToMelee.value))
        {


            BaseStats bs = melee.collider.GetComponent<BaseStats>();
            Limb lb = melee.collider.GetComponent<Limb>();
            if (bs == null && lb != null)
            {
                bs = lb.rootStats;
            }

            if (bs != null)
            {
                bool showHitMarker = false;
                int damage = Random.Range(50, 55 + 1);

                if (bs.curHealth > 0)
                {
                    showHitMarker = true;
                }

                bool canDamage = true;
                bs.headshot = false;

                if (Topan.Network.isConnected)
                {
                    Topan.NetworkView damageView = bs.GetComponent<Topan.NetworkView>();
                    if (damageView != null)
                    {
                        BotVitals hitBot = bs.GetComponent<BotVitals>();

                        if (GeneralVariables.gameModeHasTeams)
                        {
                            byte targetTeam = /*(hitBot) ? BotManager.allBotPlayers[hitBot.bm.myIndex].team : */(byte)damageView.owner.GetPlayerData("team", (byte)0);

                            if (targetTeam == (byte)Topan.Network.player.GetPlayerData("team", (byte)0))
                            {
                                if (!friendlyFire)
                                {
                                    canDamage = false;
                                }

                                showHitMarker = false;
                            }
                        }
                        else
                        {
                        }

                        if (canDamage)
                        {
                            if (Topan.Network.isServer && (damageView.ownerID == 0 || hitBot))
                            {
                                bs.ApplyDamageNetwork((byte)Mathf.Clamp(damage, 0, 255), (byte)Topan.Network.player.id, (byte)currentGC.weaponID, (byte)4);
                            }
                            else
                            {
                                damageView.RPC(Topan.RPCMode.Owner, "ApplyDamageNetwork", (byte)Mathf.Clamp(damage, 0, 255), (byte)Topan.Network.player.id, (byte)currentGC.weaponID, (byte)4);
                            }
                        }
                    }
                }
                else
                {
                    bs.ApplyDamageMain(damage, false);
                }

                if (showHitMarker)
                {
                    HitTarget(bs.curHealth <= 0);
                }
            }

            Quaternion rot = Quaternion.LookRotation(melee.normal);

            particleMeleeIndex = 0;
            string surfTag = melee.collider.tag;
            if (surfTag == "Dirt")
            {
                particleMeleeIndex = 1;
            }
            else if (surfTag == "Metal")
            {
                particleMeleeIndex = 2;
            }
            else if (surfTag == "Wood")
            {
                particleMeleeIndex = 3;
            }
            else if (surfTag == "Flesh" || surfTag == "Player - Flesh")
            {
                particleMeleeIndex = 4;
            }
            else if (surfTag == "Water")
            {
                particleMeleeIndex = 5;
            }

            PoolManager.Instance.RequestParticleEmit(particleMeleeIndex, melee.point + (melee.normal * 0.06f), rot);
        }
    }

    private IEnumerator WeaponNameTransition()
    {
        scramble = 0f;

        while (scramble < 0.99f)
        {
            scramble = DarkRef.LerpTowards(scramble, 1f, Time.deltaTime * 20f, Time.deltaTime, 0.2f);
            yield return null;
        }

        scramble = 1f;

        while (scramble > 0.01f)
        {
            scramble = DarkRef.LerpTowards(scramble, 0f, Time.deltaTime * 20f, Time.deltaTime, 0.2f);
            yield return null;
        }

        scramble = 0f;
    }

    private void UpdateReflection(Renderer rend)
    {
        foreach (Material mat in rend.materials)
        {
            if (mat == null || !mat.HasProperty("_ReflectColor"))
            {
                continue;
            }

            Color oldCol = mat.GetColor("_ReflectColor");
            mat.SetColor("_ReflectColor", oldCol * GeneralVariables.lightingFactor);
        }
    }
}