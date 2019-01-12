using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolingList cachedList;
    private static PoolManager _instance;
    public static PoolManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("Pool Manager");
                _instance = go.AddComponent<PoolManager>();
                _instance.Initialize(cachedList);
            }

            return _instance;
        }
    }

    public GameObject[] poolPrefabs;
    public List<GameObject>[] pooledObjects;
    public Transform tr;

    private ParticleManager[] poolParticlesPrefabs;
    private ParticleManager[] pooledParticles;
    private bool initialized;

    public void Initialize(PoolingList cachedList = null)
    {
        if (initialized)
        {
            return;
        }

        tr = transform;
        PoolingList cachePL = (cachedList != null) ? cachedList : (PoolingList)Resources.Load("Static Prefabs/PoolingList", typeof(PoolingList));
        poolPrefabs = cachePL.poolPrefabs;
        poolParticlesPrefabs = cachePL.poolParticles;
        pooledObjects = new List<GameObject>[poolPrefabs.Length];
        pooledParticles = new ParticleManager[poolParticlesPrefabs.Length];

        for (int i = 0; i < poolPrefabs.Length; i++)
        {
            pooledObjects[i] = new List<GameObject>();

            GameObject initObj = (GameObject)Instantiate(poolPrefabs[i]);
            initObj.name = poolPrefabs[i].name;

            PoolItem objPI = initObj.GetComponent<PoolItem>();
            objPI.prefabIndex = i;
            objPI.AddToPool();
        }

        for (int j = 0; j < poolParticlesPrefabs.Length; j++)
        {
            ParticleManager pm = (ParticleManager)Instantiate(poolParticlesPrefabs[j]);
            pm.transform.parent = tr;
            pooledParticles[j] = pm;
        }

        initialized = true;
    }

    public GameObject RequestInstantiate(GameObject go, Vector3 pos, Quaternion rot, bool callStartImmediately = true)
    {
        for (int i = 0; i < poolPrefabs.Length; i++)
        {
            if (poolPrefabs[i].name == go.name)
            {
                if (pooledObjects[i].Count > 0 && pooledObjects[i].Count <= 100)
                {
                    GameObject firstIndex = pooledObjects[i][0];
                    firstIndex.transform.parent = null;
                    firstIndex.transform.position = pos;
                    firstIndex.transform.rotation = rot;
                    firstIndex.SetActive(true);

                    PoolItem objPI = firstIndex.GetComponent<PoolItem>();
                    objPI.prefabIndex = i;

                    if (callStartImmediately)
                    {
                        objPI.InstantiateStart();
                    }

                    pooledObjects[i].RemoveAt(0);
                    return firstIndex;
                }
                else
                {
                    GameObject newInstance = (GameObject)Instantiate(go, pos, rot);
                    newInstance.name = go.name;

                    PoolItem objPI = newInstance.GetComponent<PoolItem>();
                    objPI.prefabIndex = i;

                    if (callStartImmediately)
                    {
                        objPI.InstantiateStart();
                    }

                    return newInstance;
                }
            }
        }
        return null;
    }

    //Optimized version, using a pre-defined index instead of searching for one.
    public GameObject RequestInstantiate(int index, Vector3 pos, Quaternion rot, bool callStartImmediately = true)
    {
        if (pooledObjects[index].Count > 0 && pooledObjects[index].Count <= 250)
        {
            GameObject firstIndex = pooledObjects[index][0];
            firstIndex.transform.parent = null;
            firstIndex.transform.position = pos;
            firstIndex.transform.rotation = rot;
            firstIndex.SetActive(true);

            PoolItem objPI = firstIndex.GetComponent<PoolItem>();
            objPI.prefabIndex = index;

            if (callStartImmediately)
            {
                objPI.InstantiateStart();
            }

            pooledObjects[index].RemoveAt(0);
            return firstIndex;
        }
        else
        {
            GameObject newInstance = (GameObject)Instantiate(poolPrefabs[index], pos, rot);
            newInstance.name = poolPrefabs[index].name;

            PoolItem objPI = newInstance.GetComponent<PoolItem>();
            objPI.prefabIndex = index;

            if (callStartImmediately)
            {
                objPI.InstantiateStart();
            }

            return newInstance;
        }
    }

    public void RequestParticleEmit(int index, Vector3 pos, Quaternion rot)
    {
        if (pooledParticles[index].inUse)
        {
            ParticleManager newInstance = (ParticleManager)Instantiate(poolParticlesPrefabs[index]);
            newInstance.transform.position = pos;
            newInstance.transform.rotation = rot;
            newInstance.EmitAll();
            return;
        }

        pooledParticles[index].transform.position = pos;
        pooledParticles[index].transform.rotation = rot;
        pooledParticles[index].EmitAll();
    }
}

public class PoolItem : MonoBehaviour
{
    [HideInInspector] public int prefabIndex;

    public virtual void InstantiateStart()
    {
        Debug.Log("Override me! (pool item)");
    }

    public void AddToPool()
    {
        StopCoroutine("PoolCoroutine");

        List<GameObject>[] pooledObjects = PoolManager.Instance.pooledObjects;

        if (pooledObjects[prefabIndex].Count <= 100)
        {
            transform.parent = PoolManager.Instance.tr;
            gameObject.SetActive(false);
            pooledObjects[prefabIndex].Add(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddToPool(float delay)
    {
        StartCoroutine(PoolCoroutine(delay));
    }

    private IEnumerator PoolCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        AddToPool();
    }
}