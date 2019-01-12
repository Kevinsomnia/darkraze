using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PlayerVitals))]
public class PlayerVitalsInspector : Editor {
	private static bool showpainaudio;
	
	public override void OnInspectorGUI() {
		PlayerVitals pv = target as PlayerVitals;		

		if(pv.cam == null) {
			pv.cam = (GameObject)EditorGUILayout.ObjectField("Camera", pv.cam, typeof(GameObject), true);
		}
		if(pv.arms == null) {
			pv.arms = (GameObject)EditorGUILayout.ObjectField("Arms", pv.arms, typeof(GameObject), true);
		}
		if(pv.breathingSound == null) {
			pv.breathingSound = (AudioSource)EditorGUILayout.ObjectField("Breathing Source", pv.breathingSound, typeof(AudioSource), true);
		}
		if(pv.heartbeatSound == null) {
			pv.heartbeatSound = (AudioSource)EditorGUILayout.ObjectField("Heartbeat Source", pv.heartbeatSound, typeof(AudioSource), true);
		}
		if(pv.noiseSource == null) {
			pv.noiseSource = (AudioSource)EditorGUILayout.ObjectField("Distort Noise Source", pv.noiseSource, typeof(AudioSource), true);
		}
		if(pv.shieldAlarmSource == null) {
			pv.shieldAlarmSource = (AudioSource)EditorGUILayout.ObjectField("Shield Alarm Source", pv.shieldAlarmSource, typeof(AudioSource), true);
		}
		if(pv.fallDamageSource == null) {
			pv.fallDamageSource = (AudioSource)EditorGUILayout.ObjectField("Fall Damage Source", pv.fallDamageSource, typeof(AudioSource), true);
		}
		if(pv.equipmentRattleSource == null) {
			pv.equipmentRattleSource = (AudioSource)EditorGUILayout.ObjectField("Equipment Rattle Source", pv.equipmentRattleSource, typeof(AudioSource), true);
		}
		
		GUILayout.Label("HEALTH VALUES", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();
	    GUILayout.Space(20);
		GUILayout.BeginVertical();
		pv.curHealth = EditorGUILayout.IntSlider("Current Health:", pv.curHealth, Mathf.Min(pv.maxHealth, 1), pv.maxHealth);
		pv.maxHealth = EditorGUILayout.IntField("Maximum Health:", pv.maxHealth);
		pv.healthRecoverDelay = EditorGUILayout.FloatField("Recovery Delay:", pv.healthRecoverDelay);
		EditorGUIUtility.labelWidth = 175f;
		pv.healthRecoverySpeed = EditorGUILayout.FloatField("Recovery Rate: (" + ((1 / pv.healthRecoverySpeed) * pv.healthRecoverAmount).ToString("F2") + " HP/s)", Mathf.Clamp(pv.healthRecoverySpeed, 0.001f, 1000000f));
        EditorGUIUtility.LookLikeControls();
        pv.healthRecoverAmount = EditorGUILayout.IntField("  Recover Amount:", Mathf.Clamp(pv.healthRecoverAmount, 1, 1000));
        pv.healthRecoverInfluence = EditorGUILayout.FloatField("Recover Influence:", pv.healthRecoverInfluence);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(5);
		
		GUILayout.Label("SHIELD VALUES", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();
	    GUILayout.Space(20);
		GUILayout.BeginVertical();
		pv.curShield = EditorGUILayout.IntSlider("Current Shield:", pv.curShield, 0, pv.maxShield);
		pv.maxShield = EditorGUILayout.IntField("Maximum Shield:", pv.maxShield);
		pv.shieldRecoverDelay = EditorGUILayout.FloatField("Recovery Delay:", pv.shieldRecoverDelay);
        EditorGUIUtility.labelWidth = 175f;
		pv.shieldRecoverySpeed = EditorGUILayout.FloatField("Recovery Rate: (" + ((1 / pv.shieldRecoverySpeed) * pv.shieldRecoverAmount).ToString("F2") + " SP/s)", Mathf.Clamp(pv.shieldRecoverySpeed, 0.001f, 1000000f));
		EditorGUIUtility.LookLikeControls();
        pv.shieldRecoverAmount = EditorGUILayout.IntField("  Recover Amount:", Mathf.Clamp(pv.shieldRecoverAmount, 1, 1000));
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		GUILayout.Label("STAMINA VALUES", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();
	    GUILayout.Space(20);
		GUILayout.BeginVertical();
        EditorGUIUtility.labelWidth = 175f;
		pv.staminaRecoverySpeed = EditorGUILayout.FloatField("Recovery Rate: (" + (1 / pv.staminaRecoverySpeed).ToString("F2") + " ST/s)", Mathf.Clamp(pv.staminaRecoverySpeed, 0.001f, Mathf.Infinity));
		pv.staminaDepletionRate = EditorGUILayout.FloatField("Depletion Rate: (" + (1 / pv.staminaDepletionRate).ToString("F2") + " ST/s)", Mathf.Clamp(pv.staminaDepletionRate, 0.001f, Mathf.Infinity));
        EditorGUIUtility.LookLikeControls();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		GUILayout.Label("AUDIO VALUES", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();
	    GUILayout.Space(20);
		GUILayout.BeginVertical();
		showpainaudio = EditorGUILayout.Foldout(showpainaudio, "Pain Sounds");
		if(showpainaudio) {
			EditorGUI.indentLevel += 1;
			int length = pv.painSounds.Length;
			length = EditorGUILayout.IntField("Length:", length);
			if(length != pv.painSounds.Length) {
				AudioClip[] tempAudio = pv.painSounds;
				pv.painSounds = new AudioClip[length];
				for(int i = 0; i < tempAudio.Length; i++) {
					if(i < pv.painSounds.Length) {
						pv.painSounds[i] = tempAudio[i];
					}
				}
			}
			EditorGUI.indentLevel += 1;
			for(int i = 0; i < pv.painSounds.Length; i++) {
				pv.painSounds[i] = (AudioClip)EditorGUILayout.ObjectField("Element " + i.ToString(), pv.painSounds[i], typeof(AudioClip), true);
			}
			EditorGUI.indentLevel -= 1;
			EditorGUI.indentLevel -= 1;
		}
		GUILayout.Space(10);
		pv.deathSound = (AudioClip)EditorGUILayout.ObjectField("Death:", pv.deathSound, typeof(AudioClip), true);
		pv.fallDamageSound = (AudioClip)EditorGUILayout.ObjectField("Fall Damage:", pv.fallDamageSound, typeof(AudioClip), true);
        pv.healthDamage = (AudioClip)EditorGUILayout.ObjectField("Health Damage:", pv.healthDamage, typeof(AudioClip), true);
        pv.shieldDamage = (AudioClip)EditorGUILayout.ObjectField("Shield Damage:", pv.shieldDamage, typeof(AudioClip), true);
        pv.shieldFailure = (AudioClip)EditorGUILayout.ObjectField("Shield Failure:", pv.shieldFailure, typeof(AudioClip), true);
        pv.shieldRegen = (AudioClip)EditorGUILayout.ObjectField("Shield Recover:", pv.shieldRegen, typeof(AudioClip), true);
        pv.noBreathExhale = (AudioClip)EditorGUILayout.ObjectField("No Breath Exhale:", pv.noBreathExhale, typeof(AudioClip), true);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		GUILayout.Label("MISCELLANEOUS VALUES", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();
	    GUILayout.Space(20);
		GUILayout.BeginVertical();
		pv.effectIntensity = EditorGUILayout.Slider("Effect Intensity:", pv.effectIntensity, 0, 10);
		GUILayout.Space (5f);
        EditorGUIUtility.labelWidth = 145f;
		pv.fallDamageTolerance = EditorGUILayout.FloatField("Fall Damage Tolerance:", pv.fallDamageTolerance);
		EditorGUIUtility.LookLikeControls();
		pv.fallDamageCurve = EditorGUILayout.FloatField("Fall Damage Curve:", pv.fallDamageCurve);
		GUILayout.Space (5f);
		pv.healthLowPass = (AudioLowPassFilter)EditorGUILayout.ObjectField("Low Pass Filter:", pv.healthLowPass, typeof(AudioLowPassFilter), true);
		pv.deathReplacement = (GameObject)EditorGUILayout.ObjectField("Death Replacement:", pv.deathReplacement, typeof(GameObject), true);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(15);

		if(GUI.changed) {
			EditorUtility.SetDirty(pv);
		}
	}
}