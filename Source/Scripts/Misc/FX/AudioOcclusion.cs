using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioOcclusion : MonoBehaviour
{
    public AudioLowPassFilter muffleLowPass;
    public Vector2 muffleBounds = new Vector2(40f, 250f);
    public Vector2 muffleFrequency = new Vector2(20000f, 2000f);

    private AudioSource source;
    private float distFromListener;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        distFromListener = (DarkRef.listener.transform.position - transform.position).magnitude;

        if (muffleLowPass != null)
        {
            muffleLowPass.cutoffFrequency = Mathf.Lerp(muffleFrequency.x, muffleFrequency.y, Mathf.Clamp01((distFromListener - muffleBounds.x) / (muffleBounds.y - muffleBounds.x)));
        }
    }
}