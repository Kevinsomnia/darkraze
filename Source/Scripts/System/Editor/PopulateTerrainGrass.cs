using UnityEngine;
using UnityEditor;

public class PopulateTerrainGrass : EditorWindow
{
    public static Terrain terrainToPopulate;
    public static int grassDensity = 512;
    public static int patchDetail = 16;

    [MenuItem("Terrain/Populate with Grass", false, 2000)]
    static void OpenWindow()
    {
        if (terrainToPopulate == null && Selection.activeTransform.GetComponent<Terrain>())
        {
            terrainToPopulate = Selection.activeTransform.GetComponent<Terrain>();
        }

        EditorWindow.GetWindow<PopulateTerrainGrass>(true);
    }

    void OnGUI()
    {
        GUILayout.Label("Just a reminder: You MUST manually erase some parts of grass!", EditorStyles.boldLabel);

        GUILayout.Space(10);

        terrainToPopulate = (Terrain)EditorGUILayout.ObjectField("Terrain to Populate", terrainToPopulate, typeof(Terrain), true);
        grassDensity = EditorGUILayout.IntSlider("Grass Density", grassDensity, 16, 4096);
        patchDetail = EditorGUILayout.IntSlider("Patch Detail", patchDetail, 4, 32);

        if (terrainToPopulate == null)
        {
            return;
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Populate"))
        {
            PopulateGrass();
        }
    }

    private void PopulateGrass()
    {
        terrainToPopulate.terrainData.SetDetailResolution(grassDensity, patchDetail);

        int[,] newMap = new int[grassDensity, grassDensity];

        for (int i = 0; i < grassDensity; i++)
        {
            for (int j = 0; j < grassDensity; j++)
            {
                newMap[i, j] = 1;
            }
        }

        terrainToPopulate.terrainData.SetDetailLayer(0, 0, 0, newMap);
    }
}