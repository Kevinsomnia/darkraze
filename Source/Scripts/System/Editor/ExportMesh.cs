using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
 
struct ObjMaterial
{
	public string name;
	public string textureName;
}
 
public class ExportMesh : EditorWindow {
	private static int vertexOffset = 0;
	private static int normalOffset = 0;
	private static int uvOffset = 0;
	
	private static bool exists = false;
	private static string directory = "Exported Mesh";
	
	[MenuItem("Tools/Combine and Export Mesh")]
	static void OpenWindow() {
		directory = EditorPrefs.GetString("DirectoryExport", "Exported Mesh");
		EditorWindow.GetWindow<ExportMesh>(true);
	}
	
	void OnGUI() {
		Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
		EditorGUILayout.LabelField("You have selected: " + selection.Length + " meshes to combine.", EditorStyles.boldLabel);
		GUILayout.Space(5f);
		
		if(exists) {
			GUI.color = new Color(0.4f, 1f, 0.1f);
		}
		else {
			GUI.color = new Color(1f, 0.6f, 0.3f);
		}
		
		directory = EditorGUILayout.TextField("Directory:", directory);
		GUI.color = Color.white;
		
		GUILayout.Space(5f);
		
		if(!exists) {
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
		}
		
		if(GUILayout.Button("Combine and Export Mesh") && exists) {
			EditorPrefs.SetString("DirectoryExport", directory);
			ExecuteAction();
		}
	}
	
	void Update() {
		exists = Directory.Exists(Application.dataPath + "/" + directory);
		Repaint();
	}
	
    private static string MeshToString(MeshFilter mf) 
    {
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;
 
        StringBuilder sb = new StringBuilder();
 
        sb.Append("g ").Append(mf.name).Append("\n");
        foreach(Vector3 lv in m.vertices) 
        {
        	Vector3 wv = mf.transform.TransformPoint(lv);
 
            sb.Append(string.Format("v {0} {1} {2}\n",-wv.x,wv.y,wv.z));
        }
        sb.Append("\n");
 
        foreach(Vector3 lv in m.normals) 
        {
        	Vector3 wv = mf.transform.TransformDirection(lv);
 
            sb.Append(string.Format("vn {0} {1} {2}\n",-wv.x,wv.y,wv.z));
        }
        sb.Append("\n");
 
        foreach(Vector3 v in m.uv) 
        {
            sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
        }
 
        for (int material=0; material < m.subMeshCount; material ++) {
            sb.Append("\n");
            sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            sb.Append("usemap ").Append(mats[material].name).Append("\n");
 
            try
       		{
          		ObjMaterial objMaterial = new ObjMaterial();
 
          		objMaterial.name = mats[material].name;
 
          		if (mats[material].mainTexture)
          			objMaterial.textureName = AssetDatabase.GetAssetPath(mats[material].mainTexture);
          		else 
          			objMaterial.textureName = null;
        	}
        	catch (ArgumentException)
        	{
        	}
 
 
            int[] triangles = m.GetTriangles(material);
            for (int i=0;i<triangles.Length;i+=3) 
            {
                sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n", 
                    triangles[i]+1 + vertexOffset, triangles[i+1]+1 + normalOffset, triangles[i+2]+1 + uvOffset));
            }
        }
 
        vertexOffset += m.vertices.Length;
        normalOffset += m.normals.Length;
        uvOffset += m.uv.Length;
 
        return sb.ToString();
    }
 
    private static void Clear()
    {
    	vertexOffset = 0;
    	normalOffset = 0;
    	uvOffset = 0;
    }
 
    private static void MeshesToFile(MeshFilter[] mf, string folder, string filename) 
    { 
        using (StreamWriter sw = new StreamWriter(folder +"/" + filename + ".obj")) 
        { 
        	for (int i = 0; i < mf.Length; i++)
        	{
            	sw.Write(MeshToString(mf[i]));
            }
        }
    }
 
    private static bool CreateTargetFolder() {
    	try
    	{
    		Directory.CreateDirectory("Assets/" + directory);
    	}
    	catch
    	{
    		EditorUtility.DisplayDialog("Error!", "Something went wrong! Script failed to create target folder!", "FUCK!");
    		return false;
    	}
 
    	return true;
    } 
    
    static void ExecuteAction() {
		if(!CreateTargetFolder()) {
		}
		
        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
 
        if(selection.Length == 0)
        {
        	EditorUtility.DisplayDialog("Selection Error!", "Please select one or more objects with mesh filters.", "OK");
        	return;
        }
 
        int exportedObjects = 0;
		string[] tempNames = new string[selection.Length];
 
        ArrayList mfList = new ArrayList();
 
       	for (int i = 0; i < selection.Length; i++) {
			tempNames[i] = selection[i].name;
			selection[i].name = "mesh_export";
       		Component[] meshfilter = selection[i].GetComponentsInChildren<MeshFilter>();
 
       		for (int m = 0; m < meshfilter.Length; m++)
       		{
       			exportedObjects++;
       			mfList.Add(meshfilter[m]);
       		}
       	}
 
       	if (exportedObjects > 0)
       	{
       		MeshFilter[] mf = new MeshFilter[mfList.Count];
 
       		for (int i = 0; i < mfList.Count; i++)
       		{
       			mf[i] = (MeshFilter)mfList[i];
       		}
 
       		string filename = "CombinedMesh" + exportedObjects;
 
       		int stripIndex = filename.LastIndexOf('/');
 
       		if (stripIndex >= 0)
            	filename = filename.Substring(stripIndex + 1).Trim();
 
       		MeshesToFile(mf, "Assets/" + directory, filename);
 
 
       		Debug.Log("Export Success! " + exportedObjects + " meshes were combined and exported.");
       	}
       	else {
       		Debug.Log("Export Failure! Make sure at least one of your selected objects have mesh filters!");
		}
		
		for(int i = 0; i < selection.Length; i++) {
			selection[i].name = tempNames[i];
		}
		Selection.activeTransform = null;
    }
}