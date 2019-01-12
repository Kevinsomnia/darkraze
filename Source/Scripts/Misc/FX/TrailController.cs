using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrailController : MonoBehaviour
{
    [System.Serializable]
    public class EmissionSettings
    {
        public float emissionDistance = 0.5f;
        public float emissionRadius = 0.1f;
        public Vector3[] emitVelocity = new Vector3[2] { Vector3.zero, Vector3.zero };
        public Vector2 emitSize = Vector2.zero;
        public Vector2 emitLifetime = Vector2.zero;
        public Color[] emitColors = new Color[2] { Color.white, Color.white };
        public bool interpolateEmission = true; //Particle spawn consistency, but may lag a bit more in lower FPS.
    }

    [System.Serializable]
    public class EmitterClass
    {
        public Transform emitterTransform;
        public Vector3 lastEmissionPos;
        public EmissionSettings settings;
    }

    private bool initialized;
    private List<EmitterClass> emitters;
    private ParticleSystem thisSystem;

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        thisSystem = GetComponent<ParticleSystem>();
        emitters = new List<EmitterClass>();
        initialized = true;
    }

    void Update()
    {
        for (int i = 0; i < emitters.Count; i++)
        {
            EmitterClass emit = emitters[i];
            EmissionSettings settings = emit.settings;
            float travDist = (emit.lastEmissionPos - emit.emitterTransform.position).magnitude;
            int innerLoop = Mathf.FloorToInt(travDist);

            if (settings.interpolateEmission && innerLoop > 0)
            {
                for (int j = 0; j < innerLoop; j++)
                {
                    Vector3 randomVelo = DarkRef.RandomVector3(settings.emitVelocity[0], settings.emitVelocity[1]);
                    float randomSize = Random.Range(settings.emitSize.x, settings.emitSize.y);
                    float randomLife = Random.Range(settings.emitLifetime.x, settings.emitLifetime.y);
                    Color randomColor = Color.Lerp(settings.emitColors[0], settings.emitColors[1], Random.value);
                    thisSystem.Emit(Vector3.Lerp(emit.lastEmissionPos, emit.emitterTransform.position, (1f / innerLoop) * j) + (Random.insideUnitSphere * settings.emissionRadius), randomVelo, randomSize, randomLife, randomColor);
                }
            }
            else if (travDist >= emit.settings.emissionDistance)
            {
                Vector3 randomVelo = DarkRef.RandomVector3(settings.emitVelocity[0], settings.emitVelocity[1]);
                float randomSize = Random.Range(settings.emitSize.x, settings.emitSize.y);
                float randomLife = Random.Range(settings.emitLifetime.x, settings.emitLifetime.y);
                Color randomColor = Color.Lerp(settings.emitColors[0], settings.emitColors[1], Random.value);
                thisSystem.Emit(emit.emitterTransform.position + (Random.insideUnitSphere * settings.emissionRadius), randomVelo, randomSize, randomLife, randomColor);
            }

            emit.lastEmissionPos = emit.emitterTransform.position;
        }
    }

    public void AddToEmitters(EmitterClass newClass)
    {
        Initialize();
        if (emitters.Contains(newClass))
        {
            return;
        }

        newClass.lastEmissionPos = newClass.emitterTransform.position;
        emitters.Add(newClass);
    }

    public void RemoveFromEmitters(Transform listener)
    {
        Initialize();
        if (ContainsEmitter(listener) == null)
        {
            return;
        }

        emitters.Remove(ContainsEmitter(listener));
    }

    public void ClearEmitters()
    {
        Initialize();
        emitters.Clear();
    }

    private EmitterClass ContainsEmitter(Transform emitter)
    {
        int emitID = emitter.GetInstanceID();
        foreach (EmitterClass ec in emitters)
        {
            if (ec.emitterTransform.GetInstanceID() == emitID)
            {
                return ec;
            }
        }

        return null;
    }
}