using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Bullet : PoolItem
{
    public GameObject tracer;
    public GameObject glow;
    public GameObject[] bulletHoles = new GameObject[1];
    public Vector2 bulletDecalSize = new Vector2(0.32f, 0.38f);
    public GameObject whizPrefab;
    public LayerMask layersToAffect = -1;
    public float distanceLifetime = 5000f;
    public LineRenderer smokeTrail;
    public float tracerTiling = 0.05f;
    public Vector2 tracerColorRange = new Vector2(0.6f, 0.9f);
    public float tracerOpacity = 0.25f;
    public float tracerWidth = 0.08f;

    [HideInInspector] public bool isVisuals = false;
    [HideInInspector] public bool noWhizSound = false;

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

    private int damage;
    private float force;

    private Transform tr;
    private Transform tracerTr;
    private LineRenderer smokeInstance;

    private RaycastHit hit;
    private BaseStats bs;
    private Limb lb;

    private bool player;
    private bool isServer = false;
    private int botIndex = -1;
    private int thisGunIndex = 0;
    private Vector3 startPos;
    private Vector3 oldPos;
    private Vector3 newPos;
    private Vector3 dir;
    private Vector3 gravityVector;
    private float dist;
    private float speed;
    private float gFactor;
    private float travelDist;
    private AnimationCurve dmgCurve;
    private int ricochetAmount = 0;
    private float ricochetAngle = 0f;
    private int multishotDivider = 1;

    private bool hitObject;
    private bool friendlyFire;
    private int bulletHoleIndex;
    private int hitEffectIndex;
    private int ricochetValue;

    private Transform currentWhiz;

    public override void InstantiateStart()
    {
        tr = transform;
        startPos = tr.position;
        oldPos = startPos;
        newPos = oldPos;
        gravityVector = Vector3.zero;
        travelDist = 0f;
        tracer.SetActive(false);

        if (!player)
        {
            glow.SetActive(false);
        }

        tracerTr = tracer.transform;
        tracerTr.rotation = Quaternion.LookRotation(tr.forward);
        tracerTr.localScale = new Vector3(0.025f, 0.025f, 0.53f);

        ricochetValue = 0;

        if (isVisuals)
        {
            tracerTr.localPosition = Vector3.forward * 0.2f;
            Vector3 randScale = tracerTr.localScale;
            randScale.z *= Random.Range(1.2f, 2f);
            tracerTr.localScale = randScale;
        }

        if (whizPrefab != null)
        {
            if (!noWhizSound)
            {
                GameObject whiz = (GameObject)Instantiate(whizPrefab);
                currentWhiz = whiz.transform;
                currentWhiz.parent = tr;
                currentWhiz.localPosition = Vector3.zero;
            }
        }

        if (smokeTrail != null)
        {
            smokeInstance = (LineRenderer)Instantiate(smokeTrail, tr.position, tr.rotation);
            smokeInstance.SetPosition(0, tr.position);
            smokeInstance.SetPosition(1, tr.position);

            float newTraceOpac = tracerOpacity;
            newTraceOpac *= ((isVisuals) ? 1.3f : 1f) * GeneralVariables.lightingFactor;
            smokeInstance.SetColors(new Color(1f, 1f, 1f, Random.Range(newTraceOpac * 0.9f, newTraceOpac * 1.1f) / (float)multishotDivider), new Color(1f, 1f, 1f, Random.Range(newTraceOpac, newTraceOpac * 1.2f)));
            smokeInstance.SetWidth(tracerWidth - Random.Range(0f, 0.01f), tracerWidth + Random.Range(0f, 0.01f));
            tracerTiling *= Random.Range(0.5f, 1.6f);

            if (Random.value < 0.5f)
            {
                smokeInstance.material.mainTextureScale = new Vector2(smokeInstance.material.mainTextureScale.x, smokeInstance.material.mainTextureScale.y * -1f);
            }
        }
    }

    void Update()
    {
        oldPos = tr.position;

        newPos += (tr.forward * speed) * Time.deltaTime;
        gravityVector += Physics.gravity * gFactor * Time.deltaTime;
        newPos += gravityVector * Time.deltaTime;

        dir = (newPos - oldPos);
        dist = dir.magnitude;

        if (dir != Vector3.zero)
        {
            tracerTr.rotation = Quaternion.LookRotation(dir);
        }

        if (dist > 0f)
        {
            if (Physics.Raycast(oldPos, dir, out hit, dist, layersToAffect.value))
            {
                OnHit(hit);
            }
            else
            {
                travelDist += dist;

                if (smokeInstance != null)
                {
                    smokeInstance.SetPosition(1, newPos);
                    smokeInstance.material.mainTextureScale = new Vector2(Vector3.Distance(startPos, oldPos) * tracerTiling, smokeInstance.material.mainTextureScale.y);
                }

                tracer.SetActive(true);
                glow.SetActive(true);
            }

            tr.position = newPos;
        }

        if (travelDist >= distanceLifetime)
        {
            tracer.SetActive(false);
            glow.SetActive(false);
            AddToPool();
            return;
        }
    }

    private void OnHit(RaycastHit hit)
    {
        ricochetValue++;
        float limbForceMult = 1f;
        travelDist += hit.distance;
        bool ignoreBulletHole = false;

        if (!isVisuals)
        {
            bs = hit.collider.GetComponent<BaseStats>();
            lb = hit.collider.GetComponent<Limb>();

            damage = Mathf.RoundToInt(damage * dmgCurve.Evaluate(travelDist));

            if (damage > 0)
            {
                if (bs != null)
                {
                    bool hitMarkerEnabled = false;
                    if (player && wManager != null && bs.curHealth > 0 && bs.hitIndication)
                    {
                        hitMarkerEnabled = true;
                    }

                    bool canDamage = true;
                    bs.headshot = false;

                    if (Topan.Network.isConnected)
                    {
                        Topan.NetworkView damageView = bs.GetComponent<Topan.NetworkView>();
                        if (damageView != null && damage > 0)
                        {
                            BotVitals hitBot = bs.GetComponent<BotVitals>();

                            if (GeneralVariables.gameModeHasTeams)
                            {
                                byte ownerTeam = (player) ? (byte)Topan.Network.player.GetPlayerData("team", (byte)0) : BotManager.allBotPlayers[botIndex].team;
                                byte targetTeam = /* (hitBot) ? BotManager.allBotPlayers[hitBot.bm.myIndex].team : */(byte)damageView.owner.GetPlayerData("team", (byte)0);

                                if (targetTeam == ownerTeam)
                                {
                                    if (!friendlyFire)
                                    {
                                        canDamage = false;
                                    }

                                    hitMarkerEnabled = false;
                                }
                            }
                            else
                            {
                            }

                            if (canDamage)
                            {
                                if (isServer && (damageView.ownerID == 0 || hitBot))
                                {
                                    bs.ApplyDamageNetwork((byte)Mathf.Clamp(damage, 0, 255), (botIndex > -1) ? (byte)(botIndex + 64) : (byte)Topan.Network.player.id, (byte)thisGunIndex, (byte)4);
                                }
                                else
                                {
                                    damageView.RPC(Topan.RPCMode.Owner, "ApplyDamageNetwork", (byte)Mathf.Clamp(damage, 0, 255), (botIndex > -1) ? (byte)(botIndex + 64) : (byte)Topan.Network.player.id, (byte)thisGunIndex, (byte)4);
                                }
                            }
                        }
                        else
                        {
                            bs.ApplyDamageMain(damage, true);
                        }
                    }
                    else
                    {
                        bs.ApplyDamageMain(damage, true);
                    }

                    if (hitMarkerEnabled)
                    {
                        wManager.HitTarget(bs.curHealth <= 0);
                    }

                    if (canDamage)
                    {
                        PlayerVitals playerPV = bs.GetComponent<PlayerVitals>();
                        if (bs.isLocalPlayer && playerPV != null)
                        {
                            playerPV.HitIndicator(startPos);
                        }
                    }
                }
                else if (lb != null && lb.rootStats != null)
                {
                    bs = lb.rootStats;
                    lb.ragdollVelocity = tr.forward * force * limbForceMult;
                    damage = Mathf.RoundToInt(damage * lb.realDmgMult);

                    bool hitMarkerEnabled = false;
                    if (player && wManager != null && bs.curHealth > 0 && bs.hitIndication)
                    {
                        hitMarkerEnabled = true;
                    }

                    bs.headshot = (lb.limbType == Limb.LimbType.Head);

                    if (Topan.Network.isConnected)
                    {
                        Topan.NetworkView damageView = bs.GetComponent<Topan.NetworkView>();
                        if (damageView != null)
                        {
                            BotVitals hitBot = bs.GetComponent<BotVitals>();
                            bool canDamage = true;

                            if (GeneralVariables.gameModeHasTeams)
                            {
                                byte ownerTeam = (player || botIndex <= -1) ? (byte)Topan.Network.player.GetPlayerData("team", (byte)0) : BotManager.allBotPlayers[botIndex].team;
                                byte targetTeam = /*(hitBot) ? BotManager.allBotPlayers[hitBot.bm.myIndex].team : */(byte)damageView.owner.GetPlayerData("team", (byte)0);

                                if (targetTeam == ownerTeam)
                                {
                                    if (!friendlyFire)
                                    {
                                        canDamage = false;
                                    }

                                    hitMarkerEnabled = false;
                                }
                            }
                            else
                            {
                            }

                            if (canDamage)
                            {
                                if (isServer && (damageView.ownerID == 0 || hitBot))
                                {
                                    bs.ApplyDamageNetwork((byte)Mathf.Clamp(damage, 0, 255), (botIndex > -1) ? (byte)(botIndex + 64) : (byte)Topan.Network.player.id, (byte)thisGunIndex, (byte)lb.limbType);
                                }
                                else
                                {
                                    damageView.RPC(Topan.RPCMode.Owner, "ApplyDamageNetwork", (byte)Mathf.Clamp(damage, 0, 255), (botIndex > -1) ? (byte)(botIndex + 64) : (byte)Topan.Network.player.id, (byte)thisGunIndex, (byte)lb.limbType);
                                }
                            }
                        }
                        else
                        {
                            bs.ApplyDamageMain(damage, true);
                        }
                    }
                    else
                    {
                        bs.ApplyDamageMain(damage, true);
                    }

                    limbForceMult = 3f;

                    if (hitMarkerEnabled)
                    {
                        wManager.HitTarget(bs.curHealth <= 0);
                    }
                }
            }
        }

        bulletHoleIndex = 0;
        hitEffectIndex = 0;
        string hitTag = hit.collider.tag;

        if (hitTag == "Dirt")
        {
            hitEffectIndex = 1;
        }
        else if (hitTag == "Metal")
        {
            hitEffectIndex = 2;
        }
        else if (hitTag == "Wood")
        {
            hitEffectIndex = 3;
        }
        else if (hitTag == "Flesh" || hitTag == "Player - Flesh")
        {
            bulletHoleIndex = -1;
            hitEffectIndex = 4;
        }
        else if (hitTag == "Water")
        {
            bulletHoleIndex = -1;
            hitEffectIndex = 5;
        }

        Quaternion rot = Quaternion.LookRotation(hit.normal);

        if (bulletHoleIndex >= 0 && !ignoreBulletHole)
        {
            Transform hole = PoolManager.Instance.RequestInstantiate(bulletHoles[bulletHoleIndex], hit.point + (hit.normal * 0.01f), rot).transform;

            if (!hit.collider.gameObject.isStatic && hit.rigidbody)
            {
                hole.KeepUniformScale(hit.transform);
            }

            float randomSize = Random.Range(bulletDecalSize.x, bulletDecalSize.y);
            hole.localScale = new Vector3(randomSize, randomSize, 1f);

            Vector3 euler = hole.localEulerAngles;
            euler.z = Random.value * 360f;
            hole.localEulerAngles = euler;

            DecalObject dObj = hole.GetChild(0).GetComponent<DecalObject>();
            dObj.targetObject = hit.collider.gameObject;
            dObj.UpdateDecalMesh();
        }

        PoolManager.Instance.RequestParticleEmit(hitEffectIndex, hit.point + (hit.normal * 0.06f), rot);

        Rigidbody rigid = hit.rigidbody;
        if ((lb == null || (lb != null && (lb.rootStats == null || lb.rootStats.curHealth <= 0))) && rigid != null && !rigid.isKinematic)
        {
            rigid.AddForce(tr.forward * force * limbForceMult, ForceMode.Impulse);
        }

        if (ricochetAmount > 0 && ricochetValue <= ricochetAmount)
        {
            if (Vector3.Angle(dir, hit.normal) - 90f <= ricochetAngle)
            {
                tr.rotation = Quaternion.LookRotation(Vector3.Reflect(dir, hit.normal));
                speed *= 0.75f;
                damage = Mathf.RoundToInt(damage * Random.Range(0.5f, 0.6f));
                gravityVector *= 0.5f;
                tr.position += tr.forward * speed * Time.deltaTime;
                newPos = tr.position;
            }
            else
            {
                AddToPool();
            }
        }

        if (smokeInstance != null)
        {
            smokeInstance.SetPosition(1, hit.point);
            smokeInstance = null;
        }

        if (currentWhiz != null)
        {
            currentWhiz.transform.parent = null;
            currentWhiz.transform.position = hit.point;
            currentWhiz = null;
            noWhizSound = false;
        }

        tracer.SetActive(false);
        glow.SetActive(false);
        AddToPool();
    }

    public void BulletInfo(BulletInfo bi, int gunIndex, bool tracer = false, bool isPlayer = false, int botID = -1)
    {
        multishotDivider = WeaponDatabase.GetWeaponByID(gunIndex).bulletsPerShot;
        thisGunIndex = gunIndex;
        isVisuals = tracer;
        player = (isPlayer && botID <= -1);
        botIndex = botID;
        isServer = Topan.Network.isConnected && (botID > -1 || (player && Topan.Network.isServer));
        damage = bi.damage;
        force = bi.force;
        speed = bi.muzzleVelocity;
        gFactor = bi.gravityFactor;
        dmgCurve = bi.damageFalloff;

        ricochetAmount = bi.ricochetLength;
        ricochetAngle = bi.ricochetMaxAngle;
        friendlyFire = NetworkingGeneral.friendlyFire;
    }
}