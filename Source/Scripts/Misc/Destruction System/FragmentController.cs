using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FragmentController : MonoBehaviour
{
    public Transform fragmentContainer;
    public float minForce = 12f;
    public float maxForce = 16f;
    public bool fragmentAll = false;
    public int fragmentHealth = 100;
    public float fragmentScale = 1f;
    public float fragmentMass = 2f;
    public int fragmentAmount = 1;
    public Material fragmentMaterial;
    public ParticleSystem fragmentParticle;
    public float collisionStrength = 5f;
    public float deletionDelay = 0f;
    public float disableColliderDelay = 0f;
    public float disablePhysicsDelay = 0f;
    public int combineMeshesDelay = 5;

    [HideInInspector] public int timer;
    [HideInInspector] public List<FragmentParts> fragments;

    private int fragmentAllFrame;
    private GameObject combinedFrags;

    void Start()
    {
        fragments = new List<FragmentParts>();
        foreach (Transform child in fragmentContainer)
        {
            GameObject go = child.gameObject;
            fragments.Add(go.AddComponent<FragmentParts>());
            go.AddComponent<MeshCollider>().convex = true;

            if (fragmentMaterial != null)
            {
                go.GetComponent<Renderer>().material = fragmentMaterial;
            }
        }

        if (fragmentMaterial == null)
        {
            fragmentMaterial = fragments[0].GetComponent<Renderer>().material;
        }

        if (combineMeshesDelay > 0)
        {
            InvokeRepeating("RecombineFragments", 0.1f, 0.1f);
        }
    }

    public void FragmentAll()
    {
        if (fragmentAllFrame == Time.frameCount)
        {
            return;
        }

        fragmentAllFrame = Time.frameCount;
        foreach (FragmentParts fp in fragments)
        {
            if (!fp.fragmented)
            {
                fp.DamageFragment(fragmentHealth + 1);
            }
        }
    }

    public void CombineFragments()
    {
        combinedFrags = new GameObject("CombinedFragments");
        MeshFilter mf = combinedFrags.AddComponent<MeshFilter>();
        combinedFrags.AddComponent<MeshRenderer>();

        CombineInstance[] combine = new CombineInstance[fragments.Count];
        for (int i = 0; i < fragments.Count; i++)
        {
            MeshFilter mFilter = fragments[i].GetComponent<MeshFilter>();
            combine[i].mesh = mFilter.sharedMesh;
            combine[i].transform = mFilter.transform.localToWorldMatrix;
            fragments[i].GetComponent<MeshRenderer>().enabled = false;
        }

        mf.mesh = new Mesh();
        mf.mesh.CombineMeshes(combine);
        mf.GetComponent<Renderer>().material = fragmentMaterial;
        combinedFrags.GetComponent<MeshRenderer>().enabled = true;
    }

    public void ReleaseFragments()
    {
        if (combinedFrags != null)
        {
            foreach (FragmentParts fp in fragments)
            {
                fp.GetComponent<MeshRenderer>().enabled = true;
            }
            Destroy(combinedFrags);
        }
    }

    public void RecombineFragments()
    {
        if (combineMeshesDelay > 0 && combinedFrags == null && timer > combineMeshesDelay)
        {
            CombineFragments();
        }
        else
        {
            timer++;
        }
    }
}