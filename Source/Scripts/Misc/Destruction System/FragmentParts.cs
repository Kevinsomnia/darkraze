using UnityEngine;
using System.Collections;

public class FragmentParts : BaseStats
{
    public bool fragmented;

    private FragmentController fc;
    private Transform tr;
    private Rigidbody rigid;
    private bool isFake;
    private int health;
    private Vector3 collisionVector;

    void Start()
    {
        tr = transform;
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        if (isFake)
            yield break;

        fc = transform.root.GetComponent<FragmentController>();
        health = fc.fragmentHealth;
        base.hitIndication = false;
    }

    public override void ApplyDamageMain(int dmg, bool ignore)
    {
        DamageFragment(dmg);
    }

    void OnCollisionEnter(Collision col)
    {
        if (!fragmented && !isFake)
        {
            if (!col.collider.GetComponent<FragmentParts>())
            {
                collisionVector = col.relativeVelocity;
            }
            DamageFragment(Mathf.RoundToInt((col.relativeVelocity.magnitude * 0.2f) / fc.collisionStrength));
        }
    }

    public void DamageFragment(int amount)
    {
        if (isFake)
        {
            return;
        }

        health -= amount;

        if (!fragmented && health <= 0)
        {
            fc.ReleaseFragments();
            fc.fragments.Remove(this);
            fc.timer = 0;

            rigid = gameObject.AddComponent<Rigidbody>();
            rigid.mass = fc.fragmentMass;
            AddForce(fc.minForce, fc.maxForce);
            tr.localScale *= fc.fragmentScale;
            tr.parent = null;
            fragmented = true;

            if (rigid != null && fc.disablePhysicsDelay > 0f)
            {
                StartCoroutine(DisableRigid(fc.disablePhysicsDelay));
            }

            if (fc.disableColliderDelay > 0f)
            {
                StartCoroutine(DisableCollide(fc.disableColliderDelay));
            }

            if (fc.fragmentParticle != null)
            {
                ParticleSystem particle = (ParticleSystem)Instantiate(fc.fragmentParticle, tr.position, tr.rotation);
                particle.transform.parent = tr;
            }

            if (fc.fragmentAmount > 1)
            {
                CloneFragments(fc.fragmentAmount - 1);
            }

            if (fc.deletionDelay > 0f)
            {
                StartCoroutine(Deletion(fc.deletionDelay));
            }

            if (fc.fragmentAll)
            {
                fc.FragmentAll();
            }
        }

        collisionVector = Vector3.zero;
    }

    private void CloneFragments(int clones)
    {
        if (isFake)
        {
            return;
        }

        for (int i = 0; i < clones; i++)
        {
            GameObject fake = (GameObject)Instantiate(gameObject, tr.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f)), tr.rotation);
            FragmentParts fakeFP = fake.GetComponent<FragmentParts>();
            fakeFP.isFake = true;
            fakeFP.rigid = fake.GetComponent<Rigidbody>();
            fakeFP.AddForce(fc.minForce, fc.maxForce);
            fake.transform.localScale = tr.localScale * (0.2f + (Random.value * fc.fragmentScale));

            if (fc.disablePhysicsDelay > 0f)
            {
                fakeFP.StartCoroutine(DisableRigid(fc.disablePhysicsDelay));
            }

            if (fc.disableColliderDelay > 0f)
            {
                fakeFP.StartCoroutine(DisableCollide(fc.disableColliderDelay));
            }

            if (fc.deletionDelay > 0f)
            {
                fakeFP.StartCoroutine(Deletion(fc.deletionDelay));
            }
        }
    }

    private void AddForce(float minForce, float maxForce)
    {
        float forceX = Random.Range(minForce, maxForce);
        if (Random.value < 0.5f)
        {
            forceX *= -1f;
        }
        float forceY = Random.Range(minForce, maxForce);
        if (Random.value < 0.5f)
        {
            forceY *= -1f;
        }

        rigid.AddForce(forceX, Random.Range(minForce, maxForce) * 0.8f, forceY);
        rigid.AddForce(collisionVector * 20f);
        rigid.AddTorque(Random.rotation.eulerAngles * 2f, ForceMode.Impulse);
    }

    private IEnumerator Deletion(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private IEnumerator DisableCollide(float delay)
    {
        yield return new WaitForSeconds(delay);
        GetComponent<MeshCollider>().enabled = false;
    }

    private IEnumerator DisableRigid(float delay)
    {
        yield return new WaitForSeconds(delay);
        rigid.isKinematic = true;
    }
}