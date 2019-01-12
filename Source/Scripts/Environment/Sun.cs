using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Sun : MonoBehaviour {
    public Skydome skydome;

    private float LATITUDE_RADIANS;
    private float STD_MERIDIAN;
    private Vector3 sunDirection = new Vector3();
    private Vector3 sunDirection2 = new Vector3();
    private float SolarAzimuth;
    private float solarAltitude;
    private Vector3 direction;
    private Vector3 color;
    private float theta;
    private float phi;

    void Update() {
        if(skydome == null) {
            return;
        }

        skydome.latitude = Mathf.Clamp(skydome.latitude, -90.0f, 90.0f);
        SetPosition(skydome.time + 1f);
    }

    private void SetPosition(float fTheta, float fPhi) {
        theta = fTheta;
        phi = fPhi;

        float fCosTheta = Mathf.Cos(theta);
        float fSinTheta = Mathf.Sin(theta);
        float fCosPhi = Mathf.Cos(phi);
        float fSinPhi = Mathf.Sin(phi);

        direction = new Vector3(fSinTheta * fCosPhi, fCosTheta, fSinTheta * fSinPhi);
        float phiSun = (Mathf.PI * 2f) - SolarAzimuth;

        sunDirection2 = calcDirection(theta, phiSun);
        direction = Vector3.Normalize(direction);
        transform.LookAt(sunDirection2);
        ComputeAttenuation();
    }

    private void SetPosition(float fTime) {
        LATITUDE_RADIANS = Mathf.Deg2Rad * skydome.latitude;
        STD_MERIDIAN = skydome.meridian * 15.0f;

        float t = fTime + 0.170f * Mathf.Sin((4.0f * Mathf.PI * (skydome.julianDate - 80.0f)) / 373.0f)
                - 0.129f * Mathf.Sin((2.0f * Mathf.PI * (skydome.julianDate - 8.0f)) / 355.0f)
                + (STD_MERIDIAN - skydome.longitude) / 15.0f;
        float fDelta = 0.4093f * Mathf.Sin((2.0f * Mathf.PI * (skydome.julianDate - 81.0f)) / 368.0f);

        float fSinLat = Mathf.Sin(LATITUDE_RADIANS);
        float fCosLat = Mathf.Cos(LATITUDE_RADIANS);
        float fSinDelta = Mathf.Sin(fDelta);
        float fCosDelta = Mathf.Cos(fDelta);
        float fSinT = Mathf.Sin((Mathf.PI * t) / 12.0f);
        float fCosT = Mathf.Cos((Mathf.PI * t) / 12.0f);

        solarAltitude = Mathf.Asin(fSinLat * fSinDelta - fCosLat * fCosDelta * fCosT);
        float fTheta = 0f;
        fTheta = Mathf.PI / 2.0f - solarAltitude;

        float opp = -fCosDelta * fSinT;
        float adj = -(fCosLat * fSinDelta + fSinLat * fCosDelta * fCosT);
        SolarAzimuth = Mathf.Atan2(opp, adj);

        float fPhi = Mathf.Atan((-fCosDelta * fSinT) / (fCosLat * fSinDelta - fSinLat * fCosDelta * fCosT));
        fPhi = -SolarAzimuth;
        SetPosition(fTheta, fPhi);
    }

    private Vector3 calcDirection(float thetaSun, float phiSun) {
        Vector3 dir = new Vector3();
        dir.x = Mathf.Cos(0.5f * Mathf.PI - thetaSun) * Mathf.Cos(phiSun);
        dir.y = Mathf.Sin(0.5f * Mathf.PI - thetaSun);
        dir.z = Mathf.Cos(0.5f * Mathf.PI - thetaSun) * Mathf.Sin(phiSun);
        return dir.normalized;
    }

    private void ComputeAttenuation() {
        float fBeta = 0.0460836f * skydome.turbidity - 0.0458602f;
        float fTauR, fTauA;
        float[] fTau = new float[3];
        float tmp = 93.885f - (theta / Mathf.PI * 180.0f);

        float m = 1.0f / (Mathf.Cos(theta) + 0.15f * tmp);  // Relative Optical Mass
        if(m < 0) {
            m = 20;
        }

        float[] fLambda = new float[3] {0.65f, 0.57f, 0.475f}; // red, green, blue (in um.)
        for(int i = 0; i < 3; i++) {
            fTauR = Mathf.Exp(-m * 0.008735f * Mathf.Pow(fLambda[i], -4.08f));
            const float fAlpha = 1.3f;
            fTauA = Mathf.Exp(-m * fBeta * Mathf.Pow(fLambda[i], -fAlpha));  // lambda should be in um
            fTau[i] = fTauR * fTauA;
        }

        color = new Vector3(fTau[0], fTau[1], fTau[2]);

        if(skydome.autoComputeLightColor) {
            GetComponent<Light>().color = new Color(fTau[0], fTau[1], fTau[2]);
        }
    }
}