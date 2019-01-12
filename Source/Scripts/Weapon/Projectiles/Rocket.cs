using UnityEngine;
using System.Collections;

public class Rocket : PoolItem
{
    public MeshRenderer displayMesh;
    public TrailController rocketTrail;
    public GameObject explosion;
    public float explosionOffset = 0.1f;
    public Vector3 rotationOffset = Vector3.zero;
    public int impactBonusDamage = 25;
    public float flightRandomness = 0.05f;
    public TrailController.EmissionSettings trailSettings = new TrailController.EmissionSettings();
    public LayerMask layersToHit = -1;

    [HideInInspector] public bool isDisplay;

    private Transform tr;

    private static TrailController _tc;
    private TrailController trailInstance
    {
        get
        {
            if (_tc == null && rocketTrail != null)
            {
                _tc = (TrailController)Instantiate(rocketTrail);
            }

            return _tc;
        }
    }

    private bool player;
    private bool friendlyFire;
    private int botIndex = -1;
    private int gunID = -1;
    private Vector3 oldPos;
    private Vector3 newPos;
    private Vector3 gravityVector;
    private int damage;
    private float dist;
    private float speed;
    private float gFactor;
    private float explRadius;

    private Vector3 defFwd;
    private float randTimer;
    private float randStart;

    public override void InstantiateStart()
    {
        tr = transform;

        StopCoroutine("PoolRocket");
        StartCoroutine(PoolRocket(15f));

        oldPos = tr.position;
        newPos = oldPos;
        gravityVector = Vector3.zero;

        defFwd = tr.forward;
        randStart = Random.value;
        randTimer = 0f;

        if (displayMesh != null)
        {
            displayMesh.enabled = true;
            displayMesh.transform.rotation = Quaternion.LookRotation(tr.forward);
        }

        if (trailInstance != null)
        {
            TrailController.EmitterClass newEC = new TrailController.EmitterClass();
            newEC.emitterTransform = tr;
            newEC.settings = trailSettings;
            trailInstance.AddToEmitters(newEC);
        }
    }

    void Update()
    {
        oldPos = tr.position;

        if (flightRandomness > 0f)
        {
            randTimer += Time.deltaTime * 5f;
            float x = Mathf.PerlinNoise(randTimer, 0f) - 0.5f;
            float y = Mathf.PerlinNoise(0f, randTimer) - 0.5f;
            tr.localRotation = Quaternion.LookRotation(defFwd + new Vector3(x * 1.5f, y, 0f) * flightRandomness);
        }

        newPos += (tr.forward * speed) * Time.deltaTime;
        gravityVector += Physics.gravity * gFactor * Time.deltaTime;
        newPos += gravityVector * Time.deltaTime;

        Vector3 dir = (newPos - oldPos);
        dist = dir.magnitude;

        if (displayMesh != null && dir != Vector3.zero)
        {
            displayMesh.transform.rotation = Quaternion.LookRotation(dir);
        }

        if (dist > 0f)
        {
            RaycastHit hit;
            if (Physics.Raycast(oldPos, dir, out hit, dist, layersToHit.value))
            {
                OnImpact(hit, hit.collider);
                return;
            }
            else
            {
                tr.position = newPos;
            }
        }
    }

    private void OnImpact(RaycastHit impactInfo, Collider hitTarget)
    {
        Quaternion newRotation = Quaternion.LookRotation(impactInfo.normal) * Quaternion.Euler(rotationOffset);
        GameObject expl = (GameObject)Instantiate(explosion, impactInfo.point + (impactInfo.normal * explosionOffset), newRotation);
        AreaDamage aDmg = expl.GetComponent<AreaDamage>();

        if (aDmg != null)
        {
            if (isDisplay)
            {
                aDmg.enabled = false;
            }
            else
            {
                aDmg.hitTarget = hitTarget;
                aDmg.bonusDamage = impactBonusDamage;
                aDmg.overrideMaxDmg = damage;
                aDmg.overrideMaxRange = explRadius;
                aDmg.isPlayer = player;
                aDmg.botIndex = botIndex;
                aDmg.friendlyFire = friendlyFire;
                aDmg.wepIndex = gunID;
                aDmg.layersToDamage = layersToHit;
            }
        }

        if (trailInstance != null)
        {
            trailInstance.RemoveFromEmitters(tr);
        }

        AddToPool();
    }

    public void RocketInfo(BulletInfo bi, int weaponIndex, bool isVisuals = false, bool isPlayer = false, int botID = -1)
    {
        gunID = weaponIndex;
        damage = bi.damage;
        speed = bi.muzzleVelocity;
        gFactor = bi.gravityFactor;
        isDisplay = isVisuals;
        player = (isPlayer && botID <= -1);
        botIndex = botID;
        explRadius = bi.explosionRadius;

        friendlyFire = true;
        if (Topan.Network.isConnected && NetworkingGeneral.currentGameType.customSettings.ContainsKey("Friendly Fire"))
        {
            friendlyFire = DarkRef.ConvertStringToBool(NetworkingGeneral.currentGameType.customSettings["Friendly Fire"].currentValue);
        }
    }

    private IEnumerator PoolRocket(float time)
    {
        float timer = 0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (trailInstance != null)
        {
            trailInstance.RemoveFromEmitters(tr);
        }

        AddToPool();
    }
}