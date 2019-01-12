using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class UVScrollAnimation : MonoBehaviour
{
    public Vector2 scrollSpeed = new Vector2(0.5f, 0.5f);
    public bool initRandomOffset = false;

    private Material mat;

    void Awake()
    {
        mat = GetComponent<Renderer>().material;

        if (initRandomOffset)
        {
            mat.mainTextureOffset += new Vector2(Random.value, Random.value);
        }
    }

    void Update()
    {
        mat.mainTextureOffset += scrollSpeed * Time.deltaTime;
    }
}