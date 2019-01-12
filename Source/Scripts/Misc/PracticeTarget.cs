using UnityEngine;
using System.Collections;

public class PracticeTarget : BaseStats {
    public Renderer targetRenderer;
    public string colorPropertyName = "_Color";
    public Light targetIllum;
    public float damageColorTime = 0.1f;
    public Color defaultColor = new Color(0f, 0.11f, 1f, 1f);
    public Color deadColor = new Color(1f, 0f, 0f, 1f);
    public Color inactiveColor = new Color(1f, 1f, 1f, 0.25f);
    public Color damageHitColor = new Color(1f, 0.3f, 0f, 1f);
    public float respawnTime = 5f; //0 to disable.
    public DamageText damageText;
    public Vector3 textOffset = Vector3.up;
    public Vector3 textVelocity = new Vector3(1f, 2f, 0f);

    private bool isDead = false;
    private float dmgTime = -100f;
    private float defaultIllum;

    void Awake() {
        if(targetIllum != null) {
            defaultIllum = targetIllum.intensity;
        }

        ResetTarget();
    }

    void Update() {
        if(maxHealth <= 0) {
            return;
        }

        if(Time.time - dmgTime >= damageColorTime) {
            if(targetRenderer != null) {
                targetRenderer.material.SetColor(colorPropertyName, Color.Lerp(targetRenderer.material.GetColor(colorPropertyName), (isDead) ? inactiveColor : Color.Lerp(deadColor, defaultColor, (float)curHealth / (float)maxHealth), Time.deltaTime * 10f));
            }

            if(targetIllum != null) {
                targetIllum.intensity = Mathf.Lerp(targetIllum.intensity, (isDead) ? 0f : defaultIllum, Time.deltaTime * 5f);
                targetIllum.enabled = targetIllum.intensity > 0f;
                targetIllum.color = targetRenderer.material.GetColor(colorPropertyName);
            }
        }
    }

    public override void ApplyDamageMain(int damage, bool showBlood) {
        ApplyDamage(damage);
    }

    private void ApplyDamage(int damage) {
        if(isDead || damage <= 0)
            return;
        
        if(damageText != null) {
            DamageText dt = Instantiate(damageText, transform.position + textOffset, damageText.transform.rotation);
            dt.DoDamage(damage, textVelocity + (new Vector3(textVelocity.x * Random.value, textVelocity.y * Random.value, Random.value * textVelocity.z) * 0.5f));
        }

        curHealth -= damage;
        if(targetRenderer != null) {
            targetRenderer.material.SetColor(colorPropertyName, damageHitColor);
        }

        if(targetIllum != null) {
            targetIllum.color = damageHitColor;
        }

        dmgTime = Time.time;

        if(curHealth <= 0) {
            TargetDestroyed();
        }
    }

    private void TargetDestroyed() {
        curHealth = 0;
        isDead = true;

        if(respawnTime > 0f) {
            Invoke("ResetTarget", respawnTime);
        }
    }

    public void ResetTarget() {
        curHealth = maxHealth;
        dmgTime = -damageColorTime;
        isDead = false;
    }
}