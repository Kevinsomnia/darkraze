using UnityEditor;
using UnityEngine;

public class CubeGen : ScriptableWizard
{
    public Transform renderPosition;
    public Cubemap cubemap;

    void OnWizardCreate()
    {
        GameObject cam = new GameObject("CubemapCamera");
        cam.AddComponent<Camera>();
        cam.AddComponent<FlareLayer>();
        cam.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;

        if (renderPosition.GetComponent<Renderer>())
        {
            cam.transform.position = renderPosition.GetComponent<Renderer>().bounds.center;
        }
        else
        {
            cam.transform.position = renderPosition.position;
        }

        cam.transform.rotation = Quaternion.identity;
        cam.GetComponent<Camera>().fieldOfView = 90.0f;
        cam.GetComponent<Camera>().aspect = 1.0f;

        cam.GetComponent<Camera>().RenderToCubemap(cubemap);
        DestroyImmediate(cam);
    }

    [MenuItem("Tools/Generate Cubemap", false, 4)]
    static void RenderCubemap()
    {
        ScriptableWizard.DisplayWizard<CubeGen>("CubeGen (Wizard)", "Render!");
    }
}