using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class SurfaceDetectionScript : MonoBehaviour {

	// public static SurfaceDetectionScript MainInstance;

	// TODO: dont confuse collision with ground fin with collision with roof roll (is up-side-down) trigger
	// IDEA: remove roof roll trigger? not needed?
	// NOTE: separation might be needed in the future

	public enum RotationAxisDirection {
		Clockwise,
		CounterClockwise,
		None
	}

	[Serializable]
	public struct EnvironmentEffectCollection {

		public string EnvironmentTag; // object tags that enable these effects
		public float StartVelocity;
		public List<ParticleSystem> particles;
		public List<TrailRenderer> trails;

	}

	[Header("Empty tag = always on")]

	public List<EnvironmentEffectCollection> DriftEffects;
	public List<EnvironmentEffectCollection> BoostEffects;


	public List<EnvironmentEffectCollection> ClockwiseYawEffects;
	public List<EnvironmentEffectCollection> CounterClockwiseYawEffects;



	[Tooltip("At what speed the effects start and stop")]
	public float SpeedEffectThreshold = 1f;
	public List<EnvironmentEffectCollection> AlwaysOnEffects;

	private string currentTag = ""; // NOTE: only latest environment type touched have their effects enabled
	private bool drifting = false;
	private bool boosting = false;
	private bool touchingGround = false;
	// private bool drivingFast = false;

	private float currentSqrVelocity = 0f;


	private RotationAxisDirection YawDir = RotationAxisDirection.None;



	// TODO: check that performance impact is not awful
	private void EnableEffect(IEnumerable<EnvironmentEffectCollection> effects) {
		foreach (EnvironmentEffectCollection effect in effects) {
			foreach (ParticleSystem particleSystem in effect.particles)
				CustomUtilities.StartEffect(particleSystem);
			foreach (TrailRenderer trail in effect.trails)
				CustomUtilities.StartEffect(trail);
		}
	}
	private void EnableEffect(IEnumerable<EnvironmentEffectCollection> effects, string tag) {
		EnableEffect(effects.Where(e =>
			e.StartVelocity * e.StartVelocity > currentSqrVelocity
			|| e.EnvironmentTag == tag
			|| e.EnvironmentTag == ""
		));
	}

	private void DisableEffect(IEnumerable<EnvironmentEffectCollection> effects) {
		foreach (EnvironmentEffectCollection effect in effects) {
			foreach (ParticleSystem particleSystem in effect.particles)
				CustomUtilities.StopEffect(particleSystem);
			foreach (TrailRenderer trail in effect.trails)
				CustomUtilities.StopEffect(trail);
		}
	}
	private void DisableEffect(IEnumerable<EnvironmentEffectCollection> effects, string tag) {
		DisableEffect(effects.Where(e => e.EnvironmentTag == tag || e.EnvironmentTag == ""));
	}

	private void SetEffect(IEnumerable<EnvironmentEffectCollection> effects, bool enable) {
		if (enable)
			EnableEffect(effects);
		else
			DisableEffect(effects);
	}

	private void SetEffect(IEnumerable<EnvironmentEffectCollection> effects, string tag, bool enable) {
		if (enable)
			EnableEffect(effects, tag);
		else
			DisableEffect(effects, tag);
	}

	private void EnableAllEffects(string tag) {
		if (touchingGround && drifting)
			EnableEffect(DriftEffects, tag);

		if (boosting)
			EnableEffect(BoostEffects, tag);

		switch (YawDir) {
			case RotationAxisDirection.Clockwise:
				EnableEffect(ClockwiseYawEffects, tag);
				break;
			case RotationAxisDirection.CounterClockwise:
				EnableEffect(CounterClockwiseYawEffects, tag);
				break;
			case RotationAxisDirection.None:
				break;
		}

		EnableEffect(AlwaysOnEffects);

	}

	private void EnableAllEffects() {
		EnableAllEffects(currentTag);
	}

	private void DisableAllEffects(string tag) {
		DisableEffect(DriftEffects, currentTag);
		DisableEffect(BoostEffects, currentTag);
		DisableEffect(ClockwiseYawEffects, currentTag);
		DisableEffect(CounterClockwiseYawEffects, currentTag);
		DisableEffect(AlwaysOnEffects, currentTag);
	}

	private void DisableAllEffects() {
		DisableAllEffects(currentTag);
	}

	private void OnTriggerEnter(Collider other) {
		// print("trigger enter tag: " + other.gameObject.tag);

		DisableAllEffects();
		currentTag = other.tag;
		// if (touchingGround)
		EnableAllEffects();

	}

	// private void OnTriggerExit(Collider other) {
	// }

	public void StartTouchingGround() {
		touchingGround = true;
		EnableAllEffects();
	}

	public void StopTouchingGround() {
		touchingGround = false;
		DisableAllEffects();
	}

	public void SetTouchingGround(bool value) {
		if (value)
			StartTouchingGround();
		else
			StopTouchingGround();
	}

	public void StartDrift() {
		drifting = true;
		EnableEffect(DriftEffects, currentTag);
	}

	public void StopDrift() {
		drifting = false;
		DisableEffect(DriftEffects, currentTag);
	}

	public void StartBoost() {
		boosting = true;
		EnableEffect(BoostEffects, currentTag);
	}

	public void StopBoost() {
		boosting = false;
		DisableEffect(BoostEffects, currentTag);
	}


	public void StartClockwiseYaw() {
		EnableEffect(ClockwiseYawEffects, currentTag);
		StopCounterClockwiseYaw();
		YawDir = RotationAxisDirection.Clockwise;
	}

	public void StopClockwiseYaw() {
		DisableEffect(ClockwiseYawEffects, currentTag);
		YawDir = RotationAxisDirection.None;
	}

	public void StartCounterClockwiseYaw() {
		EnableEffect(CounterClockwiseYawEffects, currentTag);
		StopClockwiseYaw();
		YawDir = RotationAxisDirection.CounterClockwise;
	}

	public void StopCounterClockwiseYaw() {
		DisableEffect(CounterClockwiseYawEffects, currentTag);
		YawDir = RotationAxisDirection.None;
	}

	public void UpdateSpeed(float sqrVelocity) {

		currentSqrVelocity = sqrVelocity;
		// TODO: figure out when to update disabled and enabled effects, dont do it too often
		// EnableEffect(AlwaysOnEffects, currentTag);

		/*
		if (sqrVelocity > SpeedffectThreshold * SpeedEffectThreshold) {
			drivingFast = true;
			EnableEffect(SpeedEffects, currentTag);
		} else {
			drivingFast = false;
			DisableEffect(SpeedEffects, currentTag);
		}
		*/
	}

}
