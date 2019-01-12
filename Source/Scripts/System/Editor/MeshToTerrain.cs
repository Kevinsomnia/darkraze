using UnityEngine;
using UnityEditor;

public class MeshToTerrain : EditorWindow
{
    public static Terrain terrainToEdit;
    public static MeshFilter meshToUse;

    [MenuItem("Terrain/Mesh to Terrain", false, 2000)]
    static void OpenWindow()
    {
        EditorWindow.GetWindow<MeshToTerrain>(true);
    }

    float sizeAdjustment;

    void OnGUI()
    {
        GUILayout.Label("Remember to create a copy of the terrain data!", EditorStyles.boldLabel);
        GUILayout.Space(20);
        GUI.SetNextControlName("Size Adjustment");
        sizeAdjustment = EditorGUILayout.FloatField("Size Adjustment:", sizeAdjustment);
        GUI.FocusControl("Size Adjustment");
        terrainToEdit = (Terrain)EditorGUILayout.ObjectField("Terrain to Edit:", terrainToEdit, typeof(Terrain), true);
        meshToUse = (MeshFilter)EditorGUILayout.ObjectField("Mesh to Use", meshToUse, typeof(MeshFilter), true);

        if (terrainToEdit == null || meshToUse == null)
        {
            return;
        }
        if (GUILayout.Button("Create Terrain") || Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.Return) || Event.current.keyCode == KeyCode.KeypadEnter)
        {
            this.Close();
            CreateTerrain();
        }
    }

    delegate void CleanUp();
    void CreateTerrain()
    {
        TerrainData terrain = terrainToEdit.terrainData;
        Undo.RecordObject(terrain, "Mesh to Terrain");

        MeshCollider collider = meshToUse.gameObject.GetComponent<MeshCollider>();
        CleanUp cleanUp = null;
        if (!collider)
        {
            collider = meshToUse.gameObject.AddComponent<MeshCollider>();
            cleanUp = () => DestroyImmediate(collider);
        }

        Bounds bounds = collider.bounds;
        bounds.Expand(new Vector3(-sizeAdjustment * bounds.size.x, 0, -sizeAdjustment * bounds.size.z));

        float[,] heights = new float[terrain.heightmapWidth, terrain.heightmapHeight];
        Ray ray = new Ray(new Vector3(bounds.min.x, bounds.max.y * 2, bounds.min.z), Vector3.down);
        RaycastHit hit = new RaycastHit();
        float meshHeightInverse = 1 / bounds.size.y;
        Vector3 rayOrigin = ray.origin;
        Vector2 stepXZ = new Vector2(bounds.size.x / heights.GetLength(1), bounds.size.z / heights.GetLength(0));

        for (int zCount = 0; zCount < heights.GetLength(0); zCount++)
        {
            for (int xCount = 0; xCount < heights.GetLength(1); xCount++)
            {
                heights[zCount, xCount] = collider.Raycast(ray, out hit, bounds.size.y * 2) ? 1 - (bounds.max.y - hit.point.y) * meshHeightInverse : 0;
                rayOrigin.x += stepXZ[0];
                ray.origin = rayOrigin;
            }
            rayOrigin.z += stepXZ[1];
            rayOrigin.x = bounds.min.x;
            ray.origin = rayOrigin;
        }

        terrain.SetHeights(0, 0, heights);

        if (cleanUp != null)
        {
            cleanUp();
        }
    }
}