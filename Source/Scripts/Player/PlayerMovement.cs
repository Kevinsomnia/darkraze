using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
	public enum MovementTransferOnJump {
		None, //The jump is not affected by velocity of floor at all.
		InitTransfer, //Jump gets its initial velocity from the floor, then gradualy comes to a stop.
		PermaTransfer, //Jump keeps initial velocity until landing.
		PermaLocked //On jump, player will be parented to platform until landing.
	}

	[System.Serializable]
	public class Movement {
		public bool enabled = true;
		public float runSpeed = 5f;
		public float sprintSpeed = 8f;
		public float crouchSpeed = 3f;
		public float walkSpeed = 3.5f;
		public float sprintAcceleration = 4.5f;

		public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90f, 1.2f), new Keyframe(0f, 1f), new Keyframe(90f, 0.4f));

		public float groundAcceleration = 30f;
		public float airAcceleration = 2f;
		public float gravity = 9.81f;
		public float maxFallSpeed = 80f;

        public CrouchDetector crouchDetection;
		public float crouchHeight = 1.2f;
		public float jumpHeight = 0.6f;
		public float perpAmount = 0.25f;
		public float pushDownFactor = 0.1f;

        public float ladderClimbMagnitude = 2f;
		public float ladderClimbRate = 3f;
	}

    [System.Serializable]
    public class Footsteps {
        public bool enabled = true;
        public AudioSource footstepPos;
        public AudioClip[] concrete = new AudioClip[2];
        public AudioClip[] dirt = new AudioClip[2];
        public AudioClip[] metal = new AudioClip[2];
        public AudioClip[] wood = new AudioClip[2];
        public AudioClip jumpLatchLadder;
        public AudioClip jumpOffLadder;
        public float runStepRate = 1.6f; //lower number is faster, this should be the lower number.
        public float sprintStepRate = 2.1f; //lower number is faster, however this should be higher than run since it takes velocity into account.
        public float ladderStepRate = 0.9f;
    }

	[System.Serializable]
	public class Sliding {
		public float slidingSpeed = 8f;
		public float driftControl = 0.5f; //Drifting horizontally while you slide.
		public LayerMask slipOnLayers = -1;
		public float slipInfluenceDist = 0.3f;
		public float slipSpeed = 1.8f;
	}

	[System.Serializable]
	public class MovingPlatform {
		public MovementTransferOnJump movementTransfer = MovementTransferOnJump.InitTransfer;
		public float platformSmoothing = 12f;
	}

	[System.Serializable]
	public class Animations {
		public bool enabled = true;
		public GameObject playerMesh;
		public float meshOffset = 0.5f;
		public ShadowController shadowControl;
		public string idleAnim = "Idle";
		public string runAnim = "Run";
		public string runLeftAnim = "RunLeft";
		public string runRightAnim = "RunRight";
		public string runBackAnim = "";
		public string crouchAnim = "Crouch";
		public string crouchForwardAnim = "CrouchForward";
		public string crouchLeftAnim = "CrouchLeft";
		public string crouchRightAnim = "CrouchRight";
		public string jumpAnim = "Jump";
	}

    [System.Serializable]
    public class PlayerPhysics {
        public bool enabled = true;
        public float pushForce = 3f;
        public float mass = 50f; //in kg
    }

	public Movement movement = new Movement();
    public Footsteps footsteps = new Footsteps();
	public Sliding sliding = new Sliding();
	public MovingPlatform movingPlatform = new MovingPlatform();
	public Animations animations = new Animations();
    public PlayerPhysics physics = new PlayerPhysics();

	public Transform head;
    public CapsuleCollider botDetector; //For bots to detect you.
    public float fadeInSpeed = 5f;
    public AudioClip initializeSound;
	public GameSettings settingsPrefab;
	public UIController uiPrefab;

	[HideInInspector] public CharacterController controller;
	[HideInInspector] public bool grounded = false;
	[HideInInspector] public bool wasSprinting;
	[HideInInspector] public bool sprinting;
	[HideInInspector] public bool crouching;
	[HideInInspector] public bool walking;
	[HideInInspector] public float controllerVeloMagn;
	[HideInInspector] public float xyVelocity;
	[HideInInspector] public float fDmgSpeedMult = 1f;
	[HideInInspector] public float speedAimMod = 1f;
	[HideInInspector] public float sprintReloadBoost = 1f;
	[HideInInspector] public bool onLadder;
	[HideInInspector] public float ladderFaceRot;
	[HideInInspector] public bool isMoving = false;
	[HideInInspector] public bool isSlipping = false;
    [HideInInspector] public Vector3 moveDirection = Vector3.zero;

	//Movement
	private CollisionFlags collisionFlags;
	private Vector2 inputVector = Vector2.zero;
	private Vector3 velocity = Vector3.zero;
	private Vector3 frameVelocity = Vector3.zero;
	private Vector3 hitPoint = Vector3.zero;
	private Vector3 lastHitPoint = Vector3.zero;
	private Vector3 groundNormal = Vector3.zero;
	private Vector3 lastGroundNormal = Vector3.zero;
	private float diagonalFactor = 1f;
	private float normalLimit = 0f;
	private float curSpeed;
	private bool isSliding;
	private float lastRunTime;
	private bool runOnce;
	private float lastCrouchTime;
	private float defaultHeight;
	private Vector3 defaultHeadPos;
	private float weightFactor;
	private float slopeMod;
	private float impactMod;

	private Vector3 influenceOffset;
	private Vector3 oldInfluenceOffset;
	private Vector3 slipMovement;

    private Rigidbody standingRigidbody;

	//Jumping
	private float jumpTime;
    private float sprintJumpRestrict;
	private Vector3 jumpDir = Vector3.up;
	private bool didJump = false;

    //Footsteps
    private float stepTimer;
    private string surfaceTag;
    private TimeScaleSound tss;
    private AudioClip footSound;
    private float stepRate;

	//Moving Platforms
	private Transform activePlatform;
	private Vector3 activeLocalPoint;
	private Vector3 activeGlobalPoint;
	private Quaternion activeLocalRotation;
	private Quaternion activeGlobalRotation;
	private Matrix4x4 lastMatrix;
	private Vector3 platformVelocity;
	private bool newPlatform;
	private Vector3 smoothPlatform;

	//Animations
	private Vector3 defaultLegPos;

	//Ladders
	private Ladder currentLadder;
	private Ladder oldLadder;
	private Vector3 climbDirection;
    private float ladderLatchTime;
    private float lastLadderUnlatch;
	private float climbDir;
    private float latchBoost;

	private Transform tr;
	private Animation playerMeshAnim;
	private ImpactAnimation ia;
	private PlayerVitals pv;
	private PlayerLook pl;
	private WeightController wc;
	private WeaponManager wm;
	private AimController ac;
	private DynamicMovement dm;
    private UISprite fadeSprite;
	private Vector3 pauseVelocity;

	void Awake() {
		GeneralVariables.player = gameObject;
		GeneralVariables.cachedUI = uiPrefab;
		GameSettings.cachedSettingsPrefab = settingsPrefab;
		GameSettings.settingsController.Initialize();     
	}

	void Start() {
        GeneralVariables.player = gameObject;
        GeneralVariables.uiController.guiCamera.enabled = true;
		tr = transform;
		controller = GetComponent<CharacterController>();
        tss = footsteps.footstepPos.GetComponent<TimeScaleSound>();
		playerMeshAnim = animations.playerMesh.GetComponent<Animation>();

        UIController uic = GeneralVariables.uiController;
        fadeSprite = uic.fadeFromBlack;
        uic.guiCamera.GetComponent<GUISway>().InitializeVariables();

		PlayerReference pr = GetComponent<PlayerReference>();
		pv = GetComponent<PlayerVitals>();
		pl = GetComponent<PlayerLook>();
		ia = pr.ia;
		wc = pr.wc;
		ac = pr.ac;
		wm = pr.wm;
		dm = pr.dm;

		defaultHeight = controller.height;
		defaultHeadPos = head.localPosition;
		defaultLegPos = animations.playerMesh.transform.localPosition + (Vector3.down * animations.meshOffset);
		normalLimit = Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad);
        curSpeed = movement.runSpeed;
		jumpTime = -0.25f;
		impactMod = 1f;
		fDmgSpeedMult = 1f;

        AntiHackSystem.ProtectFloat("runSpeed", movement.runSpeed);
        AntiHackSystem.ProtectFloat("sprintSpeed", movement.sprintSpeed);
        AntiHackSystem.ProtectFloat("crouchSpeed", movement.crouchSpeed);
        AntiHackSystem.ProtectFloat("walkSpeed", movement.walkSpeed);
        AntiHackSystem.ProtectFloat("jumpHeight", movement.jumpHeight);

		grounded = false;
        movement.crouchDetection.gameObject.SetActive(false);
        AudioSource.PlayClipAtPoint(initializeSound, footsteps.footstepPos.transform.position, 0.12f);
		StartCoroutine(FadeFromBlack());
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
        RaycastHit checkStand;
        bool hasHit = Physics.Raycast(transform.position, Vector3.down, out checkStand, 1f);
        bool standValid = (hasHit && (checkStand.collider.GetInstanceID() == hit.collider.GetInstanceID()));

        if(hit.normal.y > 0.1f && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0f) {
            if((hit.point - lastHitPoint).sqrMagnitude >= 0.001f || lastGroundNormal == Vector3.zero) {
                groundNormal = hit.normal;
            }
            else {
                groundNormal = lastGroundNormal;
            }

            if(standValid) {
                if(activePlatform != hit.collider.transform) {
                    activePlatform = hit.collider.transform;
                    lastMatrix = activePlatform.localToWorldMatrix;
                    newPlatform = true;
                }
            }
            else {
                activePlatform = null;
                newPlatform = false;
            }

            hitPoint = hit.point;
            frameVelocity = Vector3.zero;
        }

        standingRigidbody = null;
        Rigidbody rigid = hit.collider.attachedRigidbody;

        if(physics.enabled && rigid != null) {
            Vector3 pushDir = hit.moveDirection;
            pushDir.y = 0f;
            rigid.AddForceAtPosition(pushDir * physics.pushForce, hit.point, ForceMode.Impulse);
            
            if(hit.normal.y >= 0.35f && hit.moveDirection.y <= -0.3f && standValid) {
                standingRigidbody = rigid;
            }
        }

        if(!onLadder) {
            surfaceTag = hit.collider.tag;
        }        
	}

	void Update() {
        float receivedRunSpeed = AntiHackSystem.RetrieveFloat("runSpeed");

		inputVector = (!RestrictionManager.restricted && !RestrictionManager.mpMatchRestrict) ? new Vector2(cInput.GetAxis("Horizontal Move"), cInput.GetAxis("Vertical Move")) : Vector2.zero;

		bool reloading = (wm.currentGC != null && wm.currentGC.reloading);
		if(!RestrictionManager.restricted && (grounded || controller.isGrounded) && !onLadder) {
			if(cInput.GetButton("Run") && inputVector.y >= 0.15f && pv.canSprint && !ac.isAiming && (Time.time - lastRunTime) >= 0.5f && !dm.animationIsPlaying) {
				runOnce = false;

				if(reloading) {
					sprintReloadBoost = 1.25f;
				}
				else {
                    if(xyVelocity >= Mathf.Clamp(curSpeed, 0f, receivedRunSpeed) * 0.45f * weightFactor * slopeMod) {
                        if(!crouching) {
                            sprinting = true;
                            walking = false;
                        }

                        if(CanStandUp()) {
                            crouching = false;
                        }
                    }
                    else if(xyVelocity < receivedRunSpeed * 0.2f) {
                        sprinting = false;
                    }

					sprintReloadBoost = 1f;
				}
			}
			else {
				sprintReloadBoost = 1f;
				sprinting = false;
			}

			if(cInput.GetButtonDown("Walk") && !sprinting && !crouching) {
				walking = !walking;
			}

			if(cInput.GetButtonDown("Crouch") && !sprinting && (Time.time - lastCrouchTime) >= 0.25f) {
				if(!crouching) {
					ia.DoImpactAnimation(2.5f);
					crouching = true;

                    movement.crouchDetection.gameObject.SetActive(true);
				}
				else {
					if(CanStandUp()) {
						ia.DoImpactAnimation(1.9f);
						crouching = false;
                        movement.crouchDetection.gameObject.SetActive(false);
					}
				}

                walking = false;
				lastCrouchTime = Time.time;
			}
		}

		if(!sprinting && !runOnce && !reloading) {
			lastRunTime = Time.time;
			runOnce = true;
		}

		float animSpeed = Time.deltaTime * 8.5f;
        float fovLegsClipMod = (GameSettings.settingsController.FOV - 60) * 0.0065f;
		if(crouching && xyVelocity >= 0.5f) {
			controller.height = Mathf.Lerp(controller.height, movement.crouchHeight + 0.1f, animSpeed);
            animations.playerMesh.transform.localPosition = Vector3.Lerp(animations.playerMesh.transform.localPosition, defaultLegPos - new Vector3(0f, 0.31f, fovLegsClipMod), animSpeed);
		}
		else if(crouching && xyVelocity < 0.5f) {
			controller.height = Mathf.Lerp(controller.height, movement.crouchHeight, animSpeed);
            animations.playerMesh.transform.localPosition = Vector3.Lerp(animations.playerMesh.transform.localPosition, defaultLegPos - new Vector3(0f, 0.57f, fovLegsClipMod), animSpeed);
		}
		else {
			controller.height = Mathf.Lerp(controller.height, defaultHeight, animSpeed);
			animations.playerMesh.transform.localPosition = Vector3.Lerp(animations.playerMesh.transform.localPosition, defaultLegPos - new Vector3(0f, 0f, fovLegsClipMod), animSpeed);
		}

        if(onLadder) {
            wasSprinting = false;
            sprinting = false;

            if(!RestrictionManager.restricted && cInput.GetButtonDown("Jump") && pv.curStamina > 5 && Time.time - ladderLatchTime >= 0.4f) {
				climbDir = 0f; //Avoid unlatch force.
                UnlatchLadder();
                velocity += (-tr.forward * 2f) + (Vector3.up * 1.25f);
                pv.curStamina -= 5;
                jumpTime = Time.time + 0.1f;
                AudioSource.PlayClipAtPoint(footsteps.jumpOffLadder, footsteps.footstepPos.transform.position, 0.5f);
            }
        }

		float heightOffset = ((defaultHeight - controller.height) * -0.5f);
		head.localPosition = defaultHeadPos + (Vector3.up * (0.05f + (heightOffset * 2f)));
		Vector3 targetCenter = Vector3.up * (heightOffset + 0.9f);
		if(controller.center != targetCenter) {
			controller.center = targetCenter;
		}

        botDetector.center = controller.center;
        botDetector.radius = controller.radius;
        botDetector.height = controller.height;

		if(grounded && sliding.slipInfluenceDist > 0.01f) {
			float movementInput = Mathf.Clamp01(moveDirection.sqrMagnitude);
			if(xyVelocity > 0.1f && movementInput > 0.001f) {
				influenceOffset = tr.TransformDirection(moveDirection.normalized);
			}
			
			Vector3 origin = tr.position + (influenceOffset * sliding.slipInfluenceDist);
			if(xyVelocity > 0.05f || influenceOffset != oldInfluenceOffset) {
				if(!Physics.Linecast(origin, origin + (Vector3.down * 1.2f), sliding.slipOnLayers.value)) {
					isSlipping = true;
				}
				else {
					isSlipping = false;
				}
				
				oldInfluenceOffset = influenceOffset;
			}
		}

		slipMovement = (isSlipping) ? influenceOffset : Vector3.zero;
		
		weightFactor = 1f - (wc.weightPercentage * 0.3f);
		impactMod = Mathf.MoveTowards(impactMod, 1f, Time.deltaTime * 0.7f);
        diagonalFactor = (inputVector != Vector2.zero && !sprinting) ? (1f / Mathf.Max(1f, inputVector.magnitude)) : 1f;
		moveDirection = new Vector3(inputVector.x * ((sprinting) ? 0.3f : 1f), 0f, inputVector.y) * diagonalFactor * weightFactor * impactMod;

		Vector3 velo = velocity;
		velo = ApplyInputVelocity(velo);
		velo = ApplyGravityAndJump(velo);
		Vector3 currentMovementOffset = velo * Time.deltaTime;

		if(grounded) {
			currentMovementOffset += Vector3.down * movement.pushDownFactor * Time.deltaTime;
		}

        if(activePlatform != null && (grounded || movingPlatform.movementTransfer == MovementTransferOnJump.PermaLocked)) {
            try {
                Vector3 pointDiff = (activePlatform.TransformPoint(activeLocalPoint) - activeGlobalPoint);
                smoothPlatform = Vector3.Lerp(smoothPlatform, pointDiff, Time.deltaTime * movingPlatform.platformSmoothing);
                smoothPlatform.y = pointDiff.y;

                if(smoothPlatform != Vector3.zero) {
                    controller.Move(smoothPlatform);
                }

                Quaternion rotationDiff = (activePlatform.rotation * activeLocalRotation) * Quaternion.Inverse(activeGlobalRotation);

                if(rotationDiff.eulerAngles.y != 0f) {
                    pl.xRot += rotationDiff.eulerAngles.y;
                }
            }
            catch {
            }
		}

		groundNormal = Vector3.zero;

		Vector3 lastPosition = tr.position;
		collisionFlags = controller.Move(currentMovementOffset);
		lastHitPoint = hitPoint;
		lastGroundNormal = groundNormal;

		Vector3 oldHVelocity = new Vector3(velo.x, 0f, velo.z);
        velocity = (tr.position - lastPosition) / Mathf.Max(0.0001f, Time.deltaTime);
		Vector3 newHVelocity = new Vector3(velocity.x, 0f, velocity.z);

		if(oldHVelocity == Vector3.zero) {
			velocity = Vector3.up * velocity.y;
		}
		else {
			float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
			velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + (Vector3.up * velocity.y);
		}

		if(velocity.y < velo.y - 0.001f) {
			if(velocity.y < 0f) {
				velocity.y = velo.y;
			}
		}

		if(pauseVelocity != Vector3.zero && Time.timeScale > 0f) {
			velocity += pauseVelocity; //Apply saved velocity.
			pauseVelocity = Vector3.zero;
		}

		if(!controller.isGrounded) {
			isSlipping = false;
		}

		if((grounded && !controller.isGrounded && groundNormal.y <= 0.01f) || didJump) {
            grounded = (controller.collisionFlags & CollisionFlags.Below) != 0;

            if(!grounded) {
                if(!didJump) {
                    velocity -= Vector3.up * movement.gravity * 0.1f;
                }

                if(!didJump && movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer || movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer) {
                    frameVelocity = platformVelocity;
                    velocity += platformVelocity;
                }

                if(sprinting) {
                    wasSprinting = true;
                    sprintJumpRestrict = 0.15f;
                    sprinting = false;
                }

                ia.FallAnimation();
                playerMeshAnim.CrossFade(animations.jumpAnim, 0.2f);
                didJump = false;
            }
		}
		else if(!grounded && controller.isGrounded && groundNormal.y > 0.01f) {
			grounded = true;
			StartCoroutine(SubtractNewPlatformVelocity());

			if(onLadder) {
				UnlatchLadder();
			}

			float impactVelo = Mathf.Abs(velo.y);
			if(impactVelo >= 0.5f) {
				footsteps.footstepPos.volume = 0.1f + Mathf.Clamp(0.023f * impactVelo, 0f, 0.64f);
				tss.UpdatePitch(Random.Range(0.88f, 0.95f));
                SelectFootstep();
				tss.GetComponent<AudioSource>().PlayOneShot(footSound);
				stepTimer = 0f;

                if(standingRigidbody != null) {
                    Vector3 rigidImpact = controller.velocity;
                    rigidImpact.y *= (0.05f * physics.mass);
                    rigidImpact.y = Mathf.Max(-5f, rigidImpact.y);
                    standingRigidbody.AddForceAtPosition(rigidImpact, transform.position, ForceMode.Impulse);
                }

				impactMod = 1f - Mathf.Clamp((impactVelo - 0.5f) * 0.08f, 0f, 0.5f);
				pv.FallDamage(impactVelo);
				ia.DoImpactAnimation(impactVelo - 0.32f);
			}

            wasSprinting = false;
		}

        if(activePlatform != null && (grounded || movingPlatform.movementTransfer == MovementTransferOnJump.PermaLocked)) {
            activeGlobalPoint = tr.position + Vector3.up * (controller.center.y - (controller.height * 0.5f) + controller.radius);
            activeLocalPoint = activePlatform.InverseTransformPoint(activeGlobalPoint);

            activeGlobalRotation = tr.rotation;
            activeLocalRotation = Quaternion.Inverse(activePlatform.rotation) * activeGlobalRotation;
        }

		Vector3 controllerVelo = controller.velocity;
		controllerVeloMagn = controllerVelo.magnitude;
		controllerVelo.y = 0f;
		xyVelocity = controllerVelo.magnitude;

		isMoving = (controllerVeloMagn > 0.25f);

        if(((grounded && controller.isGrounded) || onLadder) && footsteps.enabled && !isSliding) {
            if(sprinting) {
                stepRate = Mathf.Lerp(stepRate, footsteps.sprintStepRate, Time.deltaTime * 8f);
            }
            else if(sprintReloadBoost > 1f) {
                stepRate = Mathf.Lerp(stepRate, footsteps.runStepRate * 1.1f, Time.deltaTime * 8f);
            }
            else if(onLadder) {
                stepRate = Mathf.Lerp(stepRate, footsteps.ladderStepRate, Time.deltaTime * 8f);
            }
            else {
                stepRate = Mathf.Lerp(stepRate, footsteps.runStepRate, Time.deltaTime * 8f);
            }

			if(Mathf.Abs(inputVector.x) + Mathf.Abs(inputVector.y) >= 0.05f) {
				stepTimer += controllerVeloMagn * Time.deltaTime;
			}
			
			if(stepTimer >= stepRate) {				
				if(sprinting) {
					footsteps.footstepPos.volume = 0.175f;
				}
				else if(crouching || walking) {
					footsteps.footstepPos.volume = 0.07f;
				}
				else {
					footsteps.footstepPos.volume = 0.12f;
				}
				
				tss.UpdatePitch(Random.Range(0.9f, 1.0f));
                SelectFootstep();
				footsteps.footstepPos.PlayOneShot(footSound);
				stepTimer -= stepRate;
			}
		}

		if(animations.enabled && xyVelocity > 0.5f && !isSliding) {
			if(sprinting) {
				playerMeshAnim[animations.runAnim].speed = xyVelocity * 0.18f;
				playerMeshAnim.CrossFade(animations.runAnim, 0.25f);
			}
			else {
				Vector3 moveDir = moveDirection.normalized;

				if(crouching) {
					float animVeloSpeed = xyVelocity * 0.5f;
					
					if(moveDir.z > 0.05f) {
						playerMeshAnim[animations.crouchForwardAnim].speed = animVeloSpeed;
						playerMeshAnim.CrossFade(animations.crouchForwardAnim, 0.25f);
					}
					else if(moveDir.z < -0.05f) {
						playerMeshAnim[animations.crouchForwardAnim].speed = -animVeloSpeed;
						playerMeshAnim.CrossFade(animations.crouchForwardAnim, 0.25f);
					}
					else if(moveDir.x < -0.05f) {
						playerMeshAnim[animations.crouchLeftAnim].speed = animVeloSpeed;
						playerMeshAnim.CrossFade(animations.crouchLeftAnim, 0.25f);
					}
					else if(moveDir.x > 0.05f) {
						playerMeshAnim[animations.crouchRightAnim].speed = animVeloSpeed;
						playerMeshAnim.CrossFade(animations.crouchRightAnim, 0.25f);
					}
				}
				else {
					float animVeloSpeed = xyVelocity * 0.25f;
					
					if(moveDir.z > 0.05f) {
						playerMeshAnim[animations.runAnim].speed = animVeloSpeed;
						playerMeshAnim.CrossFade(animations.runAnim, 0.25f);
					}
					else if(moveDir.z < -0.05f) {
						if(animations.runBackAnim != "") {
							playerMeshAnim[animations.runBackAnim].speed = animVeloSpeed;
							playerMeshAnim.CrossFade(animations.runBackAnim, 0.25f);
						}
						else {
							playerMeshAnim[animations.runAnim].speed = -animVeloSpeed;
							playerMeshAnim.CrossFade(animations.runAnim, 0.25f);
						}
					}
					else if(moveDir.x < -0.05f) {
						playerMeshAnim[animations.runLeftAnim].speed = animVeloSpeed;
						playerMeshAnim.CrossFade(animations.runLeftAnim, 0.25f);
					}
					else if(moveDir.x > 0.05f) {
						playerMeshAnim[animations.runRightAnim].speed = animVeloSpeed;
						playerMeshAnim.CrossFade(animations.runRightAnim, 0.25f);
					}
				}
			}
		}

		if(animations.enabled && xyVelocity <= 0.5f) {
			if(crouching && !isSliding) {
				playerMeshAnim.CrossFade(animations.crouchAnim, 0.25f);
			}
			else {
				playerMeshAnim.CrossFade(animations.idleAnim, 0.25f);
			}
		}

		if(activePlatform != null) {
			if(!newPlatform) {
				platformVelocity = ((Time.deltaTime > 0f) ? (activePlatform.localToWorldMatrix.MultiplyPoint3x4(activeLocalPoint) - lastMatrix.MultiplyPoint3x4(activeLocalPoint)) / Time.deltaTime : Vector3.zero);
			}
			
			lastMatrix = activePlatform.localToWorldMatrix;
			newPlatform = false;
		}
		else {
			platformVelocity = Vector3.zero;
		}

		fDmgSpeedMult = Mathf.Clamp(fDmgSpeedMult, 0.5f, 1f);
		fDmgSpeedMult = Mathf.MoveTowards(fDmgSpeedMult, 1f, Time.deltaTime * 0.007f);

		if(pauseVelocity == Vector3.zero && Time.timeScale <= 0f) {
			pauseVelocity = velocity; //Maintain velocity until the next time you un-pause.
		}
	}

    void FixedUpdate() {
        if(grounded && standingRigidbody != null) {
            standingRigidbody.AddForceAtPosition(Vector3.down * movement.gravity * physics.mass * 0.1f, Vector3.Lerp(standingRigidbody.worldCenterOfMass, transform.position, 0.55f), ForceMode.Force);
        }
    }

	private Vector3 ApplyInputVelocity(Vector3 vel) {
		if(!movement.enabled) {
			moveDirection = Vector3.zero;
		}
		
		Vector3 desiredVelocity = Vector3.zero;
		if(grounded && groundNormal.y <= normalLimit) {
			isSliding = true;
			desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
			Vector3 projectedMoveDir = Vector3.Project(moveDirection, desiredVelocity);
			desiredVelocity = desiredVelocity + (moveDirection - projectedMoveDir) * sliding.driftControl;
			desiredVelocity *= sliding.slidingSpeed;
		}
		else {
			isSliding = false;

			if(!onLadder) {
				if(grounded) {
					if(sprinting) {
                        curSpeed = Mathf.MoveTowards(curSpeed, AntiHackSystem.RetrieveFloat("sprintSpeed") - (Mathf.Clamp01((100f - pv.curStamina) / 100f) * 0.65f), Time.deltaTime * movement.sprintAcceleration);
					}
					else if(crouching) {
                        curSpeed = AntiHackSystem.RetrieveFloat("crouchSpeed");
					}
					else if(walking) {
                        curSpeed = AntiHackSystem.RetrieveFloat("walkSpeed");
					}
					else {
                        curSpeed = Mathf.MoveTowards(curSpeed, AntiHackSystem.RetrieveFloat("runSpeed"), Time.deltaTime * movement.sprintAcceleration);
					}
				}
				else {
                    curSpeed = Mathf.MoveTowards(curSpeed, AntiHackSystem.RetrieveFloat("runSpeed") * 0.5f, Time.deltaTime * 0.9f);
				}

				slopeMod = movement.slopeSpeedMultiplier.Evaluate(velocity.normalized.y * 55f);
			}

			float slipDamp = (xyVelocity > 2f) ? 0.45f : 1f;
			desiredVelocity = tr.TransformDirection(moveDirection * curSpeed * slopeMod * speedAimMod * sprintReloadBoost * fDmgSpeedMult) + (slipMovement * sliding.slipSpeed * slipDamp);

			if(onLadder) {
				desiredVelocity = Vector3.zero;
			}
		}

		if(movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer) {
			desiredVelocity += frameVelocity;
			desiredVelocity.y = 0f;
		}

		if(grounded) {
			Vector3 sideways = Vector3.Cross(Vector3.up, desiredVelocity);
			desiredVelocity = Vector3.Cross(sideways, groundNormal).normalized * desiredVelocity.magnitude;
		}
		else {
			vel.y = 0f;
		}

		float maxVelocityChange = ((grounded) ? movement.groundAcceleration : movement.airAcceleration) * Time.deltaTime;
		if(onLadder) {
			maxVelocityChange = 0f;
		}

		Vector3 velocityChangeVector = (desiredVelocity - vel);
		if(velocityChangeVector.sqrMagnitude > maxVelocityChange * maxVelocityChange) {
			velocityChangeVector = velocityChangeVector.normalized * maxVelocityChange;
		}
		
		if(grounded || movement.enabled) {
			vel += velocityChangeVector;
		}

		if(grounded) {
			vel.y = Mathf.Min(0f, vel.y);
		}

		return vel;
	}

	private Vector3 ApplyGravityAndJump(Vector3 vel) {
		if(!onLadder && grounded) {
			vel.y = Mathf.Min(0f, vel.y) - movement.gravity * Time.deltaTime;

            if(movement.enabled && !onLadder && (Time.time - jumpTime >= 0.25f + sprintJumpRestrict) && !RestrictionManager.restricted && !RestrictionManager.mpMatchRestrict && cInput.GetButtonDown("Jump") && groundNormal.y > normalLimit) {
				if(!crouching) {
					int staminaDrain = (sprinting) ? 9 : 7;

					if(pv.curStamina > staminaDrain) {
						float jumpWgtFactor = 1f - (wc.weightPercentage * 0.15f);
						grounded = false;
						didJump = true;
						ia.DoJumpAnimation();

						jumpDir = Vector3.Slerp(Vector3.up, groundNormal, movement.perpAmount);

						vel.y = 0f;
                        vel += jumpDir * Mathf.Lerp(fDmgSpeedMult, 1f, 0.5f) * (Mathf.Sqrt(2f * AntiHackSystem.RetrieveFloat("jumpHeight") * jumpWgtFactor * 9.81f));

						if(movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer || movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer) {
							Vector3 modVel = platformVelocity;
							modVel.y = 0f;
							modVel *= 0.45f;

							frameVelocity = modVel;
							vel += modVel;
						}

						pv.jumpRattleEquip = true;
						pv.curStamina -= Mathf.RoundToInt((staminaDrain - 2) * (1f / jumpWgtFactor));
					}

                    sprintJumpRestrict = 0f;
				}
				else {
                    if(CanStandUp()) {
                        ia.DoImpactAnimation(1.9f);
                        crouching = false;
                    }
				}
			}
		}
		else if(onLadder) {
			vel += currentLadder.climbDirection * Mathf.Clamp(inputVector.y + latchBoost, -1f, 1f) * movement.ladderClimbMagnitude * fDmgSpeedMult * (0.75f + (1f + Mathf.Cos(Time.time * 2f * Mathf.PI * movement.ladderClimbRate)) * 0.25f);
            latchBoost = Mathf.MoveTowards(latchBoost, 0f, Time.deltaTime * 3f);
            climbDir = inputVector.normalized.y;

			float distanceToTop = Mathf.Max(-0.1f, currentLadder.topSpot.y - tr.position.y);
			if(distanceToTop < 0.9f) {
				pl.ladderClampAnim = Mathf.Lerp(pl.ladderClampAnim, (0.9f - distanceToTop) * 100f, Time.deltaTime * 5.5f);
			}
		}
		else {
			vel.y = Mathf.Max(velocity.y - movement.gravity * Time.deltaTime, (!onLadder) ? -movement.maxFallSpeed : 0f);
			dm.terminalVelocity = (vel.y <= -movement.maxFallSpeed * 0.85f);
            jumpTime = Time.time;
		}

		return vel;
	}

	private IEnumerator SubtractNewPlatformVelocity() {
		if(movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer || movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer) {
			if(newPlatform) {
				Transform platform = activePlatform;
				yield return new WaitForFixedUpdate();
				yield return new WaitForFixedUpdate();
				
				if(grounded && platform == activePlatform) {
					yield return 1;
				}
			}
			
			velocity -= platformVelocity;
		}
	}

	void OnTriggerEnter(Collider other) {
		if(onLadder || (wm.currentGC != null && wm.currentGC.reloading)) {
			return;
		}

		if(other.CompareTag("Ladder")) {
			Ladder ladderComp = other.gameObject.GetComponent<Ladder>();
			float entryAngle = Mathf.DeltaAngle(tr.eulerAngles.y, ladderComp.faceDirectionAngle);
			if(Mathf.Abs(entryAngle) <= 45f) {
				LatchLadder(ladderComp);
			}
		}
	}

	void OnTriggerExit(Collider other) {
		if(!onLadder || (wm.currentGC != null && wm.currentGC.reloading)) {
			return;
		}

		if(other.CompareTag("Ladder")) {
			UnlatchLadder();
		}
	}

	private void LatchLadder(Ladder latchedLadder) {
        if(crouching) {
            if(CanStandUp()) {
                crouching = false;
            }

            return;
        }

		if(Time.time - lastLadderUnlatch < 0.3f || (oldLadder != null && (oldLadder.topSpot - tr.position).sqrMagnitude <= 0.8f * 0.8f && latchedLadder == oldLadder)) {
			return;
		}

		onLadder = true;
        SelectFootstep();
		currentLadder = latchedLadder;
		ladderFaceRot = currentLadder.faceDirectionAngle;
        ia.DownwardMomentum((wasSprinting) ? 15f : 7f);
        moveDirection = Vector3.zero;
		velocity = Vector3.zero;
        latchBoost = (grounded) ? 1f : 0.25f;
		sprinting = false;
        stepTimer *= 0.3f;
        ladderLatchTime = Time.time;

        if(!grounded && !controller.isGrounded) {
            AudioSource.PlayClipAtPoint(footsteps.jumpLatchLadder, footsteps.footstepPos.transform.position, 0.5f);
        }

        StartCoroutine(AdjustToLadderCenter());
	}

	private void UnlatchLadder() {
		onLadder = false;
		oldLadder = currentLadder;
		currentLadder = null;

		if(climbDir > 0f) {
			velocity += (tr.forward * 2.5f) + (Vector3.up * 0.25f);
		}
		else if(climbDir < 0f) {
			velocity -= (tr.forward * (3.2f + (moveDirection.z * 1.5f))); //Step back from ladder
		}

		pl.ladderClampAnim = 0f;
        lastLadderUnlatch = Time.time;
	}

    private IEnumerator AdjustToLadderCenter() {
        while(currentLadder != null) {
            Vector3 adjustToCenter = tr.InverseTransformDirection(currentLadder.col.bounds.center - tr.position);
			adjustToCenter.z -= currentLadder.col.bounds.extents.z;

			if(Mathf.Abs(adjustToCenter.x) + Mathf.Abs(adjustToCenter.z) > 0f) {
				float deltaX = Mathf.Clamp(Time.deltaTime * 1.6f, 0f, Mathf.Abs(adjustToCenter.x));
				float deltaZ = Mathf.Clamp(Time.deltaTime * 1.6f, 0f, Mathf.Abs(adjustToCenter.z));
				tr.position += tr.TransformDirection((Vector3.right * deltaX * CustomSign(adjustToCenter.x)) + (Vector3.forward * deltaZ * CustomSign(adjustToCenter.z)));
            }

            yield return null;
        }
    }

    private int CustomSign(float input) {
        if(input < 0f) {
            return -1;
        }
        else if(input > 0f) {
            return 1;
        }

        return 0;
    }

    private bool CanStandUp() {
        if(!crouching) {
            return false;
        }

        return movement.crouchDetection.canStandUp;
    }

    private void SelectFootstep() {
        AudioClip clipToPlay = null;

        if(onLadder) {
            do {
                clipToPlay = footsteps.metal[Random.Range(0, footsteps.metal.Length)];
            }
            while(footsteps.metal.Length > 1 && clipToPlay == footSound);
        }
        else {
            if(surfaceTag == "Dirt") {
                do {
                    clipToPlay = footsteps.dirt[Random.Range(0, footsteps.dirt.Length)];
                }
                while(footsteps.dirt.Length > 1 && clipToPlay == footSound);
            }
            else if(surfaceTag == "Metal") {
                do {
                    clipToPlay = footsteps.metal[Random.Range(0, footsteps.metal.Length)];
                }
                while(footsteps.metal.Length > 1 && clipToPlay == footSound);
            }
            else if(surfaceTag == "Wood") {
                do {
                    clipToPlay = footsteps.wood[Random.Range(0, footsteps.wood.Length)];
                }
                while(footsteps.wood.Length > 1 && clipToPlay == footSound);
            }
            else {
                do {
                    clipToPlay = footsteps.concrete[Random.Range(0, footsteps.concrete.Length)];
                }
                while(footsteps.concrete.Length > 1 && clipToPlay == footSound);
            }
        }

        footSound = clipToPlay;
    }

    private IEnumerator FadeFromBlack() {
        fadeSprite.alpha = 1f;
        while(fadeSprite.alpha > 0f) {
            fadeSprite.alpha = Mathf.MoveTowards(fadeSprite.alpha, 0f, Time.deltaTime * fadeInSpeed);
            yield return null;
        }

        fadeSprite.alpha = 0f;
    }
}