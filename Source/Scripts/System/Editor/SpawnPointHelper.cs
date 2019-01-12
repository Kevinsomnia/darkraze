using UnityEngine;
using UnityEditor;
using System.Collections;

public class SpawnPointHelper : EditorWindow {
    public enum SpawnTag {
        RedSpawn = 0,
        BlueSpawn = 1,
        UnassignedSpawn = 2
    }

    private static GameObject _p;
    public static GameObject preview {
        get {
            if(_p == null) {
                GameObject go = GameObject.Find("_overlay");
                if(go != null) {
                    DestroyImmediate(go);
                }

                _p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _p.transform.localScale = Vector3.one * 0.5f;
                _p.name = "_overlay";
                _p.hideFlags = HideFlags.HideAndDontSave;

                Material mat = new Material(Shader.Find("Transparent/Diffuse"));
                mat.hideFlags = HideFlags.HideAndDontSave;
                _p.GetComponent<Renderer>().sharedMaterial = mat;
                _p.GetComponent<Renderer>().sharedMaterial.color = new Color(0f, 1f, 0f, 0.45f);
                DestroyImmediate(_p.GetComponent<SphereCollider>());
            }

            return _p;
        }
    }

    private static GameObject _hit;
    public static GameObject hitPoint {
        get {
            if(_hit == null) {
                GameObject go = GameObject.Find("_hit");
                if(go != null) {
                    DestroyImmediate(go);
                }

                _hit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _hit.transform.localScale = Vector3.one * 0.1f;
                _hit.name = "_hit";
                _hit.hideFlags = HideFlags.HideAndDontSave;

                Material mat = new Material(Shader.Find("Reflective/Diffuse"));
                mat.hideFlags = HideFlags.HideAndDontSave;
                _hit.GetComponent<Renderer>().sharedMaterial = mat;
                _hit.GetComponent<Renderer>().sharedMaterial.color = Color.red;
                _hit.GetComponent<Renderer>().sharedMaterial.SetColor("_ReflectColor", Color.yellow);
                DestroyImmediate(_hit.GetComponent<SphereCollider>());
            }

            return _hit;
        }
    }

    private static float distance = 1f;
    private static string spawnName = "SpawnPoint";
    private static SpawnTag tagName = SpawnTag.RedSpawn;

    [MenuItem("Tools/Spawn Point Helper")]
    public static void OpenWindow() {
        EditorWindow.GetWindow(typeof(SpawnPointHelper));
        distance = EditorPrefs.GetFloat("SpawnDist", 1f);
        tagName = (SpawnTag)EditorPrefs.GetInt("SpawnTag", 0);
    }

    void OnGUI() {
        GUILayout.Label("Spawn Point Helper Tool", EditorStyles.boldLabel);

        GUILayout.Space(10f);

        distance = EditorGUILayout.FloatField("Point Offset:", distance);
        spawnName = EditorGUILayout.TextField("Spawn Name:", spawnName);
        tagName = (SpawnTag)EditorGUILayout.EnumPopup("Spawn Tag:", tagName);
        EditorGUILayout.ObjectField("Parenting To:", Selection.activeTransform, typeof(Transform), true);

        GUILayout.Space(10f);

        GUI.backgroundColor = new Color(1f, 0.2f, 0.1f, 1f);
        if(GUILayout.Button("Set Spawn Position")) {
            Camera cam = SceneView.lastActiveSceneView.camera;

            if(SceneView.lastActiveSceneView == null) {
                return;
            }

            RaycastHit hit;
            if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 100f)) {
                Transform toParent = Selection.activeTransform;

                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.localScale = Vector3.one * 0.5f;
                go.name = spawnName;
                go.tag = tagName.ToString();
                go.transform.position = hit.point + (hit.normal * distance);
                go.transform.rotation = Quaternion.identity;
                go.transform.parent = toParent;
                DestroyImmediate(go.GetComponent<SphereCollider>());
            }
        }
    }

    void Update() {
        SceneView sv = SceneView.lastActiveSceneView;

        if(sv != null) {
            RaycastHit hit;
            if(Physics.Raycast(sv.camera.transform.position, sv.camera.transform.forward, out hit, 100f)) {
                preview.transform.position = hit.point + (hit.normal * distance);
                hitPoint.transform.position = hit.point;
            }
            else {
                preview.transform.position = Vector3.down * 1000f;
                hitPoint.transform.position = Vector3.down * 1000f;
            }
        }

        Repaint();
    }

    void OnDestroy() {
        if(_p != null) {
            DestroyImmediate(_p);
        }

        if(_hit != null) {
            DestroyImmediate(_hit);
        }

        EditorPrefs.GetFloat("SpawnDist", distance);
        EditorPrefs.SetInt("SpawnTag", (int)tagName);
    }
}