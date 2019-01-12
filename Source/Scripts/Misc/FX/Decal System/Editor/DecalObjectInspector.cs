using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(DecalObject))]
public class DecalObjectInspector : Editor
{
    public static float angleOffset = 0f;

    [MenuItem("GameObject/Create Other/Decal Object %#d")]
    public static void CreateNewDecal()
    {
        Camera cam = SceneView.lastActiveSceneView.camera;
        GameObject newDecal = new GameObject("New Decal");

        RaycastHit placementInfo;
        GameObject targetObj = null;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out placementInfo, 25f))
        {
            newDecal.transform.position = placementInfo.point;
            newDecal.transform.rotation = Quaternion.LookRotation(-placementInfo.normal);
            targetObj = placementInfo.collider.gameObject;
        }
        else
        {
            newDecal.transform.position = cam.transform.position + (cam.transform.forward * 25f);
        }

        newDecal.transform.localScale = new Vector3(1f, 1f, 0.5f);

        DecalObject dObj = newDecal.AddComponent<DecalObject>();
        dObj.targetObject = targetObj;

        Selection.activeGameObject = newDecal;
    }

    public override void OnInspectorGUI()
    {
        DecalObject dObj = (DecalObject)target;

        EditorGUIUtility.LookLikeControls(140f);
        GUILayout.Space(5f);

        dObj.material = (Material)EditorGUILayout.ObjectField("Material:", dObj.material, typeof(Material));

        GUILayout.Space(5f);

        DarkRef.GUISeparator();
        EditorGUILayout.LabelField("Sprite List:");
        if (dObj.material != null && dObj.material.mainTexture != null)
        {
            dObj.curSprite = DrawSpriteList(dObj.curSprite, dObj.material.mainTexture);

            if (dObj.curSprite != null && dObj.curSprite.texture != dObj.material.mainTexture)
            {
                dObj.curSprite = null;
            }
        }
        DarkRef.GUISeparator();

        GUILayout.Space(10f);

        dObj.maxAngle = EditorGUILayout.Slider("Max Angle:", dObj.maxAngle, 1f, 90f);
        dObj.pushOffset = EditorGUILayout.FloatField("Push Offset:", Mathf.Max(0f, dObj.pushOffset));
        dObj.layersToAffect = LayerMaskField("Layers To Affect:", dObj.layersToAffect);
        dObj.optimized = EditorGUILayout.Toggle("Optimized Projection:", dObj.optimized);

        GUILayout.Space(10f);

        EditorGUILayout.ObjectField("Target Object:", dObj.targetObject, typeof(GameObject));
        EditorGUILayout.FloatField("Projection Depth:", dObj.transform.lossyScale.z);

        GUILayout.Space(10f);

        EditorGUILayout.HelpBox("CTRL + Click to drag the reposition the decal on a surface", MessageType.None);
        EditorGUILayout.HelpBox("Z and X to decrease/increase decal dimensions", MessageType.None);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(dObj);
            dObj.UpdateDecalMesh();
        }
    }

    private Sprite DrawSpriteList(Sprite sprite, Texture tex)
    {
        string pathToTexture = AssetDatabase.GetAssetPath(tex);
        Object[] allSpriteObjects = AssetDatabase.LoadAllAssetsAtPath(pathToTexture);

        List<Sprite> listOfSprites = new List<Sprite>();
        foreach (Object obj in allSpriteObjects)
        {
            if (obj.GetType() == typeof(Sprite))
            {
                listOfSprites.Add((Sprite)obj);
            }
        }

        listOfSprites.Add(null);

        GUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(50f));
        for (int i = 0, y = 0; i < listOfSprites.Count; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < 5; x++, i++)
            {
                Rect rect = GUILayoutUtility.GetAspectRect(1f);
                if (i < listOfSprites.Count)
                {
                    Sprite spr = listOfSprites[i];
                    if (DrawSpriteItem(rect, spr, sprite == spr, tex)) sprite = spr;
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        return sprite;
    }

    private bool DrawSpriteItem(Rect rect, Sprite sprite, bool isSelected, Texture texture)
    {
        if (isSelected)
        {
            GUI.color = new Color(0.2f, 0.5f, 0.8f, 0.4f);
            Rect bgRect = rect;
            bgRect.x -= 1;
            bgRect.y -= 1;
            bgRect.width += 2;
            bgRect.height += 2;
            GUI.DrawTexture(bgRect, EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
        }

        if (sprite == null)
        {
            GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            GUI.DrawTexture(rect, texture);
            GUI.color = Color.white;
        }
        else
        {
            Texture tex = sprite.texture;
            Rect texRect = sprite.rect;
            texRect.x /= tex.width;
            texRect.y /= tex.height;
            texRect.width /= tex.width;
            texRect.height /= tex.height;

            GUI.DrawTextureWithTexCoords(rect, tex, texRect);
        }

        isSelected = Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
        if (isSelected)
        {
            GUI.changed = true;
            Event.current.Use();
            return true;
        }

        return false;
    }

    public void OnSceneGUI()
    {
        Event curEvent = Event.current;

        if (curEvent.control)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        DecalObject dObj = (DecalObject)target;

        if (curEvent.control && (curEvent.type == EventType.MouseDown || curEvent.type == EventType.MouseDrag))
        {
            Ray clickRay = HandleUtility.GUIPointToWorldRay(curEvent.mousePosition);
            List<RaycastHit> allHit = new List<RaycastHit>(Physics.RaycastAll(clickRay, 100f, dObj.layersToAffect.value));
            allHit.Sort((h1, h2) => h1.distance.CompareTo(h2.distance));

            for (int i = 0; i < allHit.Count; i++)
            {
                if (!allHit[i].collider.gameObject.GetComponent<MeshFilter>())
                {
                    continue;
                }

                dObj.transform.position = allHit[i].point;
                dObj.transform.rotation = Quaternion.LookRotation(-allHit[i].normal) * Quaternion.Euler(Vector3.forward * -angleOffset);
                dObj.targetObject = allHit[i].collider.gameObject;
                dObj.UpdateDecalMesh();
                break;
            }
        }

        if (curEvent.type == EventType.KeyDown)
        {
            if (curEvent.keyCode == KeyCode.Z)
            {
                Vector3 oldSize = dObj.transform.localScale;
                oldSize.x *= 0.9f;
                oldSize.y *= 0.9f;
                dObj.transform.localScale = oldSize;
            }
            else if (curEvent.keyCode == KeyCode.X)
            {
                Vector3 oldSize = dObj.transform.localScale;
                oldSize.x *= 1.1f;
                oldSize.y *= 1.1f;
                dObj.transform.localScale = oldSize;
            }
        }
    }

    public LayerMask LayerMaskField(string label, LayerMask maskVar)
    {
        List<string> layers = new List<string>();

        for (int i = 0; i < 32; i++)
        {
            string name = LayerMask.LayerToName(i);
            if (name != "") layers.Add(name);
        }

        return EditorGUILayout.MaskField(label, maskVar, layers.ToArray());
    }
}