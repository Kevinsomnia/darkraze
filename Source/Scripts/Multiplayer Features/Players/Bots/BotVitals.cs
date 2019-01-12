using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotVitals : BaseStats
{
    public SkinnedMeshRenderer playerMesh;
    public Material dissolveMat;
    public Rigidbody[] ragdollRigidbodies;
    public Collider[] ragdollColliders;
    public float ragdollForceFactor = 1f;
    public AudioClip deathSound;

    [HideInInspector] public bool isDead = false;

    private float initTime = 0f;
    private string builtData = "";
    private Vector3 velo;
    private bool grenade;
    private List<byte> damageIDs;
    private List<int> damageInflicted;
    private int killerID = -1;
    private int headID = -1;
    private int lastWeaponID = -1;

    void Awake()
    {
        curHealth = maxHealth;
        base.hasLimbs = true;
        /*
        bm = GetComponent<BotMovement>();
        bw = GetComponent<BotWeapons>();*/
        initTime = Time.time;

        damageIDs = new List<byte>();
        damageInflicted = new List<int>();
        killerID = -1;
        headID = -1;
        lastWeaponID = -1;
        builtData = "";
    }

    [RPC]
    public override void ApplyDamageNetwork(byte damage, byte senderID, byte weaponID, byte bodyPart)
    {
        if (Time.time - initTime <= 2f || GeneralVariables.Networking.finishedGame)
        {
            return;
        }

        Limb.LimbType limb = (Limb.LimbType)Mathf.Clamp(bodyPart, 0, 4);
        ApplyDamage(damage, senderID, limb, weaponID + ((bodyPart > 4) ? 1000 : 0)); //>4 = grenade
    }

    public override void ApplyDamageMain(int damage, bool showBlood, Limb.LimbType bodyPart)
    {
        //ApplyDamage(damage, bm.myIndex + 64, bodyPart);
    }

    private void ApplyDamage(int damage, int senderID, Limb.LimbType bodyPart, int weaponID = -1)
    {
        if (!Topan.Network.isServer || isDead || damage <= 0)
        {
            return;
        }
        /*
        if(GeneralVariables.gameModeHasTeams) {
            byte teamNum = (senderID >= 64) ? BotManager.allBotPlayers[senderID - 64].team : (byte)Topan.Network.GetPlayerByID(senderID).GetPlayerData("team", (byte)0);
            if(teamNum != bm.myBotPlayer.team) {
                if(!damageIDs.Contains((byte)senderID)) {
                    damageIDs.Add((byte)senderID);
                    damageInflicted.Add(Mathf.Clamp(damage, 0, curHealth));
                }
                else {
                    int theIndex = GetDamageIndex(senderID);
                    if(theIndex >= 0) {
                        damageInflicted[theIndex] += Mathf.Clamp(damage, 0, curHealth);
                    }
                }
            }
        }*/

        curHealth -= damage;
        base.headshot = (bodyPart == Limb.LimbType.Head);

        if (!isDead && curHealth <= 0)
        {
            if (!damageIDs.Contains((byte)senderID))
            {
                damageIDs.Add((byte)senderID);
                damageInflicted.Add(9);
            }

            lastWeaponID = weaponID;
            killerID = senderID;
            headID = (base.headshot) ? senderID : -1;
            BotDeath();
        }
    }

    public void BotDeath()
    {
        if (!Topan.Network.isServer)
        {
            return;
        }

        /*
        bm.controller.enabled = false;

        bool isSuicide = (killerID == bm.myIndex + 64);
        builtData = (bm.myIndex + 64).ToString() + ((!isSuicide) ? "," : "");

        if(!isSuicide) {
            for(int i = 0; i < damageIDs.Count; i++) {
                builtData += damageIDs[i].ToString();

                if(damageIDs.Count > 1 && damageIDs[i] == killerID) {
                    builtData += "k";
                }

                if(damageIDs[i] == headID) {
                    builtData += "!";
                }

                if(i < damageIDs.Count - 1) {
                    builtData += ",";
                }
            }

            builtData += ".";

            if(damageIDs.Count > 1) {
                for(int i = 0; i < damageInflicted.Count; i++) {
                    builtData += damageInflicted[i].ToString();

                    if(i < damageIDs.Count - 1) {
                        builtData += ",";
                    }
                }
            }

            if(lastWeaponID > -1) {
                grenade = false;
                if(lastWeaponID >= 1000) {
                    lastWeaponID -= 1000;
                    grenade = true;
                }

                builtData += "." + lastWeaponID.ToString() + ((grenade) ? "*" : "");
            }
        }

        GeneralVariables.connectionView.RPC(Topan.RPCMode.Server, "KilledPlayer", builtData);
        //GeneralVariables.server.botRespawnQueue.Add(bm.myIndex);
        GetComponent<Topan.NetworkView>().Deallocate(); //Handles all the death functions.
        curHealth = 0;
        isDead = true;*/
    }

    void Topan_Deallocating()
    {
        curHealth = 0;
        isDead = true;
        // bm.botDetector.enabled = false;
        //   bw.StopReloadSound();
        SetRagdoll(true);
        GetComponent<AudioSource>().pitch = Random.Range(0.85f, 0.94f);
        GetComponent<AudioSource>().PlayOneShot(deathSound, Random.Range(0.3f, 0.35f));
    }

    public void SetRagdoll(bool e)
    {
        Limb latestLimb = null;
        //float latestTime = 0f;
        for (int i = 0; i < ragdollRigidbodies.Length; i++)
        {
            ragdollRigidbodies[i].isKinematic = !e;

            /*
            if(bm != null && e) {
                velo = (bm.controller != null) ? bm.controller.velocity : (bm.velocity / Time.deltaTime);
                ragdollRigidbodies[i].AddForce(velo * ragdollForceFactor, ForceMode.Impulse);

                Limb thisLimb = ragdollRigidbodies[i].GetComponent<Limb>();
                if(thisLimb != null) {
                    if(thisLimb.lastForceTime > latestTime) {
                        latestLimb = thisLimb;
                        latestTime = thisLimb.lastForceTime;
                    }
                }
            }*/
        }

        if (latestLimb != null)
        {
            Rigidbody rigid = latestLimb.GetComponent<Rigidbody>();
            rigid.AddForce(latestLimb.ragdollVelocity, ForceMode.Impulse);

            Limb.ExplosionVelocity lbExpl = latestLimb.explosionVelocity;
            if (lbExpl != null)
            {
                rigid.AddExplosionForce(lbExpl.forceAmount, lbExpl.origin, lbExpl.forceRadius, lbExpl.upwardForce, ForceMode.Impulse);
            }
        }

        for (int i = 0; i < ragdollColliders.Length; i++)
        {
            ragdollColliders[i].isTrigger = !e;
        }
    }

    private int GetDamageIndex(int pID)
    {
        for (int i = 0; i < damageIDs.Count; i++)
        {
            if (pID == damageIDs[i])
            {
                return i;
            }
        }

        return -1;
    }
}