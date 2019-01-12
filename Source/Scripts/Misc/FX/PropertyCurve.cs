using UnityEngine;
using System.Collections;

public class PropertyCurve : MonoBehaviour {
    public enum PropertyType {Color, Float};
    public PropertyType propertyType = PropertyType.Color;
    public string propertyName = "_Color";
    public Vector2 fadeSpeed = Vector2.one;
    public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
    public Vector2 floatValRange = new Vector2(0f, 1f);
    
    private float time = 0f;
    private float defaultAlpha = -1f;
    private float curSpeed;

    void OnEnable() {
        time = 0f;
        curSpeed = Random.Range(fadeSpeed.x, fadeSpeed.y);

        if(propertyType == PropertyType.Color) {
            GetComponent<Renderer>().enabled = true;
            Color matCol = GetComponent<Renderer>().material.GetColor(propertyName);

            if(defaultAlpha < 0f) {
                matCol.a *= Mathf.Clamp01(fadeCurve.Evaluate(0f));
                defaultAlpha = matCol.a;
            }
            else {
                matCol.a *= Mathf.Clamp01(fadeCurve.Evaluate(0f)) * defaultAlpha;
            }

            GetComponent<Renderer>().material.SetColor(propertyName, matCol);
        }
        else if(propertyType == PropertyType.Float) {
        }
    }

    void Update() {
        time += Time.deltaTime * curSpeed;

        if(propertyType == PropertyType.Color) {
            Color matCol = GetComponent<Renderer>().material.GetColor(propertyName);
            matCol.a = Mathf.Clamp01(fadeCurve.Evaluate(time)) * defaultAlpha;
            GetComponent<Renderer>().enabled = (matCol.a > 0f);
            GetComponent<Renderer>().material.SetColor(propertyName, matCol);
        }
        else if(propertyType == PropertyType.Float) {
            GetComponent<Renderer>().material.SetFloat(propertyName, Mathf.Lerp(floatValRange.x, floatValRange.y, fadeCurve.Evaluate(time)));
        }
    }
}