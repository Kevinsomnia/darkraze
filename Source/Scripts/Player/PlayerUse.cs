using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerUse : MonoBehaviour
{
    public float useRange = 2.2f;
    public float scanRate = 0.25f;
    public float dotThreshold = 0.9f;
    public LayerMask layersToUse = -1;

    [HideInInspector] public UsableObject selectedUsable;

    private Transform tr;
    private Transform mainCamTr;
    private Collider[] collidersInRange;
    private List<UsableObject> usableObjectsInRange = new List<UsableObject>();
    private UsableObject foundUO;

    private UILabel useLabel;
    private string useKeyString;

    private float lastScan = 0f;
    private RaycastHit checkHit;
    private float nearestDot = -1f;
    private float curDot = 0f;

    private DynamicMovement dm;
    private WeaponManager wm;
    private AimController ac;
    private CameraBob cb;

    void Start()
    {
        tr = transform;

        PlayerReference pr = GeneralVariables.playerRef;
        dm = pr.dm;
        wm = pr.wm;
        ac = pr.ac;
        cb = pr.cb;

        mainCamTr = GeneralVariables.mainPlayerCamera.transform;
        useLabel = GeneralVariables.uiController.useGUI;
        useKeyString = cInput.GetText("Use", 1);
    }

    void Update()
    {
        if (Time.unscaledTime - lastScan >= scanRate)
        {
            ScanUsables();
            lastScan = Time.unscaledTime;
        }

        if (RestrictionManager.restricted)
        {
            return;
        }

        bool isNotReloading = (wm.currentGC != null) ? !wm.currentGC.reloading : true;

        if (selectedUsable && !dm.animationIsPlaying && isNotReloading && !ac.isAiming)
        {
            if (cInput.GetButtonDown("Use"))
            {
                selectedUsable.OnPlayerUse();
            }

            useLabel.alpha = Mathf.MoveTowards(useLabel.alpha, useLabel.defaultAlpha, Time.deltaTime * 8f);
            if (selectedUsable.weaponPickup.enabled)
            {
                useLabel.text = "[" + useKeyString + "] to pick up " + selectedUsable.objectName;
            }
            else if (selectedUsable.drivableVehicle.enabled)
            {
                useLabel.text = "[" + useKeyString + "] to enter " + selectedUsable.objectName;
            }
            else
            {
                useLabel.text = "[" + useKeyString + "] to use " + selectedUsable.objectName;
            }
        }
        else
        {
            useLabel.alpha = Mathf.MoveTowards(useLabel.alpha, 0f, Time.deltaTime * 8f);
        }

        useLabel.transform.localScale = Vector3.one * (0.97f + (useLabel.alpha * 0.03f));
    }

    private void ScanUsables()
    {
        collidersInRange = Physics.OverlapSphere(tr.position, useRange, layersToUse);
        usableObjectsInRange.Clear();
        nearestDot = dotThreshold;
        selectedUsable = null;

        if (collidersInRange.Length <= 0)
        {
            return;
        }

        foreach (Collider col in collidersInRange)
        {
            foundUO = col.GetComponent<UsableObject>();

            if (foundUO == null)
            {
                foundUO = col.transform.root.GetComponent<UsableObject>();
            }

            if (foundUO != null)
            {
                Vector3 dir = (col.bounds.center - mainCamTr.position);
                if (Physics.Raycast(mainCamTr.position, dir, out checkHit, useRange, layersToUse.value))
                {
                    if (checkHit.collider.GetInstanceID() == col.GetInstanceID())
                    {
                        usableObjectsInRange.Add(foundUO);
                    }
                }
            }
        }

        if (usableObjectsInRange.Count <= 0)
        {
            return;
        }

        foreach (UsableObject uo in usableObjectsInRange)
        {
            Vector3 dir = (uo.transform.position - mainCamTr.position).normalized;
            curDot = Vector3.Dot(mainCamTr.forward, dir);
            if (curDot >= nearestDot)
            {
                selectedUsable = uo;
                nearestDot = curDot;
            }
        }
    }
}