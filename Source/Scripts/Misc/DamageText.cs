using UnityEngine;
using System.Collections;

public class DamageText : MonoBehaviour {
    public bool moveLocally = true;
    public float lifetime = 2f;
    public Vector3 gravityFactor = Vector3.up;
    public float damageScaleFactor = 0.01f;
    public Color damageColor = Color.red;
    public float fadeOutSpeed = 4f;
    public float fadeInSpeed = 5f;

    private Vector3 gVector;
    private Vector3 velo;
    private Vector3 defScale;
    private Transform tr;
    private TextMesh tm;
    private float timer;
    private bool isDestroying;

	void Awake() {
        tr = transform;
	    tm = GetComponent<TextMesh>();
        defScale = tr.localScale;

        StartCoroutine(StartUp());
	}
	
	void Update() {
        if(moveLocally) {
            tr.localPosition += (gVector.Multiply(Physics.gravity) + velo) * Time.deltaTime;
        }
        else {
            tr.position += (gVector.Multiply(Physics.gravity) + velo) * Time.deltaTime;
        }

        gVector += gravityFactor * Time.deltaTime;

        if(timer >= lifetime && !isDestroying) {
            StartCoroutine(Destruct());
        }
        else {
            timer += Time.deltaTime;
        }
	}

    public void DoDamage(int dmg, Vector3 velocity) {
        tm.text = dmg.ToString();
        tm.color = damageColor;
        defScale += Vector3.one * Mathf.Clamp01(dmg * damageScaleFactor * 0.1f);
        velo = velocity;
    }

    private IEnumerator StartUp() {
        tr.localScale = Vector3.zero;

        float fac = 0f;
        while(fac < 1f) {
            tr.localScale = Vector3.Lerp(Vector3.zero, defScale, fac);
            fac += Time.deltaTime * fadeInSpeed;
            yield return null;
        }
    }

    private IEnumerator Destruct() {
        isDestroying = true;

        float alpha = tm.color.a;
        while(alpha > 0f) {
            tm.color = DarkRef.SetAlpha(tm.color, alpha);
            tr.localScale = Vector3.Lerp(Vector3.zero, defScale, 0.6f + (alpha * 0.4f));
            alpha -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }

        Destroy(gameObject);
    }
}