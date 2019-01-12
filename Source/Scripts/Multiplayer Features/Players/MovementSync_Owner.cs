using UnityEngine;
using System.Collections;
using Topan.CustomTypes;

public class MovementSync_Owner : Topan.TopanMonoBehaviour {
	public Vector3 offsetProxy = Vector3.zero;
	public float sendRate = 10f;

	private float lastSentLookY = 0f;
	private Vector3 lastSentPosition = Vector3.zero;
	private float lastSentRotation = 0f;
	private float midRot = 0f;
	private float syncTimer = 0f;

	private float rate;
	private PlayerMovement pm;
	private PlayerLook pl;
	private BaseStats bs;
	
	private Transform tr;
	private bool networkStarted;
	private bool mGrounded;
	private bool mSprinting;
	private bool mCrouching;
	private bool mWalking;
	
	void Awake() {
		if(!Topan.Network.isConnected) {
			Destroy(this);
			return;
		}

		pm = GetComponent<PlayerMovement>();
		pl = GetComponent<PlayerLook>();
		bs = GetComponent<BaseStats>();
		midRot = pl.minimumY + ((pl.maximumY - pl.minimumY) / 2f);
		rate = (1f / Mathf.Max(1f, sendRate));
		networkStarted = false;
	}	
	
	public void NetworkStart() {
        GeneralVariables.Networking.playerInstances[Topan.Network.player.id] = transform;
		lastSentPosition = transform.position;
		tr = transform;
		syncTimer = 0f;
		networkStarted = true;
	}

	void Update() {
		if(!networkStarted) {
			return;
		}

		syncTimer += Time.unscaledDeltaTime;
		syncTimer = Mathf.Clamp(syncTimer, 0f, rate * 2f);
		if(syncTimer >= rate) {
			SendData();
			syncTimer -= rate;
		}
	}
	
	private void SendData() {
		if(mGrounded != pm.grounded) {
			topanNetworkView.RPC(Topan.RPCMode.Others, "PlayerGrounded", pm.grounded);
			mGrounded = pm.grounded;
		}
		if(mSprinting != pm.sprinting) {
			topanNetworkView.RPC(Topan.RPCMode.Others, "PlayerSprinting", pm.sprinting);
			mSprinting = pm.sprinting;
		}
		if(mCrouching != pm.crouching) {
			topanNetworkView.RPC(Topan.RPCMode.Others, "PlayerCrouching", pm.crouching);
			mCrouching = pm.crouching;
		}
		if(mWalking != pm.walking) {
			topanNetworkView.RPC(Topan.RPCMode.Others, "PlayerWalking", pm.walking);
			mWalking = pm.walking;
		}
		
		float look = 0f;
		if(pl.yRot < midRot) {
			look = -(pl.yRot / pl.minimumY);	
		}
		else if(pl.yRot > midRot) {
			look = (pl.yRot / pl.maximumY);	
		}

        if((tr.position - lastSentPosition).sqrMagnitude >= 0.003f || Mathf.Abs(pl.xRot - lastSentRotation) >= 2f || Mathf.Abs(look - lastSentLookY) >= 0.02f) {
            topanNetworkView.UnreliableRPC(Topan.RPCMode.OthersBuffered, "SyncTransform", tr.position + offsetProxy, pl.xRot, look);
            lastSentPosition = tr.position;
            lastSentRotation = pl.xRot;
            lastSentLookY = look;
        }
	}
		
	public void PrepareForMultiplayer() {
		topanNetworkView.observedComponents.Add(this);
	}
	
	[RPC]
	public void ApplyDamageNetwork(byte damage, byte sender, byte wepIndex, byte bodyPart) {
		bs.ApplyDamageNetwork(damage, sender, wepIndex, bodyPart);
	}

    [RPC]
    public void NetworkEMP() {
        bs.NetworkEMP();
    }
}