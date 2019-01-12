using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class WaterTile : MonoBehaviour {
	public PlanarReflection reflection;
	public WaterBase waterBase;
	
	public void Start() {
		AcquireComponents();
	}
	
	private void AcquireComponents() {
        Transform parent = transform.parent;
        if(parent == null) {
            Debug.Log("No parent specified to this target! Disabling...", this);
            this.enabled = false;
            return;
        }

		reflection = parent.GetComponent<PlanarReflection>();
		waterBase = parent.GetComponent<WaterBase>();
        GetComponent<Renderer>().castShadows = false;
	}
	
	public void OnWillRenderObject() {
		if(reflection) {
			reflection.WaterTileBeingRendered(transform, Camera.current);
        }
		if(waterBase) {
			waterBase.WaterTileBeingRendered(transform, Camera.current);	
        }
	}
}
