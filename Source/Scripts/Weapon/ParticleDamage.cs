using UnityEngine;
using System.Collections;

public class ParticleDamage : MonoBehaviour
{
    public int damage = 2;

    void OnParticleCollision(GameObject other)
    {
        BaseStats bs = other.GetComponent<BaseStats>();
        if (bs != null)
        {
            bs.ApplyDamageMain(damage, true);
        }
    }
}