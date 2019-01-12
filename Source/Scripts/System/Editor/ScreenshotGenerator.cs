using UnityEngine;
using UnityEditor;
using System.Collections;

//Generates screenshots. Mainly used for loader.
public class ScreenshotGenerator : ScriptableWizard {
	public string directory = "MAIN - Blackraze/Textures/GUI/Resources/Loader_Screenshots";
	public string nameOfScreenshot = "Screenshot";
	public Vector2 imageResolution = new Vector2(1600, 900);
	
	private Camera renderCamera;
	private RenderTexture renderTex;
	
	[MenuItem("Tools/Screenshot Generator")]
	private static void GenerateScreenshot() {
		ScriptableWizard.DisplayWizard<ScreenshotGenerator>("Screenshot Generator", "Generate");
	}
	
	void OnGUI() {		
		UpdateCamera();
		
		directory = EditorGUILayout.TextField("Directory:", directory);
		nameOfScreenshot = EditorGUILayout.TextField("File Name:", nameOfScreenshot);
		imageResolution = EditorGUILayout.Vector2Field("Image Resolution:", imageResolution);
		if(GUILayout.Button("Generate")) {
			GenerateImage();
		}
		
		GUILayout.Space(25);
		EditorGUILayout.LabelField("Image Preview (" + (int)imageResolution.x + "x" + (int)imageResolution.y + ")", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("NOTE: This may look different from final product.");
		
		if(renderTex)
			GUILayout.Box(renderTex, GUILayout.Width(Mathf.Min(Screen.width - 10, renderTex.width)), GUILayout.Height(Mathf.Min(Mathf.Clamp(Screen.height - 160, 0, Screen.height), (Screen.width / (imageResolution.x / imageResolution.y)))));
	}
	
	void Update() {
		Repaint();
	}
	
	private void GenerateImage() {		
		Texture2D screenshot = new Texture2D((int)imageResolution.x, (int)imageResolution.y, TextureFormat.RGB24, false);
		renderCamera.Render();
		RenderTexture.active = renderTex;
		screenshot.ReadPixels(new Rect(0, 0, (int)imageResolution.x, (int)imageResolution.y), 0, 0);
		RenderTexture.active = null;
		DestroyImmediate(renderTex);
			
		byte[] bytes = screenshot.EncodeToPNG();
		string filename = "";
		
		if(!System.IO.Directory.Exists(Application.dataPath + "/" + directory)) {
			System.IO.Directory.CreateDirectory(Application.dataPath + "/" + directory);
		}
			
		filename = Application.dataPath + "/" + directory + "/" + nameOfScreenshot + ".png";
			
		System.IO.File.WriteAllBytes(filename, bytes);
		Debug.Log("Took screenshot at: " + filename);
		DestroyImmediate(renderCamera.gameObject);
	}
	
	private void UpdateCamera() {
		if(renderCamera == null) {
			renderCamera = new GameObject("[Screenshot Camera]").AddComponent<Camera>();
			renderCamera.backgroundColor = new Color(0, 0, 0, 0);
			renderCamera.farClipPlane = 10000f;
			renderCamera.depth = -5;
		}
		
		if(renderTex == null) {
			renderTex = new RenderTexture(1, 1, 24);
		}
		
		if(renderTex != null && (renderTex.width != (int)imageResolution.x || renderTex.height != (int)imageResolution.y) && imageResolution.x > 1f && imageResolution.y > 1f) {
			renderTex = new RenderTexture((int)imageResolution.x, (int)imageResolution.y, 24);
			renderCamera.targetTexture = renderTex;
			renderCamera.Render();
		}
	}
	
	private void OnDestroy() {
		if(renderCamera != null) {
			DestroyImmediate(renderCamera.gameObject);
		}
		if(renderTex != null) {
			renderTex = null;
		}
	}
}