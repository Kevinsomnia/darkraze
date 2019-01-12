//Rotating.cs created by DaBossTMR for Darkraze FPS Project (Unity3D)

using UnityEngine;
using System.Collections;

public class Rotating : MonoBehaviour {
	public Vector3 rotateSpeed = new Vector3(30, 0, 0);
	public Space rotateSpace = Space.Self;

	void Update() {
		transform.Rotate(rotateSpeed * Time.deltaTime, rotateSpace);
	}
}