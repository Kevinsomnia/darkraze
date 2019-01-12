using UnityEngine;
using System.Collections;

//Used to solve the problem of play on awake not playing sometimes.
[RequireComponent(typeof(AudioSource))]
public class PlayOnAwake : MonoBehaviour
{
    void Awake()
    {
        GetComponent<AudioSource>().Play();
    }
}