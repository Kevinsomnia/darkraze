using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class DarkrazePostBuild {
	[PostProcessBuild]
	public static void OnPostProcessBuild(BuildTarget target, string buildPath) {
		string targetFolder = buildPath.Substring(0, buildPath.LastIndexOf('/') + 1);
			
		foreach(string file in Directory.GetFiles(Application.dataPath + "/MAIN - Blackraze/Post-process Files")) {
			if(file.EndsWith(".meta")) {
				continue;
			}
			
			string filePath = targetFolder + (Path.GetFileName(file));
			if (File.Exists(filePath)){
				File.Delete(filePath);
			}

			File.Copy(file, filePath);
		}
   	}
}