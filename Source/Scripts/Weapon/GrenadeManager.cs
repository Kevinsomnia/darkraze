using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrenadeManager : MonoBehaviour {
	public GrenadeList nadeList;
	
	[HideInInspector] public GrenadeController curGrenade;
	[HideInInspector] public GrenadeController[] grenadeInventory = new GrenadeController[2];
	
	[HideInInspector] public DynamicMovement dm;
	[HideInInspector] public WeaponManager wm;
	[HideInInspector] public GrenadeAmmoManager gam;

    [HideInInspector] public List<PlasticExplosive> detonationList = new List<PlasticExplosive>();
    [HideInInspector] public int grenadeIndex;

	public bool canSwitchToGrenade {
		get {
			CheckGrenades();
			return (availableGrenades.Count > 0);
		}
	}
	
	[HideInInspector] public List<GrenadeController> availableGrenades = new List<GrenadeController>();

	private bool canSwitch;
	
	public void Initialize() {
		if(nadeList != null) {
			GrenadeDatabase.savedGrenadeList = nadeList;
		}
		
		PlayerReference playerRef = GeneralVariables.playerRef;
		wm = playerRef.wm;
		dm = playerRef.dm;
		gam = playerRef.gam;
				
		grenadeInventory[0] = AddGrenadeToInventory(gam.grenadeTypeOne);
		grenadeInventory[1] = AddGrenadeToInventory(gam.grenadeTypeTwo);
		
		CheckGrenades();
		
		curGrenade = availableGrenades[0];
		curGrenade.OnSelect();
		canSwitch = true;
		gameObject.SetActive(false);
	}

    void OnDestroy() {
        for(int i = 0; i < detonationList.Count; i++) {
            detonationList[i].RemoveInstance(10f);
        }
    }
	
	void Update() {
		if(Input.GetKeyDown(KeyCode.Alpha4)) {
			CheckGrenades();

			if(!dm.animationIsPlaying && canSwitch && availableGrenades.Count > 1) {
				SelectNextGrenade();
			}
		}
	}
	
	public void OnSelect(bool multiplayer) {
        if(multiplayer) {
            if(Topan.Network.isConnected && wm.rootNetView != null) {
                wm.rootNetView.RPC(Topan.RPCMode.Others, "NetworkSelectGrenade", (byte)curGrenade.grenadeID);
            }
        }
        else {
		    DeselectAll();
            curGrenade = availableGrenades[Mathf.Clamp(grenadeIndex, 0, availableGrenades.Count - 1)];
		    curGrenade.gameObject.SetActive(true);
			curGrenade.OnSelect();
		    dm.sao = curGrenade.GetComponent<SprintAnimOverride>();
        }
	}
	
	public void OnDeselect() {
		//Nothing yet...
	}
	
	public void DeselectAll() {
		foreach(GrenadeController gc in grenadeInventory) {
			if(gc != null) {
				gc.gameObject.SetActive(false);
			}
		}
	}
	
	private void SelectNextGrenade(bool instant = false) {
		CheckGrenades();
		grenadeIndex++;

		if(grenadeIndex >= grenadeInventory.Length) {
			grenadeIndex = 0;
		}
		
		StartCoroutine(SelectGrenade(availableGrenades[Mathf.Clamp(grenadeIndex, 0, availableGrenades.Count - 1)], instant));
	}
	
	public IEnumerator SelectGrenade(GrenadeController grenade, bool immediate) {
        if(Topan.Network.isConnected && wm.rootNetView != null) {
            wm.rootNetView.RPC(Topan.RPCMode.Others, "NetworkSelectGrenade", (byte)grenade.grenadeID);
        }

        if(!immediate) {
            dm.Draw(wm.drawTime);
            canSwitch = false;
            yield return new WaitForSeconds(wm.drawTime);
            wm.GetComponent<AudioSource>().PlayOneShot(wm.drawSound, 0.2f);
        }

		DeselectAll();
		curGrenade = grenade;
		curGrenade.gameObject.SetActive(true);
		curGrenade.OnSelect();
		dm.sao = curGrenade.GetComponent<SprintAnimOverride>();

		canSwitch = true;
	}
	
	private void CheckGrenades() {
		availableGrenades.Clear();

		if(grenadeInventory[0] != null && ((gam.grenadeTypeOne > -1 && gam.typeOneGrenades > 0) || grenadeInventory[0].couldSwitchDetonate)) {
			availableGrenades.Add(grenadeInventory[0]);
		}

        if(grenadeInventory[1] != null && ((gam.grenadeTypeTwo > -1 && gam.typeTwoGrenades > 0) || grenadeInventory[1].couldSwitchDetonate)) {
			availableGrenades.Add(grenadeInventory[1]);
		}
	}
	
	public GrenadeController AddGrenadeToInventory(int targetID) {
		if(targetID < 0) {
			return null;
		}

		GrenadeController instantiation = (GrenadeController)Instantiate(GrenadeDatabase.GetGrenadeByID(targetID));
		
		if(instantiation != null) {
			Transform tra = instantiation.transform;
			tra.parent = transform;
			tra.localPosition = Vector3.zero;
			tra.localRotation = Quaternion.identity;
			instantiation.Initialize();
			instantiation.gameObject.SetActive(false);
		}
		
		return instantiation;
	}
	
	public void RemoveGrenadeFromInventory(int index) {
		int sn = Mathf.Clamp(index, 0, 1);
		
		if(grenadeInventory[sn]) {
			if(curGrenade == grenadeInventory[sn]) {
				curGrenade = null;
			}
						
			Destroy(grenadeInventory[sn].gameObject);
			if(sn == 0) {
				gam.grenadeTypeOne = -1;
			}
			else if(sn == 1) {
				gam.grenadeTypeTwo = -1;
			}
			
			grenadeInventory[sn] = null;
			SwitchToAvailableGrenade();
		}
	}
	
	public void SwitchToAvailableGrenade(bool instant = false) {
		CheckGrenades();
		
		if(availableGrenades.Count > 0) {
			SelectNextGrenade(instant);
		}
		else {
			wm.FindWeaponToUse(false);
		}
	}

    public void QuickThrow(int prevWeaponIndex) {
        StartCoroutine(QuickThrowCoroutine(prevWeaponIndex));
    }

    private IEnumerator QuickThrowCoroutine(int prevWeapon) {
        wm.isQuickThrowState = true;
        yield return new WaitForSeconds(0.25f);

        curGrenade.targetStrength = curGrenade.throwStrength;
        curGrenade.isQuickThrow = true;
        curGrenade.PullPin();

        while(!RestrictionManager.restricted && cInput.GetButton("Throw Grenade")) {
            yield return null;
        }

        yield return new WaitForSeconds(0.48f);
        wm.SelectWeapon(prevWeapon);
        curGrenade.isQuickThrow = false;
        wm.isQuickThrowState = false;
    }
}