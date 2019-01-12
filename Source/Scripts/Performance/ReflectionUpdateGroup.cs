using UnityEngine;
using System.Collections;

/// <summary>
/// Just for the use for updating reflections. It's an performance optimization, rather than looping to find all the renderers automatically.
/// </summary>
public class ReflectionUpdateGroup : MonoBehaviour
{
    public Renderer[] allRenderers;
}