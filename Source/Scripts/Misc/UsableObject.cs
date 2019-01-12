using UnityEngine;
using System.Collections;

//Network View must be attached to this object for multiplayer syncing!
public class UsableObject : MonoBehaviour
{
    [System.Serializable]
    public class WeaponPickup
    {
        public bool enabled = false;
        public WeaponSlot weaponSlot = WeaponSlot.Primary;
        public int weaponID;
        public int ammoAmount = 30;
        public int reserveAmmo = 270;

        [HideInInspector] public bool chamberedBullet = false;
    }

    [System.Serializable]
    public class DrivableVehicle
    {
        public bool enabled = false;
    }

    public string objectName = "Usable Object";
    public WeaponPickup weaponPickup = new WeaponPickup();
    public DrivableVehicle drivableVehicle = new DrivableVehicle();

    private WeaponManager _wm;
    private WeaponManager wManager
    {
        get
        {
            if (_wm == null && GeneralVariables.playerRef != null)
            {
                _wm = GeneralVariables.playerRef.wm;
            }

            return _wm;
        }
    }

    private bool insideVehicle;
    private float cooldown;

    void Update()
    {
        if (drivableVehicle.enabled && cInput.GetButtonDown("Use") && insideVehicle && (Time.time - cooldown >= 1f))
        {
            GetOut();
        }
    }

    public void OnPlayerUse()
    {
        if (RestrictionManager.restricted)
        {
            return;
        }

        if (weaponPickup.enabled && wManager != null)
        {
            if (wManager.HasWeapon(weaponPickup.weaponID))
            {
                wManager.AddAmmo(weaponPickup.weaponID, weaponPickup.ammoAmount);
            }
            else
            {
                wManager.ignoreWeightDelayOnce = true;
                wManager.AddWeapon(weaponPickup.weaponID);
                wManager.queuedAmmo = weaponPickup.ammoAmount;
                wManager.queuedReserve = weaponPickup.reserveAmmo;
                wManager.queuedChamber = weaponPickup.chamberedBullet;
            }

            if (Topan.Network.isConnected)
            {
                GetComponent<Topan.NetworkView>().Destroy();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        if (drivableVehicle.enabled)
        {
            EnterVehicle();
        }
    }

    private void GetIn()
    {
        insideVehicle = true;
        cooldown = Time.time;
    }

    private void GetOut()
    {
        insideVehicle = false;
        cooldown = Time.time;
    }

    private void EnterVehicle()
    {
        if (!insideVehicle && (Time.time - cooldown >= 1f))
        {
            GetIn();
        }
    }
}