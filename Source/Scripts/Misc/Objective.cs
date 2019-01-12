using UnityEngine;
using System.Collections;

public class Objective : MonoBehaviour
{
    public string objectiveDescription = "Destroy";

    void Awake()
    {
        DarkRef.CreateObjectiveMarker(transform, objectiveDescription);
    }
}