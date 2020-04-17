﻿using System;
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
	public List<EnvironmentEffectCollection> SpeedEffects;

	private string currentTag = ""; // NOTE: only latest environment type touched have their effects enabled
	private bool drifting = false;
	private bool boosting = false;
	private bool touchingGround = false;
	private bool drivingFast = false;


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
		EnableEffect(effects.Where(e => e.EnvironmentTag == tag));
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
		if (drifting)
			EnableEffect(DriftEffects, currentTag);

		if (boosting)
			EnableEffect(BoostEffects, currentTag);


		switch (YawDir) {
			case RotationAxisDirection.Clockwise:
				EnableEffect(ClockwiseYawEffects, currentTag);
				break;
			case RotationAxisDirection.CounterClockwise:
				EnableEffect(CounterClockwiseYawEffects, currentTag);
				break;
			case RotationAxisDirection.None:
				break;
		}

		if (drivingFast) {
			EnableEffect(SpeedEffects);
		}

	}

	private void EnableAllEffects() {
		EnableAllEffects(currentTag);
	}

	private void DisableAllEffects(string tag) {
		DisableEffect(DriftEffects, currentTag);
		DisableEffect(BoostEffects, currentTag);
		DisableEffect(ClockwiseYawEffects, currentTag);
		DisableEffect(CounterClockwiseYawEffects, currentTag);
		DisableEffect(SpeedEffects, currentTag);
	}

	private void DisableAllEffects() {
		EnableAllEffects(currentTag);
	}

	private void OnTriggerEnter(Collider other) {
		// print("trigger enter tag: " + other.gameObject.tag);

		DisableAllEffects();
		currentTag = other.tag;
		if (touchingGround)
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
		if (drifting)
			return;

		drifting = true;

		EnableEffect(DriftEffects, currentTag);

	}

	public void StopDrift() {
		// if (!drifting)
			// return;

		drifting = false;

	}

	public void StartBoost() {

	}

	public void StopBoost() {

	}


	public void StartClockwiseYaw() {
		EnableEffect(ClockwiseYawEffects);
		StopCounterClockwiseYaw();
		YawDir = RotationAxisDirection.Clockwise;
	}

	public void StopClockwiseYaw() {
		DisableEffect(ClockwiseYawEffects);
		YawDir = RotationAxisDirection.None;
	}

	public void StartCounterClockwiseYaw() {
		EnableEffect(ClockwiseYawEffects);
		StopClockwiseYaw();
		YawDir = RotationAxisDirection.CounterClockwise;
	}

	public void StopCounterClockwiseYaw() {
		DisableEffect(CounterClockwiseYawEffects);
		YawDir = RotationAxisDirection.None;
	}

	public void UpdateSpeed(float sqrVelocity) {
		if (sqrVelocity > SpeedEffectThreshold * SpeedEffectThreshold) {
			drivingFast = true;
			EnableEffect(SpeedEffects);
		} else {
			drivingFast = false;
			DisableEffect(SpeedEffects);
		}
	}

}
