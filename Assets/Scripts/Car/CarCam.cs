using UnityEngine;

public class CarCam : MonoBehaviour
{
	[Header("Camera Rotation")]
	[Tooltip("If car speed is below this value, then the camera will default to looking forwards.")]
    public float rotationSpeedThreshold = 7f;

	[Tooltip("If the angle between the car's facing direction and velocity exceeds this number, the camera will default to looking forwards")]
	public float rotationThreshold = 25.0f;

	[Tooltip("How closely the camera matches the car's velocity vector. The lower the value, the smoother the camera rotations, but too much results in not being able to see where you're going.")]
	public float cameraRotationSpeed = 5.0f;

	[Header("Camera FOV")]
	public bool useFOVFluctuation;

	[Tooltip("The relationship between camera FOV and the car's velocity")]
	public AnimationCurve velocityToFOV;

	[Range(0, 140)]
	public float minCameraFOV = 60.0f;
	[Range(0, 140)]
	public float maxCameraFOV = 80.0f;

	[Header("Camera Lag")]
	[Tooltip("The relationship between how much the camera lags behind the car and the car's velocity")]
	public AnimationCurve velocityToLag;

	[Tooltip("How closely the camera follows the car's position. This value determines the least it will ever lag behind.")]
	[Range(3, 50)]
	public float minCameraLag = 8.0f;

	[Tooltip("How closely the camera follows the car's position. This value determines the most it will ever lag behind.")]
	[Range(3, 50)]
	public float maxCameraLag = 15.0f;


	Transform rootNode;
	Transform cameraLock;
	Rigidbody carPhysics;
	SteeringScript carControls;
	float currCamRotationSpeed;
	float cameraLag;

	void Awake() {		
        rootNode = GetComponent<Transform>();
        cameraLock = rootNode.parent.GetComponent<Transform>();
        carPhysics = cameraLock.parent.GetComponent<Rigidbody>();
		carControls = cameraLock.parent.GetComponent<SteeringScript>();
		currCamRotationSpeed = cameraRotationSpeed;

    }

    void Start() {
        // Detach the camera so that it can move freely on its own.
        rootNode.parent = null;

    }

    void FixedUpdate() {

		//Uses an animation curve to adjust at what rate the camera lags behind the car
		//by comparing its current speed to its specified top speed
		float percentOfTopSpeed = carPhysics.velocity.sqrMagnitude / (carControls.VelocityCap * carControls.VelocityCap);
		percentOfTopSpeed = Mathf.Clamp(percentOfTopSpeed, 0.0f, 1.0f);

		float diffCameraLag = maxCameraLag - minCameraLag;
		float calcCameraLag = diffCameraLag * velocityToLag.Evaluate(percentOfTopSpeed);
		cameraLag = minCameraLag + calcCameraLag;

		if (useFOVFluctuation) {
			float diffFOV = maxCameraFOV - minCameraFOV;
			float calcFOV = diffFOV * velocityToFOV.Evaluate(percentOfTopSpeed);
			gameObject.GetComponent<Camera>().fieldOfView = minCameraFOV + calcFOV;
		}

		//Debug.Log(cameraLag);
		//Debug.Log(minCameraFOV + calcFOV);

		Quaternion look;
        // Moves the camera to match the car's position.
        rootNode.position = Vector3.Lerp(rootNode.position, cameraLock.position, cameraLag * Time.fixedDeltaTime);


		float lookAngleComparison = Mathf.Acos(
			//Dot product
			((cameraLock.forward.x * carPhysics.velocity.x) + (cameraLock.forward.z * carPhysics.velocity.z))/ 
			(cameraLock.forward.magnitude * carPhysics.velocity.magnitude)) * Mathf.Rad2Deg;

		// If the car isn't moving, default to looking forwards. Prevents camera from freaking out with a zero velocity getting put into a Quaternion.LookRotation
		if (carPhysics.velocity.magnitude < rotationSpeedThreshold) {
			look = Quaternion.LookRotation(cameraLock.forward);
			currCamRotationSpeed = 2.0f;
		}
		//If the car is turning very fast, this will counteract the camera severely de-synching with the direction of the car
		else if (lookAngleComparison > rotationThreshold) {
			look = Quaternion.LookRotation(cameraLock.forward);
			currCamRotationSpeed = 2.0f;
			//Debug.Log("Angle Threshold: " + lookAngleComparison);
		}
		else {
			look = Quaternion.LookRotation(carPhysics.velocity.normalized);
			currCamRotationSpeed = cameraRotationSpeed;
			//Debug.Log("Velocity Look");
		}
        
        //Rotate the camera towards the velocity vector.
        look = Quaternion.Slerp(rootNode.rotation, look, currCamRotationSpeed * Time.fixedDeltaTime);                
        rootNode.rotation = look;

    }
}
