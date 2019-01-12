using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class HoverController : MonoBehaviour {
	public float moveSpeed = 10f; //The speed of moving the vehicle using W,A,S,D.
	public float hoverForce = 8f; //When it is close enough to the ground, it will hover at this amount of force.
	public float constantHoverForce = 2.5f; //Even when in-air, it will keep adding this amount of force.
	public Transform castingPoint; //This is the position where a raycast is casted down to hover.
	public float castingDistance = 4f; //Distance from ground in order to hover.
	public AudioSource engineSource; //Audio source with engine sound on it. Loop and play on awake.
	public float windDrag = 0.1f; //Drag that is affecting the hover controller by wind (or some other force). This also controls the maximum speed.
	public float flightRandomness = 0.07f; //Makes the hovercraft a little bit wobbly. This is more noticable when not moving (more realistic).
	public LayerMask layersToHoverOn = -1; //Only hover on these layer surfaces.
	
	private Rigidbody rigid;
	private float rotationX; //Rotating the hovercraft.
	private float speed;
    private float brakeValue;
	
	private float perlinX;
	private float perlinZ;
	
	void Start() {
		rigid = GetComponent<Rigidbody>();
		
		if(engineSource)
			engineSource.Play();
	}
	
	void OnGUI() {
		GUILayout.Box(speed.ToString("F0") + " KM/H" + "\n" + (speed * 0.622f).ToString("F0") + " MPH");
	}
	
	void Update() {
		perlinX = (Mathf.PerlinNoise(Mathf.PingPong(Time.time, 25f), 0f) - 0.5f) * flightRandomness;
		perlinZ = (Mathf.PerlinNoise(0f, Mathf.PingPong(Time.time, 25f)) - 0.5f) * flightRandomness;
	}
	
	void FixedUpdate() {
		float inputX = Input.GetAxis("Horizontal");
		float inputY = Input.GetAxis("Vertical");
		float mouseX = Input.GetAxis("Mouse X");
		float backwardsMod = (inputY < 0f) ? 0.7f : 1f;
		
		Vector3 moveDirection = new Vector3((inputX * 0.65f), 0f, (inputY * backwardsMod));
		moveDirection = transform.TransformDirection(moveDirection);
		
		bool hovering = false;
		RaycastHit hit;
		if(Physics.Raycast(castingPoint.position, -castingPoint.up, out hit, castingDistance + Mathf.Clamp(-rigid.velocity.y * 0.8f, 0f, 9f), layersToHoverOn.value)) {
			hovering = true;
			float distanceMod = 1f - Mathf.Clamp((Vector3.Distance(castingPoint.position, hit.point) / castingDistance) * 0.1f, 0f, 0.1f);
			rigid.AddForce(castingPoint.up * hoverForce * rigid.mass * (distanceMod + Mathf.Clamp(-rigid.velocity.y * 0.1f, 0f, 0.7f) + Mathf.Abs(rigid.velocity.z * 0.004f)), ForceMode.Acceleration);
		
			if(hit.rigidbody) {
				hit.rigidbody.AddForceAtPosition(-castingPoint.up * hoverForce * 0.5f, hit.point);
			}
		}
		
		rigid.AddForce((moveDirection * moveSpeed * ((hovering) ? 1f : 0.4f)) + (Vector3.up * constantHoverForce));
		rigid.angularVelocity = Vector3.Lerp(rigid.angularVelocity, Vector3.zero, Time.deltaTime);
		rotationX += Time.deltaTime * mouseX * 25f;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3((inputY * 5f) + Mathf.Clamp(-rigid.velocity.y * 2f, 0f, 40f) + Mathf.Clamp((perlinZ / flightRandomness) * 5f, -5f, 5f), ClampAngle(), (-inputX * 10f) + (mouseX * 2f) + Mathf.Clamp((perlinX / flightRandomness) * 7f, -7f, 7f))), Time.deltaTime * 2.5f);
		
		brakeValue = 0f;
        if(Mathf.Abs(inputX) + Mathf.Abs(inputY) <= 0f) {
            brakeValue = 0.5f;
        }
		else if(Input.GetKey(KeyCode.Space)) {
            brakeValue = 0.75f;
		}

        Vector3 resistanceVector = -rigid.velocity * (windDrag + brakeValue + Mathf.Clamp(Mathf.Abs(mouseX) * 0.1f, 0f, 0.7f));
		resistanceVector.y = 0f;
		rigid.AddForce(resistanceVector);
		
		Vector3 flightRandom = new Vector3(perlinX, (perlinX + perlinZ) * 1.5f, perlinZ);
		rigid.AddForce(flightRandom, ForceMode.VelocityChange);
		
		speed = rigid.velocity.magnitude;
		
		Vector3 XZVelocity = rigid.velocity;
		XZVelocity.y = 0f;
		
		if(engineSource)
			engineSource.pitch = Mathf.Lerp(engineSource.pitch, 1f + (XZVelocity.magnitude * 0.015f), Time.deltaTime * 4f);
	}
	
	private float ClampAngle() {
		if(rotationX > 360f) {
			rotationX -= 360f;
		}
		else if(rotationX < 0f) {
			rotationX += 360f;
		}
		
		return rotationX;
	}
}