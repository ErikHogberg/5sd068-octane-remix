using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using UnityEngine.Events;

public class CarParticleHandlerScript : MonoBehaviour, IObserver<bool> {

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
	public class EnvironmentEffectCollection {
		[Tooltip("The tag of the road surface collider that enables the effects, leave field blank to always be active")]
		public string EnvironmentTag; // object tags that enable these effects
		[Tooltip("At what speed the effects start and stop")]
		public float StartVelocity;
		public ParticleSystem[] particles;
		public TrailRenderer[] trails;
		public UnityEvent startEvents;
		public UnityEvent endEvents;
	}

	[Header("Empty tag = always on")]

	public List<EnvironmentEffectCollection> DriftEffects;
	public List<EnvironmentEffectCollection> BoostEffects;
	public List<EnvironmentEffectCollection> BoostWindupEffects;

	public List<EnvironmentEffectCollection> ClockwiseYawEffects;
	public List<EnvironmentEffectCollection> CounterClockwiseYawEffects;

	public List<EnvironmentEffectCollection> AlwaysOnEffects;

	private IEnumerable<EnvironmentEffectCollection> AllEffects =>
		AlwaysOnEffects
			.Concat(DriftEffects)
			.Concat(BoostEffects)
			.Concat(BoostWindupEffects)
			.Concat(ClockwiseYawEffects)
			.Concat(CounterClockwiseYawEffects)
		;

	private string currentTag = ""; // NOTE: only latest environment type touched have their effects enabled
	private bool drifting = false;
	private bool boosting = false;
	private bool isWoundUp = false;
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
	private bool dirty = true;


	private void Awake() {
		GetComponent<SteeringScript>().BoostStartObservers.Add(this);
	}

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
				effect.startEvents.Invoke();
			} else {
				foreach (ParticleSystem particleSystem in effect.particles)
					CustomUtilities.StopEffect(particleSystem);
				foreach (TrailRenderer trail in effect.trails)
					CustomUtilities.StopEffect(trail);
				effect.endEvents.Invoke();
			}
		}
	}

	private void DisableEffect(IEnumerable<EnvironmentEffectCollection> effects, string tag) {
		foreach (EnvironmentEffectCollection effect in effects.Where(e => e.EnvironmentTag == tag || e.EnvironmentTag == "")) {
			foreach (ParticleSystem particleSystem in effect.particles)
				CustomUtilities.StopEffect(particleSystem);
			foreach (TrailRenderer trail in effect.trails)
				CustomUtilities.StopEffect(trail);
			effect.endEvents.Invoke();
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

		if (isWoundUp)
			EnableEffect(BoostWindupEffects, tag);

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

		EnableEffect(AlwaysOnEffects, tag);

	}

	private void EnableAllEffects() {
		EnableAllEffects(currentTag);
	}

	private void DisableAllEffects(string tag) {
		DisableEffect(DriftEffects, currentTag);
		DisableEffect(BoostEffects, currentTag);
		DisableEffect(BoostWindupEffects, currentTag);
		DisableEffect(ClockwiseYawEffects, currentTag);
		DisableEffect(CounterClockwiseYawEffects, currentTag);
		DisableEffect(AlwaysOnEffects, currentTag);
	}

	public void DisableAllEffects() {
		DisableAllEffects(currentTag);
	}


	private void ClearEffects(EnvironmentEffectCollection effects) {
		foreach (var item in effects.particles)
			item.Clear();
		foreach (var item in effects.trails)
			item.Clear();
	}

	private void ClearEffects(IEnumerable<EnvironmentEffectCollection> effects) {
		foreach (var item in effects)
			ClearEffects(item);
	}

	public void ClearAllEffects() {
		ClearEffects(AllEffects);
	}

	public void UpdateSpeed(float sqrVelocity) {
		if (currentSqrVelocity == sqrVelocity)
			return;

		// IDEA: use its own rigidbody reference, update itself instead, only call action changes from other script instead of every frame

		dirty = true;
		currentSqrVelocity = sqrVelocity;
	}

	public void StartTouchingGround() {
		if (touchingGround)
			return;

		StopClockwiseYaw();
		StopCounterClockwiseYaw();

		dirty = true;
		touchingGround = true;
	}

	public void StopTouchingGround() {
		if (!touchingGround)
			return;

		dirty = true;
		touchingGround = false;
	}

	public void SetTouchingGround(bool value) {
		if (value)
			StartTouchingGround();
		else
			StopTouchingGround();
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

	public void StartDrift() {
		if (drifting)
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

	public void StartBoost(bool isWoundUp) {
		if (boosting && isWoundUp == this.isWoundUp)
			return;

		dirty = true;
		boosting = true;
		this.isWoundUp = isWoundUp;
		// EnableEffect(BoostEffects, currentTag);
	}

	public void StopBoost() {
		if (!boosting)
			return;

		dirty = true;
		boosting = false;
		isWoundUp = false;
		DisableEffect(BoostEffects, currentTag);
		DisableEffect(BoostWindupEffects, currentTag);
	}

	public void StopClockwiseYaw() {
		DisableEffect(ClockwiseYawEffects, currentTag);
		YawDir = RotationAxisDirection.None;
	}

	public void StopCounterClockwiseYaw() {
		DisableEffect(CounterClockwiseYawEffects, currentTag);
		YawDir = RotationAxisDirection.None;
	}

	public void StartClockwiseYaw() {
		StopCounterClockwiseYaw();
		YawDir = RotationAxisDirection.Clockwise;
	}

	public void StartCounterClockwiseYaw() {
		StopClockwiseYaw();
		YawDir = RotationAxisDirection.CounterClockwise;
	}

	public void Notify(bool carIsInvulnerable) {
		StartBoost(carIsInvulnerable);
	}

}
