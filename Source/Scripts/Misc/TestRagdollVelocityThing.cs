using UnityEngine;
using System.Collections;

public class TestRagdollVelocityThing : MonoBehaviour
{
    public float speed = 6f;

    private Vector3 moveDirection;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Input.GetKeyDown("y"))
        {
            GetComponent<BaseStats>().ApplyDamageMain(300, false);
        }

        moveDirection = new Vector3(0f, 0f, speed);
        moveDirection = transform.TransformDirection(moveDirection);
        controller.SimpleMove(moveDirection);
    }
}