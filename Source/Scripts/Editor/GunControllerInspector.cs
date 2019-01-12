using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GunController))]
public class GunControllerInspector : Editor
{
    private GunController gcScript;

    public override void OnInspectorGUI()
    {
        gcScript = target as GunController;

        GUILayout.Space(10);

        if (gcScript.firePos == null)
        {
            GUILayout.Box("IMPORTANT! Assign a fire position before continuing!");
        }
        gcScript.firePos = (Transform)EditorGUILayout.ObjectField("Fire Position:", gcScript.firePos, typeof(Transform), true);
        if (gcScript.firePos == null)
        {
            return;
        }

        gcScript.leftHandTransform = (Transform)EditorGUILayout.ObjectField("Left Hand:", gcScript.leftHandTransform, typeof(Transform), true);

        GUILayout.Space(5);

        if (GUILayout.Button("Convert To Pickup"))
        {
            PrefabUtility.DisconnectPrefabInstance(gcScript.gameObject);

            GunVisuals gv = gcScript.GetComponent<GunVisuals>();
            if (gv)
            {
                foreach (GameObject go in gv.activateOnUse)
                {
                    DestroyImmediate(go);
                }
                foreach (GameObject go in gv.deactivateOnUse)
                {
                    go.SetActive(true);
                }
                DestroyImmediate(gcScript.firePos.gameObject);
                DestroyImmediate(gv);
            }

            PistolAnim pa = gcScript.GetComponent<PistolAnim>();
            if (pa)
            {
                DestroyImmediate(pa);
            }

            gcScript.gameObject.AddComponent<Rigidbody>().mass = 3f;
            UsableObject uo = gcScript.gameObject.AddComponent<UsableObject>();
            uo.weaponPickup = new UsableObject.WeaponPickup();
            uo.weaponPickup.enabled = true;
            uo.weaponPickup.ammoAmount = gcScript.currentAmmo;
            uo.objectName = gcScript.gunName;
            uo.weaponPickup.weaponID = gcScript.weaponID;
            gcScript.transform.parent = null;
            gcScript.transform.name += " (Pickup)";

            AntiClipVariables acv = gcScript.GetComponent<AntiClipVariables>();
            if (acv)
            {
                DestroyImmediate(acv);
            }

            DestroyImmediate(gcScript);

            return;
        }

        if (GUILayout.Button("Reset IDs"))
        {
            WeaponDatabase.RefreshIDs();
            return;
        }

        DarkRef.GUISeparator();

        if (GUILayout.Button("Set First Person Info"))
        {
            GunController toSetFirst = WeaponDatabase.GetWeaponByID(gcScript.weaponID);
            toSetFirst.firstPersonPosition = gcScript.transform.localPosition;
            toSetFirst.firstPersonRotation = gcScript.transform.localRotation;

            gcScript.firstPersonPosition = gcScript.transform.localPosition;
            gcScript.firstPersonRotation = gcScript.transform.localRotation;
        }
        GUILayout.Label("First Person Info:", EditorStyles.boldLabel);
        GUILayout.Label("    Position: " + DarkRef.PreciseStringVector3(gcScript.firstPersonPosition));
        GUILayout.Label("    Rotation: " + gcScript.firstPersonRotation.eulerAngles);


        DarkRef.GUISeparator();

        if (GUILayout.Button("Set Third Person Info"))
        {
            GunController toSetThird = WeaponDatabase.GetWeaponByID(gcScript.weaponID);
            toSetThird.thirdPersonPosition = gcScript.transform.localPosition;
            toSetThird.thirdPersonRotation = gcScript.transform.localRotation;

            gcScript.thirdPersonPosition = gcScript.transform.localPosition;
            gcScript.thirdPersonRotation = gcScript.transform.localRotation;
        }

        if ((gcScript.transform.parent != null && gcScript.transform.parent.name == "WeaponsParent") && GUILayout.Button("Pose Transform"))
        {
            gcScript.transform.localPosition = gcScript.thirdPersonPosition;
            gcScript.transform.localRotation = gcScript.thirdPersonRotation;
        }

        GUILayout.Label("Third Person Info:", EditorStyles.boldLabel);
        GUILayout.Label("    Position: " + DarkRef.PreciseStringVector3(gcScript.thirdPersonPosition));
        GUILayout.Label("    Rotation: " + gcScript.thirdPersonRotation.eulerAngles);

        DarkRef.GUISeparator();

        GUILayout.Label("GUI Properties", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.BeginVertical();
        gcScript.iconTexture = (Texture2D)EditorGUILayout.ObjectField("Icon:", gcScript.iconTexture, typeof(Texture2D));
        gcScript.iconScale = EditorGUILayout.Vector2Field("Icon Scale:", gcScript.iconScale);
        gcScript.iconOffset = EditorGUILayout.Vector2Field("Icon Offset:", gcScript.iconOffset);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        gcScript.gunName = EditorGUILayout.TextField("Weapon Name:", gcScript.gunName);
        gcScript.weaponSlot = (WeaponSlot)EditorGUILayout.EnumPopup("Weapon Slot:", gcScript.weaponSlot);
        GUILayout.Space(5);

        GUILayout.Label("Fire Mode Settings", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.BeginVertical();
        gcScript.firstMode = (GunController.FireMode)EditorGUILayout.EnumPopup("Fire Mode #1:", gcScript.firstMode);
        if (gcScript.firstMode != GunController.FireMode.None)
        {
            gcScript.firstRPM = EditorGUILayout.FloatField("    Fire Rate (RPM):", gcScript.firstRPM);
        }
        gcScript.secondMode = (GunController.FireMode)EditorGUILayout.EnumPopup("Fire Mode #2:", gcScript.secondMode);
        if (gcScript.secondMode != GunController.FireMode.None)
        {
            gcScript.secondRPM = EditorGUILayout.FloatField("    Fire Rate (RPM):", gcScript.secondRPM);
            GUI.enabled = false;
        }
        if (gcScript.secondMode == GunController.FireMode.None)
        {
            GUI.enabled = false;
            gcScript.thirdMode = GunController.FireMode.None;
        }
        else
        {
            GUI.enabled = true;
        }

        gcScript.thirdMode = (GunController.FireMode)EditorGUILayout.EnumPopup("Fire Mode #3:", gcScript.thirdMode);
        if (gcScript.thirdMode != GunController.FireMode.None && gcScript.secondMode != GunController.FireMode.None)
        {
            gcScript.thirdRPM = EditorGUILayout.FloatField("    Fire Rate (RPM):", gcScript.thirdRPM);
        }
        GUI.enabled = true;
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        BulletInfo.BulletType bulletType = gcScript.bulletInfo.bulletType;
        GUILayout.Label((bulletType == BulletInfo.BulletType.Bullet) ? "Bullet Properties" : "Projectile Properties", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.BeginVertical();
        PoolingList poolList = (PoolingList)Resources.Load("Static Prefabs/PoolingList", typeof(PoolingList));
        gcScript.bulletInfo.bulletType = (BulletInfo.BulletType)EditorGUILayout.EnumPopup("Bullet Type:", gcScript.bulletInfo.bulletType);

        GUI.color = new Color(1f, 0.8f, 0.8f);
        EditorGUILayout.ObjectField((bulletType == BulletInfo.BulletType.Bullet) ? "Bullet Prefab:" : "Projectile Prefab:", poolList.poolPrefabs[Mathf.Clamp(gcScript.bulletInfo.poolIndex, 0, poolList.poolPrefabs.Length - 1)], typeof(GameObject), true);
        GUI.color = Color.white;

        EditorGUI.indentLevel += 1;
        gcScript.bulletInfo.poolIndex = EditorGUILayout.IntField("Pool Index:", Mathf.Clamp(gcScript.bulletInfo.poolIndex, 0, poolList.poolPrefabs.Length - 1));
        EditorGUI.indentLevel -= 1;
        GUILayout.Space(8f);

        gcScript.bulletInfo.damage = EditorGUILayout.IntField((bulletType == BulletInfo.BulletType.Rocket) ? "Explosion Damage" : "Damage:", gcScript.bulletInfo.damage);

        if (bulletType == BulletInfo.BulletType.Rocket)
        {
            gcScript.bulletInfo.explosionRadius = EditorGUILayout.FloatField("Explosion Radius:", gcScript.bulletInfo.explosionRadius);
        }
        else
        {
            gcScript.bulletInfo.force = EditorGUILayout.FloatField("Impact Force:", gcScript.bulletInfo.force);
        }

        gcScript.bulletInfo.muzzleVelocity = EditorGUILayout.FloatField("Base Velocity (m/s):", gcScript.bulletInfo.muzzleVelocity);
        gcScript.bulletInfo.gravityFactor = EditorGUILayout.Slider("Gravity (" + (-Physics.gravity.y * gcScript.bulletInfo.gravityFactor).ToString("F2") + " m/s)", gcScript.bulletInfo.gravityFactor, 0f, 5f);

        gcScript.bulletInfo.damageFalloff = EditorGUILayout.CurveField("Damage Falloff:", gcScript.bulletInfo.damageFalloff);
        for (int i = 0; i < gcScript.bulletInfo.damageFalloff.length; i++)
        {
            Keyframe modKey = gcScript.bulletInfo.damageFalloff.keys[i];
            modKey.time = Mathf.RoundToInt(Mathf.Max(0f, modKey.time));
            modKey.value = Mathf.Round(Mathf.Clamp(modKey.value, 0f, 1f) * 100f) / 100f;
            gcScript.bulletInfo.damageFalloff.MoveKey(i, modKey);
        }

        //        if(gcScript.bulletInfo.penetrationDistance <= 0f) {
        gcScript.bulletInfo.ricochetLength = EditorGUILayout.IntField("Ricochet Amount:", Mathf.Max(gcScript.bulletInfo.ricochetLength, 0));
        gcScript.bulletInfo.ricochetMaxAngle = EditorGUILayout.FloatField("Ricochet Max Angle:", Mathf.Clamp(gcScript.bulletInfo.ricochetMaxAngle, 0f, 90f));
        /*
        }
        else {
            EditorGUILayout.HelpBox("You must disable penetration to enable ricochet", MessageType.Info);
        }

        gcScript.bulletInfo.penetrationDistance = EditorGUILayout.FloatField("Penetration Distance: ", Mathf.Clamp(gcScript.bulletInfo.penetrationDistance, 0f, 10000f));
        gcScript.bulletInfo.penetrationDamageReduction = EditorGUILayout.FloatField("Penetration Damage Reduction: ", Mathf.Clamp(gcScript.bulletInfo.penetrationDamageReduction, 0f, 100f));
        gcScript.bulletInfo.penetrationSpeedReduction = EditorGUILayout.FloatField("Penetration Speed Reduction: ", Mathf.Clamp(gcScript.bulletInfo.penetrationSpeedReduction, 0f, 100f));
        */

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.Label("Sound Settings", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.BeginVertical();
        gcScript.fireSound = (AudioClip)EditorGUILayout.ObjectField("Fire Sound:", gcScript.fireSound, typeof(AudioClip), true);
        gcScript.emptySound = (AudioClip)EditorGUILayout.ObjectField("Empty Sound:", gcScript.emptySound, typeof(AudioClip), true);

        string reloadLabel = (gcScript.reloadMethod == GunController.ReloadMethod.Magazine) ? "Reload Sound:" : "Reload Sound (Loop):";
        gcScript.reloadSound = (AudioClip)EditorGUILayout.ObjectField(reloadLabel, gcScript.reloadSound, typeof(AudioClip), true);
        if (gcScript.reloadMethod == GunController.ReloadMethod.Singular)
        {
            gcScript.reloadEnd = (AudioClip)EditorGUILayout.ObjectField("Reload End:", gcScript.reloadEnd, typeof(AudioClip), true);
        }
        else if (gcScript.reloadMethod == GunController.ReloadMethod.Magazine)
        {
            gcScript.reloadSoundEmpty = (AudioClip)EditorGUILayout.ObjectField("Reload Sound (empty):", gcScript.reloadSoundEmpty, typeof(AudioClip), true);
        }

        gcScript.switchSound = (AudioClip)EditorGUILayout.ObjectField("Switch Fire Mode:", gcScript.switchSound, typeof(AudioClip), true);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.Label("Shoot Settings", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.BeginVertical();

        gcScript.bulletsPerShot = EditorGUILayout.IntSlider("Bullets Per Shot:", gcScript.bulletsPerShot, 1, 50);
        gcScript.countsAsOneBullet = EditorGUILayout.Toggle("Subtract 1 Bullet:", gcScript.countsAsOneBullet);
        if (gcScript.firstMode == GunController.FireMode.BurstFire || gcScript.secondMode == GunController.FireMode.BurstFire || gcScript.thirdMode == GunController.FireMode.BurstFire)
        {
            gcScript.bulletsPerBurst = EditorGUILayout.IntSlider("Rounds Per Burst:", gcScript.bulletsPerBurst, 2, Mathf.Max(3, gcScript.clipSize));
            gcScript.burstInterval = EditorGUILayout.Slider("Burst Interval:", gcScript.burstInterval, 0.01f, 1f);
            gcScript.burstCooldown = EditorGUILayout.Slider("Burst Cooldown:", gcScript.burstCooldown, 0.01f, 1f);
        }
        GUILayout.Space(10);
        gcScript.muzzleProbability = EditorGUILayout.Slider("Muzzle Probability:", gcScript.muzzleProbability, 0f, 1f);
        gcScript.muzzleFlash = (ParticleEmitter)EditorGUILayout.ObjectField("Muzzle Flash:", gcScript.muzzleFlash, typeof(ParticleEmitter), true);
        gcScript.muzzleLight = (Light)EditorGUILayout.ObjectField("Muzzle Light:", gcScript.muzzleLight, typeof(Light), true);
        EditorGUI.indentLevel += 1;
        EditorGUIUtility.labelWidth = 140f;
        gcScript.muzzleSpeed = EditorGUILayout.FloatField("Light Fade Speed:", Mathf.Max(5f, gcScript.muzzleSpeed));
        EditorGUIUtility.LookLikeControls();
        EditorGUI.indentLevel -= 1;

        GUILayout.Space(5f);

        gcScript.shootParticle = (ParticleEmitter)EditorGUILayout.ObjectField("Shoot Smoke:", gcScript.shootParticle, typeof(ParticleEmitter), true);

        GUILayout.Space(10);
        gcScript.ejectionEnabled = EditorGUILayout.Toggle("Shell Ejection:", gcScript.ejectionEnabled);
        if (gcScript.ejectionEnabled)
        {
            gcScript.ejectionPos = (Transform)EditorGUILayout.ObjectField("    Ejection Position:", gcScript.ejectionPos, typeof(Transform), true);
            gcScript.bulletShellIndex = EditorGUILayout.IntField("    Bullet Shell Index:", gcScript.bulletShellIndex);
            gcScript.ejectionDelay = EditorGUILayout.FloatField("    Ejection Delay:", gcScript.ejectionDelay);
            gcScript.ejectionMinForce = EditorGUILayout.Vector3Field("    Minimum Force:", gcScript.ejectionMinForce);
            gcScript.ejectionMaxForce = EditorGUILayout.Vector3Field("    Maximum Force:", gcScript.ejectionMaxForce);
            gcScript.ejectionRotation = EditorGUILayout.FloatField("    Rotation Force:", gcScript.ejectionRotation);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.Label("Ammo Settings", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.BeginVertical();
        gcScript.currentAmmo = EditorGUILayout.IntSlider("Current Ammo:", gcScript.currentAmmo, 0, gcScript.clipSize);
        gcScript.clipSize = EditorGUILayout.IntField("    Magazine Size:", gcScript.clipSize);

        GUILayout.Space(5f);

        gcScript.ammoLeft = EditorGUILayout.IntSlider("Reserve Ammo:", gcScript.ammoLeft, 0, gcScript.ammoLeftCap);
        gcScript.ammoLeftCap = EditorGUILayout.IntField("    Reserve Limit:", gcScript.ammoLeftCap);

        DarkRef.GUISeparator(7f);

        gcScript.reloadMethod = (GunController.ReloadMethod)EditorGUILayout.EnumPopup("Reload Method:", gcScript.reloadMethod);

        EditorGUI.indentLevel += 1;
        EditorGUIUtility.labelWidth = 140f;
        gcScript.reloadOnMouseClick = EditorGUILayout.Toggle("Reload On Click:", gcScript.reloadOnMouseClick);
        EditorGUIUtility.LookLikeControls();
        if (gcScript.reloadMethod == GunController.ReloadMethod.Magazine)
        {
            EditorGUIUtility.labelWidth = 140f;
            gcScript.reloadLength = EditorGUILayout.FloatField("Reload Duration:", Mathf.Clamp(gcScript.reloadLength, 0f, 30f));
            if (gcScript.reloadSoundEmpty != null)
            {
                gcScript.reloadLengthEmpty = EditorGUILayout.FloatField("   Empty Duration:", Mathf.Clamp(gcScript.reloadLengthEmpty, 0f, 30f));
            }
            gcScript.includeChamberRound = EditorGUILayout.Toggle("Chamber Round:", gcScript.includeChamberRound);
            EditorGUIUtility.LookLikeControls();
        }
        else if (gcScript.reloadMethod == GunController.ReloadMethod.Singular)
        {
            EditorGUIUtility.labelWidth = 135f;
            gcScript.reloadDelay = EditorGUILayout.FloatField("Reload Delay:", Mathf.Clamp(gcScript.reloadDelay, 0f, 5f));
            gcScript.reloadInterval = EditorGUILayout.FloatField("Reload Interval:", Mathf.Clamp(gcScript.reloadInterval, 0f, 5f));
            gcScript.reloadAmount = EditorGUILayout.IntField("Reload Amount:", Mathf.Clamp(gcScript.reloadAmount, 1, gcScript.clipSize));
            GUILayout.Box("Reload sound will now loop, make sure that the sound can loop seamlessly.");
            EditorGUIUtility.LookLikeControls();
        }
        EditorGUI.indentLevel -= 1;
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 165f;

        GUILayout.Space(5);

        GUILayout.Label("Aim Settings", EditorStyles.boldLabel);
        gcScript.sniperAimEffect = EditorGUILayout.Toggle("    Sniper Aim Effect:", gcScript.sniperAimEffect);
        gcScript.playerSpeedAim = EditorGUILayout.Slider("    Player Speed Aim:", gcScript.playerSpeedAim, 0.1f, 1f);
        gcScript.mouseSensitivityAim = EditorGUILayout.Slider("    Mouse Sensitivity Aim:", gcScript.mouseSensitivityAim, 0.01f, 1.0f);
        gcScript.aimPos = EditorGUILayout.Vector3Field("    Aim Position:", gcScript.aimPos);
        gcScript.aimSpeedFactor = EditorGUILayout.FloatField("    Aim Speed Factor:", gcScript.aimSpeedFactor);
        gcScript.addZoomFOV = EditorGUILayout.FloatField("    Additional Zoom (FOV):", Mathf.Clamp(gcScript.addZoomFOV, -15f, 75f));
        gcScript.aimSwayFactor = EditorGUILayout.Slider("    Sniper Sway Factor:", gcScript.aimSwayFactor, 0f, 5f);

        GUILayout.Space(5);

        GUILayout.Label("Crosshair Settings", EditorStyles.boldLabel);
        gcScript.crosshairsEnabled = EditorGUILayout.Toggle("    Enabled:", gcScript.crosshairsEnabled);

        GUILayout.Space(5);

        GUILayout.Label("Accuracy and Recoil Settings", EditorStyles.boldLabel);

        gcScript.baseSpreadAmount = EditorGUILayout.FloatField("    Base Bullet Spread:", gcScript.baseSpreadAmount);
        gcScript.spreadSpeed = EditorGUILayout.FloatField("    Bullet Spread Speed:", gcScript.spreadSpeed);
        gcScript.spreadAimFactor = EditorGUILayout.Slider("        Spread Aim Factor:", gcScript.spreadAimFactor, 0f, 1f);
        gcScript.recoverSpeed = EditorGUILayout.FloatField("    Recover Spread Speed:", gcScript.recoverSpeed);
        gcScript.movementSpreadAmount = EditorGUILayout.FloatField("    Movement Spread Amount:", gcScript.movementSpreadAmount);
        gcScript.maxSpreadAmount = EditorGUILayout.FloatField("    Max Bullet Spread:", gcScript.maxSpreadAmount);
        gcScript.aimSpreadModifier = EditorGUILayout.Slider("    Aim Spread Mod:", gcScript.aimSpreadModifier, 0f, 1f);
        gcScript.crouchWalkModifier = EditorGUILayout.Slider("    Crouch/Walk Mod:", gcScript.crouchWalkModifier, 0f, 1f);
        GUILayout.Space(10);
        gcScript.recoilAmount = EditorGUILayout.FloatField("    Offset Recoil:", gcScript.recoilAmount);

        GUILayout.Space(6);

        gcScript.upKickAmount = EditorGUILayout.FloatField("    Up-kick Amount:", gcScript.upKickAmount);
        gcScript.autoReturn = EditorGUILayout.FloatField("        Auto Return:", gcScript.autoReturn);

        GUILayout.Space(5f);

        gcScript.sideKickAmount = EditorGUILayout.FloatField("    Side-kick Amount:", gcScript.sideKickAmount);
        gcScript.aimUpkickModifier = EditorGUILayout.Slider("        Aim Kick Modifier:", gcScript.aimUpkickModifier, 0f, 1f);
        gcScript.crouchUpkickModifier = EditorGUILayout.Slider("        Crouch Kick Modifier:", gcScript.crouchUpkickModifier, 0f, 1f);

        gcScript.kickInfluence = EditorGUILayout.Vector2Field("    Kick Influence", gcScript.kickInfluence);
        GUILayout.Space(5);

        EditorGUIUtility.LookLikeControls(200f);
        gcScript.extraRecoilThreshold = EditorGUILayout.IntField("    Extra Recoil Threshold:", gcScript.extraRecoilThreshold);
        gcScript.extraRecoilAmount = EditorGUILayout.FloatField("    Extra Recoil Amount (" + (gcScript.extraRecoilAmount * 100f).ToString() + "%):", Mathf.Max(0f, gcScript.extraRecoilAmount));
        gcScript.maxExtraRecoil = EditorGUILayout.FloatField("    Extra Recoil Limit (+" + (gcScript.maxExtraRecoil * 100f).ToString() + "%):", Mathf.Max(0f, gcScript.maxExtraRecoil));
        EditorGUIUtility.LookLikeControls(150f);

        GUILayout.Space(10f);

        gcScript.kickCameraTilt = EditorGUILayout.FloatField("    Camera Recoil Tilt:", gcScript.kickCameraTilt);
        gcScript.kickGunTilt = EditorGUILayout.FloatField("    Gun Recoil Tilt:", gcScript.kickGunTilt);
        gcScript.camShakeAnim = EditorGUILayout.FloatField("    Camera Animation:", gcScript.camShakeAnim);

        GUILayout.Space(5f);

        gcScript.kickBackAmount = EditorGUILayout.FloatField("    Kickback Amount:", gcScript.kickBackAmount);
        gcScript.kickSpeedFactor = EditorGUILayout.FloatField("    Kickback Speed:", gcScript.kickSpeedFactor);
        gcScript.kickbackAimFactor = EditorGUILayout.Slider("    Kickback Aim Factor:", gcScript.kickbackAimFactor, 0f, 1f);

        GUILayout.Space(5);

        GUILayout.Label("Animation Settings", EditorStyles.boldLabel);
        gcScript.reloadAnim = EditorGUILayout.TextField("    Reload Name:", gcScript.reloadAnim);

        GUILayout.Space(5);

        GUILayout.Label("Miscellaneous Settings", EditorStyles.boldLabel);
        gcScript.flashlight = (Light)EditorGUILayout.ObjectField("    Flashlight:", gcScript.flashlight, typeof(Light), true);
        gcScript.flashlightClick = (AudioClip)EditorGUILayout.ObjectField("    Flashlight Click:", gcScript.flashlightClick, typeof(AudioClip), true);
        GUILayout.Space(10f);
        gcScript.weaponWeight = EditorGUILayout.FloatField("    Weapon Weight (kg):", Mathf.Clamp(gcScript.weaponWeight, 0f, 100f));
        gcScript.aimBobFactor = EditorGUILayout.Slider("    Aim Bob Factor:", gcScript.aimBobFactor, 0.01f, 1f);

        GUILayout.Space(10f);
        GUILayout.Label("    DoF Settings");
        gcScript.dofBlurAmount = EditorGUILayout.FloatField("        Blur Amount:", Mathf.Clamp(gcScript.dofBlurAmount, 0f, 50f));
        gcScript.dofBlurDistance = EditorGUILayout.FloatField("        Blur Distance:", Mathf.Clamp(gcScript.dofBlurDistance, 0f, 25f));
        gcScript.dofBlurAperture = EditorGUILayout.FloatField("        Blur Aperture:", Mathf.Clamp(gcScript.dofBlurAperture, 0f, 40f));
        gcScript.dofBlurFocalSize = EditorGUILayout.FloatField("        Focal Size:", Mathf.Clamp(gcScript.dofBlurFocalSize, 0f, 10f));
        gcScript.dofBlendBlur = EditorGUILayout.FloatField("        Blend Blur:", Mathf.Clamp(gcScript.dofBlendBlur, 0f, 50f));

        GUILayout.Space(10);

        gcScript.weaponID = EditorGUILayout.IntField("Weapon ID", Mathf.Clamp(gcScript.weaponID, -1, 2000));

        if (GUI.changed)
        {
            EditorUtility.SetDirty(gcScript);
        }
    }
}