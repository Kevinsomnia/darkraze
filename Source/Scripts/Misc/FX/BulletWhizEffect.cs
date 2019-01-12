using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class BulletWhizEffect : MonoBehaviour
{
    public Vector2 whizPitch = new Vector2(1f, 1.5f);
    public Vector2 whizVolume = new Vector2(0.4f, 0.5f);
    public Vector2 whizRange = new Vector2(35f, 40f);
    public float fadeInSpeed = 15f;
    public float fadeOutSpeed = 10f;

    private AudioSource source;
    private float targetVolume;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.pitch = Random.Range(whizPitch.x, whizPitch.y);
        source.volume = 0f;
        targetVolume = Random.Range(whizVolume.x, whizVolume.y);
        source.maxDistance = Random.Range(whizRange.x, whizRange.y);
    }

    void Update()
    {
        if (transform.parent == null)
        {
            source.volume = Mathf.Lerp(source.volume, 0f, Time.deltaTime * fadeOutSpeed);

            if (source.volume <= 0.001f)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            source.volume = Mathf.Lerp(source.volume, targetVolume, Time.deltaTime * fadeInSpeed);
        }
    }
}