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
		[Tooltip("The tag of the road surface collider that enables the effects, leave field blank to always be active")]
		public string EnvironmentTag; // object tags that enable these effects
		[Tooltip("At what speed the effects start and stop")]
		public float StartVelocity;
		public List<ParticleSystem> particles;
		public List<TrailRenderer> trails;
	}

	[Header("Empty tag = always on")]

	public List<EnvironmentEffectCollection> DriftEffects;
	public List<EnvironmentEffectCollection> BoostEffects;


	public List<EnvironmentEffectCollection> ClockwiseYawEffects;
	public List<EnvironmentEffectCollection> CounterClockwiseYawEffects;


	public List<EnvironmentEffectCollection> AlwaysOnEffects;

	private string currentTag = ""; // NOTE: only latest environment type touched have their effects enabled
	private bool drifting = false;
	private bool boosting = false;
	private bool touchingGround = false;
	// private bool drivingFast = false;

	private float currentSqrVelocity = 0f;

	private RotationAxisDirection yawDir = RotationAxisDirection.None;
	private RotationAxisDirection YawDir {
		get { return yawDir; }
		set {
			if (value == yawDir)
				return;

			dirty = true;
			yawDir = value;
		}
	}

	// true if data has changed, and effects should be updated
	public bool dirty = true;


	// TODO: check that performance impact is not awful
	private void EnableEffect(IEnumerable<EnvironmentEffectCollection> effects, string tag) {
		foreach (EnvironmentEffectCollection effect in effects.Where(e =>
			e.EnvironmentTag == tag
			|| e.EnvironmentTag == ""
		)) {
			if (effect.StartVelocity * effect.StartVelocity <= currentSqrVelocity) {
				foreach (ParticleSystem particleSystem in effect.particles)
					CustomUtilities.StartEffect(particleSystem);
				foreach (TrailRenderer trail in effect.trails)
					CustomUtilities.StartEffect(trail);
			} else {
				foreach (ParticleSystem particleSystem in effect.particles)
					CustomUtilities.StopEffect(particleSystem);
				foreach (TrailRenderer trail in effect.trails)
					CustomUtilities.StopEffect(trail);
			}
		}
	}

	private void DisableEffect(IEnumerable<EnvironmentEffectCollection> effects, string tag) {
		foreach (EnvironmentEffectCollection effect in effects.Where(e => e.EnvironmentTag == tag || e.EnvironmentTag == "")) {
			foreach (ParticleSystem particleSystem in effect.particles)
				CustomUtilities.StopEffect(particleSystem);
			foreach (TrailRenderer trail in effect.trails)
				CustomUtilities.StopEffect(trail);
		}
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
				// DisableEffect(CounterClockwiseYawEffects, tag);
				break;
			case RotationAxisDirection.CounterClockwise:
				EnableEffect(CounterClockwiseYawEffects, tag);
				// DisableEffect(ClockwiseYawEffects, tag);
				break;
			case RotationAxisDirection.None:
				break;
		}

		EnableEffect(AlwaysOnEffects, tag);

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

	public void UpdateEffects(float sqrVelocity, bool touchingGround) {

		UpdateSpeed(sqrVelocity);
		SetTouchingGround(touchingGround);

		if (!dirty)
			return;

		dirty = false;

		EnableAllEffects();

	}

	private void OnTriggerEnter(Collider other) {
		DisableAllEffects();
		currentTag = other.tag;
		EnableAllEffects();
	}

	public void StartTouchingGround() {
		if (touchingGround)
			return;

		dirty = true;
		touchingGround = true;
		// EnableAllEffects();
	}

	public void StopTouchingGround() {
		if (!touchingGround)
			return;

		dirty = true;
		touchingGround = false;
		// DisableAllEffects();
	}

	public void SetTouchingGround(bool value) {
		if (value)
			StartTouchingGround();
		else
			StopTouchingGround();
	}

	public void StartDrift() {
		if (boosting)
			return;

		dirty = true;
		drifting = true;
		// EnableEffect(DriftEffects, currentTag);
	}

	public void StopDrift() {
		if (!drifting)
			return;

		dirty = true;
		drifting = false;
		DisableEffect(DriftEffects, currentTag);
	}

	public void StartBoost() {
		if (boosting)
			return;

		dirty = true;
		boosting = true;
		// EnableEffect(BoostEffects, currentTag);
	}

	public void StopBoost() {
		if (!boosting)
			return;

		dirty = true;
		boosting = false;
		DisableEffect(BoostEffects, currentTag);
	}


	public void StartClockwiseYaw() {
		// EnableEffect(ClockwiseYawEffects, currentTag);
		StopCounterClockwiseYaw();
		YawDir = RotationAxisDirection.Clockwise;
	}

	public void StopClockwiseYaw() {
		DisableEffect(ClockwiseYawEffects, currentTag);
		YawDir = RotationAxisDirection.None;
	}

	public void StartCounterClockwiseYaw() {
		// EnableEffect(CounterClockwiseYawEffects, currentTag);
		StopClockwiseYaw();
		YawDir = RotationAxisDirection.CounterClockwise;
	}

	public void StopCounterClockwiseYaw() {
		DisableEffect(CounterClockwiseYawEffects, currentTag);
		YawDir = RotationAxisDirection.None;
	}

	public void UpdateSpeed(float sqrVelocity) {

		if (currentSqrVelocity == sqrVelocity)
			return;

		dirty = true;
		currentSqrVelocity = sqrVelocity;

	}

}
