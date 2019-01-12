using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class AimLensEffect : MonoBehaviour {
	public int aimAlpha = 16;
	public int notAimAlpha = 64;

	private Material mat;

	private AimController ac;
	private AimController aControl {
		get {
            if(ac == null && transform.root.GetComponent<PlayerReference>() != null) {
                ac = transform.root.GetComponent<PlayerReference>().ac;
			}
			
			return ac;
		}
	}

	private Color col1;
	private Color col2;
    private bool initialized = false;

	void Start() {
        if(aControl == null) {
            return;
        }

		mat = GetComponent<Renderer>().material;

		Color col = mat.color;
		col.a = Mathf.Clamp01((notAimAlpha * 1f) / 255f);
		col1 = col;
		col.a = Mathf.Clamp01((aimAlpha * 1f) / 255f);
		col2 = col;

		mat.color = col2;
        initialized = true;
	}
	
	void Update() {
        if(aControl == null) {
            return;
        }

        if(!initialized) {
            Start();
        }

		mat.color = Color.Lerp(col1, col2, aControl.aimTransition);
	}
}