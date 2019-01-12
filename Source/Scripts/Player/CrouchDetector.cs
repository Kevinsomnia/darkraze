using UnityEngine;
using System.Collections;

public class CrouchDetector : MonoBehaviour {
    public bool canStandUp {
        get {
            return (currentObstruction == null);
        }
    }

    private Collider currentObstruction;

    void OnDisable() {
        currentObstruction = null;
    }

    void OnTriggerEnter(Collider col) {
        if(col.transform.name == transform.root.name) {
            return;
        }
        
        currentObstruction = col;
    }

    void OnTriggerExit(Collider col) {
        if(col.transform.name == transform.root.name) {
            return;
        }

        currentObstruction = null;
    }
}