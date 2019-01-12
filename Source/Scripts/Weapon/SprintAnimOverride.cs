using UnityEngine;
using System.Collections;

public class SprintAnimOverride : MonoBehaviour {
	public Vector3 sprintRot = new Vector3(15, -45, 22);
	public Vector3 offset = new Vector3(0f, 0f, 0.08f);
	public Vector2 sprintBobAmount = Vector2.one;
    public Vector2 sprintRotFactor = Vector2.one;
    public float animationSpeed = 1f;
	public float offsetSmoothing = 5f;
	public bool rotateWeaponTransform = false;
}