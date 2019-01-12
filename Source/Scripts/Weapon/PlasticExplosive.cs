using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class PlasticExplosive : MonoBehaviour
{
    public LayerMask layersToStick = -1;
    public float stickOffset = 0.1f;
    public Vector3 rotationAngle = Vector3.up;
    public float rotationTransitionSpeed = 10f;
    public AudioClip stickSound;
    public int explosionDamage = 100;
    public float explosionRadius = 5f;

    public GameObject detonationPrefab;

    [HideInInspector] public bool onlyVisual;
    [HideInInspector] public int myID = -1;

    private float syncTime;
    private Vector3 lastSync;

    private bool canExplode;
    private float timer;
    private Vector3 oldPos;
    private Vector3 newPos;

    void Start()
    {
        timer = 0f;
        canExplode = false;
        oldPos = transform.position;
        newPos = transform.position;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
        }
        else if (canExplode)
        {
            Explode();
        }

        oldPos = newPos;
        newPos = transform.position;

        Vector3 dir = (newPos - oldPos);

        if (!GetComponent<Rigidbody>().isKinematic)
        {
            float dist = dir.magnitude;

            if (dist > 0f)
            {
                RaycastHit hit;
                if (Physics.Raycast(oldPos, dir, out hit, dist, layersToStick.value))
                {
                    if (!hit.collider.gameObject.isStatic && hit.rigidbody)
                    {
                        transform.parent = hit.collider.transform;
                    }

                    transform.position = hit.point + (hit.normal * stickOffset);
                    StartCoroutine(TransitionRotation(Quaternion.FromToRotation(rotationAngle.normalized, hit.normal)));
                    GetComponent<AudioSource>().pitch *= Random.Range(0.85f, 1.2f);
                    GetComponent<AudioSource>().PlayOneShot(stickSound);
                    GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        }
    }

    public void Detonate(float delay)
    {
        canExplode = true;
        timer = delay;
    }

    private void Explode()
    {
        GameObject dInst = (GameObject)Instantiate(detonationPrefab, transform.position, transform.rotation);

        AreaDamage aDmg = dInst.GetComponent<AreaDamage>();
        if (aDmg != null)
        {
            if (onlyVisual)
            {
                aDmg.enabled = false;
            }
            else
            {
                aDmg.overrideMaxDmg = explosionDamage;
                aDmg.overrideMaxRange = explosionRadius;
            }
        }

        TimeScaleSound tss = dInst.GetComponent<TimeScaleSound>();
        if (tss != null)
        {
            tss.UpdatePitch(Random.Range(0.88f, 1f));
            tss.PlaySound();
        }
        else if (dInst.GetComponent<AudioSource>() != null)
        {
            dInst.GetComponent<AudioSource>().pitch *= Random.Range(0.88f, 1f);
        }

        RemoveInstance();
    }

    public void RemoveInstance(float delay = 0f)
    {
        if (delay > 0f)
        {
            StartCoroutine(RemovalCoroutine(delay));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator RemovalCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }

    private IEnumerator TransitionRotation(Quaternion rot)
    {
        float time = 0f;
        Quaternion curRot = transform.rotation;
        while (time < 1f)
        {
            time += Time.deltaTime * rotationTransitionSpeed;
            transform.rotation = Quaternion.Slerp(curRot, rot, time);
            yield return null;
        }
    }
}