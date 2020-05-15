using UnityEngine;

public class CarCam : MonoBehaviour {
	[Header("Camera Rotation")]
	[Tooltip("If car speed is below this value, then the camera will default to looking forwards.")]
	public float RotationSpeedThreshold = 3f;

	[Tooltip("If the angle between the car's facing direction and velocity exceeds this number, the camera will default to looking forwards")]
	public float RotationThreshold = 25.0f;

	[Tooltip("What angle in comparison to the car's forward vector will the camera move towards while the car is turning?")]
	public float CarSteeringAdaptation = 50.0f;

	[Tooltip("How closely the camera matches the car's velocity vector. The lower the value, the smoother the camera rotations, but too much results in not being able to see where you're going.")]
	public float CameraRotationSpeed = 5.0f;

	[Header("Camera FOV")]
	public bool UseFOVFluctuation;

	[Tooltip("The relationship between camera FOV and the car's velocity")]
	public AnimationCurve VelocityToFOV;

	[Range(0, 140)]
	public float MinCameraFOV = 60.0f;
	[Range(0, 140)]
	public float MaxCameraFOV = 80.0f;

	[Header("Camera Lag")]
	[Tooltip("The relationship between how much the camera lags behind the car and the car's velocity")]
	public AnimationCurve VelocityToLag;

	[Tooltip("How closely the camera follows the car's position. This value determines the least it will ever lag behind.")]
	[Range(3, 50)]
	public float MinCameraLag = 8.0f;

	[Tooltip("How closely the camera follows the car's position. This value determines the most it will ever lag behind.")]
	[Range(3, 50)]
	public float MaxCameraLag = 15.0f;

	[Header("Follow Target")]
	[Tooltip("The car that this particular camera will follow, but not necessary at this stage")]
	public GameObject Car;

	public GameObject OptionalParent;

	public float SteeringMaxOffset = 1f;
	public float MaxOffsetRotationSpeed = 1f;
	private float steeringBuffer = 0f;

	// TODO: camera yaw and pitch offset
	// IDEA: just override lookat target

	//The transform of the camera object
	private Transform rootNode;

	//A gameobject which represents the position in relation to the car that the camera always tries to return to
	private Transform cameraLock;

	//The car's associated Rigidbody component
	private Rigidbody carPhysics;

	//The car's associated SteeringScript
	private SteeringScript carControls;

	//Variables for handling smooth variation in camera rotation speed
	private float currCamRotationSpeed;
	private float goalCamRotationSpeed;
	private float camRotationSpeedLerp = 3f;

	//The current effective camera lag which depends on the min/max values set in editor
	private float cameraLag;

	//Variables for handling smooth variation in steering
	private float steeringValue = 0.0f;
	private float effectiveSteerValue = 0.0f;
	private float steerValueLerp = 5f;

	//The vector between the camera lock and the car itself, for stability purposes
	private Vector3 lockToCar;


	void Awake() {
		rootNode = GetComponent<Transform>();
		//Temporary measure until we all collectively adapt our prefabs and put a car reference in this script
		if (Car == null) {
			cameraLock = rootNode.parent.GetComponent<Transform>();
			Car = cameraLock.parent.gameObject;
		} else {
			cameraLock = Car.transform.Find("CameraLock");
		}

		carPhysics = Car.GetComponent<Rigidbody>();
		carControls = Car.GetComponent<SteeringScript>();
		currCamRotationSpeed = CameraRotationSpeed;

	}

	void Start() {
		// Detach the camera so that it can move freely on its own.
		if (OptionalParent)
			rootNode.parent = OptionalParent.transform;
		else
			rootNode.parent = null;

	}

	private void Update() {
		
	}

	void FixedUpdate() {

		// TODO: separate interpolation and camera positon update logic from target position update logic to get camera movement as smooth as framerate, instead of same as physics step

		//Uses an animation curve to adjust at what rate the camera lags behind the car
		//by comparing its current speed to its specified top speed
		float percentOfTopSpeed = carPhysics.velocity.sqrMagnitude / (carControls.VelocityCap * carControls.VelocityCap);
		percentOfTopSpeed = Mathf.Clamp(percentOfTopSpeed, 0.0f, 1.0f);

		float diffCameraLag = MaxCameraLag - MinCameraLag;
		float calcCameraLag = diffCameraLag * VelocityToLag.Evaluate(percentOfTopSpeed);
		cameraLag = MinCameraLag + calcCameraLag;

		if (UseFOVFluctuation) {
			float diffFOV = MaxCameraFOV - MinCameraFOV;
			float calcFOV = diffFOV * VelocityToFOV.Evaluate(percentOfTopSpeed);
			gameObject.GetComponent<Camera>().fieldOfView = MinCameraFOV + calcFOV;
		}
		// Moves the camera to match the car's position.
		rootNode.position = Vector3.Lerp(rootNode.position, cameraLock.position, cameraLag * Time.fixedDeltaTime);



		//Gets steering values from car
		float steer = carControls.GetSteering();
		float yaw = carControls.GetYaw();
		if (steer != 0.0f)
			steeringValue = steer;
		else if (yaw != 0.0f)
			steeringValue = yaw;

		//Lerping steer value to make a smoother transition between the min and max possible values
		effectiveSteerValue = Mathf.Lerp(effectiveSteerValue, steeringValue, steerValueLerp * Time.fixedDeltaTime);
		//Debug.Log(steeringValue + " " + effectiveSteerValue );

		float lookAngleComparison = Mathf.Acos(
		//Dot product
			((cameraLock.forward.x * carPhysics.velocity.x) + (cameraLock.forward.z * carPhysics.velocity.z)) /
			(cameraLock.forward.magnitude * carPhysics.velocity.magnitude)
		) * Mathf.Rad2Deg;

		Quaternion look;
		lockToCar = (Car.transform.position - rootNode.transform.position);
		//Y-value adjustment for a view that looks a bit more forward than down
		lockToCar.y += 4f;

		//Different behaviors for the camera, depending on what the car is doing

		// If the car isn't moving, default to looking forwards. Prevents camera from freaking out with a zero velocity getting put into a Quaternion.LookRotation
		if (carPhysics.velocity.magnitude < RotationSpeedThreshold) {
			look = Quaternion.LookRotation(cameraLock.forward + lockToCar);
			goalCamRotationSpeed = 2.0f;
		}
		//If the car is turning very fast, this will counteract the camera severely de-synching with the direction of the car
		else if (lookAngleComparison > RotationThreshold) {
			look = Quaternion.LookRotation((Quaternion.AngleAxis(CarSteeringAdaptation * effectiveSteerValue, Car.transform.up) * cameraLock.forward) + lockToCar);
			goalCamRotationSpeed = 2.0f;
			//Debug.Log("Angle Threshold: " + lookAngleComparison);
		}
		//If the player is steering in a certain direction, the camera will look slightly towards that direction
		else if (steeringValue != 0.0f) {
			look = Quaternion.LookRotation((Quaternion.AngleAxis(CarSteeringAdaptation * effectiveSteerValue, Car.transform.up) * cameraLock.forward) + lockToCar);
			goalCamRotationSpeed = 2.0f;
			steeringValue = 0.0f;
			//Debug.Log("Steering Look");
		}
		//Looking direction based on the car's velocity vector
		else {
			look = Quaternion.LookRotation((Quaternion.AngleAxis(CarSteeringAdaptation * effectiveSteerValue, Car.transform.up) * carPhysics.velocity.normalized) + lockToCar);
			goalCamRotationSpeed = CameraRotationSpeed;
			//Debug.Log("Velocity Look");
		}
		//Lerping changes in camera rotation speed, for stability purposes
		currCamRotationSpeed = Mathf.Lerp(currCamRotationSpeed, goalCamRotationSpeed, camRotationSpeedLerp * Time.fixedDeltaTime);

		//Rotate the camera towards the velocity vector.
		look = Quaternion.Slerp(rootNode.rotation, look, currCamRotationSpeed * Time.fixedDeltaTime);

		steeringBuffer = Mathf.MoveTowards(steeringBuffer, steeringValue, MaxOffsetRotationSpeed * Time.deltaTime);
		var steeringOffset = Quaternion.AngleAxis(steeringBuffer * SteeringMaxOffset, Car.transform.up);
		
		rootNode.rotation = steeringOffset * look ;
		// rootNode.Rotate(Vector3.up, effectiveSteerValue * SteeringMaxOffset, Space.Self);

	}
}
