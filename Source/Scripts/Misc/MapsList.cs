using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class StaticMapsList
{
    private static MapsList m_maps;
    private static Map[] _allMapsArray;

    public static Map[] allMapsArray
    {
        get
        {
            if (_allMapsArray == null)
            {
                _allMapsArray = allMaps.Values.ToArray();
            }

            return _allMapsArray;
        }
    }

    private static List<Map> _maSort;
    public static List<Map> mapsArraySorted
    {
        get
        {
            if (_maSort == null)
            {
                _maSort = new List<Map>();

                int curOrderNum = 0;
                Map curMap = null;
                for (int i = 0; i < allMapsArray.Length; i++)
                {
                    if (allMapsArray[i].orderIndex <= -1)
                    {
                        continue;
                    }

                    try
                    {
                        curMap = GetMapByOrder(curOrderNum);
                    }
                    catch
                    {
                        curOrderNum++;
                        continue;
                    }

                    _maSort.Add(curMap);
                    curOrderNum++;
                }

                foreach (Map m in StaticMapsList.allMapsArray)
                {
                    if (m.orderIndex > -1)
                    {
                        continue;
                    }

                    _maSort.Add(m);
                }
            }

            return _maSort;
        }
    }

    public static Map GetMapByOrder(int order)
    {
        for (int i = 0; i < maps.Length; i++)
        {
            if (maps[i].orderIndex != order)
            {
                continue;
            }

            return maps[i];
        }

        return null;
    }

    private static Dictionary<string, Map> _allMaps = null;
    public static Dictionary<string, Map> allMaps
    {
        get
        {
            if (_allMaps == null)
            {
                _allMaps = new Dictionary<string, Map>();

                foreach (Map map in maps)
                {
                    _allMaps[map.mapName] = map;
                }
            }
            return _allMaps;
        }
    }

    public static Map[] maps
    {
        get
        {
            if (m_maps == null)
            {
                m_maps = ((GameObject)Resources.Load("Static Prefabs/MapList")).GetComponent<MapsList>();
            }
            return m_maps.maps;
        }
    }
}

[System.Serializable]
public class Map
{
    public string mapName = "Test";
    public string sceneName = "Test_Scene";
    public Texture2D previewIcon;
    public Texture2D loaderScreenshot;
    public string loaderSubheader = "Subheader";
    public string loaderDescription = "Example description for the loader.";

    public int orderIndex = -1; // -1 will ignore order and put it to last priority to order according to list index.
    public float lightingMultiplier = 1.0f; //Just a multiplier to affect many things such as reflection intensity, and bullet trail opacity. 1.0 should be in a night time setting.
}

public class MapsList : MonoBehaviour
{
    public Map[] maps = new Map[1];

    public int GetIndex(string sceneName)
    {
        for (int i = 0; i < maps.Length; i++)
        {
            if (maps[i].sceneName == sceneName)
            {
                return i;
            }
        }

        return 0;
    }
}