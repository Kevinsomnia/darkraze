using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MultiplayerMenu))]
public class MultiplayerMenuInspector : Editor
{
    public override void OnInspectorGUI()
    {
        MultiplayerMenu mpMenu = target as MultiplayerMenu;

        mpMenu.gameNameInput = (UIInput)EditorGUILayout.ObjectField("Game Name Input:", mpMenu.gameNameInput, typeof(UIInput), true);
        mpMenu.IPInput = (UIInput)EditorGUILayout.ObjectField("IP Input:", mpMenu.IPInput, typeof(UIInput), true);
        mpMenu.portInput = (UIInput)EditorGUILayout.ObjectField("Port Input:", mpMenu.portInput, typeof(UIInput), true);
        mpMenu.portForwardCheckbox = (UIToggle)EditorGUILayout.ObjectField("Port Forward Toggle:", mpMenu.portForwardCheckbox, typeof(UIToggle), true);
        mpMenu.hostPortInput = (UIInput)EditorGUILayout.ObjectField("Host Port Input:", mpMenu.hostPortInput, typeof(UIInput), true);
        mpMenu.hostLocalCheckbox = (UIToggle)EditorGUILayout.ObjectField("Host Local Toggle:", mpMenu.hostLocalCheckbox, typeof(UIToggle), true);
        mpMenu.maxPlayerSlider = (SliderAction)EditorGUILayout.ObjectField("Max Player Slider:", mpMenu.maxPlayerSlider, typeof(SliderAction), true);
        mpMenu.gameDurationSlider = (SliderAction)EditorGUILayout.ObjectField("Game Duration Slider:", mpMenu.gameDurationSlider, typeof(SliderAction), true);
        mpMenu.mapSelection = (GameObject)EditorGUILayout.ObjectField("Map Selection:", mpMenu.mapSelection, typeof(GameObject), true);

        DarkRef.GUISeparator(5);

        if (mpMenu.cachedNetworking == null)
        {
            mpMenu.cachedNetworking = (GameObject)EditorGUILayout.ObjectField("Cached Networking:", mpMenu.cachedNetworking, typeof(GameObject), true);
            DarkRef.GUISeparator(5);
        }

        EditorGUILayout.LabelField("HOST SERVER MENU", EditorStyles.boldLabel);
        EditorGUI.indentLevel += 1;
        EditorGUIUtility.labelWidth = 140f;
        mpMenu.mapSelectionBox = (UISprite)EditorGUILayout.ObjectField("Map Selection Box:", mpMenu.mapSelectionBox, typeof(UISprite), true);
        mpMenu.sliderStart = (Transform)EditorGUILayout.ObjectField("Selection Start:", mpMenu.sliderStart, typeof(Transform), true);
        mpMenu.mapSpacing = EditorGUILayout.FloatField("Selection Spacing:", mpMenu.mapSpacing);

        GUILayout.Space(5f);
        mpMenu.backButton = (GameObject)EditorGUILayout.ObjectField("Back Button:", mpMenu.backButton, typeof(GameObject), true);
        mpMenu.hostServerButton = (UIButton)EditorGUILayout.ObjectField("Host Server Button:", mpMenu.hostServerButton, typeof(UIButton), true);
        mpMenu.editServerButton = (UIButton)EditorGUILayout.ObjectField("Edit Server Button:", mpMenu.editServerButton, typeof(UIButton), true);
        mpMenu.moreInfoButton = (UIButton)EditorGUILayout.ObjectField("More Info Button:", mpMenu.moreInfoButton, typeof(UIButton), true);
        EditorGUI.indentLevel -= 1;
        EditorGUIUtility.LookLikeControls();
        DarkRef.GUISeparator(5);

        mpMenu.mServerStatus = (UILabel)EditorGUILayout.ObjectField("Server Status:", mpMenu.mServerStatus, typeof(UILabel), true);
        mpMenu.mServerPing = (UILabel)EditorGUILayout.ObjectField("Server Ping:", mpMenu.mServerPing, typeof(UILabel), true);
        mpMenu.mCheckingIcon = (UISprite)EditorGUILayout.ObjectField("Checking Icon:", mpMenu.mCheckingIcon, typeof(UISprite), true);
        mpMenu.mServerPingButton = (UIButton)EditorGUILayout.ObjectField("Ping Button:", mpMenu.mServerPingButton, typeof(UIButton), true);
        mpMenu.mSettingControl = (GM_SettingsControl)EditorGUILayout.ObjectField("Settings Control:", mpMenu.mSettingControl, typeof(GM_SettingsControl), true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(mpMenu);
        }
    }
}