using UnityEngine;
using UnityEditor;
using System.Collections;

public class MeshInfo : ScriptableObject
{
    [MenuItem("Window/Show Mesh Info %#i")]
    public static void ShowCount()
    {
        int triangles = 0;
        int vertices = 0;
        int meshCount = 0;

        foreach (GameObject go in Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel))
        {
            Component[] meshes = go.GetComponentsInChildren(typeof(MeshFilter));

            foreach (MeshFilter mesh in meshes)
            {
                if (mesh.sharedMesh)
                {
                    vertices += mesh.sharedMesh.vertexCount;
                    triangles += mesh.sharedMesh.triangles.Length / 3;
                    meshCount++;
                }
            }
        }

        if (meshCount <= 0)
        {
            EditorUtility.DisplayDialog("Error", "Please select at least one mesh to continue!", "OK");
            return;
        }

        EditorUtility.DisplayDialog("Vertex and Triangle Count", triangles + " triangles and " + vertices + " vertices in selection. \n" + "(" + meshCount + " meshes in selection)", "OK!");
    }
}