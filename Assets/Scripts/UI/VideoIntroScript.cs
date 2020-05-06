using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoIntroScript : MonoBehaviour {

	Vector3 initPos;

	VideoPlayer video;
	Vector3 videoInitPos;

	public float Duration = 1f;
	private float time = 0f;
	private bool forward = true;

	public float MovementDistance = 100f;
	public AnimationCurve MovementCurve;
	public bool PingPong = true;

	void Start() {
		initPos = transform.position;
		video = GetComponentInChildren<VideoPlayer>();
		videoInitPos = video.transform.position;
	}

	void Update() {
		if (time > Duration) {
			time = Duration;
			if (PingPong) {
				forward = false;
			} else {
				gameObject.SetActive(false);
				video.Stop();
			}
		} else { }
		if (time < 0) {
			gameObject.SetActive(false);
			video.Stop();
			return;
		}

		if (forward) {
			time += Time.deltaTime;
		} else {
			time -= Time.deltaTime;
		}


		float percentage = MovementCurve.Evaluate(time / Duration);
		transform.position = initPos + Vector3.right * MovementDistance * percentage;

		video.transform.position = videoInitPos;

	}

	public void Restart() {
		time = 0;
		forward = true;

		video.Play();

		gameObject.SetActive(true);
	}
}
