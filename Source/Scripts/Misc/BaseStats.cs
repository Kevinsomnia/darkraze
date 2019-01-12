using UnityEngine;
using System.Collections;

public class BaseStats : MonoBehaviour {
    private int _chealth = 100;
	public int curHealth {
        get {
            if(isLocalPlayer && Application.isPlaying) {
                _chealth = AntiHackSystem.RetrieveInt("curHealth");
            }

            return _chealth;
        }
        set {
            _chealth = value;

            if(isLocalPlayer && Application.isPlaying) {
                AntiHackSystem.ProtectInt("curHealth", _chealth);
            }
        }
    }

	public int maxHealth = 100;

	[HideInInspector] public bool headshot = false;
    [HideInInspector] public bool isLocalPlayer = false;
    [HideInInspector] public bool hasLimbs = false;
    [HideInInspector] public bool hitIndication = true;

	public virtual void ApplyDamageMain(int damage, bool showBlood) {
		ApplyDamageMain(damage, showBlood, Limb.LimbType.None);
	}

	public virtual void ApplyDamageMain(int damage, bool showBlood, Limb.LimbType bodyPart) {
		Debug.Log("Override me: Sent over local.");
	}

	public virtual void ApplyDamageNetwork(byte damage, byte senderID, byte weaponID, byte bodyPart) {
		Debug.Log("Override me: Sent over network.");
	}

    public virtual void NetworkEMP() {
        Debug.Log("Override me: Networked EMP");
    }
}