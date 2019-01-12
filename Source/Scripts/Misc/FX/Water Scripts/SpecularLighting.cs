using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(WaterBase))]
public class SpecularLighting : MonoBehaviour {		
	public Transform specularLight;
	private WaterBase waterBase = null;
	
	public void Start() {
		waterBase = GetComponent<WaterBase>();		
	}	

	public void Update() {
		if(specularLight && waterBase.sharedMaterial) {
			waterBase.sharedMaterial.SetVector("_WorldLightDir", specularLight.transform.forward);
        }
	}
}