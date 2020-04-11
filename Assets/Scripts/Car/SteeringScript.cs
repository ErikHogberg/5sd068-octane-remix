﻿﻿using System.Linq;

using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;


[RequireComponent(typeof(Rigidbody))]
public class SteeringScript : MonoBehaviour {

	public enum TractionMode {
		FrontTraction,
		RearTraction,
		FourWheelTraction,
	}


	[Header("Required objects")]

	[Tooltip("Front wheels")]
	public List<WheelCollider> FrontWheelColliders;
	public List<GameObject> FrontWheelModels;
	[Tooltip("Rear wheels")]
	public List<WheelCollider> RearWheelColliders;
	public List<GameObject> RearWheelModels;

	private IEnumerable<WheelCollider> allWheelColliders;
	private IEnumerable<GameObject> allWheelModels;


	[Header("Optional objects")]

	[Tooltip("Trail renderers that will be turned on or off with boost")]
	public List<TrailRenderer> BoostTrails;
	private bool IsBoostTrailEmitting { // NOTE: pretty useless accessor compared to bloat created, but useful as an example of simplifying the API using accessors
		get {
			if (BoostTrails.Any())
				return BoostTrails[0].emitting;
			return false;
		}
		set {
			foreach (TrailRenderer boostTrail in BoostTrails)
				boostTrail.emitting = value;
		}
	}

	[Space]

	[Tooltip("Trail renderers that will be turned on or off with drift")]
	public List<TrailRenderer> DriftTrails;

	[Space]

	[Tooltip("Trail renderers that will be turned on or off when turning right using the right stick")]
	public List<TrailRenderer> YawClockwiseTrails;
	[Tooltip("Trail renderers that will be turned on or off when turning left using the right stick")]
	public List<TrailRenderer> YawCounterClockwiseTrails;
	
	/*
	[Tooltip("Trail renderers that will be turned on or off when pitching up using the right stick")]
	public List<TrailRenderer> PitchUpTrails;
	[Tooltip("Trail renderers that will be turned on or off when pitching down using the right stick")]
	public List<TrailRenderer> PitchDownTrails;
	// */

	[Space]
	public Transform CustomCenterOfMass;


	[Header("Key bindings (Required)")]
	public InputActionReference SteeringKeyBinding;
	public InputActionReference GasKeyBinding;
	public InputActionReference ReverseKeyBinding;
	public InputActionReference BrakeKeyBinding;
	public InputActionReference HandbrakeKeyBinding;
	public InputActionReference JumpKeyBinding;
	public InputActionReference BoostKeyBinding;


	public InputActionReference YawKeyBinding;
	public InputActionReference PitchKeyBinding;

	public InputActionReference ResetKeyBinding;


	// TODO: reset car position to closest track position
	// TODO: reverse?



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
	[Tooltip("Reduces the max steering angle as the car speeds up, the angle narrowing at the rate set on this curve, 1.0 on the X axis is the max narrowing speed, 1.0 on the Y axis is the normal max steering angle")]
	public AnimationCurve SteeringNarrowingCurve;
	[Tooltip("Offsets the car's center of mass by this vector3")]
	public Vector3 CenterOfMassOffset;

	[Header("Gas")]
	public float GasSpeed = 100f;
	public AnimationCurve GasPedalCurve;

	[Header("Brakes")]
	public float BrakeForce = 100f;

	[Tooltip("How much of the brake force is applied to front wheels, the rest is applied to the rear wheels")]
	[Range(0, 1)]
	public float BrakeDistribution = 0.75f;

	public AnimationCurve BrakePedalCurve;

	[Header("Handbrake")]
	public float HandbrakeForce = 100f;
	public AnimationCurve HandbrakePedalCurve;

	[Header("Downward force")]
	public bool EnableDownwardForce = true;
	[Tooltip("At what speed does DownwardForce switch off by default")]
	public float MinDownwardsForceSpeed = 1.0f;
	public float DownwardForce = 10f;
	public ForceMode DownwardForceMode = ForceMode.Acceleration;
	[Tooltip("Whether or not the force direction should be relative to the car orientation instead of world.")]
	public bool UseRelativeDownwardForce = true;

	[Header("In-air controls")]
	public float YawSpeed = 200f;

	[Tooltip("Add the tilt of the joystick as an offset instead of rotation over time, rotating back again when releasing stick")]
	public AnimationCurve YawInputCurve;

	public bool UseYawOffsetMode = false;
	public float PitchSpeed = 100f;
	public AnimationCurve PitchInputCurve;

	[Header("Boost")]
	private bool boosting = false;
	public float BoostSpeed = 100f;
	private double boostAmount = 1;
	private bool BoostNotEmpty {
		get { return boostAmount > 0; }
	}

	[Tooltip("How much % of the boost tank is emptied per second when boosting")]
	[Range(0, 1)]
	public double BoostConsumptionRate = .4;

	[Tooltip("How much % of the boost tank is added per second when not boosting")]
	[Range(0, 1)]
	public double BoostFillRate = .25;

	[Tooltip("How much boost tank % is required to start boosting")]
	[Range(0, 1)]
	public double MinBoostLevel = .2;


	[Header("Velocity cap")]
	public bool CapVelocity = true;
	public float VelocityCap = 20f;
	public float BoostVelocityCap = 30f;

	[Header("Drifting")]

	[Tooltip("prerequisite angle at which drifting starts")]
	[Range(0, 180)]
	public float DriftStartAngle = 30f;

	[Tooltip("At which angle drifting stops")]
	[Range(0, 180)]
	public float DriftStopAngle = 30f;

	[Tooltip("prerequisite velocity at which drifting starts")]
	public float DriftStartVelocity = 1f;
	[Tooltip("At which velocity drifting stops")]
	public float DriftStopVelocity = .5f;


	// input buffers
	private float steeringBuffer = 0f;
	private float gasBuffer = 0f;
	private float lastAppliedGasValue = 0f;
	private float brakeBuffer = 0f;
	private float handbrakeBuffer = 0f;
	private float jumpBuffer = 0f;

	private float yawBuffer = 0f;
	private float oldYawBuffer = 0f;
	private float pitchBuffer = 0f;



	private float[] wheelRotationBuffers;

	private Rigidbody rb;
	private float springInit;

	void Start() {
		rb = GetComponent<Rigidbody>();

		if (CustomCenterOfMass != null) {
			rb.centerOfMass = CustomCenterOfMass.position - transform.position;
		}
		rb.centerOfMass += CenterOfMassOffset;

		allWheelColliders = FrontWheelColliders.Concat(RearWheelColliders);
		allWheelModels = FrontWheelModels.Concat(RearWheelModels);

		springInit = FrontWheelColliders[0].suspensionSpring.spring;

		wheelRotationBuffers = new float[FrontWheelColliders.Count + RearWheelColliders.Count];

		// TODO: camera that follows car behind position, not locked to car orientation
		// IDEA: have camera as child, reparent to parent of car on start
		// IDEA: use physics joints for camera arm

	}

	void Awake() {
		// IDEA: add null check to input bindings, dont crash if not set in editor
		InitInput();
	}

	private void InitInput() {
		// adds press actions
		SteeringKeyBinding.action.performed += SetSteering;
		GasKeyBinding.action.performed += SetGas;
		ReverseKeyBinding.action.performed += StartReverse;
		BrakeKeyBinding.action.performed += SetBraking;
		HandbrakeKeyBinding.action.performed += SetHandbraking;
		JumpKeyBinding.action.performed += SetJump;
		YawKeyBinding.action.performed += SetYaw;
		PitchKeyBinding.action.performed += SetPitch;
		BoostKeyBinding.action.performed += StartBoost;

		ResetKeyBinding.action.performed += Reset;

		// adds release actions
		SteeringKeyBinding.action.canceled += StopSteering;
		GasKeyBinding.action.canceled += StopGas;
		ReverseKeyBinding.action.canceled += StopReverse;
		BrakeKeyBinding.action.canceled += StopBraking;
		HandbrakeKeyBinding.action.canceled += StopHandbraking;
		JumpKeyBinding.action.canceled += ReleaseJump;
		YawKeyBinding.action.canceled += StopYaw;
		PitchKeyBinding.action.canceled += StopPitch;
		BoostKeyBinding.action.canceled += StopBoost;
	}

	private void EnableInput() {
		SteeringKeyBinding.action.Enable();
		GasKeyBinding.action.Enable();
		ReverseKeyBinding.action.Enable();
		BrakeKeyBinding.action.Enable();
		HandbrakeKeyBinding.action.Enable();
		JumpKeyBinding.action.Enable();
		YawKeyBinding.action.Enable();
		PitchKeyBinding.action.Enable();
		ResetKeyBinding.action.Enable();
		BoostKeyBinding.action.Enable();
	}

	private void DisableInput() {
		SteeringKeyBinding.action.Disable();
		GasKeyBinding.action.Disable();
		ReverseKeyBinding.action.Disable();
		BrakeKeyBinding.action.Disable();
		HandbrakeKeyBinding.action.Disable();
		JumpKeyBinding.action.Disable();
		YawKeyBinding.action.Disable();
		PitchKeyBinding.action.Disable();
		ResetKeyBinding.action.Disable();
		BoostKeyBinding.action.Disable();
	}


	void OnEnable() {
		EnableInput();

		// TODO: enable/disable controls when losing window focus, pausing, etc.
	}

	void OnDisable() {
		DisableInput();
	}

	// void Update() {
	// }

	private bool touchedGroundLastTick = true;//false;

	void FixedUpdate() {
		float dt = Time.deltaTime;

		if (EnableDownwardForce && rb.velocity.sqrMagnitude > MinDownwardsForceSpeed * MinDownwardsForceSpeed)
			if (UseRelativeDownwardForce)
				rb.AddRelativeForce(Vector3.down * DownwardForce, DownwardForceMode);
			else
				rb.AddForce(Vector3.down * DownwardForce, DownwardForceMode);


		Steer(dt);
		Gas(dt);
		Boost(dt);
		Brake(dt);
		Handbrake(dt);

		// TODO: only allow yaw/pitch controls if in-air (or upside-down?)
		Yaw(dt);
		Pitch(dt);

		Jump(dt);

		ApplyVelocityCap();
		ApplyAnimations();

		CheckDrift();

		//To keep the velocity needle moving smoothly
		RefreshUI();

		// IDEA: velocity forward correction, alter velocity direction each tick to move towards car forward direction (or wheel direction?), keeping magnitude the same

		// touchedGroundLastTick = false;
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

	// private void OnCollisionStay(Collision other) {
	// 	// IDEA: use a timer to give some "coyote-time", restart timer every tick that car collides with ground
	// 	if (other.gameObject.tag == "Ground")
	// 		touchedGroundLastTick = true;

	// }

	#region UI
	private void RefreshUI() {
		GasNeedleUIScript.Refresh();
	}
	private void UpdateUI() {
		// float gasAmount = GasSpeed * gasBuffer;

		float percentage = rb.velocity.sqrMagnitude / (VelocityCap * VelocityCap);
		float kmph = (float)rb.velocity.magnitude * 3.6f;

		if (boosting) {
			if (percentage >= 1f)
				percentage = Random.Range(1f, 1.05f);
			GasNeedleUIScript.SetBarPercentage(percentage, true);
		} else {
			GasNeedleUIScript.SetBarPercentage(percentage, false);
		}
		GasNeedleUIScript.SetKMPH(kmph);
	}
	#endregion

	private void ApplyVelocityCap() {
		if (CapVelocity) {
			if (boosting) {
				if (rb.velocity.sqrMagnitude > BoostVelocityCap * BoostVelocityCap)
					rb.velocity = Vector3.Normalize(rb.velocity) * BoostVelocityCap;
			} else {
				if (rb.velocity.sqrMagnitude > VelocityCap * VelocityCap)
					rb.velocity = Vector3.Normalize(rb.velocity) * VelocityCap;
			}
		}
	}

	#region Drifting

	private void StartDrift() {
		// TODO: enable drifting bool, create drift method in fixed update which uses bool
		// TODO: only call this method the frame that drifting starts
		foreach (TrailRenderer driftTrail in DriftTrails)
			driftTrail.emitting = true;

		SetDebugUIText(11, "true");

	}

	private void StopDrift() {
		foreach (TrailRenderer driftTrail in DriftTrails)
			driftTrail.emitting = false;

		SetDebugUIText(11, "false");

	}

	// check if drifting

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

		if (touchedGroundLastTick
			&& absAngle > DriftStartAngle
			&& velocity.sqrMagnitude > DriftStartVelocity * DriftStartVelocity
		) {
			StartDrift();
		}

		if (!touchedGroundLastTick
			|| absAngle < DriftStopAngle
			|| velocity.sqrMagnitude < DriftStopVelocity * DriftStopVelocity
		) {
			StopDrift();
		}

		SetDebugUIText(12, angle.ToString("F2"));
	}

	#endregion

	#region Input callbacks
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
			if (sqrVelocity < sqrMaxNarrowingSpeed)
				speedProgress -= sqrVelocity / sqrMaxNarrowingSpeed;

			narrowing = SteeringNarrowingCurve.Evaluate(speedProgress);
			SetDebugUIText(10, speedProgress.ToString("F2"));
		}

		SetDebugUIText(9, narrowing.ToString("F2"));


		float steeringAmount = steeringBuffer * SteeringMax * narrowing;
		foreach (WheelCollider FrontWheelCollider in FrontWheelColliders)
			FrontWheelCollider.steerAngle = steeringAmount;

		SetDebugUIText(0, steeringBuffer.ToString("F2"));
	}

	private void SetSteering(CallbackContext c) {
		float input = c.ReadValue<float>();
		steeringBuffer = SteeringCurve.EvaluateMirrored(input);

		SetDebugUIText(1, input.ToString("F2"));
	}

	private void StopSteering(CallbackContext _) {
		steeringBuffer = 0;

		SetDebugUIText(1);
	}
	#endregion

	#region Gas

	private void Gas(float dt) {
		if (brakeBuffer == 0f || gasBuffer < lastAppliedGasValue)
			ApplyGasTorque();

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

	private void StopGas(CallbackContext _) {
		gasBuffer = 0;

		SetDebugUIText(5);
	}
	#endregion

	#region Reverse

	private void StartReverse(CallbackContext _) {
		gasBuffer = -1;
	}

	private void StopReverse(CallbackContext _) {
		gasBuffer = 0;
	}

	#endregion

	#region Braking
	private void Brake(float dt) {
		ApplyBrakeTorque();
	}

	private void Handbrake(float dt) {
		ApplyHandbrakeTorque();
	}

	private void ApplyBrakeTorque() {

		float brakeAmount = BrakeForce * brakeBuffer;
		float frontBrakeAmount = brakeAmount * BrakeDistribution;
		float rearBrakeAmount = brakeAmount * (1f - BrakeDistribution);

		// Debug.Log("brake amount, front: " + frontBrakeAmount + ", rear: " + rearBrakeAmount);

		foreach (WheelCollider frontWheelCollider in FrontWheelColliders) {
			frontWheelCollider.brakeTorque = frontBrakeAmount;
		}

		foreach (WheelCollider rearWheelCollider in RearWheelColliders) {
			rearWheelCollider.brakeTorque = rearBrakeAmount;
		}


		SetDebugUIText(2, brakeBuffer.ToString("F2"));
	}
	private void ApplyHandbrakeTorque() {
		foreach (WheelCollider frontWheelCollider in FrontWheelColliders) {
			frontWheelCollider.brakeTorque = HandbrakeForce * handbrakeBuffer;
		}

		SetDebugUIText(6, handbrakeBuffer.ToString("F2"));
	}

	private void SetBraking(CallbackContext c) {
		float input = c.ReadValue<float>();
		brakeBuffer = BrakePedalCurve.EvaluateMirrored(input);

		SetDebugUIText(3, input.ToString("F2"));
	}
	private void SetHandbraking(CallbackContext c) {
		float input = c.ReadValue<float>();
		handbrakeBuffer = HandbrakePedalCurve.EvaluateMirrored(input);

		SetDebugUIText(7, input.ToString("F2"));
	}

	private void StopBraking(CallbackContext _) {
		brakeBuffer = 0f;
		SetDebugUIText(3);
	}

	private void StopHandbraking(CallbackContext _) {
		handbrakeBuffer = 0;

		SetDebugUIText(7);
	}

	#endregion

	#region Jumping, Hopping
	private void ApplyJump() {
		foreach (WheelCollider frontWheelCollider in FrontWheelColliders) {
			JointSpring spring = frontWheelCollider.suspensionSpring;
			spring.spring = springInit * (1f - jumpBuffer);
			frontWheelCollider.suspensionSpring = spring;
		}

		foreach (WheelCollider rearWheelCollider in RearWheelColliders) {
			JointSpring spring = rearWheelCollider.suspensionSpring;
			spring.spring = springInit * (1f - jumpBuffer);
			rearWheelCollider.suspensionSpring = spring;
		}


	}

	private void SetJump(CallbackContext c) {
		float input = c.ReadValue<float>();
		jumpBuffer = input;
		// ApplyJump();
	}

	private void ReleaseJump(CallbackContext _) {
		jumpBuffer = 0;
		// ApplyJump();
	}

	private void Jump(float dt) {
		ApplyJump();
	}
	#endregion

	#region Yaw, Pitch

	private void Yaw(float dt) {

		if (UseYawOffsetMode) {
			float delta = yawBuffer - oldYawBuffer;
			// TODO: interpolate joystick input?
			float yawAmount = YawSpeed * delta;
			rb.rotation *= Quaternion.Euler(0, yawAmount, 0);
		} else {
			float yawAmount = YawSpeed * yawBuffer * dt;

			if (yawAmount > 0) {
				foreach (var item in YawClockwiseTrails)
					item.emitting = true;
				foreach (var item in YawCounterClockwiseTrails)
					item.emitting = false;
			} else if (yawAmount < 0) {
				foreach (var item in YawClockwiseTrails)
					item.emitting = false;
				foreach (var item in YawCounterClockwiseTrails)
					item.emitting = true;
			} else { //if (pitchBuffer == 0) {
				foreach (var item in YawClockwiseTrails)
					item.emitting = false;
				foreach (var item in YawCounterClockwiseTrails)
					item.emitting = false;
			}


			rb.rotation *= Quaternion.Euler(0, yawAmount, 0);
		}

		oldYawBuffer = yawBuffer;
	}

	private void Pitch(float dt) {
		float pitchAmount = PitchSpeed * pitchBuffer * dt;

		/*
		if (pitchAmount > 0) {
			foreach (var item in PitchUpTrails)
				item.emitting = true;
			foreach (var item in PitchDownTrails)
				item.emitting = false;
		} else if (pitchAmount < 0) {
			foreach (var item in PitchUpTrails)
				item.emitting = false;
			foreach (var item in PitchDownTrails)
				item.emitting = true;
		} else if (yawBuffer == 0) {
			foreach (var item in PitchUpTrails)
				item.emitting = false;
			foreach (var item in PitchDownTrails)
				item.emitting = false;
		}
		// */

		rb.rotation *= Quaternion.Euler(pitchAmount, 0, 0);
	}

	private void SetYaw(CallbackContext c) {
		float input = c.ReadValue<float>();
		yawBuffer = SteeringCurve.EvaluateMirrored(input);
	}
	private void SetPitch(CallbackContext c) {
		float input = c.ReadValue<float>();
		pitchBuffer = SteeringCurve.EvaluateMirrored(input);
	}

	private void StopYaw(CallbackContext _) {
		yawBuffer = 0;
	}

	private void StopPitch(CallbackContext _) {
		pitchBuffer = 0;
	}

	#endregion

	#region Boost

	private void Boost(float dt) {
		if (!BoostNotEmpty) {
			StopBoost();
		}

		if (!boosting) {
			AddBoost(BoostFillRate * dt);
			return;
		}

		IsBoostTrailEmitting = true;
		AddBoost(-BoostConsumptionRate * dt);

		if (BoostNotEmpty)
			rb.AddRelativeForce(Vector3.forward * BoostSpeed, ForceMode.Acceleration);
		else
			boosting = false;

	}

	private void AddBoost(double amount) {
		boostAmount += amount;

		if (boostAmount > 1)
			boostAmount = 1;

		if (boostAmount < 0)
			boostAmount = 0;

		Color barColor = Color.white;
		if (boostAmount < MinBoostLevel)
			barColor = Color.grey;

		BoostBarUIScript.SetBarPercentage((float)boostAmount, barColor);

	}

	private void StartBoost(CallbackContext _) {
		if (boostAmount < MinBoostLevel)
			return;

		boosting = true;
	}

	private void StopBoost() {
		IsBoostTrailEmitting = false;
		boosting = false;
	}

	private void StopBoost(CallbackContext _) {
		StopBoost();
	}

	#endregion

	#endregion

	private void Reset(CallbackContext _) {
		Reset();
	}

	private void Reset() {
		if (LevelWorldScript.CurrentLevel != null) {
			Transform resetSpot = LevelWorldScript.CurrentLevel.TestRespawnSpot;

			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;

			rb.MovePosition(resetSpot.position);
			rb.MoveRotation(resetSpot.rotation);

			//Debug.Log("Reset car to test spot");
		}
	}

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


}
