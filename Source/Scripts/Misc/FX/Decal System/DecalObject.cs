using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DecalObject : MonoBehaviour
{
    public Material material;
    public float maxAngle = 90f;
    public float pushOffset = 0.001f;
    public bool optimized = false;
    public LayerMask layersToAffect = -1;

    [HideInInspector] public GameObject targetObject = null;
    [HideInInspector] public Sprite curSprite = null;

    private Transform tr;
    private MeshFilter filter;
    private Vector3 lastPosition = Vector3.zero;
    private Quaternion lastRotation = Quaternion.identity;
    private Vector3 lastScale = Vector3.one;

    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> triangles = new List<int>();

    private bool initialized = false;
    private Vector3[] targetVerts;
    private int[] targetTris;

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 0.5f, 0.3f, 0.5f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    private void Awake()
    {
        tr = GetComponent<Transform>();
        filter = GetComponent<MeshFilter>();
    }

    public void Start()
    {
        if (!initialized)
        {
            UpdateDecalMesh();
        }
    }

    private void Update()
    {
        if ((tr.localPosition - lastPosition).sqrMagnitude > 0f || Quaternion.Angle(tr.localRotation, lastRotation) > 0f || (tr.localScale - lastScale).sqrMagnitude > 0f)
        {
            UpdateDecalMesh();
        }
    }

    public void UpdateDecalMesh()
    {
        if (targetObject != null)
        {
            BuildDecalGeometry(targetObject);
        }

        initialized = true;
    }

    private void BuildDecalGeometry(GameObject affectedObject)
    {
        MeshFilter affectedMesh = affectedObject.GetComponent<MeshFilter>();

        if (affectedMesh == null)
        {
            return;
        }

        if (!affectedMesh.sharedMesh.isReadable)
        {
            return;
        }

        Plane leftPlane = new Plane(-Vector3.right, -Vector3.right * 0.5f);
        Plane rightPlane = new Plane(Vector3.right, Vector3.right * 0.5f);

        Plane topPlane = new Plane(Vector3.up, Vector3.up * 0.5f);
        Plane bottomPlane = new Plane(-Vector3.up, -Vector3.up * 0.5f);

        Plane frontPlane = new Plane(Vector3.forward, Vector3.forward * 0.5f);
        Plane backPlane = new Plane(-Vector3.forward, -Vector3.forward * 0.5f);

        targetVerts = affectedMesh.sharedMesh.vertices;
        targetTris = affectedMesh.sharedMesh.triangles;
        int startIndex = vertices.Count;

        Matrix4x4 matrix = tr.worldToLocalMatrix * affectedObject.transform.localToWorldMatrix;

        for (int i = 0; i < targetTris.Length; i += 3)
        {
            Vector3 v1 = matrix.MultiplyPoint(targetVerts[targetTris[i]]);
            Vector3 v2 = matrix.MultiplyPoint(targetVerts[targetTris[i + 1]]);
            Vector3 v3 = matrix.MultiplyPoint(targetVerts[targetTris[i + 2]]);

            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

            if (Vector3.Angle(-Vector3.forward, normal) >= maxAngle - 0.01f)
                continue;

            DecalPolygon poly = new DecalPolygon(v1, v2, v3);

            poly = DecalPolygon.ClipPolygon(poly, leftPlane);
            if (poly == null) continue;
            poly = DecalPolygon.ClipPolygon(poly, rightPlane);
            if (poly == null) continue;

            poly = DecalPolygon.ClipPolygon(poly, topPlane);
            if (poly == null) continue;
            poly = DecalPolygon.ClipPolygon(poly, bottomPlane);
            if (poly == null) continue;

            if (!optimized)
            {
                poly = DecalPolygon.ClipPolygon(poly, frontPlane);
                if (poly == null) continue;
                poly = DecalPolygon.ClipPolygon(poly, backPlane);
                if (poly == null) continue;
            }

            AddPolygon(poly, normal);
        }

        CalculateUVs(startIndex);

        if (pushOffset > 0f)
        {
            Push(pushOffset);
        }

        GenerateDecalMesh();
    }

    private void AddPolygon(DecalPolygon polygon, Vector3 normal)
    {
        int triangle = CalculateVertex(polygon.vertices[0], normal);

        for (int i = 1; i < polygon.vertices.Count - 1; i++)
        {
            triangles.Add(triangle);
            triangles.Add(CalculateVertex(polygon.vertices[i], normal));
            triangles.Add(CalculateVertex(polygon.vertices[i + 1], normal));
        }
    }

    private int CalculateVertex(Vector3 vertex, Vector3 normal)
    {
        int index = SearchVertex(vertex);

        if (index <= -1)
        {
            vertices.Add(vertex);
            normals.Add(normal);
            index = vertices.Count - 1;
        }
        else
        {
            normals[index] = (normals[index] + normal).normalized;
        }

        return index;
    }

    private int SearchVertex(Vector3 vertex)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if ((vertex - vertices[i]).sqrMagnitude < 0.001f)
            {
                return i;
            }
        }

        return -1;
    }

    private void CalculateUVs(int start)
    {
        Rect rect = new Rect();
        if (curSprite != null)
        {
            rect = curSprite.rect;
            rect.x /= curSprite.texture.width;
            rect.y /= curSprite.texture.height;
            rect.width /= curSprite.texture.width;
            rect.height /= curSprite.texture.height;
        }

        for (int i = start; i < vertices.Count; i++)
        {
            Vector2 uv = new Vector2(vertices[i].x + 0.5f, vertices[i].y + 0.5f);
            uv.x = Mathf.Lerp((curSprite != null) ? rect.xMin : 0f, (curSprite != null) ? rect.xMax : 1f, uv.x);
            uv.y = Mathf.Lerp((curSprite != null) ? rect.yMin : 0f, (curSprite != null) ? rect.yMax : 1f, uv.y);

            uvs.Add(uv);
        }
    }

    public void Push(float distance)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] += normals[i] * distance;
        }
    }

    private void GenerateDecalMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "DecalMesh";
        mesh.hideFlags = HideFlags.HideAndDontSave;

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();

        vertices.Clear();
        normals.Clear();
        uvs.Clear();
        triangles.Clear();

        filter.mesh = mesh;

        if (material != null)
        {
            filter.GetComponent<Renderer>().material = material;
        }

        lastPosition = tr.localPosition;
        lastRotation = tr.localRotation;
        lastScale = tr.localScale;
    }

    private void ClearDecalMesh()
    {
        if (filter.sharedMesh != null && filter.sharedMesh.name == "DecalMeshCleared")
        {
            return;
        }

        Mesh mesh = new Mesh();
        mesh.name = "DecalMeshCleared";
        mesh.hideFlags = HideFlags.HideAndDontSave;

        vertices.Clear();
        normals.Clear();
        uvs.Clear();
        triangles.Clear();

        filter.mesh = mesh;
    }

    public Bounds GetBounds()
    {
        Vector3 min = tr.localScale * -0.5f;
        Vector3 max = tr.lossyScale * 0.5f;

        Vector3[] vts = new Vector3[8] {
            new Vector3(min.x, min.y, min.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(max.x, max.y, min.z),

            new Vector3(min.x, min.y, max.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, max.y, max.z)
        };

        for (int i = 0; i < 8; i++)
        {
            vts[i] = transform.TransformDirection(vts[i]);
        }

        min = max = vts[0];
        foreach (Vector3 v in vts)
        {
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }

        return new Bounds(tr.position, max - min);
    }
}