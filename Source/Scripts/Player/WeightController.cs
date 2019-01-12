using UnityEngine;
using System.Collections;

public class WeightController : MonoBehaviour
{
    public float maximumWeight = 20f; //In kilograms.

    [HideInInspector] public float curWeight;
    [HideInInspector] public float weightPercentage;

    private WeaponManager wm;

    void Start()
    {
        wm = GeneralVariables.playerRef.wm;
    }

    void Update()
    {
        CalculateCurrentWeight();
    }

    private void CalculateCurrentWeight()
    {
        curWeight = 0f;
        if (wm.heldWeapons[0])
        {
            float isEquipped = (wm.currentGC == wm.heldWeapons[0]) ? 1f : 0.5f;
            curWeight += (wm.heldWeapons[0].weaponWeight * isEquipped);
        }
        if (wm.heldWeapons[1])
        {
            float isEquipped = (wm.currentGC == wm.heldWeapons[1]) ? 1f : 0.5f;
            curWeight += (wm.heldWeapons[1].weaponWeight * isEquipped);
        }

        curWeight = Mathf.Clamp(curWeight, 0f, maximumWeight);
        weightPercentage = curWeight / maximumWeight;
    }
}