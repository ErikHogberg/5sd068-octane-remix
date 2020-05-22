using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class GenericSwipeMaskUIScript : MonoBehaviour {

	public enum SwipeMode {
		None,
		SwipeIn,
		SwipeOut,
	}

	public SwipeMode StartMode = SwipeMode.None;
	private SwipeMode mode;

	[Header("Fade In")]

	[Min(0f)]
	public float FadeInTime;
	public Color FadeInFromColor;
	public AnimationCurve FadeInCurve;

	[Header("Fade Out")]

	[Min(0f)]
	public float FadeOutTime;
	public Color FadeOutToColor;
	public AnimationCurve FadeOutCurve;

	Vector3 initPos;

	// VideoPlayer video;

	Vector3 videoInitPos;

	public float Duration = 1f;
	private float time = 0f;
	private bool forward = true;

	public float MovementDistance = 100f;
	public AnimationCurve MovementCurve;
	public bool PingPong = true;

	void Start() {
		initPos = transform.position;
		// video = GetComponentInChildren<VideoPlayer>();
		// videoInitPos = video.transform.position;
	}

	void Update() {
		if (time > Duration) {
			time = Duration;
			if (PingPong) {
				forward = false;
			} else {
				gameObject.SetActive(false);
			}
		} else { }
		if (time < 0) {
			gameObject.SetActive(false);
			return;
		}

		if (forward) {
			time += Time.deltaTime;
		} else {
			time -= Time.deltaTime;
		}


		float percentage = MovementCurve.Evaluate(time / Duration);
		transform.position = initPos + Vector3.right * MovementDistance * percentage;

		// video.transform.position = videoInitPos;

	}

	public void Restart() {
		time = 0;
		forward = true;

		gameObject.SetActive(true);
	}
}
