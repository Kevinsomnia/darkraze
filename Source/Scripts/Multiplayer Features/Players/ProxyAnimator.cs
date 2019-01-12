using UnityEngine;
using System.Collections;

public class ProxyAnimator : Topan.TopanMonoBehaviour {	
	[HideInInspector] public float requestedWeight = 0f;
	
	private Transform tr;
	private WeaponHandler_Proxy wh;
	private Vector3 lastPos = Vector3.zero;
	private OptimizedAnimator optimizedAnimator;
	private Animator animator;
	private float speed = 0f;
	private float strafe = 0f;
	private bool jumping = false;
	
	public void ToggleCrouch(bool crouch) {
		animator.SetBool("Crouching", crouch);
	}
	
	void NetworkStart() {
		tr = transform;
		wh = GetComponent<WeaponHandler_Proxy>();
		animator = GetComponent<Animator>();
		animator.SetLayerWeight(1, 1f);
		animator.SetLayerWeight(2, 1f);
		
		optimizedAnimator = new OptimizedAnimator(animator);
	}
	
	private IEnumerator JumpAnimRoutine() {
		jumping = true;
		animator.SetBool("Jumping", true);
		bool setJumpToFalse = false;
		
		while(!setJumpToFalse) {
			if(animator.IsInTransition(1)) {
				AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(1);		
				
				if(state.IsName("Legs.Jump")) {
					animator.SetBool("Jumping", false);
					setJumpToFalse = true;
				}
			}

			yield return null;	
		}
		
		jumping = false;
	}
	
	void Update() {
		if(Time.timeScale <= 0f || tr == null) {
			return;
		}

		Vector3 vel = tr.InverseTransformDirection((tr.position - lastPos) / Time.deltaTime);
		lastPos = tr.position;
		
		if(vel.z > 0.2f) {
			speed = Mathf.Lerp(speed, 1f, Time.deltaTime * 8f);
		}
		else if(vel.z < -0.2f) {
			speed = Mathf.Lerp(speed, -1f, Time.deltaTime * 8f);
		}
		else {
			speed = Mathf.Lerp(speed, 0f, Time.deltaTime * 8f);
		}

		optimizedAnimator.SetFloat("Speed", speed);	
		
		float strafeVal = 0.5f;
		if(Mathf.Abs(vel.z) <= 0.2f) {
			strafeVal *= 2f;	
		}
		
		if(vel.x > 0.2f) {
			strafe = Mathf.Lerp(strafe, strafeVal, Time.deltaTime * 8f);
		}
		else if(vel.x < -0.2f) {
			strafe = Mathf.Lerp(strafe, -strafeVal, Time.deltaTime * 8f);
		}
		else {
			strafe = Mathf.Lerp(strafe, 0f, Time.deltaTime * 8f);
		}

		optimizedAnimator.SetFloat("Strafe", strafe);	
	}
	
	/*
	void OnAnimatorIK(int layerIndex) {
		if(layerIndex == 2) {
			Debug.Log("IKIK " + (1f - requestedWeight));
			animator.SetIKPosition(AvatarIKGoal.LeftHand, wh.currentGC.leftHandTransform.position);
			animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f - requestedWeight); 
	
			animator.SetIKRotation(AvatarIKGoal.LeftHand, wh.currentGC.leftHandTransform.rotation);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f - requestedWeight); 
		}
	}
	*/
}