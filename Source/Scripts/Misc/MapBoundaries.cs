using UnityEngine;
using System.Collections;

public class MapBoundaries : MonoBehaviour
{
    public MeshRenderer[] mapMeshes = new MeshRenderer[0];
    public bool calculateChildren = true;
    public float boundaryThreshold = 10f;

    [HideInInspector] public Bounds mapBounds;

    private bool calculated = false;

    void Awake()
    {
        CalculateBoundaries();
    }

    public void CalculateBoundaries()
    {
        if (calculateChildren)
        {
            mapMeshes = GetComponentsInChildren<MeshRenderer>();
        }

        mapBounds = new Bounds();

        foreach (Renderer rend in mapMeshes)
        {
            mapBounds.Encapsulate(rend.bounds.min);
            mapBounds.Encapsulate(rend.bounds.max);
        }

        mapBounds.Expand(boundaryThreshold);
    }
}