using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretAI : MonoBehaviour
{
    public Transform lineOfSight;
    public List<string> targetTags = new List<string>();
    public Transform turretPivotGO;
    public float maxSightDistance = 20;
    public float minAngle = -120;
    public float maxAngle = 80;
    public float scanSpeed = 10;
    public float scanPauseTime = 1.5f;
    public float followTargetSpeed = 8;
    public float giveUpTime = 2;
    public LayerMask affectedLayers;
    public GameObject shootPosition;
    public BulletInfo bulletInfo;
    public float roundsPerMinute = 600f;
    public AudioClip fireSound;
    public ParticleEmitter muzzleFlash;
    public Light muzzleLight;
    public float spreadAmount = 2;

    private Transform selectedTarget;
    private Vector3 defaultRot;
    private bool detectedTarget;
    private float vx;
    private float vy;
    private float rotY;
    private bool switcher;
    private bool rotating;
    private bool lockedOn;
    private float timer;
    private float giveUpTimer;
    private Vector3 lookPos;
    private Vector3 finalPos;

    void Start()
    {
        defaultRot = shootPosition.transform.localEulerAngles;
        switcher = true;
        rotating = true;
        lockedOn = false;
    }

    void Update()
    {
        if (selectedTarget != null)
        {
            lookPos = new Vector3(selectedTarget.position.x, turretPivotGO.position.y, selectedTarget.position.z);
        }
        else
        {
            lookPos = lineOfSight.position + lineOfSight.forward;
            lookPos.y = turretPivotGO.position.y;
        }

        finalPos = Vector3.Lerp(finalPos, lookPos, Time.deltaTime * followTargetSpeed);

        if (!lockedOn)
        {
            if (rotating)
            {
                rotY += Time.deltaTime * scanSpeed * ((switcher) ? 1f : -1f);
            }

            if (rotY > maxAngle)
            {
                StartCoroutine(ScanPause(false));
            }
            else if (rotY < minAngle)
            {
                StartCoroutine(ScanPause(true));
            }

            rotY = Mathf.Clamp(rotY, minAngle, maxAngle);
            turretPivotGO.localRotation = Quaternion.Slerp(turretPivotGO.localRotation, Quaternion.Euler(0f, rotY, 0f), Time.deltaTime * 4f);
        }
        else if (selectedTarget != null)
        {
            turretPivotGO.LookAt(finalPos);
        }

        RaycastHit los;
        if (Physics.Raycast(lineOfSight.position, lineOfSight.forward, out los, maxSightDistance, affectedLayers.value))
        {
            if (targetTags.Contains(los.collider.tag))
            {
                if (!detectedTarget)
                {
                    timer = 0f;
                }

                detectedTarget = true;
                selectedTarget = los.collider.transform;
                lockedOn = true;
                giveUpTimer = 0f;
            }
            else
            {
                giveUpTimer += Time.deltaTime;
                lookPos = los.point;

                if (giveUpTimer >= giveUpTime)
                {
                    lockedOn = false;
                    detectedTarget = false;
                    selectedTarget = null;
                }
            }
        }
        else
        {
            giveUpTimer += Time.deltaTime;

            if (giveUpTimer >= giveUpTime)
            {
                lockedOn = false;
                detectedTarget = false;
                selectedTarget = null;
            }
        }
    }

    void FixedUpdate()
    {
        if (detectedTarget && timer >= (60f / roundsPerMinute))
        {
            Shoot();
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    private void Shoot()
    {
        GameObject go = PoolManager.Instance.RequestInstantiate(0, shootPosition.transform.position, shootPosition.transform.rotation, false);
        Bullet projBul = go.GetComponent<Bullet>();
        projBul.BulletInfo(bulletInfo, -1);
        projBul.InstantiateStart();

        SpreadControl();
        shootPosition.transform.localEulerAngles = new Vector3(defaultRot.x + vy, defaultRot.y + vx, defaultRot.z);

        if (muzzleFlash != null)
        {
            StartCoroutine(MuzzleControl());
        }

        GetComponent<AudioSource>().PlayOneShot(fireSound);
        timer = 0;
    }

    private IEnumerator MuzzleControl()
    {
        if (Random.value < 0.8f)
        {
            muzzleFlash.Emit();
        }

        muzzleLight.GetComponent<Light>().enabled = true;
        yield return null;
        muzzleLight.GetComponent<Light>().enabled = false;
    }

    private void SpreadControl()
    {
        vx = Random.Range(-spreadAmount, spreadAmount);
        vy = Random.Range(-spreadAmount, spreadAmount);
    }

    private IEnumerator ScanPause(bool t)
    {
        rotating = false;

        float timer = 0f;
        while (timer < scanPauseTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        rotating = true;
        switcher = t;
    }
}