using UnityEngine;
using System.Collections;

public delegate void Task();
public class ScheduledFunction : MonoBehaviour {
	private Task toDo;
	private float timer;
	private bool started = false;
	private bool destroyObject = false;

	public void SetTask(Task task, float time, bool destroyAfter) {
		toDo = task;
		timer = time;
		destroyObject = destroyAfter;
		started = true;
	}
	
	void Update() {
		if(started) {
			timer -= Time.deltaTime;
			if(timer <= 0f) {
				toDo();
				if(destroyObject) {
					Destroy(gameObject);	
				}
				else {
					Destroy(this);	
				}
			}
		}
	}
}
