using UnityEngine;
using System.Collections;

public class MainMenuLook : MonoBehaviour
{
    public Transform target;
    public float multiplierX = 4;
    public float multiplierY = 3;
    public float smoothing = 1;

    private Transform tr;
    private Vector3 defaultPos;

    void Start()
    {
        tr = transform;
        defaultPos = tr.localPosition;
    }

    void Update()
    {
        float inputX = Mathf.Clamp(((Input.mousePosition.x - (Screen.width * 0.5f)) / Screen.width) * multiplierX, -multiplierX * 0.5f, multiplierX * 0.5f);
        float inputY = Mathf.Clamp(((Input.mousePosition.y - (Screen.height * 0.5f)) / Screen.height) * multiplierY, -multiplierY * 0.5f, multiplierY * 0.5f);

        tr.localPosition = Vector3.Lerp(tr.localPosition, defaultPos + new Vector3(inputX, inputY, 0f), Time.deltaTime * smoothing);
        tr.rotation = Quaternion.LookRotation(target.position - tr.position);
    }
}
