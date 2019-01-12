using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class LaserPointer : MonoBehaviour {
    public float maximumDistance = 10000f;
    public Transform laserDecal;
    public float laserWidth = 0.1f;
    public LayerMask layersToAffect = -1;

    private RaycastHit hitInfo;
    private Transform tr;
    private LineRenderer lRenderer;

    void Awake() {
        tr = transform;
        lRenderer = GetComponent<LineRenderer>();
        lRenderer.useWorldSpace = true;
    }

    void Update() {
        lRenderer.SetWidth(laserWidth, laserWidth);
        lRenderer.SetPosition(0, tr.position);

        if(Physics.Raycast(tr.position, tr.forward, out hitInfo, maximumDistance, layersToAffect.value)) {
            lRenderer.SetPosition(1, hitInfo.point);
            lRenderer.material.SetTextureScale("_MainTex", new Vector2(hitInfo.distance * 0.8f, 1f));

            if(laserDecal != null) {
                laserDecal.GetComponent<Renderer>().enabled = true;
                laserDecal.position = hitInfo.point + (hitInfo.normal * 0.002f);
                laserDecal.rotation = Quaternion.LookRotation(hitInfo.normal);
            }
        }
        else {
            lRenderer.SetPosition(1, tr.forward * maximumDistance);
            lRenderer.material.SetTextureScale("_MainTex", new Vector2(maximumDistance * 0.8f, 1f));

            if(laserDecal != null) {
                laserDecal.GetComponent<Renderer>().enabled = false;
            }
        }
    }
}