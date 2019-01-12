using UnityEngine;
using System.Collections;

public class GrenadeDatabase : MonoBehaviour {
	public static bool initialized = false;
	public static GrenadeController[] customGrenadeList = new GrenadeController[0];
	
	private static GrenadeList _savedGL;
	public static GrenadeList savedGrenadeList {
		get {
			if(_savedGL == null) {
                _savedGL = (GrenadeList)Resources.Load("Static Prefabs/Grenade List", typeof(GrenadeList));
			}
			
			return _savedGL;
		}
		set {
			_savedGL = value;
		}
	}
	
	public static GrenadeController[] publicGrenadeControllers {
		get {
			if(!initialized) {
				Initialize();
			}

			return customGrenadeList;
		}
	}
	
	public static void ClearIDs() {
		customGrenadeList = savedGrenadeList.savedGrenades;

		foreach(GrenadeController o in customGrenadeList) {
			if(o != null) {
				o.grenadeID = -1;
			}
		}
	}
	
	public static void RefreshIDs() {
		ClearIDs();
		Initialize();
	}
	
	public static void Initialize() {
		customGrenadeList = savedGrenadeList.savedGrenades;
		
		for(int i = 0; i < customGrenadeList.Length; i++) {
			customGrenadeList[i].grenadeID = i;
		}

		initialized = true;
	}
	
	public static GrenadeController GetGrenadeByID(int id) {
		if(!initialized) {
			Initialize();
		}

        if(id > publicGrenadeControllers.Length || id < 0) {
			return null;
		}
		else {
            return publicGrenadeControllers[id];
		}
	}
}