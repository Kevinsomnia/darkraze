using UnityEngine;
using System.Collections;

public class SpectatorFollow : MonoBehaviour {
    public float rotationSpeed = 5f;
    public float positionDamp = 6f;
    public float rotationDamp = 16f;
    public float minDistance = 2.5f;
    public float maxDistance = 5f;
    public float zoomSpeed = 3f;
    public bool followRotation = false;
    public bool rotateUseRMB = false;
    public LayerMask layersToCheck = -1;

    private Transform _target = null;
    public Transform target {
        get {
            return _target;
        }
        set {
            _target = value;

            if(_target != value && value != null) {
                targetPos = (startingPosition != Vector3.zero) ? startingPosition : (_target.position + offset);
            }

            if(_target != null) {
                startTime = Time.time;
            }
        }
    }

    private Vector3 startPos;
    public Vector3 startingPosition {
        get {
            return startPos;
        }
        set {
            startPos = value;
            targetPos = startPos;
        }
    }

    [HideInInspector] public Vector3 offset;
    [HideInInspector] public bool canOrbit = true;
    [HideInInspector] public float startTime;

    private Transform tr;
    private Vector3 targetPos;
    private Vector3 dirPos;
    private float rotX;
    private float rotY;
    private float smoothedXRot;
    private float smoothedYRot;

    private float zoomDistance;
    private float oldDist;
    private float curDist;
    private float targetDist;

    private float followRotFactor;
    private float quickDamp;
    private float oldTargetRot;
    private float curTargetRot;

    void Start() {
        tr = transform;
        rotX = tr.rotation.eulerAngles.y;
        rotY = tr.rotation.eulerAngles.x;
        smoothedXRot = rotX;
        smoothedYRot = rotY;
        zoomDistance = maxDistance;
        canOrbit = true;
    }

    void Update() {
        if(target == null) {
            return;
        }

        bool holdingRMB = true;
        if(rotateUseRMB) {
            holdingRMB = Input.GetMouseButton(1);
        }

        if(holdingRMB && canOrbit) {
            rotX += cInput.GetAxisRaw("Horizontal Look") * rotationSpeed;
            rotY += -cInput.GetAxisRaw("Vertical Look") * rotationSpeed;
            followRotFactor = Mathf.Lerp(followRotFactor, 0.5f, Time.deltaTime * 2f);
        }
        else {
            followRotFactor = Mathf.Lerp(followRotFactor, 1f, Time.deltaTime * 2f);
        }

        if(followRotation) {
            curTargetRot = target.rotation.eulerAngles.y;
            rotX += (curTargetRot - oldTargetRot) * followRotFactor;
            oldTargetRot = target.rotation.eulerAngles.y;
        }

        rotY = Mathf.Clamp(rotY, -30f, 80f);

        smoothedXRot = Mathf.LerpAngle(smoothedXRot, rotX, Time.deltaTime * rotationDamp);
        smoothedYRot = Mathf.Lerp(smoothedYRot, rotY, Time.deltaTime * rotationDamp);

        zoomDistance += -Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * (maxDistance - minDistance);

        zoomDistance = Mathf.Clamp(zoomDistance, minDistance, maxDistance);
        quickDamp = 1f + (Mathf.Clamp(Time.time - startTime, 0f, 2f) * 0.5f);
        targetPos = Vector3.Lerp(targetPos, target.position + offset, Time.deltaTime * positionDamp * quickDamp);

        RaycastHit[] checkHit = Physics.RaycastAll(target.position + offset, (tr.position - (target.position + offset)).normalized, zoomDistance, layersToCheck.value);
        if(checkHit.Length > 0) {
            float closestDist = zoomDistance;
            for(int i = 0; i < checkHit.Length; i++) {
                if(checkHit[i].collider.transform.root == target.root) {
                    continue;
                }

                if((checkHit[i].distance - 0.1f) < closestDist) {
                    closestDist = checkHit[i].distance - 0.1f;
                }
            }

            targetDist = closestDist;
        }
        else {
            targetDist = zoomDistance;
        }

        curDist = Mathf.Lerp(curDist, targetDist, (targetDist < oldDist && Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) < 0.02f) ? (1f) : (Time.deltaTime * 5f));
        oldDist = targetDist;

        tr.rotation = Quaternion.Euler(smoothedYRot, smoothedXRot, 0f);
        tr.position = targetPos + (-tr.forward * curDist);
    }
}