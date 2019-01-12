using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIController))]
public class UIControllerInspector : Editor
{
    public static bool isOpen;

    public override void OnInspectorGUI()
    {
        UIController uic = target as UIController;

        uic.guiCamera = (Camera)EditorGUILayout.ObjectField("Main GUI Camera:", uic.guiCamera, typeof(Camera), true);
        uic.guiRoot = (Transform)EditorGUILayout.ObjectField("Main GUI ROOT:", uic.guiRoot, typeof(Transform), true);

        GUILayout.Space(5f);

        EditorGUILayout.LabelField("Vitals GUI", EditorStyles.boldLabel);
        EditorGUI.indentLevel += 1;
        uic.healthBar = (UISlider)EditorGUILayout.ObjectField("Health-bar", uic.healthBar, typeof(UISlider), true);
        uic.healthText = (UILabel)EditorGUILayout.ObjectField("Health-text", uic.healthText, typeof(UILabel), true);
        uic.shieldBar = (UISlider)EditorGUILayout.ObjectField("Shield-bar", uic.shieldBar, typeof(UISlider), true);
        uic.shieldText = (UILabel)EditorGUILayout.ObjectField("Shield-text", uic.shieldText, typeof(UILabel), true);
        uic.staminaBar = (UISlider)EditorGUILayout.ObjectField("Stamina-bar", uic.staminaBar, typeof(UISlider), true);

        DarkRef.GUISeparator();
        uic.bloodyScreen = (MeshRenderer)EditorGUILayout.ObjectField("Blood Screen", uic.bloodyScreen, typeof(MeshRenderer), true);
        uic.shieldTexture = (UITexture)EditorGUILayout.ObjectField("Shield Texture", uic.shieldTexture, typeof(UITexture), true);
        uic.muzzleHelmetGlow = (Renderer)EditorGUILayout.ObjectField("Muzzle Helmet Glow", uic.muzzleHelmetGlow, typeof(Renderer), true);
        EditorGUI.indentLevel -= 1;

        DarkRef.GUISeparator();

        EditorGUILayout.LabelField("Weapon GUI", EditorStyles.boldLabel);
        EditorGUI.indentLevel += 1;
        uic.weaponName = (UILabel)EditorGUILayout.ObjectField("Weapon Name:", uic.weaponName, typeof(UILabel), true);
        uic.curAmmoDisplay = (UILabel)EditorGUILayout.ObjectField("Cur Ammo:", uic.curAmmoDisplay, typeof(UILabel), true);
        uic.ammoLeftDisplay = (UILabel)EditorGUILayout.ObjectField("Ammo Left:", uic.ammoLeftDisplay, typeof(UILabel), true);
        uic.ammoBar = (UISlider)EditorGUILayout.ObjectField("Ammo Bar:", uic.ammoBar, typeof(UISlider), true);
        uic.weaponIcon = (UITexture)EditorGUILayout.ObjectField("Weapon Icon:", uic.weaponIcon, typeof(UITexture), true);
        uic.weaponSlot = (UILabel)EditorGUILayout.ObjectField("Weapon Slot:", uic.weaponSlot, typeof(UILabel), true);
        uic.fireModeLabel = (UILabel)EditorGUILayout.ObjectField("Fire-mode:", uic.fireModeLabel, typeof(UILabel), true);
        uic.reloadIndicatorLabel = (UILabel)EditorGUILayout.ObjectField("Reload Indicator:", uic.reloadIndicatorLabel, typeof(UILabel), true);
        uic.crosshairs = (CrosshairGUI)EditorGUILayout.ObjectField("Crosshairs:", uic.crosshairs, typeof(CrosshairGUI), true);

        DarkRef.GUISeparator(4f);

        uic.grenadeSelectionSprite = (UISprite)EditorGUILayout.ObjectField("Grenade Selection:", uic.grenadeSelectionSprite, typeof(UISprite), true);
        uic.grenadeSelectionLabel = (UILabel)EditorGUILayout.ObjectField("Grenade Selection Label:", uic.grenadeSelectionLabel, typeof(UILabel), true);
        uic.grenadeOneLabel = (UILabel)EditorGUILayout.ObjectField("Grenade Label #1:", uic.grenadeOneLabel, typeof(UILabel), true);
        uic.grenadeTwoLabel = (UILabel)EditorGUILayout.ObjectField("Grenade Label #2:", uic.grenadeTwoLabel, typeof(UILabel), true);
        GUILayout.Space(5);
        uic.grenadeOneIcon = (UITexture)EditorGUILayout.ObjectField("Grenade Icon #1:", uic.grenadeOneIcon, typeof(UITexture), true);
        uic.grenadeTwoIcon = (UITexture)EditorGUILayout.ObjectField("Grenade Icon #2:", uic.grenadeTwoIcon, typeof(UITexture), true);

        EditorGUI.indentLevel -= 1;
        DarkRef.GUISeparator();

        EditorGUILayout.LabelField("Miscellaneous GUI", EditorStyles.boldLabel);
        EditorGUI.indentLevel += 1;
        uic.useGUI = (UILabel)EditorGUILayout.ObjectField("Use GUI:", uic.useGUI, typeof(UILabel), true);
        uic.parentOfObjectives = (Transform)EditorGUILayout.ObjectField("Parent of Objectives:", uic.parentOfObjectives, typeof(Transform), true);
        uic.grabText = (UILabel)EditorGUILayout.ObjectField("Grab Text Label:", uic.grabText, typeof(UILabel), true);
        uic.mpGUI = (MultiplayerGUI)EditorGUILayout.ObjectField("Multiplayer GUI (Control):", uic.mpGUI, typeof(MultiplayerGUI), true);
        uic.teamMarkerSystem = (TeamMarkingSystem)EditorGUILayout.ObjectField("Team Marking System:", uic.teamMarkerSystem, typeof(TeamMarkingSystem), true);
        uic.rainFX = (ParticleSystem)EditorGUILayout.ObjectField("Rain FX:", uic.rainFX, typeof(ParticleSystem), true);
        uic.fadeFromBlack = (UISprite)EditorGUILayout.ObjectField("Fade From Black:", uic.fadeFromBlack, typeof(UISprite), true);
        uic.hitIndicatorRoot = (Transform)EditorGUILayout.ObjectField("Hit Indicator Root:", uic.hitIndicatorRoot, typeof(Transform), true);
        uic.empRecalibrate = (UILabel)EditorGUILayout.ObjectField("Recalibrate EMP:", uic.empRecalibrate, typeof(UILabel), true);

        GUILayout.Space(5f);

        isOpen = EditorGUILayout.Foldout(isOpen, "Flickering Panels");
        if (isOpen)
        {
            int length = uic.flickeringPanels.Length;
            FlickeringGUI[] tempStorage = uic.flickeringPanels;
            EditorGUI.indentLevel += 1;
            length = EditorGUILayout.IntField("Length:", length);
            if (length != uic.flickeringPanels.Length)
            {
                uic.flickeringPanels = new FlickeringGUI[length];
                for (int i = 0; i < tempStorage.Length; i++)
                {
                    if (i < uic.flickeringPanels.Length)
                    {
                        uic.flickeringPanels[i] = tempStorage[i];
                    }
                }
            }
            EditorGUI.indentLevel += 1;
            for (int i = 0; i < length; i++)
            {
                uic.flickeringPanels[i] = (FlickeringGUI)EditorGUILayout.ObjectField("Element " + i.ToString(), uic.flickeringPanels[i], typeof(FlickeringGUI), true);
            }
            EditorGUI.indentLevel -= 1;
            EditorGUI.indentLevel -= 1;
        }

        DarkRef.GUISeparator();

        uic.pauseMenu = (GameObject)EditorGUILayout.ObjectField("Pause Menu:", uic.pauseMenu, typeof(GameObject), true);
        uic.pauseBlur = (BlurEffect)EditorGUILayout.ObjectField("Pause Blur (Normal):", uic.pauseBlur, typeof(BlurEffect), true);
        uic.pauseBlur2 = (BlurEffect)EditorGUILayout.ObjectField("Pause Blur (Settings):", uic.pauseBlur2, typeof(BlurEffect), true);
        uic.settingsPanel = (UIPanel)EditorGUILayout.ObjectField("Settings Panel:", uic.settingsPanel, typeof(UIPanel), true);

        DarkRef.GUISeparator();

        uic.waveCounter = (UILabel)EditorGUILayout.ObjectField("Wave Counter:", uic.waveCounter, typeof(UILabel), true);
        uic.enemiesLeftCounter = (UILabel)EditorGUILayout.ObjectField("Enemies Left Counter:", uic.enemiesLeftCounter, typeof(UILabel), true);
        EditorGUI.indentLevel -= 1;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(uic);
        }
    }
}