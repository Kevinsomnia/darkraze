using UnityEngine;
using System.Collections;

public class ScopeSway : MonoBehaviour {
    public Transform scopeEffect;
    public float swayFactor = 1f;
    public float swayLimit = 0.5f;

    private PlayerReference pr;
    private PlayerReference pRef {
        get {
            if(pr == null) {
                pr = GeneralVariables.playerRef;
            }

            return pr;
        }
    }

    private Vector3 defaultPos;
    private Vector3 cameraOffset;

	void Start() {
	    defaultPos = scopeEffect.localPosition;
	}
	
	void Update() {
        if(scopeEffect == null) {
            return;
        }

        cameraOffset = ((pRef != null && pRef.wm.currentGC != null) ? ((pRef.dm.defaultPos - pRef.wm.currentGC.aimPos) - pRef.wm.transform.localPosition) : Vector3.zero);
        scopeEffect.localPosition = defaultPos + (new Vector3(Mathf.Clamp(-cameraOffset.x, -swayLimit * 0.005f, swayLimit * 0.005f), Mathf.Clamp(cameraOffset.y, -swayLimit * 0.005f, swayLimit * 0.005f), 0f) * 10f * swayFactor);
	}
}
