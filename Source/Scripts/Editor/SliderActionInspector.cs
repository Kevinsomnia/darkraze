using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SliderAction))]
public class SliderActionInspector : Editor
{
    private bool isOpen = false;
    private bool isOpen1 = false;
    private bool isOpen2 = false;

    public override void OnInspectorGUI()
    {
        SliderAction sa = target as SliderAction;

        if (sa.label == null)
        {
            GUILayout.Label("Please assign all GUI Objects!", EditorStyles.boldLabel);
            GUILayout.Space(10);
            sa.label = (UILabel)EditorGUILayout.ObjectField("Label:", sa.label, typeof(UILabel), true);
        }

        sa.minValue = EditorGUILayout.FloatField("Minimum Value:", Mathf.Clamp(sa.minValue, 0, Mathf.Infinity));
        sa.maxValue = EditorGUILayout.FloatField("Maximum Value:", Mathf.Clamp(sa.maxValue, sa.minValue, Mathf.Infinity));
        sa.defaultValue = EditorGUILayout.Slider("Default Value:", sa.defaultValue, sa.minValue, sa.maxValue);
        sa.decimalPlaces = EditorGUILayout.IntField("Decimal Places:", Mathf.Clamp(sa.decimalPlaces, 0, 10));
        sa.prefix = EditorGUILayout.TextField("Prefix:", sa.prefix);
        sa.suffix = EditorGUILayout.TextField("Suffix:", sa.suffix);

        GUILayout.Space(15);

        sa.isSensitivitySlider = EditorGUILayout.Toggle("Sensitivity:", sa.isSensitivitySlider);
        if (sa.isSensitivitySlider)
        {
            sa.sDir = (SliderAction.SensitivityDir)EditorGUILayout.EnumPopup("Direction", sa.sDir);
        }

        sa.isMouseSmoothingSlider = EditorGUILayout.Toggle("Mouse Smoothing:", sa.isMouseSmoothingSlider);
        sa.isShadowDistanceSlider = EditorGUILayout.Toggle("Shadow Distance:", sa.isShadowDistanceSlider);
        sa.isVegetationDistanceSlider = EditorGUILayout.Toggle("Vegetation Distance:", sa.isVegetationDistanceSlider);
        sa.isVegetationDensitySlider = EditorGUILayout.Toggle("Vegetation Density:", sa.isVegetationDensitySlider);
        sa.isTreeDrawDistanceSlider = EditorGUILayout.Toggle("Tree Draw Distance", sa.isTreeDrawDistanceSlider);
        sa.isMaxTreesSlider = EditorGUILayout.Toggle("Tree Mesh Limit:", sa.isMaxTreesSlider);
        sa.isSoundVolumeSlider = EditorGUILayout.Toggle("Sound Volume:", sa.isSoundVolumeSlider);
        sa.isFOVSlider = EditorGUILayout.Toggle("FOV:", sa.isFOVSlider);
        sa.isGammaSlider = EditorGUILayout.Toggle("Brightness:", sa.isGammaSlider);
        sa.isGameDurationSlider = EditorGUILayout.Toggle("Game Duration:", sa.isGameDurationSlider);
        if (sa.isGameDurationSlider)
        {
            EditorGUI.indentLevel += 1;
            isOpen = EditorGUILayout.Foldout(isOpen, "Available Durations (" + sa.availableDurations.Length + "):");
            if (isOpen)
            {
                int length = sa.availableDurations.Length;
                int[] tempStorage = sa.availableDurations;
                EditorGUI.indentLevel += 1;
                length = EditorGUILayout.IntField("Length:", length);
                if (length != sa.availableDurations.Length)
                {
                    sa.availableDurations = new int[length];
                    for (int i = 0; i < tempStorage.Length; i++)
                    {
                        if (i < sa.availableDurations.Length)
                        {
                            sa.availableDurations[i] = tempStorage[i];
                        }
                    }
                }
                EditorGUI.indentLevel += 1;
                for (int i = 0; i < length; i++)
                {
                    sa.availableDurations[i] = EditorGUILayout.IntField("Element " + i.ToString() + ":", sa.availableDurations[i]);
                }
                EditorGUI.indentLevel -= 1;
                EditorGUI.indentLevel -= 1;
            }
            EditorGUI.indentLevel -= 1;
        }

        sa.isRoundAmountSlider = EditorGUILayout.Toggle("Round Amount:", sa.isRoundAmountSlider);
        if (sa.isRoundAmountSlider)
        {
            EditorGUI.indentLevel += 1;
            isOpen1 = EditorGUILayout.Foldout(isOpen1, "Available Round Amounts (" + sa.availableRoundCounts.Length + "):");
            if (isOpen1)
            {
                int length = sa.availableRoundCounts.Length;
                int[] tempStorage = sa.availableRoundCounts;
                EditorGUI.indentLevel += 1;
                length = EditorGUILayout.IntField("Length:", length);
                if (length != sa.availableRoundCounts.Length)
                {
                    sa.availableRoundCounts = new int[length];
                    for (int i = 0; i < tempStorage.Length; i++)
                    {
                        if (i < sa.availableRoundCounts.Length)
                        {
                            sa.availableRoundCounts[i] = tempStorage[i];
                        }
                    }
                }
                EditorGUI.indentLevel += 1;
                for (int i = 0; i < length; i++)
                {
                    sa.availableRoundCounts[i] = EditorGUILayout.IntField("Element " + i.ToString() + ":", sa.availableRoundCounts[i]);
                }
                EditorGUI.indentLevel -= 1;
                EditorGUI.indentLevel -= 1;
            }
            EditorGUI.indentLevel -= 1;
        }

        sa.isIdleTimerSlider = EditorGUILayout.Toggle("Idle Timer Limit:", sa.isIdleTimerSlider);
        if (sa.isIdleTimerSlider)
        {
            EditorGUI.indentLevel += 1;
            isOpen2 = EditorGUILayout.Foldout(isOpen2, "Available Idle Times (" + sa.availableIdleLimit.Length + "):");
            if (isOpen2)
            {
                int length = sa.availableIdleLimit.Length;
                int[] tempStorage = sa.availableIdleLimit;
                EditorGUI.indentLevel += 1;
                length = EditorGUILayout.IntField("Length:", length);
                if (length != sa.availableIdleLimit.Length)
                {
                    sa.availableIdleLimit = new int[length];
                    for (int i = 0; i < tempStorage.Length; i++)
                    {
                        if (i < sa.availableIdleLimit.Length)
                        {
                            sa.availableIdleLimit[i] = tempStorage[i];
                        }
                    }
                }
                EditorGUI.indentLevel += 1;
                for (int i = 0; i < length; i++)
                {
                    sa.availableIdleLimit[i] = EditorGUILayout.IntField("Element " + i.ToString() + ":", sa.availableIdleLimit[i]);
                }
                EditorGUI.indentLevel -= 1;
                EditorGUI.indentLevel -= 1;
            }
            EditorGUI.indentLevel -= 1;
        }

        sa.isMaxPlayerSlider = EditorGUILayout.Toggle("Max Players:", sa.isMaxPlayerSlider);
        sa.isBotCountSlider = EditorGUILayout.Toggle("Bot Count:", sa.isBotCountSlider);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(sa);
        }
    }
}