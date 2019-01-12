using UnityEngine;
using System.Collections;

public class SpectatorFreeLook : MonoBehaviour
{
    public float rotationSpeed = 3f;
    public float normalSpeed = 10;
    public bool ignoreTimescale = false;

    private Transform tr;
    private Vector3 oldPos;
    private Vector3 newPos;
    private Vector3 moveDir;
    private Vector3 worldDir;
    private float rotX;
    private float rotY;

    void Start()
    {
        tr = transform;
        oldPos = tr.position;
        newPos = tr.position;
    }

    void Update()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")) * normalSpeed;
        worldDir = new Vector3(0f, GetAxisDir(KeyCode.LeftControl, KeyCode.Space), 0f) * normalSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveDir *= 1.6f;
            worldDir *= 1.6f;
        }

        float delta = ((ignoreTimescale) ? Time.unscaledDeltaTime : Time.deltaTime);

        moveDir = tr.TransformDirection(moveDir);

        oldPos = tr.position;
        newPos += (moveDir + worldDir) * delta;
        Vector3 dir = (newPos - oldPos);

        RaycastHit hit;
        if (Physics.Raycast(oldPos, dir, out hit, dir.magnitude))
        {
            newPos = hit.point + (hit.normal * 0.05f);
        }

        tr.position = newPos;

        float inputX = cInput.GetAxisRaw("Horizontal Look");
        float inputY = cInput.GetAxisRaw("Vertical Look");
        rotX += inputX * rotationSpeed;
        rotY += inputY * rotationSpeed;
        rotY = Mathf.Clamp(rotY, -90f, 90f);

        tr.rotation = Quaternion.Euler(-rotY, rotX, 0f);
    }

    private float GetAxisDir(KeyCode neg, KeyCode pos)
    {
        if (Input.GetKey(pos))
        {
            return 1f;
        }
        else if (Input.GetKey(neg))
        {
            return -1f;
        }

        return 0f;
    }
}