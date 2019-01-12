using UnityEngine;
using System.Collections;

public class ImpactAnimation : MonoBehaviour {
	public float impactMagnitude = 0.1f;
	public float impactSmoothing = 6;
	public float verticalRot = 10;
	public float horizontalRot = 6;
	public float maxImpact = 1;
	
	[HideInInspector] public Vector3 currentPos;
    [HideInInspector] public Vector3 jumpCurrentPos;
	[HideInInspector] public float impactY;
    [HideInInspector] public float shakeTime;
		
	private Transform pTr;
	private Transform tr;
    private PlayerMovement pm;
	private AimController ac;
	private PlayerVitals pv;
	
	private bool startedDown = false;
	private bool startedUp = false;
	private bool falling = false;
    private float jumpRotDown;
    private float downMomentum;
    private float targetY;
    private float targetSpeed;
	private float randomX;

    private float shakeX;
    private float shakeY;
	
	void Start() {
		tr = transform;
		pTr = tr.parent;
		
		PlayerReference pr = GeneralVariables.playerRef;
        pm = pr.GetComponent<PlayerMovement>();
        pv = pr.GetComponent<PlayerVitals>();
		ac = pr.ac;
	}
	
	void Update() {
		if(Time.timeScale <= 0f) {
			return;
		}
		
        jumpRotDown = Mathf.Lerp(jumpRotDown, 0f, Time.deltaTime * 4f);
        downMomentum = Mathf.Lerp(downMomentum, 0f, Time.deltaTime * 15f);
		impactY = Mathf.Clamp(impactY, -0.7f, (0.15f * ((ac.isAiming) ? (0.45f - (Mathf.Clamp01(Mathf.Abs(pm.controller.velocity.y * 0.2f)) * 0.25f)) : 1f)) + (Mathf.Clamp01(-pm.controller.velocity.y * 0.1f) * 0.022f));
		
		float aimMod = (ac.isAiming) ? 0.7f : 1f;
		float dampingMod = (falling) ? 4.8f : impactSmoothing;

		currentPos = Vector3.Lerp(currentPos, Vector3.up * impactY * aimMod, Time.deltaTime * dampingMod);
        jumpCurrentPos = Vector3.Lerp(jumpCurrentPos, Vector3.up * jumpRotDown, Time.deltaTime * 6.5f);
		tr.position = pTr.position + (currentPos * 0.8f) + (jumpCurrentPos * 0.25f);
        tr.localRotation = Quaternion.Slerp(tr.localRotation, Quaternion.Euler((-currentPos.y - jumpCurrentPos.y - downMomentum) * verticalRot * ((ac.isAiming) ? 4f : 1f), randomX * currentPos.y, (jumpCurrentPos.y * 8f) + downMomentum), Time.deltaTime * dampingMod);

        if(pm.grounded || pm.controller.isGrounded) {
            falling = false;
        }

        if(falling) {
            targetY += Time.deltaTime * 0.065f;
            targetSpeed = Mathf.MoveTowards(targetSpeed, 1.8f, Time.deltaTime * 1.5f);
            impactY = Mathf.MoveTowards(impactY, targetY * Mathf.Clamp01(Mathf.Abs(pm.controller.velocity.y * 0.33f)), Time.deltaTime * targetSpeed);
        }
		else {
		    if(startedDown && currentPos.y <= (impactY + 0.1f) * aimMod) {
			    impactY = 0f;
                targetY = 0f;
			    startedDown = false;
			    startedUp = true;
		    }
		
		    if(startedUp && currentPos.y >= -0.1f * aimMod) {
			    startedDown = false;
			    startedUp = false;
		    }

            targetSpeed = 5f;
        }
	}
	
	public void DoImpactAnimation(float velocity) {
		if(startedDown || startedUp) {
			return;
		}
		
		impactY = -Mathf.Max(1.3f, velocity) * impactMagnitude * ((pm.sprinting || pm.wasSprinting) ? 1.6f : 1f) * ((ac.isAiming) ? 0.6f : 1f);
		randomX = Random.Range(-horizontalRot, horizontalRot);
        shakeTime = Time.time + 0.3f;
        ac.shakeIntensity = Mathf.Clamp((velocity - 1.8f) * 0.7f, 0f, 6f);
		startedDown = true;
		pv.jumpRattleEquip = true;
		falling = false;
	}

    public void DownwardMomentum(float rotAmount) {
        downMomentum = -rotAmount;
    }

    public void DoJumpAnimation() {
        jumpRotDown = ((pm.sprinting || pm.wasSprinting) ? -0.15f : -0.28f) * ((ac.isAiming) ? 0.3f : 1f);
    }
	
	public void FallAnimation() {
		falling = true;
		targetY = 0.03f;
        targetSpeed = 5f;
	}
}