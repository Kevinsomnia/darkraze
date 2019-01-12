using UnityEngine;
using System.Collections;

public class RainParticleSystem : MonoBehaviour
{

    void Start()
    {
        RainEffect rain = (RainEffect)FindObjectOfType(typeof(RainEffect));
        if (rain != null)
        {
            rain.RainEnabled(true);
        }
    }
}