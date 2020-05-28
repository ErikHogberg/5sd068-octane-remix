using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemixCameraRotateScript : MonoBehaviour {

	private static RemixCameraRotateScript mainInstance;

	public Vector3 Axis = Vector3.up;
	public float Speed = 1f;
	public Space RelativeMode = Space.World;

	public AnimationCurve WindupCurve = AnimationCurve.Linear(0, 0, 1, 1);
	[Min(.001f)]
	public float WindupTime = 1f;
	public float StopWaitTime = 3f;

	float timer = -1;

	bool waiting = false;

	private void Awake() {
		mainInstance = this;
	}

	private void OnDestroy() {
		mainInstance = null;
	}

	void Update() {

		float percentage = 1f;

		if (timer > 0) {
			timer -= Time.unscaledDeltaTime;
		}

		if (waiting) {
			// percentage = 0f;
			if (timer < 0) {
				waiting = false;
				timer = WindupTime;
			}
			return;
		}

		percentage = Mathf.Clamp01(timer / WindupTime);
		percentage = WindupCurve.Evaluate(1 - percentage);

		transform.Rotate(Axis, Speed * Time.deltaTime * percentage, RelativeMode);

	}

	public void Stop() {
		waiting = true;
		timer = StopWaitTime;
	}

	public static void StopStatic(){
		mainInstance?.Stop();
	}
}
