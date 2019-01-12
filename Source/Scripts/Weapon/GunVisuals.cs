using UnityEngine;
using System.Collections;

public class GunVisuals : MonoBehaviour {
    public float dropLifetime = 60f;
	public ParticleSystem muzzleFlash;
	public ParticleSystem muzzleSmoke;
	public ParticleSystem muzzleGlow;
	public ParticleSystem muzzleSpark;
		
	public Light flashlight;
	
	public GameObject[] activateOnUse;
	public GameObject[] deactivateOnUse;

    private Topan.NetworkView netView;
	
	void NetworkStart() {
        if(!Topan.Network.isConnected) {
            return;
        }

		GunController gc = GetComponent<GunController>();

        netView = GetComponent<Topan.NetworkView>();
		if(gc != null && netView != null && netView.HasInitialData("force")) {
			Vector3 forceDir = Vector3.zero;
			forceDir = (Vector3)netView.GetInitialData("force");

			gc.MakePickup(forceDir);

			if(netView.HasInitialData("curammo") && netView.HasInitialData("ammoleft")) {
				bool chambered = (bool)netView.GetInitialData("chamber");
				int curAmmo = (int)netView.GetInitialData("curammo");
				int ammoLeft = (int)netView.GetInitialData("ammoleft");

				UsableObject uo = GetComponent<UsableObject>();
				uo.weaponPickup.ammoAmount = curAmmo;
				uo.weaponPickup.reserveAmmo = ammoLeft;
				uo.weaponPickup.chamberedBullet = chambered;
			}
		}

        Invoke("AutoDestroy", dropLifetime);
	}

    private void AutoDestroy() {
        if(!Topan.Network.isServer || netView == null) {
            return;
        }

        netView.Destroy();
    }
}