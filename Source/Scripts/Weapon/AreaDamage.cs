using UnityEngine;
using System.Collections.Generic;

public class AreaDamage : MonoBehaviour
{
    public bool damageOnce = true;
    public float damageRate = 0.2f;
    public float lifetime = 2; //0 for infinite.
    public bool raycastCheck = true;
    public Vector3 raycastOffset;
    public AnimationCurve damageFalloff = AnimationCurve.EaseInOut(0f, 100f, 10f, 0f);
    public float damageForce = 45.0f;
    public float forceUpwards = 3.0f;
    public bool shakeCamera = false;
    public float shakeRadius = 40f;
    public float shakeLength = 0.4f;
    public float shakeSpeed = 0.6f;
    public float shakeIntensity = 1.6f;
    public bool explosionCameraEffect = true;
    public LayerMask layersToDamage = -1;
    public bool useHitIndicator = true;
    public bool isEMP = false;

    [HideInInspector] public int overrideMaxDmg = 0;
    [HideInInspector] public float overrideMaxRange = 0f;
    [HideInInspector] public bool isPlayer;
    [HideInInspector] public int botIndex = -1;
    [HideInInspector] public int wepIndex = -1;
    [HideInInspector] public int grenIndex = -1;
    [HideInInspector] public bool friendlyFire;
    [HideInInspector] public Collider hitTarget;
    [HideInInspector] public int bonusDamage = 0;

    private RaycastHit hit;

    private WeaponManager wm;
    private WeaponManager wManager
    {
        get
        {
            if (wm == null && GeneralVariables.playerRef != null)
            {
                wm = GeneralVariables.playerRef.wm;
            }

            return wm;
        }
    }

    private List<BaseStats> dmgBaseStats = new List<BaseStats>();
    private float lastDamageTime;
    private float damageRadius;
    private Collider[] toAffect;
    private Transform tr;

    void Start()
    {
        tr = transform;
        DamageArea();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!damageOnce && Time.time - lastDamageTime >= damageRate)
        {
            DamageArea();
        }
    }

    private void DamageArea()
    {
        dmgBaseStats.Clear();

        PlayerReference pRef = GeneralVariables.playerRef;
        if (shakeCamera && pRef != null)
        {
            float shakeDistance = Vector3.Distance(tr.position, pRef.transform.position);
            if (shakeDistance < shakeRadius)
            {
                pRef.GetComponent<PlayerLook>().ShakeCamera(shakeLength, shakeSpeed, shakeIntensity * 2.5f * Mathf.Clamp01((shakeRadius - shakeDistance) / shakeRadius));
            }
        }

        damageRadius = damageFalloff[damageFalloff.length - 1].time;
        toAffect = Physics.OverlapSphere(tr.position, ((overrideMaxRange > 0f) ? overrideMaxRange : damageRadius), layersToDamage.value);

        foreach (Collider col in toAffect)
        {
            float distanceFromCollider = Mathf.Clamp(Vector3.Distance(tr.position, col.ClosestPointOnBounds(tr.position)), 0, (overrideMaxRange > 0f) ? overrideMaxRange : damageRadius);

            float evalDamage = damageFalloff.Evaluate(distanceFromCollider * ((overrideMaxRange > 0f) ? (damageRadius / overrideMaxRange) : 1f));
            if (overrideMaxDmg > 0)
            {
                evalDamage *= ((float)overrideMaxDmg / damageFalloff.Evaluate(0f));
            }

            if (isEMP || evalDamage >= 0.5f)
            {
                int dmg = Mathf.RoundToInt(evalDamage);
                bool thisIsTarget = (hitTarget != null && col.GetInstanceID() == hitTarget.GetComponent<Collider>().GetInstanceID());

                if (raycastCheck)
                {
                    if (thisIsTarget || Physics.Raycast(tr.position + raycastOffset, (col.bounds.center - (tr.position + raycastOffset)), out hit, ((overrideMaxRange > 0f) ? overrideMaxRange : damageRadius), layersToDamage.value))
                    {
                        if (!thisIsTarget && hit.collider.GetInstanceID() != col.GetInstanceID())
                        {
                            continue;
                        }

                        DoAreaAction(col, ((overrideMaxRange > 0f) ? overrideMaxRange : damageRadius), dmg + ((thisIsTarget) ? bonusDamage : 0), distanceFromCollider);
                    }
                }
                else
                {
                    DoAreaAction(col, ((overrideMaxRange > 0f) ? overrideMaxRange : damageRadius), dmg + ((thisIsTarget) ? bonusDamage : 0), distanceFromCollider);
                    continue;
                }
            }
        }

        lastDamageTime = Time.time;
        bonusDamage = 0;
        hitTarget = null;
    }

    private void DoAreaAction(Collider col, float dmgRadius, int dmg, float distance)
    {
        Limb lb = col.GetComponent<Limb>();
        BaseStats bs = col.GetComponent<BaseStats>();

        if (lb != null)
        {
            bs = lb.rootStats;

            if (!dmgBaseStats.Contains(lb.rootStats))
            {
                lb.explosionVelocity = new Limb.ExplosionVelocity(tr.position, damageForce, dmgRadius, forceUpwards);
            }
        }

        if ((lb == null || (lb != null && (bs == null || bs.curHealth <= 0))) && col.GetComponent<Rigidbody>() != null)
        {
            col.GetComponent<Rigidbody>().AddExplosionForce(damageForce, tr.position, dmgRadius, forceUpwards, ForceMode.Impulse);
        }

        if (bs != null && !dmgBaseStats.Contains(bs))
        {
            ApplyDamageStat(bs, dmg, distance, dmgRadius);
            dmgBaseStats.Add(bs);
        }
    }

    private void ApplyDamageStat(BaseStats bs, int dmg, float dist, float rad)
    {
        if (dmg <= 0 && !isEMP)
        {
            return;
        }

        PlayerVitals playerPV = bs.GetComponent<PlayerVitals>();
        if (bs.isLocalPlayer && playerPV != null)
        {
            playerPV.HitIndicator(transform.position);

            if (isEMP)
            {
                playerPV.pe.StartPhase_EMP();
            }
            else if (explosionCameraEffect)
            {
                playerPV.pe.ExplosionVisualEffect(1.05f - Mathf.Clamp01((dist * 1.35f) / rad));
            }
        }

        bool showHitMarker = false;
        if (isPlayer && !bs.isLocalPlayer && wManager != null && bs.curHealth > 0)
        {
            showHitMarker = true;
        }

        if (Topan.Network.isConnected)
        {
            Topan.NetworkView damageView = bs.GetComponent<Topan.NetworkView>();

            if (damageView != null && !(isPlayer && bs.isLocalPlayer))
            {
                bool canDamage = true;
                BotVitals hitBot = bs.GetComponent<BotVitals>();
                byte ownerTeam = (isPlayer || botIndex <= -1) ? (byte)Topan.Network.player.GetPlayerData("team", (byte)0) : BotManager.allBotPlayers[botIndex].team;
                byte targetTeam = /*(hitBot != null) ? BotManager.allBotPlayers[hitBot.bm.myIndex].team :*/ (byte)damageView.owner.GetPlayerData("team", (byte)0);

                if (GeneralVariables.gameModeHasTeams)
                {
                    if (targetTeam == ownerTeam)
                    {
                        canDamage = friendlyFire;
                        showHitMarker = false;
                    }
                }
                else
                {
                }

                /*
                if((hitBot != null && botIndex > -1 && hitBot.bm.myIndex == botIndex)) {
                    canDamage = true;
                }*/

                if (canDamage)
                {
                    damageView.RPC(Topan.RPCMode.Owner, "ApplyDamageNetwork", (byte)dmg, (botIndex > -1) ? (byte)(botIndex + 64) : (byte)Topan.Network.player.id, GetTrueID(), (grenIndex > -1) ? (byte)255 : (byte)4);
                }

                if (isEMP && !hitBot)
                {
                    damageView.RPC(Topan.RPCMode.Owner, "NetworkEMP");
                }
            }
            else
            {
                bs.headshot = false;
                bs.ApplyDamageMain(dmg, true);
            }
        }
        else
        {
            bs.headshot = false;
            bs.ApplyDamageMain(dmg, true);
        }

        if (showHitMarker)
        {
            wManager.HitTarget(bs.curHealth <= 0);
        }
    }

    private byte GetTrueID()
    {
        if (wepIndex > -1)
        {
            return (byte)wepIndex;
        }
        else if (grenIndex > -1)
        {
            return (byte)grenIndex;
        }

        return 0;
    }
}