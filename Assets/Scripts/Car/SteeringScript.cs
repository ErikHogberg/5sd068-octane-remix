﻿
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Assets.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class SteeringScript : MonoBehaviour {

	public enum TractionMode {
		FrontTraction,
		RearTraction,
		FourWheelTraction,
	}

	public enum BoostSkill {
		None,
		Invulnerability,
		SloMo
	}

	// TODO: list of instances for split screen multiplayer, indexed by player order
	public static SteeringScript MainInstance;

	#region Steering fields
	[Header("Steering")]

	[Tooltip("Sets which wheels are connected to the engine, rear axis, front axis, or both")]
	public TractionMode Mode = TractionMode.RearTraction;

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

	[Tooltip("How much the motor torque is reduced per second for each wheel collider")]
	public float MotorBrakeAmount = 100f;

	[Tooltip("How much of the brake force is applied to front wheels, the rest is applied to the rear wheels")]
	[Range(0, 1)]
	public float BrakeDistribution = 0.75f;

	public AnimationCurve BrakePedalCurve;

	[Space]

	[Tooltip("If the velocity of the rigidbody itself should be braked/dampened when braking")]
	public bool DampenRigidBody = true;

	[Tooltip("Velocity that will be lost per second while braking")]
	// [Range(0,1)]
	public float BrakeDampeningAmount = 2000f;

	[Header("Handbrake")]
	public float HandbrakeForce = 100f;
	public AnimationCurve HandbrakePedalCurve;

	[Range(-180, 180)]
	public float HandbrakeDriftAngle = 30f;
	#endregion

	#region Downward force fields
	[Header("Downward force")]
	public bool EnableDownwardForce = true;
	[Tooltip("At what speed does DownwardForce switch off by default")]
	public float MinDownwardsForceSpeed = 1.0f;
	public float DownwardForce = 10f;
	public ForceMode DownwardForceMode = ForceMode.Acceleration;
	[Tooltip("Whether or not the force direction should be relative to the car orientation instead of world.")]
	public bool UseRelativeDownwardForce = true;

	[Header("In-air controls")]

	public bool LeftStickRotationWhenInAir = false;

	public float YawSpeed = 200f;

	public AnimationCurve YawInputCurve;

	public float PitchSpeed = 100f;
	public AnimationCurve PitchInputCurve;
	#endregion

	#region Boost fields
	[Header("Boost")]
	public float BoostSpeed = 100f;
	private float boostAmount = 1;
	private bool boosting = false;
	private bool BoostNotEmpty => boostAmount > 0;

	//Limits boost based on temperature. 0.0 means no limitation, 1.0 means the maximum limitation is in place
	private float boostLimiter = 0.0f;
	//How many percent can the boost resource max be reduced by due to temperature? -30% or -50% or maybe -70%?
	private float boostLimitMax = 0.5f;

	//Returns between 0.0 and -boostLimitMax
	private float BoostLimit() { return (0.0f - (boostLimitMax * boostLimiter)); }
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
	// [Tooltip("If the car becomes invulnerable while boosting")]

	// public bool BoostInvulnerability = false;
	[Tooltip("How long time the car has to boost to become invulnerable")]
	[Min(0)]
	public float BoostWindup = 1f;

	// public bool BoostSloMo = false;
	[Range(0, 1)]
	public float BoostSloMoTimescale = 1f;

	private float boostWindupTimer = 0f;

	public float BoostWindupProgress => Mathf.Clamp(boostWindupTimer / BoostWindup, 0, 1);
	public bool BoostWindupReady => boosting && boostWindupTimer >= BoostWindup;
	public bool IsInvulnerable => BoostWindupSkill == BoostSkill.Invulnerability && BoostWindupReady;
	public bool IsInSloMo => BoostWindupSkill == BoostSkill.SloMo && BoostWindupReady;

	#endregion

	#region Velocity cap fields
	[Header("Velocity cap")]
	public bool CapVelocity = true;
	public float VelocityCap = 20f;
	public float BoostVelocityCap = 30f;
	[Min(0)]
	public float VelocityCapCorrectionSpeed = 1f;

	[Tooltip("Immediate velocity cap, does not use correction speed, car never goes above this speed (except for the speed gained last frame)")]
	public float AbsoluteVelocityCap = 200f;
	#endregion

	#region Drifting fields

	[Header("Drifting")]

	[Tooltip("Change the left stick to rotate instead of steer when the player drifts, as if holding left bumper")]
	public bool UseYawControlWhenDrifting = false;

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

	public bool EnableRumble = false;

	[Tooltip("Distribution of high vs low Hz rumble motor amount, more high hz => buzzing, more low hz => shaking")]
	[Range(0, 1)]
	public float EngineRumbleHiLoHzRatio = .5f;
	[Tooltip("How much the distribution is multiplied when applied, max 200%, meaning at 50% distrition both motors are 100% at max amount ")]
	[Range(0, 2)]
	public float EngineRumbleAmount = .5f;

	public Vector2 EngineRumbleSpeedMinMax;
	public AnimationCurve EngineRumbleCurve;

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

	[Header("Misc.")]

	public bool EnableSound = false;

	[Space]
	public bool OverrideGravity = false;
	public float GravityOverride = 1;

	[Space]
	public bool InAirStabilization = false;
	public float InAirStabilizationAmount = 1f;
	[Space]

	[Tooltip("If the car starts right in front of the goal post. Makes the first time crossing the finish line not count as a lap")]
	public bool StartBeforeGoalPost = false;

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

	[Header("Key bindings")]
	public InputActionReference SteeringKeyBinding;
	public InputActionReference GasKeyBinding;
	public InputActionReference BrakeKeyBinding;
	public InputActionReference HandbrakeKeyBinding;
	[Space]
	public InputActionReference BoostKeyBinding;
	public InputActionReference ResetKeyBinding;
	// public InputActionReference PauseKeyBinding;
	[Space]
	public InputActionReference YawKeyBinding;
	public InputActionReference PitchKeyBinding;
	public InputActionReference LeftRotateToggleKeyBinding;
	public InputActionReference LeftYawKeyBinding;
	public InputActionReference LeftPitchKeyBinding;

	#endregion


	private float[] wheelRotationBuffers;

	private Rigidbody rb;
	public Vector3 Velocity => rb.velocity;
	private float springInit;

	// IDEA: make observers instead?
	private CarParticleHandlerScript effects;
	private CarSoundHandler carSound;
	// private TemperatureAndIntegrity tempAndInteg;

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
			if (StartBeforeGoalPost) {
				StartBeforeGoalPost = false;
				return;
			}

			lapsCompleted = value;
			foreach (var item in LapCompletedObservers)
				item.Notify(lapsCompleted);
		}
	}


	void Start() {

		if (OverrideGravity) {
			Physics.gravity = Physics.gravity.normalized * GravityOverride;
		}

		allWheelColliders = FrontWheelColliders.Concat(RearWheelColliders);
		allWheelModels = FrontWheelModels.Concat(RearWheelModels);

		springInit = FrontWheelColliders[0].suspensionSpring.spring;

		wheelRotationBuffers = new float[FrontWheelColliders.Count + RearWheelColliders.Count];

		// TODO: teleport car to start segment reset spot
		// FIXME: reset triggers penalty popup, prematurely starting game
		// IDEA: reset fn with option to disable penalty
		// LevelPieceSuperClass.ResetToStart();

	}

	void Awake() {
		rb = GetComponent<Rigidbody>();
		carSound = GetComponent<CarSoundHandler>();
		if (EnableSound) carSound.enabled = true;
		else carSound.enabled = false;

		// IDEA: add null check to input bindings, dont crash if not set in editor
		InitInput();

		effects = GetComponent<CarParticleHandlerScript>();
		// tempAndInteg = GetComponent<TemperatureAndIntegrity>();

	}

	void OnEnable() {
		EnableInput();

		InputSystem.ResumeHaptics();

		MainInstance = this;
		LevelPieceSuperClass.ClearCurrentSegment();
		
	}

	void OnDisable() {
		DisableInput();

		InputSystem.PauseHaptics();
			
	}

	private void OnDestroy() {
		InputSystem.ResetHaptics();
	}

	private bool touchingGround = true;

	void FixedUpdate() {
		float dt = Time.deltaTime;
		float unscaledDt = Time.unscaledDeltaTime;

		lowHzRumble = 0;
		highHzRumble = 0;

		touchingGround = CheckIfTouchingGround();

		float sqrVelocity = rb.velocity.sqrMagnitude;

		// TODO: curve for gradual downwards force with velocity
		if (EnableDownwardForce && sqrVelocity > MinDownwardsForceSpeed * MinDownwardsForceSpeed)
			if (UseRelativeDownwardForce)
				rb.AddRelativeForce(Vector3.down * DownwardForce, DownwardForceMode);
			else
				rb.AddForce(Vector3.down * DownwardForce, DownwardForceMode);

		Steer(dt);
		Gas(dt);

		Boost(dt, unscaledDt);

		if (touchingGround && SteeringStrafeHelp > float.Epsilon) {
			// Strafe help
			rb.AddRelativeForce(Vector3.right * SteeringStrafeHelp * steeringBuffer, SteeringStrafeMode);
		}

		Brake(dt);
		Handbrake(dt);

		float yawAmount = YawSpeed * yawBuffer * dt;
		Yaw(yawAmount, true);
		if (touchingGround) {
			float steeringYawAmount = SteeringRotationHelp * steeringBuffer * dt;
			Yaw(steeringYawAmount, false);
		}

		Pitch(dt);


		if (InAirStabilization && !touchingGround) {
			// rb.transform.up = Vector3.RotateTowards(rb.transform.up, Vector3.up, InAirStabilizationAmount, 0);
			var rot = Quaternion.FromToRotation(rb.transform.up, Vector3.up);
			rb.AddTorque(new Vector3(rot.x, rot.y, rot.z) * InAirStabilizationAmount);
			// rb.AddTorque(rot * Vector3.up * InAirStabilizationAmount);

		}

		ApplyVelocityCap(dt);
		ApplyAnimations();

		Drift(dt);

		if (effects)
			effects.UpdateEffects(sqrVelocity, touchingGround);

		// TODO: ambient engine rumble using controller rumble
		// if (sqrVelocity > EngineRumbleSpeedMinMax.x) {
		// 	var engineRumble = EngineRumbleCurve.Evaluate(( EngineRumbleSpeedMinMax.y - EngineRumbleSpeedMinMax.x)/sqrVelocity ); // not done
		// }


		//To keep the velocity needle moving smoothly
		RefreshUI();

		SetDebugUIText(13, touchingGround.ToString());
		// touchedGroundLastTick = false;

		Rumble();

	}

	//To avoid jittery number updates on the UI
	int updateCount = 0;
	int updateInterval = 5;
	void LateUpdate() {
		if (updateCount >= updateInterval) {
			UpdateUI();
			updateCount = 0;
		}

		updateCount++;
	}

	private bool CheckIfTouchingGround() {
		// IDEA: use a timer to give some "coyote-time", restart timer every tick that car collides with ground

		foreach (WheelCollider wheelCollider in allWheelColliders) {
			if (wheelCollider.isGrounded && EnableSound) {
				carSound.RecieveGroundedData(true);
				return true;
			}
		}
		if (EnableSound)
			carSound.RecieveGroundedData(false);
		return false;
	}


	#region UI
	private void RefreshUI() {
		GasNeedleUIScript.Refresh();
	}

	private void UpdateUI() {
		// float gasAmount = GasSpeed * gasBuffer;

		float percentage = rb.velocity.sqrMagnitude / (VelocityCap * VelocityCap);
		float kmph = (float)rb.velocity.magnitude * 3.6f;
		if (EnableSound) carSound.RecieveVelocityData(percentage);

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
	#endregion

	private void ApplyVelocityCap(float dt) {
		if (CapVelocity) {
			if (boosting) {
				if (rb.velocity.sqrMagnitude > BoostVelocityCap * BoostVelocityCap) {
					// rb.velocity = Vector3.Normalize(rb.velocity) * BoostVelocityCap;
					rb.velocity = Vector3.MoveTowards(
						rb.velocity,
						Vector3.Normalize(rb.velocity) * BoostVelocityCap,
						VelocityCapCorrectionSpeed * dt
					);

				}

			} else {
				if (rb.velocity.sqrMagnitude > VelocityCap * VelocityCap) {
					// rb.velocity = Vector3.Normalize(rb.velocity) * VelocityCap;
					rb.velocity = Vector3.MoveTowards(
						rb.velocity,
						Vector3.Normalize(rb.velocity) * VelocityCap,
						VelocityCapCorrectionSpeed * dt
					);
				}
			}

			if (rb.velocity.sqrMagnitude > AbsoluteVelocityCap * AbsoluteVelocityCap)
				rb.velocity = Vector3.Normalize(rb.velocity) * AbsoluteVelocityCap;

		}
	}

	#region Drifting

	private void StartDrift() { // NOTE: called every frame while drifting, not just on drift status change
		if (drifting == false) {
			if (EnableSound && (rb.velocity.magnitude * 3.6f) > 100f)
				SoundManager.PlaySound("drift_continuous");
		}
		drifting = true;

		effects?.StartDrift();

		SetDebugUIText(11, "True");
	}

	private void StopDrift() { // NOTE: called every frame while not drifting, not just on drift status change
		if (drifting == true) {
			if (EnableSound && (rb.velocity.magnitude * 3.6f) > 100f)
				SoundManager.StopLooping("drift_continuous");
		}
		drifting = false;

		effects?.StopDrift();

		SetDebugUIText(11, "False");
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
			lowHzRumble += (1f - DriftRumbleHiLoHzRatio) * DriftRumbleAmount;
			highHzRumble += DriftRumbleHiLoHzRatio * DriftRumbleAmount;
			StartDrift();
		}

		if (!touchingGround
			|| absAngle < DriftStopAngle
			|| velocity.sqrMagnitude < DriftStopVelocity * DriftStopVelocity
		) {
			StopDrift();
		}

		SetDebugUIText(12, angle.ToString("F2"));
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
	float handbrakeBuffer = 0f;

	float yawBuffer = 0f;
	float oldHandbrakeBuffer = 0f;
	float pitchBuffer = 0f;

	private bool leftStickRotationEnabled = false;


	private void InitInput() {
		// adds press actions
		SteeringKeyBinding.action.performed += SetSteering;
		GasKeyBinding.action.performed += SetGas;
		BrakeKeyBinding.action.performed += SetBraking;
		HandbrakeKeyBinding.action.performed += StartHandbraking;

		BoostKeyBinding.action.performed += StartBoost;

		YawKeyBinding.action.performed += SetYaw;
		PitchKeyBinding.action.performed += SetPitch;
		LeftYawKeyBinding.action.performed += SetLeftYaw;
		LeftPitchKeyBinding.action.performed += SetLeftPitch;
		LeftRotateToggleKeyBinding.action.performed += EnableLeftStickRotation;

		ResetKeyBinding.action.performed += Reset;
		// PauseKeyBinding.action.performed += (_) => { PauseScript.ToggleStatic(); };
		// adds release actions
		SteeringKeyBinding.action.canceled += SetSteering;
		GasKeyBinding.action.canceled += SetGas;
		BrakeKeyBinding.action.canceled += SetBraking;
		HandbrakeKeyBinding.action.canceled += StopHandbraking;

		BoostKeyBinding.action.canceled += StopBoost;

		YawKeyBinding.action.canceled += SetYaw;
		PitchKeyBinding.action.canceled += SetPitch;
		LeftYawKeyBinding.action.canceled += SetLeftYaw;
		LeftPitchKeyBinding.action.canceled += SetLeftPitch;
		LeftRotateToggleKeyBinding.action.canceled += DisableLeftStickRotation;

	}

	private void EnableInput() {
		SteeringKeyBinding.action.Enable();
		GasKeyBinding.action.Enable();
		BrakeKeyBinding.action.Enable();
		HandbrakeKeyBinding.action.Enable();
		ResetKeyBinding.action.Enable();
		// PauseKeyBinding.action.Enable();
		BoostKeyBinding.action.Enable();

		YawKeyBinding.action.Enable();
		PitchKeyBinding.action.Enable();
		LeftYawKeyBinding.action.Enable();
		LeftPitchKeyBinding.action.Enable();
		LeftRotateToggleKeyBinding.action.Enable();
	}

	private void DisableInput() {
		SteeringKeyBinding.action.Disable();
		GasKeyBinding.action.Disable();
		BrakeKeyBinding.action.Disable();
		HandbrakeKeyBinding.action.Disable();
		ResetKeyBinding.action.Disable();
		// PauseKeyBinding.action.Disable();
		BoostKeyBinding.action.Disable();

		YawKeyBinding.action.Disable();
		PitchKeyBinding.action.Disable();
		LeftYawKeyBinding.action.Disable();
		LeftPitchKeyBinding.action.Disable();
		LeftRotateToggleKeyBinding.action.Disable();
	}


	#region Steering

	private void Steer(float dt) {
		ApplySteeringTorque();
	}

	private void ApplySteeringTorque() {

		float sqrVelocity = rb.velocity.sqrMagnitude;

		SetDebugUIText(8, sqrVelocity.ToString("F2")); // F2 sets format to 2 decimals, 0.00

		// narrow steering angle as speed increases
		float sqrMaxNarrowingSpeed = MaxNarrowingSpeed * MaxNarrowingSpeed;

		float narrowing = 1f;

		if (!EnableNarrowing) {
			SetDebugUIText(10, "1.00");
		} else {
			float speedProgress = 1f;

			// if (sqrVelocity < sqrMaxNarrowingSpeed)
			speedProgress -= sqrVelocity / sqrMaxNarrowingSpeed;

			narrowing = SteeringNarrowingCurve.Evaluate(speedProgress);
			narrowing = MaxNarrowingAmount + narrowing * (1f - MaxNarrowingAmount);
			SetDebugUIText(10, speedProgress.ToString("F2"));
		}

		SetDebugUIText(9, narrowing.ToString("F2"));


		float steeringAmount = steeringBuffer * SteeringMax * narrowing;
		foreach (WheelCollider FrontWheelCollider in FrontWheelColliders)
			FrontWheelCollider.steerAngle = steeringAmount;

		SetDebugUIText(0, steeringBuffer.ToString("F2"));
	}

	private void SetSteering(CallbackContext c) {
		if (leftStickRotationEnabled)
			return;

		float input = c.ReadValue<float>();
		steeringBuffer = SteeringCurve.EvaluateMirrored(input);

		SetDebugUIText(1, input.ToString("F2"));
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

		float gasAmount = GasSpeed * gasBuffer;

		switch (Mode) {
			case TractionMode.FrontTraction:
				gasAmount /= FrontWheelColliders.Count;

				foreach (WheelCollider frontWheelCollider in FrontWheelColliders)
					frontWheelCollider.motorTorque = gasAmount;

				break;
			case TractionMode.RearTraction:
				gasAmount /= RearWheelColliders.Count;

				foreach (WheelCollider rearWheelCollider in RearWheelColliders)
					rearWheelCollider.motorTorque = GasSpeed * gasBuffer;

				break;
			case TractionMode.FourWheelTraction:
				gasAmount /= FrontWheelColliders.Count + RearWheelColliders.Count;

				foreach (WheelCollider rearWheelCollider in RearWheelColliders)
					rearWheelCollider.motorTorque = GasSpeed * gasBuffer;

				foreach (WheelCollider frontWheelCollider in FrontWheelColliders)
					frontWheelCollider.motorTorque = GasSpeed * gasBuffer;

				break;
		}

		lastAppliedGasValue = gasBuffer;

		SetDebugUIText(4, gasBuffer.ToString("F2"));
	}

	private void SetGas(CallbackContext c) {
		float input = c.ReadValue<float>();
		gasBuffer = GasPedalCurve.EvaluateMirrored(input);

		SetDebugUIText(5, input.ToString("F2"));
	}

	#endregion

	#region Braking
	private void Brake(float dt) {
		// if (brakeBuffer < float.Epsilon)
		// return;

		float rpm = 0;
		foreach (var item in allWheelColliders) {
			if (rpm < item.rpm)
				rpm = item.rpm;
		}

		if (rpm > float.Epsilon) {

			float brakeAmount = BrakeForce * brakeBuffer;
			float frontBrakeAmount = brakeAmount * BrakeDistribution;
			float rearBrakeAmount = brakeAmount * (1f - BrakeDistribution);

			foreach (WheelCollider frontWheelCollider in FrontWheelColliders) {
				frontWheelCollider.brakeTorque = frontBrakeAmount;
				// frontWheelCollider.motorTorque = Mathf.MoveTowards(frontWheelCollider.motorTorque, 0, brakeAmount * MotorBrakeAmount * dt);
			}

			foreach (WheelCollider rearWheelCollider in RearWheelColliders) {
				rearWheelCollider.brakeTorque = rearBrakeAmount;
				// rearWheelCollider.motorTorque = Mathf.MoveTowards(rearWheelCollider.motorTorque, 0, brakeAmount * MotorBrakeAmount * dt);
			}

			if (DampenRigidBody && brakeBuffer > 0) {
				// IDEA: minimum velocity for brake help, to disallow slow fall
				rb.AddForce(-BrakeDampeningAmount * brakeBuffer * rb.velocity);
			}

		} else if (brakeBuffer > float.Epsilon) {
			foreach (var item in allWheelColliders)
				item.brakeTorque = 0;
			gasBuffer = -1;
		}

		// TODO: reverse instead if rpm is 0

		SetDebugUIText(2, brakeBuffer.ToString("F2"));
	}

	private void Handbrake(float dt) {


		// foreach (WheelCollider frontWheelCollider in FrontWheelColliders) {
		// frontWheelCollider.brakeTorque = HandbrakeForce * handbrakeBuffer;
		// }

		SetDebugUIText(6, handbrakeBuffer.ToString("F2"));
	}

	private void SetBraking(CallbackContext c) {
		float input = c.ReadValue<float>();
		float pastBrakeBuffer = brakeBuffer;
		brakeBuffer = BrakePedalCurve.EvaluateMirrored(input);

		if (EnableSound) {
			if (brakeBuffer > pastBrakeBuffer) {
				if (brakeBuffer > 0.2f)
					SoundManager.PlaySound("wind_brake");
			} else {
				SoundManager.StopLooping("wind_brake");
			}
		}


		SetDebugUIText(3, input.ToString("F2"));
	}

	private void StartHandbraking(CallbackContext c) {
		float input = c.ReadValue<float>();
		handbrakeBuffer = HandbrakePedalCurve.EvaluateMirrored(input);

		// float delta = handbrakeBuffer - oldHandbrakeBuffer;
		// TODO: interpolate input?
		// float driftAmount = HandbrakeDriftAngle * delta;

		// rb.rotation = Quaternion.Euler(rb.velocity) * Quaternion.Euler(0, driftAmount, 0);

		if (drifting)
			return;

		float dir;

		if (steeringBuffer > 0) {
			dir = 1;
		} else if (steeringBuffer < 0) {
			dir = -1;
		} else {
			return;
		}

		transform.forward = Vector3.RotateTowards(transform.forward, transform.right, handbrakeBuffer * HandbrakeDriftAngle * Mathf.Deg2Rad * dir, 0);

		SetDebugUIText(7, input.ToString("F2"));

		oldHandbrakeBuffer = handbrakeBuffer;
	}

	private void StopHandbraking(CallbackContext c) {
		// rb.rotation = Quaternion.Euler(rb.velocity);
		// transform.forward = rb.velocity;
	}

	#endregion

	#region Yaw, Pitch

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
		float pitchAmount = PitchSpeed * pitchBuffer * dt;

		rb.rotation *= Quaternion.Euler(pitchAmount, 0, 0);
	}

	private void SetYaw(CallbackContext c) {
		float input = c.ReadValue<float>();
		yawBuffer = SteeringCurve.EvaluateMirrored(input);
	}
	public float GetYaw() { return yawBuffer; }

	private void SetPitch(CallbackContext c) {
		float input = c.ReadValue<float>();
		pitchBuffer = SteeringCurve.EvaluateMirrored(input);
	}


	private void SetLeftYaw(CallbackContext c) {
		if (leftStickRotationEnabled || (LeftStickRotationWhenInAir && !touchingGround) || (UseYawControlWhenDrifting && drifting)) {
			float input = c.ReadValue<float>();
			yawBuffer = SteeringCurve.EvaluateMirrored(input);
			// TODO: reset yaw buffer when touching ground again or drifting
			// IDEA: separate buffer for left stick? check bools in update?
		}
	}
	private void SetLeftPitch(CallbackContext c) {
		if (leftStickRotationEnabled || (LeftStickRotationWhenInAir && !touchingGround)) {
			float input = c.ReadValue<float>();
			pitchBuffer = SteeringCurve.EvaluateMirrored(input);
		}
	}

	private void EnableLeftStickRotation(CallbackContext _) {
		leftStickRotationEnabled = true;
	}
	private void DisableLeftStickRotation(CallbackContext _) {
		leftStickRotationEnabled = false;
		yawBuffer = 0;
		pitchBuffer = 0;
		// NOTE: might feel weird if releasing toggle while using right stick, as it will reset the stick to 0 for 1 frame
		// TODO: fix this by having an additional set of stick buffers
	}


	#endregion

	#region Boost

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
			rb.AddRelativeForce(boostDir * BoostSpeed, ForceMode.Acceleration);
			lowHzRumble += (1f - BoostRumbleHiLoHzRatio) * BoostRumbleAmount;
			highHzRumble += BoostRumbleHiLoHzRatio * BoostRumbleAmount;
		} else {
			StopBoost();
		}

	}

	private void AddBoost(float amount) {
		boostAmount += amount;
		boostAmount = Mathf.Clamp(boostAmount, 0, 1 + BoostLimit());

		BoostBarUIScript.SetBarPercentage((float)boostAmount);
	}

	private void StartBoost(CallbackContext _) {
		if (boostAmount < MinBoostLevel)
			return;

		if (EnableSound && boosting == false) {
			SoundManager.PlaySound("boost_start");
			SoundManager.PlaySound("boost_continuous");
			//UnityEngine.Debug.Log("Boost sound start");
		}
		boosting = true;
		
	}

	private void StopBoost() {

		boostWindupTimer = 0f;
		Time.timeScale = 1f;

		effects?.StopBoost();

		if (EnableSound && boosting == true) {
			SoundManager.StopLooping("boost_continuous");
			SoundManager.PlaySound("boost_end");
			//UnityEngine.Debug.Log("Boost sound end");
		}
		boosting = false;
	}

	private void StopBoost(CallbackContext _) {
		StopBoost();
	}

	#endregion

	private void CallResetObservers() {
		foreach (var observer in ResetObservers)
			// TODO: use exactly car camera instead of global current camera, in case there are multiple cars
			observer.Notify(Camera.main);
	}

	public void Reset(Vector3 pos, Quaternion rot) {
		CallResetObservers();
		StartCountdownScript.StartPenaltyCountdownStatic(1.5f);

		effects?.ClearAllEffects();

		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		rb.MovePosition(pos);
		rb.MoveRotation(rot);
	}

	public void Reset() {
		CallResetObservers();
		// StartCountdownScript.StartPenaltyCountdownStatic(1f);

		if (!LevelPieceSuperClass.ResetToCurrentSegment() && LevelWorldScript.CurrentLevel != null) {
			Transform resetSpot = LevelWorldScript.CurrentLevel.TestRespawnSpot;

			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;

			rb.MovePosition(resetSpot.position);
			rb.MoveRotation(resetSpot.rotation);

			//For some reason, calling FreezeRB stops car from actually being moved to the resetspot?
			//CarRBHandler.Instance.FreezeRB(2.0f);
		}
	}

	private void Reset(CallbackContext _) {
		Reset();
	}

	private void Rumble() {
		if (EnableRumble) {
			lowHzRumble = Mathf.Clamp(lowHzRumble, 0, 1);
			highHzRumble = Mathf.Clamp(highHzRumble, 0, 1);

			Gamepad.current.SetMotorSpeeds(lowHzRumble, highHzRumble);
		}
	}

	#endregion

	private void SetDebugUIText(int index, string text = "0.00") {
		if (DebugUIScript.MainInstance == null || DebugUIScript.MainInstance.TextBoxes == null || DebugUIScript.MainInstance.TextBoxes.Count <= index)
			return;

		DebugUIScript.MainInstance.SetText(text, index);
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
		enabled = false;
		boostWindupTimer = 0;
		// preFreezeTimescale = Time.timeScale;
		Time.timeScale = 0f;
	}

	public void Unfreeze() {
		enabled = true;
		// Time.timeScale = preFreezeTimescale;
		Time.timeScale = 1.0f;
	}

	public static void FreezeCurrentCar() {
		MainInstance?.Freeze();
	}

	public static void UnfreezeCurrentCar() {
		MainInstance?.Unfreeze();
	}

}
