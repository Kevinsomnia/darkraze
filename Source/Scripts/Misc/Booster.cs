using UnityEngine;
using System.Collections;

public class Booster : MonoBehaviour
{
    public Transform thrustDirection;
    public float thrustPower = 10f;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>())
        {
            other.GetComponent<Rigidbody>().AddForce(thrustDirection.forward * thrustPower * 1000f);
        }
        else
        {
            other.gameObject.SendMessage("Thrust", thrustDirection.forward * thrustPower, SendMessageOptions.DontRequireReceiver);
        }
    }
}