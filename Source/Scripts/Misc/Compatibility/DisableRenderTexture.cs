using UnityEngine;
using System.Collections;

public class DisableRenderTexture : MonoBehaviour {
	void Start() {
	    if(!SystemInfo.supportsRenderTextures) {
            gameObject.SetActive(false);
        }
	}
}