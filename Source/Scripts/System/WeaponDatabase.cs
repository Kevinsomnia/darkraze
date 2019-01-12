using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponDatabase : MonoBehaviour
{
    public static bool initialized = false;
    public static GunController[] customWeaponList = new GunController[0];

    private static WeaponList _savedWL;
    public static WeaponList savedWeaponList
    {
        get
        {
            if (_savedWL == null)
            {
                _savedWL = (WeaponList)Resources.Load("Static Prefabs/Weapon List", typeof(WeaponList));
            }

            return _savedWL;
        }
        set
        {
            _savedWL = value;
        }
    }

    public static GunController[] publicGunControllers
    {
        get
        {
            if (!initialized)
            {
                Initialize();
            }

            return customWeaponList;
        }
    }

    public static void ClearIDs()
    {
        customWeaponList = savedWeaponList.savedWeapons;

        for (int i = 0; i < customWeaponList.Length; i++)
        {
            if (customWeaponList[i] != null)
            {
                customWeaponList[i].weaponID = -1;
            }
        }
    }

    public static void RefreshIDs()
    {
        ClearIDs();
        Initialize();
    }

    public static void Initialize()
    {
        customWeaponList = savedWeaponList.savedWeapons;

        for (int i = 0; i < customWeaponList.Length; i++)
        {
            if (customWeaponList[i] == null)
            {
                continue;
            }

            customWeaponList[i].weaponID = i;
        }

        initialized = true;
    }

    public static GunController GetWeaponByID(int id)
    {
        if (id >= 0 && id < publicGunControllers.Length)
        {
            return publicGunControllers[id];
        }

        Debug.Log("WARNING: Index out of range");
        return null;
    }

    public static int GetAvailableID(int id)
    {
        return Mathf.Clamp(id, 0, publicGunControllers.Length - 1);
    }
}