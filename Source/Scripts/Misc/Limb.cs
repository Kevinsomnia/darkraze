using UnityEngine;
using System.Collections;

public class Limb : MonoBehaviour
{
    [System.Serializable]
    public class ExplosionVelocity
    {
        public ExplosionVelocity(Vector3 pos, float velocity, float radius, float upwards)
        {
            origin = pos;
            forceAmount = velocity;
            forceRadius = radius;
            upwardForce = upwards;
        }

        public Vector3 origin;
        public float forceAmount;
        public float forceRadius;
        public float upwardForce;
    }

    public enum LimbType { Head = 0, Chest = 1, Legs = 2, Arms = 3, None = 4 };
    public LimbType limbType = LimbType.Chest;
    public BaseStats rootStats;

    public bool overrideMultiplier = false;
    public float damageMultOverride = 1f;

    [HideInInspector] public float realDmgMult;
    [HideInInspector] public float lastForceTime = 0f;

    private Vector3 _rv = Vector3.zero;
    public Vector3 ragdollVelocity
    {
        get
        {
            return _rv;
        }
        set
        {
            _ev = null;
            _rv = value;
            lastForceTime = Time.time;
        }
    }

    private ExplosionVelocity _ev = null;
    public ExplosionVelocity explosionVelocity
    {
        get
        {
            return _ev;
        }
        set
        {
            _rv = Vector3.zero;
            _ev = value;
            lastForceTime = Time.time;
        }
    }

    void Start()
    {
        if (overrideMultiplier)
        {
            realDmgMult = damageMultOverride;
        }
        else
        {
            if (limbType == LimbType.Head)
            {
                realDmgMult = 3f;
            }
            else if (limbType == LimbType.Chest)
            {
                realDmgMult = 1f;
            }
            else
            {
                realDmgMult = 0.7f;
            }
        }
    }
}