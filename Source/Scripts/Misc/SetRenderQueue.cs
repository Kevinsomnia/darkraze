using UnityEngine;
using System.Collections;

public class SetRenderQueue : MonoBehaviour
{
    public int renderQueue = 3000;

    private void Awake()
    {
        Material[] materials = GetComponent<Renderer>().materials;
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].renderQueue = renderQueue;
        }
    }
}