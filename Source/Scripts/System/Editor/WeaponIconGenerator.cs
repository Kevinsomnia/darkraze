using UnityEngine;
using UnityEditor;
using System.Collections;

public class WeaponIconGenerator : ScriptableWizard {
	public GameObject weaponMesh;
	public string nameOfWeapon;
	public float sizeOfImage = 0.5f;
	public Vector3 imageOffset = Vector3.zero;
	public Vector2 imageResolution = new Vector2(512, 256);
	public bool preview;
	public int cameraRotation;
    public float objectRotation;
	public RenderTexture texture;
	public string directory = "MAIN - Blackraze/Textures/GUI/Weapon Icons";
	
	private static Material skyboxMat;
	private static Color tempAmbient;
	private static Light[] lights;
	private bool update;
	private GameObject wMesh;
	private Camera renderCamera;
	private GameObject curWeaponSelection;
    private Vector2 lastRes;
	
	[MenuItem("Tools/Weapon Icon Generator", false, 5)]
	static void GenerateWeaponIcon() {
		lights = FindObjectsOfType(typeof(Light)) as Light[];
		tempAmbient = RenderSettings.ambientLight;
		skyboxMat = RenderSettings.skybox;
		ScriptableWizard.DisplayWizard<WeaponIconGenerator>("Weapon Icon Generator", "Generate");
	}
	
	[MenuItem("Tools/Get System Info")]
	static void DebugSystemInfo() {
		Debug.Log(DarkRef.GetSystemID);
	}
	
	void OnGUI() {
		directory = EditorGUILayout.TextField("Directory:", directory);
		weaponMesh = (GameObject)EditorGUILayout.ObjectField("Weapon Mesh", weaponMesh, typeof(GameObject), true);
		nameOfWeapon = EditorGUILayout.TextField("Name Of Weapon:", nameOfWeapon);
		cameraRotation = EditorGUILayout.IntSlider("Camera Rotation:", cameraRotation, 0, 1);
        objectRotation = EditorGUILayout.Slider("Object Rotation:", objectRotation, 0f, 360f);
		sizeOfImage = EditorGUILayout.Slider("Image Scale:", sizeOfImage, 0.001f, 10f);
		imageOffset = EditorGUILayout.Vector3Field("Image Offset:", imageOffset);
		imageResolution = EditorGUILayout.Vector2Field("Image Resolution:", imageResolution);

        if(weaponMesh == null) {
            GUI.color = new Color(0.5f, 0.5f, 1f, 0.4f);
        }

		if(GUILayout.Button("GENERATE!") && weaponMesh != null) {
			GenerateImage();
		}

        if(weaponMesh == null) {
            return;
        }

        GUI.color = Color.white;
		GUILayout.Space(25);
		preview = EditorGUILayout.Toggle("Preview Image", preview);
		PreviewImage();
		if(preview) {
            if(lastRes != imageResolution) {
                GUI.color = new Color(1f, 0.3f, 0.2f);
                GUILayout.Label("THE preview image HAS NOT updated its resolution yet!", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }

			if(wMesh == null) {
				GUILayout.Label("Please assign a weapon mesh to preview...");
			}
			else {
				GUILayout.Label("NOTE: The preview may look different from final product");
				GUILayout.Box(texture, GUILayout.Width(Mathf.Min(Screen.width - 10, texture.width)), GUILayout.Height(Mathf.Min(Mathf.Max(0f, Screen.height - 280f), texture.height)));
			}			
		}
	}
	
	void Update() {
		Repaint();

        if(objectRotation >= 360f) {
            objectRotation -= 360f;
        }
		
		if(weaponMesh != null && curWeaponSelection != weaponMesh) {
            nameOfWeapon = weaponMesh.name.ToLower().Replace(" ", "_");
			curWeaponSelection = weaponMesh;
		}
		
		if(renderCamera != null && wMesh != null) {
			renderCamera.depth = -9;
			renderCamera.backgroundColor = Color.white;
			renderCamera.orthographic = true;
			renderCamera.orthographicSize = sizeOfImage;
			renderCamera.transform.position = wMesh.transform.position + (Vector3.forward * 5f) + imageOffset;
			renderCamera.transform.rotation = Quaternion.Euler(0, cameraRotation * 180, 0);

            wMesh.transform.rotation = Quaternion.Euler(wMesh.transform.eulerAngles.x, objectRotation, wMesh.transform.eulerAngles.z);
		}
	}
	
	void GenerateImage() {		
		if(weaponMesh != null) {
			if(wMesh == null) {
				wMesh = Instantiate(weaponMesh, new Vector3(0, 1000, 0), Quaternion.identity) as GameObject;
			}
			if(renderCamera == null) {
				renderCamera = new GameObject("RenderCam").AddComponent<Camera>();
			}
			RenderSettings.ambientLight = Color.black;
			RenderSettings.skybox = null;
			renderCamera.backgroundColor = Color.white;
			if(lights.Length > 0) {
				foreach(Light l in lights) {
					l.gameObject.SetActive(false);
				}
			}
			
			RenderTexture rt = new RenderTexture((int)imageResolution.x, (int)imageResolution.y, 16);
			renderCamera.targetTexture = rt;
			Texture2D screenshot = new Texture2D((int)imageResolution.x, (int)imageResolution.y, TextureFormat.RGB24, false);
			renderCamera.Render();
			RenderTexture.active = rt;
			screenshot.ReadPixels(new Rect(0, 0, (int)imageResolution.x, (int)imageResolution.y), 0, 0);
			renderCamera.targetTexture = null;
			RenderTexture.active = null;
			DestroyImmediate(rt);
			
			
			byte[] bytes = screenshot.EncodeToPNG();
			string filename = string.Empty;
			
			if(!System.IO.Directory.Exists(Application.dataPath + "/" + directory)) {
				System.IO.Directory.CreateDirectory(Application.dataPath + "/" + directory);
			}
			
			filename = Application.dataPath + "/" + directory + "/icon_" + nameOfWeapon + ".png";
				
			System.IO.File.WriteAllBytes(filename, bytes);
            EditorUtility.DisplayDialog("Screenshot Successful", "Took screenshot at: " + filename, "OK");
			
			RenderSettings.ambientLight = tempAmbient;
			RenderSettings.skybox = skyboxMat;
			DestroyImmediate(renderCamera.gameObject);
			DestroyImmediate(wMesh);
			if(lights.Length > 0) {
				foreach(Light l in lights) {
					l.gameObject.SetActive(true);
				}
			}
		}
	}
	
	void PreviewImage() {
		if(preview) {
			if(update && weaponMesh != null) {
				if(weaponMesh != null && wMesh == null) {
					wMesh = Instantiate(weaponMesh, new Vector3(0, 1000, 0), Quaternion.identity) as GameObject;
				}
				if(renderCamera == null) {
					renderCamera = new GameObject("RenderCam").AddComponent<Camera>();
				}
				tempAmbient = RenderSettings.ambientLight;
				skyboxMat = RenderSettings.skybox;
				RenderSettings.ambientLight = Color.black;
				if(lights != null && lights.Length > 0) {
					foreach(Light l in lights) {
						l.gameObject.SetActive(false);
					}
				}

				texture = new RenderTexture((int)imageResolution.x, (int)imageResolution.y, 16);
				renderCamera.targetTexture = texture;
				renderCamera.Render();
				
                lastRes = imageResolution;
				update = false;
			}
		}
		else {
			OnDestroy();
		}
	}
	
	private void OnDestroy() {
		RenderSettings.ambientLight = tempAmbient;
		if(renderCamera != null) {
			DestroyImmediate(renderCamera.gameObject);
		}
		if(wMesh != null) {
			DestroyImmediate(wMesh);
		}
		if(texture != null) {
			texture = null;
		}
		if(lights != null && lights.Length > 0) {
			foreach(Light l in lights) {
				l.gameObject.SetActive(true);
			}
		}
		update = true;
	}
	
	void OnWizardUpdate() {
		helpString = "This script allows you to generate a image of a weapon icon";
	}
}