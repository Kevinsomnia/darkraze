using UnityEngine;
using System.Collections;

public class BloodSplatter : MonoBehaviour {
    public DecalObject splatDecal;
    public float splatOffset = 0.01f;
    public Vector2 splatCount = Vector2.one;
    public Vector2 splatDistance = Vector2.one * 1.5f;
    public LayerMask splatLayers = -1;

    private RaycastHit hit;
    private Transform tr;

	void Start() {
        tr = transform;
	    BloodSplat();
	}

    public void BloodSplat() {
        int randCount = DarkRef.RandomRange((int)splatCount.x, (int)splatCount.y);
        for(int i = 0; i < randCount; i++) {
            if(Physics.Raycast(tr.position, Random.onUnitSphere, out hit, Random.Range(splatDistance.x, splatDistance.y), splatLayers.value)) {
                DecalObject splat = (DecalObject)Instantiate(splatDecal, hit.point, Quaternion.LookRotation(-hit.normal));
                splat.transform.parent = hit.transform;
                splat.transform.Rotate(Vector3.forward * Random.value * 360f, Space.Self);

                splat.targetObject = hit.collider.gameObject;
                splat.pushOffset = splatOffset;
                splat.layersToAffect = splatLayers;
            }
        }
    }
}