using UnityEngine;
using System.Collections;

public class GrenadeAmmoManager : MonoBehaviour {
	public int grenadeTypeOne = 0;
	public int typeOneGrenades = 3;
	public int typeOneMaxGrenades = 3;
	
	public int grenadeTypeTwo = 1;
	public int typeTwoGrenades = 3;
	public int typeTwoMaxGrenades = 3;
	
	public bool grenadeIsAvailable {
		get {
            return (AntiHackSystem.RetrieveInt("t1Grenade") > 0 || AntiHackSystem.RetrieveInt("t2Grenade") > 0);
		}
	}
	
    private float lastUpdateTime;
	private UILabel slotOneLabel;
	private UILabel slotTwoLabel;
    private PlayerEffects pe;
	
	void Start() {
		slotOneLabel = GeneralVariables.uiController.grenadeOneLabel;
		slotTwoLabel = GeneralVariables.uiController.grenadeTwoLabel;

        AntiHackSystem.ProtectInt("t1Grenade", typeOneGrenades);
        AntiHackSystem.ProtectInt("t1GrenadeMax", typeOneMaxGrenades);
        AntiHackSystem.ProtectInt("t2Grenade", typeTwoGrenades);
        AntiHackSystem.ProtectInt("t2GrenadeMax", typeTwoMaxGrenades);
        pe = GeneralVariables.player.GetComponent<PlayerEffects>();
	}
	
	void Update() {
		ClampGrenadeAmount();

        if(Time.time - lastUpdateTime >= 0.1f) {
            slotOneLabel.text = (pe.hasEMP) ? Random.Range(0, 10).ToString() : AntiHackSystem.RetrieveInt("t1Grenade").ToString();
            slotTwoLabel.text = (pe.hasEMP) ? Random.Range(0, 10).ToString() : AntiHackSystem.RetrieveInt("t2Grenade").ToString();

            lastUpdateTime = Time.time;
        }
	}
	
	public void ChangeGrenadeAmount(int id, int amount) {
		if(id == grenadeTypeOne) {
            AntiHackSystem.ProtectInt("t1Grenade", typeOneGrenades + amount);
		}
		else if(id == grenadeTypeTwo) {
            AntiHackSystem.ProtectInt("t2Grenade", typeTwoGrenades + amount);
		}
		
		ClampGrenadeAmount();
	}
	
	private void ClampGrenadeAmount() {
        typeOneMaxGrenades = AntiHackSystem.RetrieveInt("t1GrenadeMax");
        typeTwoMaxGrenades = AntiHackSystem.RetrieveInt("t2GrenadeMax");
        typeOneGrenades = Mathf.Clamp(AntiHackSystem.RetrieveInt("t1Grenade"), 0, typeOneMaxGrenades);
        typeTwoGrenades = Mathf.Clamp(AntiHackSystem.RetrieveInt("t2Grenade"), 0, typeTwoMaxGrenades);
	}
}