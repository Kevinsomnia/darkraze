using UnityEngine;
using System.Collections;

public class SpectatorCamera : MonoBehaviour {
    public SpectatorFollow playerFollow;
    public SpectatorFreeLook freeLook;

    public Transform target {
        get {
            if(!playerFollow.enabled) {
                return null;
            }

            return playerFollow.target;
        }
        set {
            Transform _target = value;
			freeLook.enabled = (_target == null);
            playerFollow.enabled = (_target != null);
            playerFollow.target = _target;
        }
    }
}