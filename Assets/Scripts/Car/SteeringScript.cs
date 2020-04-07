using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

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


	[Header("Key bindings")]
	public InputActionReference SteeringKeyBinding;
	public InputActionReference GasKeyBinding;
	public InputActionReference BrakeKeyBinding;
	public InputActionReference HandbrakeKeyBinding;
	public InputActionReference JumpKeyBinding;


	public InputActionReference YawKeyBinding;
	public InputActionReference PitchKeyBinding;

	public InputActionReference ResetKeyBinding;


	// TODO: reset car orientation?
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

	[Tooltip("Reduces the max steering angle as the car speeds up")]
	public bool EnableNarrowing = true;
	[Tooltip("Reduces the max steering angle as the car speeds up, reaching its narrowest angle at this speed")]
	[Min(0)]
	public float MaxNarrowingSpeed = 100;
	[Tooltip("Reduces the max steering angle as the car speeds up, the angle narrowing at the rate set on this curve, 1.0 on the X axis is the max narrowing speed, 1.0 on the Y axis is the normal max steering angle")]
	public AnimationCurve SteeringNarrowingCurve;

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
	public float DownwardForce = 10f;
	public ForceMode DownwardForceMode = ForceMode.Acceleration;
	[Tooltip("Wether or not the force direction should be relative to the car orientation instead of world.")]
	public bool UseRelativeDownwardForce = true;

	[Header("In-air controls")]
	public float YawSpeed = 100f;
	public AnimationCurve YawInputCurve;
	public float PitchSpeed = 100f;
	public AnimationCurve PitchInputCurve;


	// input buffers
	private float steeringBuffer = 0f;
	private float gasBuffer = 0f;
	private float lastAppliedGasValue = 0f;
	private float brakeBuffer = 0f;
	private float handbrakeBuffer = 0f;
	private float jumpBuffer = 0f;

	private float yawBuffer = 0f;
	private float pitchBuffer = 0f;


	private float[] wheelRotationBuffers;

	private Rigidbody rb;
	private float springInit;

	void Start() {
		rb = GetComponent<Rigidbody>();

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
		BrakeKeyBinding.action.performed += SetBraking;
		HandbrakeKeyBinding.action.performed += SetHandbraking;
		JumpKeyBinding.action.performed += SetJump;
		YawKeyBinding.action.performed += SetYaw;
		PitchKeyBinding.action.performed += SetPitch;

		ResetKeyBinding.action.performed += Reset;

		// adds release actions
		SteeringKeyBinding.action.canceled += StopSteering;
		GasKeyBinding.action.canceled += StopGas;
		BrakeKeyBinding.action.canceled += StopBraking;
		HandbrakeKeyBinding.action.canceled += StopHandbraking;
		JumpKeyBinding.action.performed += ReleaseJump;
		YawKeyBinding.action.performed += StopYaw;
		PitchKeyBinding.action.performed += StopPitch;
	}

	private void EnableInput() {
		SteeringKeyBinding.action.Enable();
		GasKeyBinding.action.Enable();
		BrakeKeyBinding.action.Enable();
		HandbrakeKeyBinding.action.Enable();
		JumpKeyBinding.action.Enable();
		YawKeyBinding.action.Enable();
		PitchKeyBinding.action.Enable();
		ResetKeyBinding.action.Enable();
	}

	private void DisableInput() {
		SteeringKeyBinding.action.Disable();
		GasKeyBinding.action.Disable();
		BrakeKeyBinding.action.Disable();
		HandbrakeKeyBinding.action.Disable();
		JumpKeyBinding.action.Disable();
		YawKeyBinding.action.Disable();
		PitchKeyBinding.action.Disable();
		ResetKeyBinding.action.Disable();
	}


	void OnEnable() {
		EnableInput();

		// TODO: enable/disable controls when losing window focus, pausing, etc.
	}

	void OnDisable() {
		DisableInput();
	}

	void Update() {
	}

	void FixedUpdate() {
		float dt = Time.deltaTime;

		if (EnableDownwardForce) {
			if (UseRelativeDownwardForce) {
				rb.AddRelativeForce(Vector3.down * DownwardForce, DownwardForceMode);
			} else {
				rb.AddForce(Vector3.down * DownwardForce, DownwardForceMode);
			}
		}

		Steer(dt);
		Gas(dt);
		Brake(dt);
		Handbrake(dt);

		// TODO: only allow yaw/pitch controls if in-air (or upside-down?)
		Yaw(dt);
		Pitch(dt);

		Jump(dt);

		ApplyAnimations();
	}

	#region Input callbacks
	#region Steering

	private void Steer(float dt) {
		// TODO: in-air movement

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
			if (sqrVelocity < sqrMaxNarrowingSpeed) {
				speedProgress -= sqrVelocity / sqrMaxNarrowingSpeed;
			}

			narrowing = SteeringNarrowingCurve.Evaluate(speedProgress);
			SetDebugUIText(10, speedProgress.ToString("F2"));
		}

		SetDebugUIText(9, narrowing.ToString("F2"));


		float steeringAmount = steeringBuffer * SteeringMax * narrowing;
		foreach (WheelCollider FrontWheelCollider in FrontWheelColliders) {
			FrontWheelCollider.steerAngle = steeringAmount;
		}

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

				foreach (WheelCollider frontWheelCollider in FrontWheelColliders) {
					frontWheelCollider.motorTorque = gasAmount;
				}

				break;
			case TractionMode.RearTraction:
				gasAmount /= RearWheelColliders.Count;

				foreach (WheelCollider rearWheelCollider in RearWheelColliders) {
					rearWheelCollider.motorTorque = GasSpeed * gasBuffer;
				}

				break;
			case TractionMode.FourWheelTraction:
				gasAmount /= FrontWheelColliders.Count + RearWheelColliders.Count;

				foreach (WheelCollider rearWheelCollider in RearWheelColliders) {
					rearWheelCollider.motorTorque = GasSpeed * gasBuffer;
				}

				foreach (WheelCollider frontWheelCollider in FrontWheelColliders) {
					frontWheelCollider.motorTorque = GasSpeed * gasBuffer;
				}

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
		float yawAmount = YawSpeed * yawBuffer * dt;
		// rb.angularVelocity += Vector3.up * yawAmount;
		rb.AddRelativeTorque(Vector3.up * yawAmount);
		// rb.AddForceAtPosition(Vector3.up * yawAmount, rb.position + Vector3.forward);

	}

	private void Pitch(float dt) {
		float pitchAmount = PitchSpeed * pitchBuffer * dt;
		// rb.angularVelocity += Vector3.right * pitchAmount;
		rb.AddRelativeTorque(Vector3.right * pitchAmount);
		// rb.AddForceAtPosition(Vector3.right * pitchAmount, rb.position + Vector3.forward);
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

	#endregion

	private void Reset(CallbackContext _) {
		Reset();
	}

	private void Reset() {
		if (LevelWorldScript.CurrentLevel != null) {
			rb.MovePosition(LevelWorldScript.CurrentLevel.TestRespawnSpot.transform.position);
			rb.MoveRotation(Quaternion.identity);
			Debug.Log("Reset car to test spot");
		}
	}

	private void SetDebugUIText(int index, string text = "0.00") {
		if (DebugUIScript.MainInstance == null)
			return;

		DebugUIScript.MainInstance.SetText(text, index);
	}

	private void ApplyAnimations() {

		int frontWheelCount = FrontWheelColliders.Count;


		for (int i = 0; i < frontWheelCount; i++) {
			WheelCollider frontWheelCollider = FrontWheelColliders[i];
			GameObject frontWheelModel = FrontWheelModels[i];

			wheelRotationBuffers[i] += frontWheelCollider.rpm * 360f;
			wheelRotationBuffers[i] %= 360f;
			float rotBuffer = wheelRotationBuffers[i];

			float angle = frontWheelCollider.steerAngle;
			Quaternion rotation = Quaternion.Euler(0, angle, 0);// * Quaternion.Euler(rotBuffer, 0, 0);

			frontWheelModel.transform.localRotation = Quaternion.Euler(0, angle, 0);
		}

		// for (int i = 0; i < RearWheelColliders.Count; i++) {
		// 	WheelCollider rearWheelCollider = RearWheelColliders[i];
		// 	wheelRotationBuffers[frontWheelCount + i] += rearWheelCollider.rpm * 360f;
		// 	wheelRotationBuffers[frontWheelCount + i] %= 360f;

		// 	float rotBuffer = wheelRotationBuffers[frontWheelCount + i];

		// 	Quaternion rotation = Quaternion.Euler(rotBuffer, 0, 0);

		// 	rearWheelCollider.gameObject.transform.localRotation = rotation;

		// }
	}

}
