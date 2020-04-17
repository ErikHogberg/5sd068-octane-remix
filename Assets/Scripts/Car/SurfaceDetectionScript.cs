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

	[Serializable]
	public struct EnvironmentEffectCollection {

		public string EnvironmentTag; // object tags that enable these effects
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
		DisableEffect(effects.Where(e => e.EnvironmentTag == tag));
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

	private void EnableAllEffects(string tag){
		if (drifting) {
			EnableEffect(DriftEffects, currentTag);
		}
	}

	private void EnableAllEffects(){
		EnableAllEffects(currentTag);
	}

	private void DisableAllEffects(string tag){
		if (drifting) {
			DisableEffect(DriftEffects, currentTag);
		}
	}

	private void DisableAllEffects(){
		EnableAllEffects(currentTag);
	}


	private void OnTriggerEnter(Collider other) {
		// print("trigger enter tag: " + other.gameObject.tag);

		// IDEA: ignore specific tag names, for default effects that are active for all environments


		// TODO: disable current tag effects
		if (drifting) {
			DisableEffect(DriftEffects, currentTag);
		}

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

		drifting = true;

		EnableEffect(DriftEffects, currentTag);

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
		StopCounterClockwiseYaw();
	}

	public void StopClockwiseYaw() {

	}

	public void StartCounterClockwiseYaw() {
		StopClockwiseYaw();
	}

	public void StopCounterClockwiseYaw() {

	}



}
