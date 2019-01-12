using UnityEngine;

public class LockCursorEnsure : MonoBehaviour {
    private void Update() {
        if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
            LockCursor();
        }
    }
    
    private void LockCursor() {
        if(!RestrictionManager.restricted) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}