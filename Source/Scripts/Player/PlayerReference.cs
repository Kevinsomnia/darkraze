using UnityEngine;
using System.Collections;

public class PlayerReference : MonoBehaviour {
	public DynamicMovement dm;
	public WeaponManager wm;
	public GrenadeAmmoManager gam;
	public ImpactAnimation ia;
	public AimController ac;
	public AntiClipSystem acs;
	public CameraBob cb;
	public WeightController wc;
	
	void Awake() {
		GeneralVariables.playerRef = this;
	}
}