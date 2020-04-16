using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceDetectionScript : MonoBehaviour {

	// public static SurfaceDetectionScript MainInstance;

	// TODO: dont confuse collision with ground fin with collision with roof roll (is up-side-down) trigger
	// IDEA: remove roof roll trigger? not needed?
	// NOTE: separation might be needed in the future

	[Serializable]
	public struct EnvironmentEffectCollection {

		public string EnvironmentTag;
		public List<ParticleSystem> particles;
		public List<TrailRenderer> trails;

	}

	public List<EnvironmentEffectCollection> DriftEffects;
	public List<EnvironmentEffectCollection> BoostEffects;


	public List<EnvironmentEffectCollection> ClockwiseYawEffects;
	public List<EnvironmentEffectCollection> CounterClockwiseYawEffects;



	[Tooltip("At what speed the effects start and stop")]
	public float SpeedEffectThreshold = 1f;
	public List<EnvironmentEffectCollection> SpeedEffects;

	private string currentTag = ""; // NOTE: only latest environment type touched have their effects enabled
	private bool drifting = false;


	private void EnableEffect(string tag) {

	}

	private void DisableEffect(string tag) {

	}

	private void OnTriggerEnter(Collider other) {
		// print("trigger enter tag: " + other.gameObject.tag);

		// IDEA: ignore specific tag names, for default effects that are active for all environments


		// TODO: disable current tag effects
		currentTag = other.tag;
		// TODO: enable new tag effects


	}

	private void OnTriggerExit(Collider other) {
		// print("trigger exit tag: " + other.gameObject.tag);

		if (other.tag == currentTag) {
			// TODO: disable current tag effects
		}


	}

	public void StartDrift() {
		if (drifting)
			return;

		drifting = true; // useless?

	}

	public void StopDrift() {
		if (!drifting)
			return;

		drifting = false;

	}

	public void StartBoost() {


	}

	public void StopBoost() {

	}


	public void StartClockwiseYaw() {

	}

	public void StopClockwiseYaw() {

	}

	public void StartCounterClockwiseYaw() {

	}

	public void StopCounterClockwiseYaw() {

	}



}
