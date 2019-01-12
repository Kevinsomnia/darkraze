using UnityEngine;
using System.Collections;

public class TeammateMarker : MonoBehaviour {
    public UISprite markerTexture;
    public UILabel userLabel;
    public LayerMask layersToCheck = -1;
    public LayerMask markerOcclude = 0;
    public float nameTagDistance = 25f;
    public float nameTagDisplayRadius = 0.6f;
    public float tagFlickerLength = 0.25f; //When teammates die, the length of the flicker effect.
    public float occludedMarkerFactor = 0.4f;

    [HideInInspector] public Transform targetObserver;

    private Transform tr;
    private bool aiming;
    private bool showNameTag;
    private bool showMarkerSprite;

    private Vector3 vpPos;
    private float fadeOutTag;
    private float distanceAlphaMod;
    private bool queueDestroy;
    private int curWidth;
    private int normalWidth;
    private int modernWideWidth; //16:9
    private int oldWideWidth; //16:10
    private int normalHeight;

    void Start() {
        markerTexture.alpha = 0f;
        userLabel.alpha = 0f;
        tr = transform;

        queueDestroy = false;
        normalWidth = DarkRef.NormalNGUIWidth;
        modernWideWidth = DarkRef.ModernWideNGUIWidth;
        oldWideWidth = DarkRef.OldWideNGUIWidth;
        normalHeight = DarkRef.NGUIHeight;
    }

    void FixedUpdate() {
        showNameTag = false;
        showMarkerSprite = false;
        vpPos = Vector3.forward * -1000f;
        
        if(GeneralVariables.mainPlayerCamera != null) {
            Vector3 point = targetObserver.position + (Vector3.up * 2.05f);
            vpPos = GeneralVariables.mainPlayerCamera.WorldToViewportPoint(point);
            showNameTag = !Physics.Linecast(GeneralVariables.mainPlayerCamera.transform.position, point, layersToCheck.value);
            showMarkerSprite = !Physics.Linecast(GeneralVariables.mainPlayerCamera.transform.position, point, markerOcclude.value);
        }
    }

    void Update() {
        if(targetObserver == null) {
            Destroy(gameObject);
            return;
        }

        if(!queueDestroy) {
            BaseStats bs = targetObserver.GetComponent<BaseStats>();
            if(bs != null && bs.curHealth <= 0) {
                StartCoroutine(TeammateFlickerEffect());
                return;
            }
            else if(targetObserver == null) {
                StartCoroutine(FadeOutEffect());
                return;
            }
        }

        userLabel.text = targetObserver.name;

        curWidth = normalWidth;
        if(DarkRef.isModernWidescreen) {
            curWidth = modernWideWidth;
        }
        else if(DarkRef.isOldWidescreen) {
            curWidth = oldWideWidth;
        }

        if(!queueDestroy) {
            bool showMarker = (vpPos.z > 0f && Mathf.Abs(vpPos.x - 0.5f) <= 0.6f && Mathf.Abs(vpPos.y - 0.5f) <= 0.6f);
            aiming = (GeneralVariables.playerRef != null && GeneralVariables.playerRef.ac.isAiming);

            fadeOutTag = 1f - Mathf.Clamp01((vpPos.z - nameTagDistance) * 0.5f);
            distanceAlphaMod = 1f - Mathf.Clamp01(((new Vector2(vpPos.x - 0.5f, vpPos.y - 0.5f) * 2f).magnitude - nameTagDisplayRadius) / (nameTagDisplayRadius * 0.25f));

            float finalMarkerAlpha = 0f;
            if(showMarker && !(aiming && !showMarkerSprite)) {
                finalMarkerAlpha = markerTexture.defaultAlpha * ((showNameTag) ? 1f : occludedMarkerFactor);
            }

            markerTexture.alpha = Mathf.MoveTowards(markerTexture.alpha, finalMarkerAlpha, Time.deltaTime * 8f);
            userLabel.alpha = Mathf.MoveTowards(userLabel.alpha, (showMarker && !aiming && showNameTag) ? userLabel.defaultAlpha * fadeOutTag * distanceAlphaMod : 0f, Time.deltaTime * 8f);
        }

        tr.localPosition = new Vector3((vpPos.x - 0.5f) * 2f * curWidth, (vpPos.y - 0.5f) * 2f * normalHeight, 0f);
        tr.localScale = Vector3.one * (1f - (Mathf.Clamp01(vpPos.z * 0.022f) * 0.4f));
    }

    private IEnumerator FadeOutEffect() {
        queueDestroy = true;

        while(markerTexture.alpha > 0f || userLabel.alpha > 0f) {
            markerTexture.alpha -= Time.deltaTime * 2f;
            userLabel.alpha -= Time.deltaTime * 2f;
            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator TeammateFlickerEffect() {
        queueDestroy = true;

        float time = 0f;
        float defMTAlpha = markerTexture.alpha;
        float defULAlpha = userLabel.alpha;
        while(time < tagFlickerLength) {
            time += Time.deltaTime;
            markerTexture.alpha = defMTAlpha * Mathf.PerlinNoise(25f + Random.value, time * 20f + Random.value);
            userLabel.alpha = defULAlpha * Mathf.PerlinNoise(time * 21f + Random.value, -69f + Random.value);
            yield return null;
        }

        Destroy(gameObject);
    }
}