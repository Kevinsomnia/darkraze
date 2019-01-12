using UnityEngine;
using System.Collections;

public enum GrenadeType { Explosive, Smoke, Sticky }
public class GrenadeScript : MonoBehaviour
{
    public GrenadeType grenadeType = GrenadeType.Explosive;
    public bool startDelayOnImpact = false;
    public float detonationDelay = 4f;
    public int explosionDamage = 150;
    public float explosionRadius = 8f;
    public GameObject explosionPrefab;
    public ParticleSystem smokeEmitter;
    public AudioClip beepSound;

    [HideInInspector] public bool onlyVisual = false;
    [HideInInspector] public int myID = -1;
    [HideInInspector] public int databaseID = -1;

    private bool isReadyForSync;
    private float syncTime;
    private Vector3 lastSync;
    private float startTime = -1f;
    private bool pulledPin = false;
    private bool exploded = false;

    void Start()
    {
        if (Topan.Network.isConnected && onlyVisual)
        {
            NetworkingGeneral.GrenadeSync newSync = new NetworkingGeneral.GrenadeSync();
            newSync.instance = GetComponent<Rigidbody>();

            NetworkingGeneral.syncGrenadesList.Add(myID, newSync);
        }

        if (grenadeType == GrenadeType.Smoke)
        {
            smokeEmitter.enableEmission = false;
        }
        else if (grenadeType == GrenadeType.Sticky)
        {
            GetComponent<AudioSource>().PlayOneShot(beepSound);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (grenadeType == GrenadeType.Sticky)
        {
            if (!col.gameObject.isStatic && col.rigidbody)
            {
                transform.parent = col.transform;
            }

            GetComponent<Collider>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }
        else if (startDelayOnImpact && startTime < 0f)
        {
            startTime = Time.time;
        }
    }

    void Update()
    {
        if (!startDelayOnImpact)
        {
            if (Time.time - startTime >= detonationDelay && !exploded && pulledPin)
            {
                Explode();
            }
        }
        else
        {
            if (startTime > 0f && Time.time - startTime >= detonationDelay && !exploded && pulledPin)
            {
                Explode();
            }
        }

        if (Topan.Network.isConnected && !onlyVisual && Time.time - syncTime >= 0.35f && (GetComponent<Rigidbody>().position - lastSync).sqrMagnitude >= 0.0024f)
        {
            GeneralVariables.connectionView.RPC(Topan.RPCMode.Others, "SyncGrenade", myID, GetComponent<Rigidbody>().position, GetComponent<Rigidbody>().velocity);
            lastSync = GetComponent<Rigidbody>().position;
            syncTime = Time.time;
        }
    }

    void OnDestroy()
    {
        if (Topan.Network.isConnected && onlyVisual && grenadeType == GrenadeType.Smoke)
        {
            NetworkingGeneral.syncGrenadesList.Remove(myID);
        }
    }

    private void Explode()
    {
        if (grenadeType == GrenadeType.Explosive || grenadeType == GrenadeType.Sticky)
        {
            GameObject go = (GameObject)Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            AreaDamage aDmg = go.GetComponent<AreaDamage>();
            if (aDmg != null)
            {
                if (onlyVisual)
                {
                    aDmg.enabled = false;
                }
                else
                {
                    aDmg.isPlayer = true;
                    aDmg.wepIndex = -1;
                    aDmg.grenIndex = databaseID;
                    aDmg.overrideMaxDmg = explosionDamage;
                    aDmg.overrideMaxRange = explosionRadius;
                }
            }

            if (Topan.Network.isConnected && onlyVisual)
            {
                NetworkingGeneral.syncGrenadesList.Remove(myID);
            }

            Destroy(gameObject);
        }
        else if (grenadeType == GrenadeType.Smoke)
        {
            smokeEmitter.enableEmission = true;
        }

        exploded = true;
    }

    public void PulledPin()
    {
        pulledPin = true;

        if (!startDelayOnImpact)
        {
            startTime = Time.time;
        }
    }
}