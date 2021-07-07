﻿
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Assets.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

using static UnityEngine.InputSystem.InputAction;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class SteeringScript : MonoBehaviour {

	public enum BoostSkill {
		None,
		Invulnerability,
		SloMo
	}

	public static bool EnableProfileChange = true;

	// TODO: efficient way to store and share replays
	// IDEA: option to dynamically speed up or slow down replays depending on distance to active player car, keeping the ghost close to the player regardless of how far ahead it actually is
	private class GhostData {
		public Vector3 Position;
		public Quaternion Rotation;
		public float TimeStamp;

		// TODO: toggle effects

		public GhostData(Vector3 pos, Quaternion rot, float time) {
			Position = pos;
			Rotation = rot;
			TimeStamp = time;
		}

		public GhostData(Transform transform, float time) {
			Position = transform.position;
			Rotation = transform.rotation;
			TimeStamp = time;
		}
	}

	[Serializable]
	public class SpeedProfile {

		public string ProfileName;

		#region Steering fields
		[Header("Steering")]

		[Tooltip("Max steering angle")]
		[Range(0, 90)]
		public float SteeringMax = 45f;
		[Tooltip("Adapts the joystick tilt before changing the steering angle")]
		public AnimationCurve SteeringCurve;

		[Space]
		[Tooltip("Reduces the max steering angle as the car speeds up")]
		public bool EnableNarrowing = true;
		[Tooltip("Reduces the max steering angle as the car speeds up, reaching its narrowest angle at this speed")]
		[Min(0)]
		public float MaxNarrowingSpeed = 100;

		[Range(0, 1)]
		public float MaxNarrowingAmount = 0.05f;
		[Tooltip("Reduces the max steering angle as the car speeds up, the angle narrowing at the rate set on this curve, 1.0 on the X axis is the max narrowing speed, 1.0 on the Y axis is the normal max steering angle")]
		public AnimationCurve SteeringNarrowingCurve;

		[Space]
		[Tooltip("How much the rigidbody rotates with the steering amount to aid in turning")]
		public float SteeringRotationHelp = 0;
		[Tooltip("How much the rigidbody is pushed sidewats with the steering amount to aid in horizontal movement/strafing")]
		public float SteeringStrafeHelp = 0;
		[Tooltip("Which force mode the strafe help uses to push the car sideways")]
		public ForceMode SteeringStrafeMode = ForceMode.VelocityChange;

		#endregion

		#region Gas and brake fields
		[Header("Gas")]
		public float GasSpeed = 100f;
		public AnimationCurve GasPedalCurve;

		[Header("Brakes")]
		[Tooltip("How much brake torque is applied to each wheel collider")]
		public float BrakeForce = 100f;
		public AnimationCurve BrakePedalCurve;

		[Space]

		[Tooltip("If the velocity of the rigidbody itself should be braked/dampened when braking")]
		public bool DampenRigidBody = true;
		[Tooltip("Velocity that will be lost per second while braking")]
		public float BrakeDampeningAmount = 2000f;

		#endregion

		#region Downward force fields
		[Header("Downward force")]
		public bool EnableDownwardForce = true;
		[Tooltip("Downward force at max force")]
		public float DownwardForce = 10f;
		[Tooltip("At what speed downward force switches off")]
		public float MinDownwardsForceSpeed = 1.0f;
		[Tooltip("At what speed downward force reaches max force")]
		public float MaxDownwardsForceSpeed = 10.0f;
		[Tooltip("How the downwards force (percent of max force) is mapped to the velocities between the min and max speed. X values below 0 and above 1 are not used, the curve evaluation is skipped/bypassed for those value ranges.")]
		public AnimationCurve DownwardsForceSpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);
		[Tooltip("Which force mode is used to apply the downwards force to the car rigidbody")]
		public ForceMode DownwardForceMode = ForceMode.Acceleration;
		[Tooltip("Whether or not the force direction should be relative to the car orientation instead of world.")]
		public bool UseRelativeDownwardForce = true;

		#endregion

		#region Boost fields
		[Header("Boost")]
		public float BoostSpeed = 100f;

		#endregion

		#region Velocity cap fields
		[Header("Velocity cap")]
		public bool CapVelocity = true;
		public bool DisableCapInAir = true;
		public float VelocityCap = 20f;
		public float BoostVelocityCap = 30f;
		[Min(0)]
		public float VelocityCapCorrectionSpeed = 1f;

		[Tooltip("Immediate velocity cap, does not use correction speed, car never goes above this speed (except for the speed gained last frame)")]
		public float AbsoluteVelocityCap = 200f;
		#endregion

		[Header("Wheel friction")]
		[Tooltip("How much friction the front wheels have for forward traction, how effective gas and brake is")]
		public float FrontWheelForwardStiffness = 1;
		[Tooltip("How much friction the front wheels have for sideways traction, how hard the car can turn without drifting")]
		public float FrontWheelSidewaysStiffness = 1;
		[Tooltip("How much friction the rear wheels have for forward traction, how effective gas and brake is, higher rear forward value than front forward value potentially makes the car more stable when braking")]
		public float RearWheelForwardStiffness = 1;
		[Tooltip("How much friction the front wheels have for forward traction, how hard the car can turn without drifting, higher rear sideways value than front sideways value potentially makes the car more stable while steering, but harder to keep drifting")]
		public float RearWheelSidewaysStiffness = 1;

	}

	public static SteeringScript MainInstance;
	private static bool freezeNextFrame = false;

	private Vector3 emergencyResetPos = Vector3.zero;
	private Quaternion emergencyResetRot = Quaternion.identity;

	public int CurrentProfileIndex = 0;
	public SpeedProfile CurrentProfile => SpeedProfiles[CurrentProfileIndex];
	public List<SpeedProfile> SpeedProfiles;
	[Space]
	public Color ProfileChangeColor = Color.green;

	[Header("In-air controls")]
	public bool LeftStickRotationWhenInAir = false;
	public bool ZeroAngularVelocityOnLanding = false;
	public bool ZeroAngularVelocityOnAir = false;
	private bool ignoreNextOnAirZeroing = false;
	public float YawSpeed = 200f;
	public AnimationCurve YawInputCurve;
	public float PitchSpeed = 100f;
	public AnimationCurve PitchInputCurve;

	#region Boost fields
	[Header("Boost")]
	private float boostAmount = 1;
	private bool boosting = false;
	private bool BoostNotEmpty => boostAmount > 0;

	//Limits boost based on temperature. 0.0 means no limitation, 1.0 means the maximum limitation is in place
	private float boostLimiter = 0.0f;
	//How many percent can the boost resource max be reduced by due to temperature? -30% or -50% or maybe -70%?
	private float boostLimitMax = 0.5f;

	//Returns between 0.0 and -boostLimitMax
	private float BoostLimit => (0.0f - (boostLimitMax * boostLimiter));
	public void SetBoostLimit(float limit) { boostLimiter = limit; }


	[Tooltip("How much % of the boost tank is emptied per second when boosting")]
	[Range(0, 1)]
	public float BoostConsumptionRate = .4f;

	[Tooltip("How much % of the boost tank is added per second when not boosting")]
	[Range(0, 1)]
	public float BoostFillRate = .25f;

	[Tooltip("How much boost tank % is required to start boosting")]
	[Range(0, 1)]
	public float MinBoostLevel = .2f;

	[Tooltip("If the boost direction is affected by steering direction")]
	public bool BoostAffectedBySteering = false;

	[Tooltip("How many degrees the boost direction is turned at max steering")]
	[Range(-90, 90)]
	public float BoostMaxSteering = 45.0f;

	// IDEA: option for adding angular velocity on boost while steering


	public BoostSkill BoostWindupSkill = BoostSkill.None;

	[Tooltip("How long time the car has to boost to become invulnerable")]
	[Min(0)]
	public float BoostWindup = 1f;

	[Range(0, 1)]
	public float BoostSloMoTimescale = 1f;

	private float boostWindupTimer = 0f;

	public float BoostWindupProgress => Mathf.Clamp(boostWindupTimer / BoostWindup, 0, 1);
	public bool BoostWindupReady => boosting && boostWindupTimer >= BoostWindup;
	public bool IsInvulnerable => BoostWindupSkill == BoostSkill.Invulnerability && BoostWindupReady;
	public bool IsInSloMo => BoostWindupSkill == BoostSkill.SloMo && BoostWindupReady;
	#endregion

	#region Drifting fields

	[Header("Drifting")]

	[Tooltip("Prerequisite delta angle at which drifting starts")]
	[Range(0, 180)]
	public float DriftStartAngle = 30f;

	[Tooltip("At which delta angle drifting always stops")]
	[Range(0, 180)]
	public float DriftStopAngle = 30f;

	[Tooltip("Prerequisite velocity at which drifting starts")]
	public float DriftStartVelocity = 1f;
	[Tooltip("At which velocity drifting always stops")]
	public float DriftStopVelocity = .5f;

	[Tooltip("How much extra the velocity will be altered to point towards the direction of the car, while gassing/throttling, while drifting, in degrees per second")]
	[Range(0, 180)]
	public float DriftCorrectionSpeed = 20f;

	[Tooltip("How much the magnitude of the velocity is allowed to be reduced when correcting velocity direction by throttling while drifting")]
	public float DriftSpeedReductionWhenCorrecting = 0f;
	private bool drifting = false;
	#endregion

	#region Rumble fields
	[Header("Rumble")]

	public static bool EnableRumble = true;

	public float EngineRumbleHiHzMaxVelocity;
	public AnimationCurve EngineRumbleHiHzCurve;
	public float EngineRumbleLoHzMaxVelocity;
	public AnimationCurve EngineRumbleLoHzCurve;

	// [Tooltip("Distribution of high vs low Hz rumble motor amount, more high hz => buzzing, more low hz => shaking")]
	// [Range(0, 1)]
	// public float EngineRumbleHiLoHzRatio = .5f;
	// [Tooltip("How much the distribution is multiplied when applied, max 200%, meaning at 50% distrition both motors are 100% at max amount ")]
	// [Range(0, 2)]
	// public float EngineRumbleAmount = .5f;

	// public Vector2 EngineRumbleSpeedMinMax;
	// public AnimationCurve EngineRumbleCurve;

	[Tooltip("Distribution of high vs low Hz rumble motor amount, more high hz => buzzing, more low hz => shaking")]
	[Range(0, 1)]
	public float BoostRumbleHiLoHzRatio = .5f;
	[Tooltip("How much the distribution is multiplied when applied, max 200%, meaning at 50% distrition both motors are 100% at max amount ")]
	[Range(0, 2)]
	public float BoostRumbleAmount = .5f;

	[Tooltip("Distribution of high vs low Hz rumble motor amount, more high hz => buzzing, more low hz => shaking")]
	[Range(0, 1)]
	public float DriftRumbleHiLoHzRatio = .5f;
	[Tooltip("How much the distribution is multiplied when applied, max 200%, meaning at 50% distrition both motors are 100% at max amount ")]
	[Range(0, 2)]
	public float DriftRumbleAmount = .5f;
	#endregion

	[Header("Score")]
	public float DriftScorePerSec = 200f;
	[Tooltip("How long you need to drift continously to gain drift score")]
	public float DriftTimeThreshold = .1f;
	[Space]
	public float BoostScorePerSec = 200f;
	[Tooltip("How long you need to Boost continously to gain boost score")]
	public float BoostTimeThreshold = .1f;
	[Space]
	public float AirTimeScorePerSec = 200f;
	[Tooltip("How long you need to stay in air to gain air time score")]
	public float AirTimeTimeThreshold = .1f;
	[Space]
	public long DestructionScore = 1000;

	[Header("Misc.")]
	public bool EnableCheatMitigation = true;

	[Space]
	[Tooltip("How many physics/fixed update frames that should be skipped between UI updates")]
	public int UIUpdateInterval = 3;

	[Space]
	public bool OverrideGravity = false;
	public float GravityOverride = 1;

	[Space]
	public bool InAirStabilization = false;
	public float InAirStabilizationAmount = 1f;
	[Space]

	public bool AllowYawOnGround = false;

	// [Tooltip("If the car starts right in front of the goal post. Makes the first time crossing the finish line not count as a lap")]
	// public bool StartBeforeGoalPost = false;
	// private bool startBeforeGoalPost = false;

	#region object refs and input bindings

	[Header("Required objects")]

	[Tooltip("Front wheels")]
	public List<WheelCollider> FrontWheelColliders;
	public List<GameObject> FrontWheelModels;
	[Tooltip("Rear wheels")]
	public List<WheelCollider> RearWheelColliders;
	public List<GameObject> RearWheelModels;

	private IEnumerable<WheelCollider> allWheelColliders;
	private IEnumerable<GameObject> allWheelModels;

	#endregion


	private float[] wheelRotationBuffers;

	private Rigidbody rb;
	public Vector3 Velocity => rb.velocity;
	private float springInit;

	// TODO: try converting into observers
	private CarParticleHandlerScript effects;
	private CarSoundHandler carSound;

	[HideInInspector]
	public List<IObserver<bool>> BoostStartObservers = new List<IObserver<bool>>();
	[HideInInspector]
	public List<IObserver<int>> LapCompletedObservers = new List<IObserver<int>>();
	[HideInInspector]
	public List<IObserver<Camera>> ResetObservers = new List<IObserver<Camera>>();

	private float lowHzRumble = 0;
	private float highHzRumble = 0;

	private int lapsCompleted = 0;
	public int LapsCompleted {
		get { return lapsCompleted; }
		set {
			// if (startBeforeGoalPost) {
			// 	startBeforeGoalPost = false;
			// 	print("lap invalidated");
			// 	return;
			// }

			lapsCompleted = value;
			foreach (var item in LapCompletedObservers)
				item.Notify(lapsCompleted);
		}
	}

	[HideInInspector]
	public CenterlineProgressScript progressScript;

	public List<Camera> Cameras = new List<Camera>();
	// public Camera CurrentCamera = null;
	public int CurrentCameraIndex = 0;
	public Camera CurrentCamera => Cameras != null && Cameras.Count > 0 ? Cameras[CurrentCameraIndex] : null;


	void Start() {

		if (OverrideGravity) {
			Physics.gravity = Physics.gravity.normalized * GravityOverride;
		}

		emergencyResetPos = transform.position;
		emergencyResetRot = transform.rotation;

		allWheelColliders = FrontWheelColliders.Concat(RearWheelColliders);
		allWheelModels = FrontWheelModels.Concat(RearWheelModels);

		springInit = FrontWheelColliders[0].suspensionSpring.spring;

		wheelRotationBuffers = new float[FrontWheelColliders.Count + RearWheelColliders.Count];

		// SetProfile(CurrentProfileIndex);
		UpdateWheelFriction();

		// TODO: check if the below comments are still relevant, or were related to the now removed segment system based cheat mitigation system

		// TODO: teleport car to start segment reset spot
		// FIXME: reset triggers penalty popup, prematurely starting game
		// IDEA: reset fn with option to disable penalty
		// LevelPieceSuperClass.ResetToStart();

		// FIXME: move car before freeze
		// RemixEditorGoalPost.MoveCarToStart();

		progressScript = GetComponent<CenterlineProgressScript>();

	}

	void Awake() {
		rb = GetComponent<Rigidbody>();
		carSound = GetComponent<CarSoundHandler>();

		effects = GetComponent<CarParticleHandlerScript>();
		LevelPieceSuperClass.ClearCurrentSegment();

		// RemixEditorGoalPost.MoveCarToStart();
		// startBeforeGoalPost = false;
		// if (RemixEditorGoalPost.StartSpot && RemixEditorGoalPost.FinishSpot)
		// 	startBeforeGoalPost = RemixEditorGoalPost.StartSpot == RemixEditorGoalPost.FinishSpot;
	}

	void OnEnable() {
		InputSystem.ResumeHaptics();

		MainInstance = this;
		// LevelPieceSuperClass.ClearCurrentSegment();

	}

	void OnDisable() {
		InputSystem.PauseHaptics();
	}

	private void OnDestroy() {
		InputSystem.ResetHaptics();
	}

	private bool touchingGround = true;
	float airTimeTimer = 0f;

	void FixedUpdate() {

		if (freezeNextFrame) {
			Freeze();
			return;
		}

		float dt = Time.deltaTime;
		float unscaledDt = Time.unscaledDeltaTime;
		float sqrVelocity = rb.velocity.sqrMagnitude;

		lowHzRumble = 0;
		highHzRumble = 0;

		bool wasTouchingGround = touchingGround;
		touchingGround = CheckIfTouchingGround();

		if (touchingGround) {
			if (!wasTouchingGround) {
				// First frame on ground
				if (ZeroAngularVelocityOnLanding) {
					rb.angularVelocity = Vector3.zero;
				}

				if (airTimeTimer > AirTimeTimeThreshold) {
					// IDEA: make async call?
					// TODO: dont get score for falling after resetting
					// TODO: dont get score for air time while falling off track, discard score on next ground touch if car was reset since last ground touch?
					ScoreBoard boardOne = ScoreManager.Board(0);
					if (boardOne != null) {
						boardOne.AddSkill(ScoreSkill.AIRTIME, (int)(airTimeTimer * AirTimeScorePerSec));
					}
				}

				airTimeTimer = 0f;
				ignoreNextOnAirZeroing = false;

			}
		} else {
			airTimeTimer += Time.deltaTime;

			if (wasTouchingGround) {
				// First frame in air
				if (ZeroAngularVelocityOnAir && !ignoreNextOnAirZeroing) {
					rb.angularVelocity = Vector3.zero;
				}
				ignoreNextOnAirZeroing = false;
			}
		}

		ApplyDownwardForce(dt, sqrVelocity);

		Steer(dt);
		Gas(dt);

		Boost(dt, unscaledDt);

		if (touchingGround && CurrentProfile.SteeringStrafeHelp > float.Epsilon) {
			// Strafe help
			rb.AddRelativeForce(Vector3.right * CurrentProfile.SteeringStrafeHelp * steeringBuffer, CurrentProfile.SteeringStrafeMode);
		}

		Brake(dt);

		if (AllowYawOnGround || !touchingGround) {
			Yaw(dt);
			Pitch(dt);
		}

		if (InAirStabilization && !touchingGround) {
			// rb.transform.up = Vector3.RotateTowards(rb.transform.up, Vector3.up, InAirStabilizationAmount, 0);
			var rot = Quaternion.FromToRotation(rb.transform.up, Vector3.up);
			// rb.AddTorque(new Vector3(rot.x, rot.y, rot.z) * InAirStabilizationAmount);
			//  rb.transform.up
			rb.AddRelativeTorque(rot * Vector3.up * InAirStabilizationAmount);
		}

		ApplyVelocityCap(dt);
		ApplyAnimations();

		Drift(dt);

		if (effects)
			effects.UpdateEffects(sqrVelocity, touchingGround);


		// To keep the velocity needle moving smoothly
		RefreshUI();

		// touchedGroundLastTick = false;

		Rumble();

		// IDEA: dont check every frame, define a cheat check interval
		if (progressScript) {
			if (progressScript.QueryProgress(out bool lapCompleted)) {
				if (lapCompleted)
					LapsCompleted++;
			} else {
				ResetTransform();
				CallResetEvents();
			}
		}


	}

	// To avoid jittery number updates on the UI
	int updateCount = 0;
	void LateUpdate() {
		if (updateCount >= UIUpdateInterval) {
			UpdateUI();
			updateCount = 0;
		}

		updateCount++;
	}

	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("FinishLine")) {
			bool finishedLap = progressScript.ValidateFinishCrossing(out bool shouldReset);
			
			if (finishedLap)
				lapsCompleted++;

			if (shouldReset) {
				ResetTransform();
				CallResetEvents();
			}
		}

	}

	private bool CheckIfTouchingGround() {
		// NOTE: also sets sound parameter
		foreach (WheelCollider wheelCollider in allWheelColliders) {
			if (wheelCollider.isGrounded) {
				carSound.RecieveGroundedData(true);
				return true;
			}
		}
		carSound.RecieveGroundedData(false);
		return false;
	}

	private void RefreshUI() {
		GasNeedleUIScript.Refresh();
	}

	private void UpdateUI() {
		// float gasAmount = GasSpeed * gasBuffer;

		float percentage = rb.velocity.sqrMagnitude / (CurrentProfile.VelocityCap * CurrentProfile.VelocityCap);
		float kmph = rb.velocity.magnitude * 3.6f;
		carSound.RecieveVelocityData(percentage);

		if (boosting) {
			if (percentage >= 1f)
				percentage = Random.Range(1.05f, 1.1f);
			GasNeedleUIScript.SetBarPercentage(percentage, true);
		} else {
			percentage = Mathf.Clamp(percentage, 0.0f, 1.05f);
			GasNeedleUIScript.SetBarPercentage(percentage, false);
		}
		GasNeedleUIScript.SetKMPH(kmph);
	}

	private void ApplyVelocityCap(float dt) {
		if (CurrentProfile.CapVelocity) {
			if (boosting) {
				if (rb.velocity.sqrMagnitude > CurrentProfile.BoostVelocityCap * CurrentProfile.BoostVelocityCap) {
					// rb.velocity = Vector3.Normalize(rb.velocity) * BoostVelocityCap;
					rb.velocity = Vector3.MoveTowards(
						rb.velocity,
						Vector3.Normalize(rb.velocity) * CurrentProfile.BoostVelocityCap,
						CurrentProfile.VelocityCapCorrectionSpeed * dt
					);

				}

			} else if (!CurrentProfile.DisableCapInAir || touchingGround) {
				if (rb.velocity.sqrMagnitude > CurrentProfile.VelocityCap * CurrentProfile.VelocityCap) {
					// rb.velocity = Vector3.Normalize(rb.velocity) * VelocityCap;
					rb.velocity = Vector3.MoveTowards(
						rb.velocity,
						Vector3.Normalize(rb.velocity) * CurrentProfile.VelocityCap,
						CurrentProfile.VelocityCapCorrectionSpeed * dt
					);
				}
			}

			if (rb.velocity.sqrMagnitude > CurrentProfile.AbsoluteVelocityCap * CurrentProfile.AbsoluteVelocityCap)
				rb.velocity = Vector3.Normalize(rb.velocity) * CurrentProfile.AbsoluteVelocityCap;

		}
	}

	#region Drifting

	float driftTimer = 0f;

	private void StartDrift() { // NOTE: called every frame while drifting, not just on drift status change
		if (drifting == false) {
			if ((rb.velocity.magnitude * 3.6f) > 100f)
				SoundManager.PlaySound("drift_continuous");
		}

		driftTimer += Time.deltaTime;

		drifting = true;
		effects?.StartDrift();
	}

	private void StopDrift() { // NOTE: called every frame while not drifting, not just on drift status change
		if (drifting == true) {
			SoundManager.StopLooping("drift_continuous", false);

			if (driftTimer > DriftTimeThreshold) {
				// IDEA: make async call?
				ScoreBoard boardOne = ScoreManager.Board(0);
				if (boardOne != null) {
					boardOne.AddSkill(ScoreSkill.DRIFT, (int)(DriftScorePerSec * driftTimer));
				}
			}
			driftTimer = 0f;
		}
		drifting = false;
		effects?.StopDrift();
	}

	private float GetDriftAngle() {
		Vector3 carDir = transform.forward;
		Vector3 velocity = rb.velocity;

		velocity = Vector3.ProjectOnPlane(velocity, transform.up);

		float angle = Vector3.SignedAngle(carDir, velocity, transform.up);

		return angle;
	}

	private void CheckDrift() {
		Vector3 carDir = transform.forward;
		Vector3 velocity = rb.velocity;

		velocity = Vector3.ProjectOnPlane(velocity, transform.up);

		float angle = Vector3.SignedAngle(carDir, velocity, transform.up);
		float absAngle = Mathf.Abs(angle);

		if (touchingGround
			&& absAngle > DriftStartAngle
			&& velocity.sqrMagnitude > DriftStartVelocity * DriftStartVelocity
		) {
			// lowHzRumble += (1f - DriftRumbleHiLoHzRatio) * DriftRumbleAmount;
			float driftLoRumble = (1f - DriftRumbleHiLoHzRatio) * DriftRumbleAmount;
			if (lowHzRumble < driftLoRumble) {
				lowHzRumble = driftLoRumble;
			}
			// highHzRumble += DriftRumbleHiLoHzRatio * DriftRumbleAmount;
			float driftHiRumble = DriftRumbleHiLoHzRatio * DriftRumbleAmount;
			if (highHzRumble < driftHiRumble) {
				highHzRumble = driftHiRumble;
			}

			StartDrift();
		}

		if (!touchingGround
			|| absAngle < DriftStopAngle
			|| velocity.sqrMagnitude < DriftStopVelocity * DriftStopVelocity
		) {
			StopDrift();
		}

	}

	private void Drift(float dt) {
		CheckDrift();

		if (!drifting)
			return;

		// IDEA: increase boost recharge rate while drifting?

		rb.velocity = Vector3.RotateTowards(rb.velocity, transform.forward, gasBuffer * DriftCorrectionSpeed * Mathf.Deg2Rad * dt, DriftSpeedReductionWhenCorrecting);

	}

	#endregion

	#region Input callbacks

	// input buffers
	float steeringBuffer = 0f;
	float gasBuffer = 0f;
	float lastAppliedGasValue = 0f;
	float brakeBuffer = 0f;

	float yawBuffer = 0f;
	float pitchBuffer = 0f;
	float leftPitchBuffer = 0f;

	private bool leftStickRotationEnabled = false;

	// Left stick X, kbd A and D
	public void OnSteer(InputValue value) {
		SetSteering(value.Get<float>());
	}

	// Right trigger, kbd W
	public void OnGas(InputValue value) {
		SetGas(value.Get<float>());
	}

	// Left trigger, kbd S
	public void OnBrake(InputValue value) {
		SetBraking(value.Get<float>());
	}

	// A button, kbd spacebar
	public void OnBoost(InputValue value) {
		if (value.Get<float>() > 0)
			StartBoost();
		else
			StopBoost();
	}

	public void OnHandbrake(InputValue value) {
		// if (value.Get<float>() > 0f)

	}

	// Select button, kbd backspace
	public void OnReset(InputValue value) {
		if (value.Get<float>() > 0f)
			ResetIfAllowed();

	}

	// Right stick X, kbd left and right arrows
	public void OnYaw(InputValue value) {
		SetYaw(value.Get<float>());
	}

	// Right stick Y, kbd up and down arrows
	public void OnPitch(InputValue value) {
		SetPitch(value.Get<float>());
	}

	public void OnChangeCamera(InputValue value) {
		if (value.Get<float>() > 0f)
			NextCamera();
	}

	#endregion

	#region Steering

	private void Steer(float dt) {
		ApplySteeringTorque();
	}

	private void ApplySteeringTorque() {

		float sqrVelocity = rb.velocity.sqrMagnitude;


		// narrow steering angle as speed increases
		float sqrMaxNarrowingSpeed = CurrentProfile.MaxNarrowingSpeed * CurrentProfile.MaxNarrowingSpeed;

		float narrowing = 1f;

		if (!CurrentProfile.EnableNarrowing) {
		} else {
			float speedProgress = 1f;

			// if (sqrVelocity < sqrMaxNarrowingSpeed)
			speedProgress -= sqrVelocity / sqrMaxNarrowingSpeed;

			narrowing = CurrentProfile.SteeringNarrowingCurve.Evaluate(speedProgress);
			narrowing = CurrentProfile.MaxNarrowingAmount + narrowing * (1f - CurrentProfile.MaxNarrowingAmount);
		}

		float steeringAmount = steeringBuffer * CurrentProfile.SteeringMax * narrowing;
		foreach (WheelCollider FrontWheelCollider in FrontWheelColliders)
			FrontWheelCollider.steerAngle = steeringAmount;

	}

	private void SetSteering(float input) {
		if (leftStickRotationEnabled)
			return;

		steeringBuffer = CurrentProfile.SteeringCurve.EvaluateMirrored(input);

	}

	public float GetSteering() { return steeringBuffer; }

	#endregion

	#region Gas

	private void Gas(float dt) {
		if (brakeBuffer == 0f || gasBuffer < lastAppliedGasValue) {
			ApplyGasTorque();
		}
	}


	private void ApplyGasTorque() {

		// NOTE: torque is distributed, dividing gas speed by number of wheels with traction
		// NOTE: this keeps gas settings the same, regardless of traction mode

		float gasAmount = CurrentProfile.GasSpeed * gasBuffer;

		foreach (WheelCollider wheelCollider in allWheelColliders)
			wheelCollider.motorTorque = CurrentProfile.GasSpeed * gasBuffer;

		lastAppliedGasValue = gasBuffer;
	}

	private void SetGas(float input) {
		// Debug.Log("gas car: " + gameObject.name);
		// float input = c.ReadValue<float>();
		gasBuffer = CurrentProfile.GasPedalCurve.EvaluateMirrored(input);

	}

	#endregion

	#region Braking
	private void Brake(float dt) {

		float rpm = 0;
		// get largest rpm from wheels
		foreach (var item in allWheelColliders) {
			if (rpm < item.rpm)
				rpm = item.rpm;
		}

		if (rpm > float.Epsilon) {
			// brake if wheels have not stopped
			float brakeAmount = CurrentProfile.BrakeForce * brakeBuffer;

			foreach (WheelCollider wheelCollider in allWheelColliders) {
				wheelCollider.brakeTorque = brakeAmount;
				// wheelCollider.motorTorque = Mathf.MoveTowards(wheelCollider.motorTorque, 0, brakeAmount * MotorBrakeAmount * dt);
			}

			if (CurrentProfile.DampenRigidBody && brakeBuffer > 0) {
				// IDEA: minimum velocity for brake help, to disallow slow fall
				rb.AddForce(-CurrentProfile.BrakeDampeningAmount * brakeBuffer * rb.velocity);
			}
		} else if (brakeBuffer > float.Epsilon) {
			// reverse if wheels have stopped
			foreach (var item in allWheelColliders)
				item.brakeTorque = 0;
			gasBuffer = -1;
		}

	}

	private void SetBraking(float input) {
		// float input = c.ReadValue<float>();
		float pastBrakeBuffer = brakeBuffer;
		brakeBuffer = CurrentProfile.BrakePedalCurve.EvaluateMirrored(input);

		if (brakeBuffer > pastBrakeBuffer) {
			if (brakeBuffer > 0.2f) {
				SoundManager.PlaySound("dry_ice_brake");
			}
		} else {
			SoundManager.StopLooping("dry_ice_brake", false);
		}
	}
	#endregion

	#region Yaw, Pitch

	private void Yaw(float dt) {

		float yawAmount;
		if (leftStickRotationEnabled) {
			yawAmount = YawSpeed * steeringBuffer * dt;
		} else {
			yawAmount = YawSpeed * yawBuffer * dt;
		}

		Yaw(yawAmount, true);
		if (touchingGround) {
			float steeringYawAmount = CurrentProfile.SteeringRotationHelp * steeringBuffer * dt;
			Yaw(steeringYawAmount, false);
		}
	}

	private void Yaw(float yawAmount, bool triggerEffects) {

		// float yawAmount = YawSpeed * yawBuffer * dt;
		rb.rotation *= Quaternion.Euler(0, yawAmount, 0);

		if (!effects)
			return;

		if (!triggerEffects)
			return;

		if (yawAmount > 0) {
			effects.StopCounterClockwiseYaw();
			effects.StartClockwiseYaw();
		} else if (yawAmount < 0) {
			effects.StopClockwiseYaw();
			effects.StartCounterClockwiseYaw();
		} else {
			effects.StopClockwiseYaw();
			effects.StopCounterClockwiseYaw();
		}

	}

	private void Pitch(float dt) {
		float pitchAmount;
		if (leftStickRotationEnabled) {
			pitchAmount = PitchSpeed * leftPitchBuffer * dt;
		} else {
			pitchAmount = PitchSpeed * pitchBuffer * dt;
		}

		rb.rotation *= Quaternion.Euler(pitchAmount, 0, 0);
	}

	private void SetYaw(float input) {
		// float input = c.ReadValue<float>();
		yawBuffer = CurrentProfile.SteeringCurve.EvaluateMirrored(input);
	}
	public float GetYaw() { return yawBuffer; }

	private void SetPitch(float input) {
		// float input = c.ReadValue<float>();
		pitchBuffer = CurrentProfile.SteeringCurve.EvaluateMirrored(input);
	}

	private void SetLeftPitch(CallbackContext c) {
		float input = c.ReadValue<float>();
		leftPitchBuffer = CurrentProfile.SteeringCurve.EvaluateMirrored(input);
	}

	private void EnableLeftStickRotation(CallbackContext _) {
		leftStickRotationEnabled = true;
	}
	private void DisableLeftStickRotation(CallbackContext _) {
		leftStickRotationEnabled = false;
	}


	#endregion

	#region Boost

	float boostTimer = 0f;

	private void Boost(float dt, float unscaledDt) {
		if (!BoostNotEmpty) {
			StopBoost();
		}

		if (!boosting) {
			AddBoost(BoostFillRate * dt);
			return;
		}

		if (BoostWindupSkill != BoostSkill.None && boostWindupTimer < BoostWindup)
			boostWindupTimer += Time.deltaTime;

		boostTimer += Time.deltaTime;

		if (IsInSloMo) {
			Time.timeScale = BoostSloMoTimescale;
		} else {
			Time.timeScale = 1f;
		}

		foreach (var item in BoostStartObservers)
			item.Notify(BoostWindupReady);

		AddBoost(-BoostConsumptionRate * unscaledDt);

		if (BoostNotEmpty) {
			Vector3 boostDir = Vector3.forward;
			if (BoostAffectedBySteering) {
				boostDir = Quaternion.AngleAxis(steeringBuffer * BoostMaxSteering, Vector3.up) * boostDir;
			}
			rb.AddRelativeForce(boostDir * CurrentProfile.BoostSpeed, ForceMode.Acceleration);

			float boostLoRumble = (1f - BoostRumbleHiLoHzRatio) * BoostRumbleAmount;
			if (lowHzRumble < boostLoRumble) {
				lowHzRumble = boostLoRumble;
			}

			float boostHiRumble = BoostRumbleHiLoHzRatio * BoostRumbleAmount;
			if (highHzRumble < boostHiRumble) {
				highHzRumble = boostHiRumble;
			}

		} else {
			StopBoost();
		}

	}

	private void AddBoost(float amount) {
		boostAmount += amount;
		boostAmount = Mathf.Clamp(boostAmount, 0, 1 + BoostLimit);

		BoostBarUIScript.SetBarPercentage((float)boostAmount);
	}

	private void StartBoost() {
		if (StartCountdownScript.IsShown)
			return;

		if (boostAmount < MinBoostLevel)
			return;

		if (!boosting) {
			SoundManager.PlaySound("boost_start");
			SoundManager.PlaySound("boost_continuous");
			//UnityEngine.Debug.Log("Boost sound start");
		}
		boosting = true;

	}

	private void StopBoost() {

		if (boostTimer > BoostTimeThreshold) {
			ScoreBoard boardOne = ScoreManager.Board(0);
			if (boardOne != null) {
				boardOne.AddSkill(ScoreSkill.BOOST, (int)(BoostScorePerSec * boostTimer));
			}
		}
		boostTimer = 0f;

		boostWindupTimer = 0f;

		if (!StartCountdownScript.IsShown)
			Time.timeScale = 1f;

		effects?.StopBoost();

		if (boosting) {
			SoundManager.StopLooping("boost_continuous", false);
			SoundManager.PlaySound("boost_end");
			//UnityEngine.Debug.Log("Boost sound end");
		}

		boosting = false;
	}

	private void StopBoost(CallbackContext _) {
		StopBoost();
	}

	#endregion

	private void ApplyDownwardForce(float dt, float sqrVelocity) {
		float sqrMinSpeed = CurrentProfile.MinDownwardsForceSpeed * CurrentProfile.MinDownwardsForceSpeed;
		if (CurrentProfile.EnableDownwardForce && sqrVelocity > sqrMinSpeed) {
			float sqrMaxSpeed = CurrentProfile.MaxDownwardsForceSpeed * CurrentProfile.MaxDownwardsForceSpeed;
			float speedPercentage = 1;

			if (sqrVelocity < sqrMaxSpeed) {
				speedPercentage = CurrentProfile.DownwardsForceSpeedCurve.Evaluate(sqrVelocity / (CurrentProfile.MaxDownwardsForceSpeed - CurrentProfile.MinDownwardsForceSpeed));
			}

			Vector3 resultForce = Vector3.down * CurrentProfile.DownwardForce * speedPercentage * dt;

			if (CurrentProfile.UseRelativeDownwardForce)
				rb.AddRelativeForce(resultForce, CurrentProfile.DownwardForceMode);
			else
				rb.AddForce(resultForce, CurrentProfile.DownwardForceMode);
		}
	}

	public bool SetProfile(int index, bool sendNotification = true) {
		if (!EnableProfileChange || index < 0 || index >= SpeedProfiles.Count || index == CurrentProfileIndex) {
			return false;
		}

		CurrentProfileIndex = index;
		UpdateWheelFriction();

		if (sendNotification)
			UINotificationSystem.Notify("Speed change to " + CurrentProfile.ProfileName + "!", Color.green, 1.5f);
		// TODO: music change?

		return true;
	}

	public void UpdateWheelFriction() {
		foreach (var wheelCollider in FrontWheelColliders) {
			var forwardFriction = wheelCollider.forwardFriction;
			forwardFriction.stiffness = CurrentProfile.FrontWheelForwardStiffness;
			wheelCollider.forwardFriction = forwardFriction;

			var sidewaysFriction = wheelCollider.sidewaysFriction;
			sidewaysFriction.stiffness = CurrentProfile.FrontWheelSidewaysStiffness;
			wheelCollider.sidewaysFriction = sidewaysFriction;
		}

		foreach (var wheelCollider in RearWheelColliders) {
			var forwardFriction = wheelCollider.forwardFriction;
			forwardFriction.stiffness = CurrentProfile.RearWheelForwardStiffness;
			wheelCollider.forwardFriction = forwardFriction;

			var sidewaysFriction = wheelCollider.sidewaysFriction;
			sidewaysFriction.stiffness = CurrentProfile.RearWheelSidewaysStiffness;
			wheelCollider.sidewaysFriction = sidewaysFriction;
		}
	}

	public void CallResetObservers() {
		foreach (var observer in ResetObservers) {
			if (CurrentCamera) {
				observer.Notify(CurrentCamera);
			} else {
				observer.Notify(Camera.main);
			}
		}
	}

	public void ResetTo(Vector3 pos, Quaternion rot) {
		CallResetObservers();

		effects?.DisableAllEffects();
		effects?.ClearAllEffects();

		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;


		// rb.MovePosition(pos);
		// rb.MoveRotation(rot);
		transform.position = pos;
		transform.rotation = rot;


		StartCountdownScript.StartPenaltyCountdownStatic(1.5f);
	}

	public void CallResetEvents() {
		// CallResetObservers();
		// StartCountdownScript.StartPenaltyCountdownStatic(1f);

		foreach (var item in allWheelColliders) {
			item.motorTorque = 0;
			item.brakeTorque = 0;
		}

		// if (!LevelPieceSuperClass.ResetToCurrentSegment()// && LevelWorldScript.CurrentLevel != null
		// ) {

		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		// if (!progressScript.Reset()) {

		// 	rb.MovePosition(emergencyResetPos);
		// 	rb.MoveRotation(emergencyResetRot);
		// }

		CallResetObservers();
		StartCountdownScript.StartPenaltyCountdownStatic(1.5f);


		//For some reason, calling FreezeRB stops car from actually being moved to the resetspot?
		//CarRBHandler.Instance.FreezeRB(2.0f);
		// }
	}

	public void ResetTransform() {
		if (!progressScript || !progressScript.ResetTransform()) {
			rb.MovePosition(emergencyResetPos);
			rb.MoveRotation(emergencyResetRot);
		}
	}


	private void ResetIfAllowed() {
		if (!StartCountdownScript.IsShown) {
			ResetTransform();
			CallResetEvents();
		}
	}

	public void Teleport(Vector3 pos, Quaternion rot) {
		effects?.DisableAllEffects();
		effects?.ClearAllEffects();

		// Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * rot;

		// rb.velocity = Vector3.zero;
		// rb.velocity = relativeRotation * rb.velocity;
		rb.velocity = rot * Vector3.forward * rb.velocity.magnitude;
		rb.angularVelocity = Vector3.zero;

		// rb.MovePosition(pos);
		// rb.MoveRotation(rot);
		transform.position = pos;
		transform.rotation = rot;
	}

	private void Rumble() {
		if (EnableRumble) {

			float engineLoRumble = EngineRumbleLoHzCurve.Evaluate(Mathf.Clamp01(Velocity.sqrMagnitude / (EngineRumbleLoHzMaxVelocity * EngineRumbleLoHzMaxVelocity)));
			float engineHiRumble = EngineRumbleHiHzCurve.Evaluate(Mathf.Clamp01(Velocity.sqrMagnitude / (EngineRumbleHiHzMaxVelocity * EngineRumbleHiHzMaxVelocity)));

			if (lowHzRumble < engineLoRumble) {
				lowHzRumble = engineLoRumble;
			}
			if (highHzRumble < engineHiRumble) {
				highHzRumble = engineHiRumble;
			}

			lowHzRumble = Mathf.Clamp(lowHzRumble, 0, 1);
			highHzRumble = Mathf.Clamp(highHzRumble, 0, 1);

			Gamepad.current?.SetMotorSpeeds(lowHzRumble, highHzRumble);
		}
	}

	private void ApplyAnimations() {
		foreach ((WheelCollider collider, Transform ModelTransform) in allWheelColliders.Zip(allWheelModels, (collider, model) => (collider, model.transform))) {
			Vector3 pos = ModelTransform.position;
			Quaternion rotation = ModelTransform.rotation;

			collider.GetWorldPose(out pos, out rotation);

			ModelTransform.position = pos;
			ModelTransform.rotation = rotation;
		}
	}

	// private float preFreezeTimescale = 1f;
	public void Freeze() {
		freezeNextFrame = false;
		enabled = false;
		boostWindupTimer = 0;
		// preFreezeTimescale = Time.timeScale;
		Time.timeScale = 0f;
		// Debug.LogWarning("car frozen");
	}

	public void Unfreeze() {
		freezeNextFrame = false;
		enabled = true;
		// Time.timeScale = preFreezeTimescale;
		Time.timeScale = 1.0f;
		// Debug.LogWarning("car unfrozen");
	}

	public static void FreezeCurrentCar() {
		if (MainInstance) {
			MainInstance.Freeze();
		} else {
			freezeNextFrame = true;
		}
		// MainInstance?.Freeze();
		// if(!MainInstance){
		// 	Debug.LogError("Could not freeze car: no car instance");
		// }
	}

	public static void UnfreezeCurrentCar() {
		freezeNextFrame = false;
		MainInstance?.Unfreeze();
	}

	public void DontZeroNextOnAir() {
		ignoreNextOnAirZeroing = true;
	}

	public void NextCamera() {
		if (Cameras == null || Cameras.Count < 1)
			return;

		CurrentCameraIndex++;
		if (CurrentCameraIndex >= Cameras.Count)
			CurrentCameraIndex = 0;

		for (int i = 0; i < Cameras.Count; i++) {
			Cameras[i].gameObject.SetActive(i == CurrentCameraIndex);
		}

	}

	// Recording

	/*
	// TODO: load ghost data
	private Queue<GhostData> ghostRecording = new Queue<GhostData>();
	private GhostData currentGhostData = null;
	private bool recording;

	public void ClearRecording() {
		ghostRecording.Clear();
	}

	// TODO: static timer, incremented by main instance, used by ghosts
	float GhostTimer = 0f;
	// TODO: ghost playback should probably be performed on a different object using a separate script
	public void PlayBackRecording() {

		while (true) {
			if (currentGhostData == null) {
				if (ghostRecording.Count < 1)
					return;

				currentGhostData = ghostRecording.Dequeue();
			}

			if (currentGhostData.TimeStamp > GhostTimer) {
				// TODO: apply transform
				continue;
			}

			break;

		}

	}

	public void RecordTransform() {
		if (!recording)
			return;

		ghostRecording.Enqueue(new GhostData(transform, GhostTimer));
	}
	// */

}
