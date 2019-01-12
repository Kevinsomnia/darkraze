using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour {
	public bool spawnManually = false;
	public GameObject[] spawnLocations;
	public GameObject[] objectsToSpawn;
	public int amountToSpawn = 0; // 0 = Infinity.
	public float spawnRadius = 10;
	public float spawnRate = 1.5f;
	public Vector3 spawnOffset = Vector3.up;
	
	private Vector3 spawnPos;
	private int spawnNum;
	private bool hasLimit;
	private float timer;
	
	void Update() {
		if(!spawnManually) {
			timer += Time.deltaTime;
			
            hasLimit = (amountToSpawn > 0);
			
			if(hasLimit) {
				if(spawnNum < amountToSpawn) {
					if(timer > spawnRate) {
						if(spawnLocations.Length > 0) {
							Vector3 selectedLocation = spawnLocations[Random.Range(0, spawnLocations.Length)].transform.position;
							Vector3 spawnRandom = new Vector3(selectedLocation.x + Random.Range(-spawnRadius, spawnRadius), 2000f, selectedLocation.z + Random.Range(-spawnRadius, spawnRadius));
							
                            RaycastHit hit;
							if(Physics.Raycast(spawnRandom, Vector3.down, out hit, 10000f)) {
								spawnPos = hit.point + spawnOffset;
							}
						}
						
						Spawn(spawnPos);
					}
				}
			}
			else {
				if(timer > spawnRate) {
					if(spawnLocations.Length > 0) {
						Vector3 selectedLocation = spawnLocations[Random.Range(0, spawnLocations.Length)].transform.position;
						Vector3 spawnRandom = new Vector3(selectedLocation.x + Random.Range(-spawnRadius, spawnRadius), 2000f, selectedLocation.z + Random.Range(-spawnRadius, spawnRadius));
						
                        RaycastHit hit;
						if(Physics.Raycast(spawnRandom, Vector3.down, out hit, 10000f)) {
							spawnPos = hit.point + spawnOffset;
						}
					}
						
					Spawn(spawnPos);
				}
			}
		}
	}
	
	public void Spawn(Vector3 pos) {
		Instantiate(objectsToSpawn[Random.Range(0, objectsToSpawn.Length)], pos, transform.rotation);
		spawnNum++;
		timer = 0;
	}
	
	public void SpawnSurvivalEnemy() {
		if(spawnLocations.Length > 0) {
			Vector3 selectedLocation = spawnLocations[Random.Range(0, spawnLocations.Length)].transform.position;
			Vector3 spawnRandom = new Vector3(selectedLocation.x + Random.Range(-spawnRadius, spawnRadius), 2000f, selectedLocation.z + Random.Range(-spawnRadius, spawnRadius));
			RaycastHit hit;
			
			if(Physics.Raycast(spawnRandom, Vector3.down, out hit, 10000f)) {
				Instantiate(objectsToSpawn[Random.Range(0, objectsToSpawn.Length)], hit.point + spawnOffset, Quaternion.identity);
			}
		}
	}
}