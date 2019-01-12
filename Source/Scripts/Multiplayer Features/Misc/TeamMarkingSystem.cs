using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeamMarkingSystem : MonoBehaviour {
    public TeammateMarker markerPrefab;
    public Transform markerRoot;

    private NetworkingGeneral nGen;
    private int oldPlayerCount;
    private List<TeammateMarker> teamMarkers;

    void Start() {
        nGen = GeneralVariables.Networking;
        teamMarkers = new List<TeammateMarker>();
    }

    public void AddPlayerMarker(int id, bool isBot = false) {
        TeammateMarker tMarker = (TeammateMarker)Instantiate(markerPrefab);
        tMarker.transform.parent = markerRoot;
        tMarker.transform.localPosition = Vector3.zero;
        tMarker.transform.localScale = Vector3.one;

        if(!isBot) {
            tMarker.targetObserver = nGen.playerInstances[id];
        }
        else {
            tMarker.targetObserver = nGen.botInstances[id];
        }

        teamMarkers.Add(tMarker);
    }
}