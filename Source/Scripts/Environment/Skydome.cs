using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Skydome : MonoBehaviour {
    public Light sun;
    public bool autoComputeLightColor = true;

    public float julianDate = 150;
    public float longitude = 0.0f;
    public float latitude = 0.0f;
    public float meridian = 0.0f;
    public float time = 8.0f;
    public float turbidity = 2.0f;

    public float rayFactor = 1000.0f;
    public float mieFactor = 0.7f;
    public float directionalityFactor = 0.6f;
    public float sunColorIntensity = 1.0f;

    private Vector4 vBetaRayleigh = new Vector4();
    private Vector4 vBetaMie = new Vector4();
    private Vector3 m_vBetaRayTheta = new Vector3();
    private Vector3 m_vBetaMieTheta = new Vector3();
    private float domeRadius = 2500f;
    private float LATITUDE_RADIANS;
    private float LONGITUDE_RADIANS;
    private float STD_MERIDIAN;


    void Start() {
        LATITUDE_RADIANS = Mathf.Deg2Rad * latitude;
        LONGITUDE_RADIANS = Mathf.Deg2Rad * longitude;
        STD_MERIDIAN = meridian * 15.0f;
    }

    void Update() {
        CalculateAtmosphere();

        Material sharedMat = GetComponent<Renderer>().sharedMaterial;
        sharedMat.SetVector("vBetaRayleigh", vBetaRayleigh);
        sharedMat.SetVector("BetaRayTheta", m_vBetaRayTheta);
        sharedMat.SetVector("vBetaMie", vBetaMie);
        sharedMat.SetVector("BetaMieTheta", m_vBetaMieTheta);
        sharedMat.SetVector("g_vEyePt", Vector3.zero);
        sharedMat.SetVector("LightDir", sun.transform.forward);
        sharedMat.SetVector("g_vSunColor", sun.color);
        sharedMat.SetFloat("DirectionalityFactor", directionalityFactor);
        sharedMat.SetFloat("SunColorIntensity", sunColorIntensity);
    }

    void CalculateAtmosphere() {
        CalculateRay();
        CalculateMieCoeff();
        InitSunThetaPhi();
    }

    void CalculateRay() {
        float fRayleighFactor = rayFactor * 1.384826e-31f;

        m_vBetaRayTheta.x = fRayleighFactor / 3.570126e-25f;
        m_vBetaRayTheta.y = fRayleighFactor / 2.1112e-25f;
        m_vBetaRayTheta.z = fRayleighFactor / 1.018133e-25f;

        vBetaRayleigh.x = 8.0f * fRayleighFactor / 5.355189e-25f;
        vBetaRayleigh.y = 8.0f * fRayleighFactor / 3.1668e-25f;
        vBetaRayleigh.z = 8.0f * fRayleighFactor / 1.527199e-25f;
    }

    void CalculateMieCoeff() {
        float c = (0.6544f * turbidity - 0.6510f) * 1e-16f;	//Concentration factor

        float fMieFactor = mieFactor * c * 17.13363f;

        float pow1 = Mathf.Pow(650e-9f, 2.0f);
        float pow2 = Mathf.Pow(570e-9f, 2.0f);
        float pow3 = Mathf.Pow(475e-9f, 2.0f);

        m_vBetaMieTheta.x = fMieFactor / (2.0f * pow1);
        m_vBetaMieTheta.y = fMieFactor / (2.0f * pow2);
        m_vBetaMieTheta.z = fMieFactor / (2.0f * pow3);

        vBetaMie.x = 0.685f * fMieFactor / pow1;
        vBetaMie.y = 0.682f * fMieFactor / pow2;
        vBetaMie.z = 0.67f * fMieFactor / pow3;
    }

    void ComputeAttenuation(float m_fTheta) {
        float fBeta = 0.0460836f * turbidity - 0.0458602f;
        float[] fTau = new float[3];
        float tmp = 93.885f - (m_fTheta / Mathf.PI * 180.0f);

        float m = 1.0f / (Mathf.Cos(m_fTheta) + 0.15f * tmp);
        if(m < 0f) {
            m = 20f;
        }

        float[] fLambda = new float[3] { 0.65f, 0.57f, 0.475f };
        for(int i = 0; i < 3; i++) {
            float fTauR = Mathf.Exp(-m * 0.008735f * Mathf.Pow(fLambda[i], -4.08f));
            float fTauA = Mathf.Exp(-m * fBeta * Mathf.Pow(fLambda[i], -1.3f));
            fTau[i] = fTauR * fTauA;
        }

        if(autoComputeLightColor) {
            sun.color = new Color(fTau[0], fTau[1], fTau[2]);
        }
    }

    void InitSunThetaPhi() {
        float solarTime = time + 1 + (0.170f * Mathf.Sin(4f * Mathf.PI * (julianDate - 80f) / 373f) - 0.129f * Mathf.Sin(2f * Mathf.PI * (julianDate - 8f) / 355f)) + (STD_MERIDIAN - LONGITUDE_RADIANS) / 15.0f;
        float solarDeclination = (0.4093f * Mathf.Sin(2f * Mathf.PI * (julianDate - 81f) / 368f));
        float solarAltitude = Mathf.Asin(Mathf.Sin(LATITUDE_RADIANS) * Mathf.Sin(solarDeclination) -
        Mathf.Cos(LATITUDE_RADIANS) * Mathf.Cos(solarDeclination) * Mathf.Cos(Mathf.PI * solarTime / 12f));

        float opp = -Mathf.Cos(solarDeclination) * Mathf.Sin(Mathf.PI * solarTime / 12f);
        float adj = -(Mathf.Cos(LATITUDE_RADIANS) * Mathf.Sin(solarDeclination) +
            Mathf.Sin(LATITUDE_RADIANS) * Mathf.Cos(solarDeclination) * Mathf.Cos(Mathf.PI * solarTime / 12f));

        float phiS = -Mathf.Atan2(opp, adj);
        float thetaS = Mathf.PI / 2.0f - solarAltitude;
        Vector3 sunDirection = new Vector3(domeRadius, phiS, solarAltitude);
        Vector3 sunDirection2 = calcDirection(thetaS, phiS);
        sun.transform.position = SphericalToCartesian(sunDirection);
        sun.transform.LookAt(sunDirection2);
        ComputeAttenuation(thetaS);
    }

    private Vector3 calcDirection(float thetaSun, float phiSun) {
        Vector3 dir = new Vector3();
        dir.x = Mathf.Cos(0.5f * Mathf.PI - thetaSun) * Mathf.Cos(phiSun);
        dir.y = Mathf.Sin(0.5f * Mathf.PI - thetaSun);
        dir.z = Mathf.Cos(0.5f * Mathf.PI - thetaSun) * Mathf.Sin(phiSun);
        return dir.normalized;
    }

    private static Vector3 SphericalToCartesian(Vector3 sphereCoords) {
        Vector3 store;
        store.y = sphereCoords.x * Mathf.Sin(sphereCoords.z);
        float a = sphereCoords.x * Mathf.Cos(sphereCoords.z);
        store.x = a * Mathf.Cos(sphereCoords.y);
        store.z = a * Mathf.Sin(sphereCoords.y);
        return store;
    }

    private static Vector3 CartesianToSpherical(Vector3 cartCoords) {
        Vector3 store;
        if(cartCoords.x == 0)
            cartCoords.x = Mathf.Epsilon;
        store.x = Mathf.Sqrt((cartCoords.x * cartCoords.x) + (cartCoords.y * cartCoords.y) + (cartCoords.z * cartCoords.z));
        store.y = Mathf.Atan(cartCoords.z / cartCoords.x);
        if(cartCoords.x < 0)
            store.y += Mathf.PI;
        store.z = Mathf.Asin(cartCoords.y / store.x);
        return store;
    }
}