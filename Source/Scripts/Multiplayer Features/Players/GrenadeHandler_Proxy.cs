using UnityEngine;
using System.Collections;

public class GrenadeHandler_Proxy : MonoBehaviour {
    public Rigidbody grenadePrefab;
    public MeshRenderer displayMesh;
    public Collider[] ignoreColliders;
    public float throwStr = 18f;
    public float tossStr = 11f;

    [HideInInspector] public WeaponHandler_Proxy whp;

    private Rigidbody currentExplosive;

    public void DoPullPin(AudioClip clip, int gID) {
        GetComponent<AudioSource>().PlayOneShot(clip);
        displayMesh.enabled = false;
        currentExplosive = (Rigidbody)Instantiate(grenadePrefab, displayMesh.transform.position, displayMesh.transform.rotation);
        currentExplosive.transform.parent = transform;
        currentExplosive.isKinematic = true;

        if(currentExplosive.GetComponent<Collider>() != null) {
            currentExplosive.GetComponent<Collider>().enabled = false;
        }

		GrenadeScript greS = currentExplosive.GetComponent<GrenadeScript>();
        PlasticExplosive pExpl = currentExplosive.GetComponent<PlasticExplosive>();

        if(greS != null) {
            greS.PulledPin();
            greS.onlyVisual = true;
            greS.myID = gID;
        }
        else if(pExpl != null) {
            whp.detonationList.Add(pExpl);
            pExpl.onlyVisual = true;
            pExpl.myID = gID;
        }
    }

    public void DoThrow(AudioClip throwSound, Vector3 velocity, Vector3 position) {
        if(currentExplosive != null) {
			GetComponent<AudioSource>().PlayOneShot(throwSound);

            currentExplosive.transform.parent = null;
            currentExplosive.transform.position = displayMesh.transform.position;
            currentExplosive.isKinematic = false;

            if(currentExplosive.GetComponent<Collider>() != null) {
                currentExplosive.GetComponent<Collider>().enabled = true;

                foreach(Collider iC in ignoreColliders) {
                    Physics.IgnoreCollision(currentExplosive.GetComponent<Collider>(), iC);
                }
            }

            currentExplosive.transform.position = position;
            currentExplosive.velocity = velocity;
            currentExplosive.angularVelocity = new Vector3(1f, 0.69f, 0.86f) * 4.5f;
            currentExplosive = null;
		}
    }
}