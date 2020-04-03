using UnityEngine;

public class CarCam : MonoBehaviour
{
    Transform rootNode;
    Transform carLock;
    Rigidbody carPhysics;

    [Tooltip("If car speed is below this value, then the camera will default to looking forwards.")]
    public float rotationSpeedThreshold = 7f;

	[Tooltip("If the angle between the car's facing direction and velocity exceeds this number, the camera will default to looking forwards")]
	public float rotationThreshold = 25.0f;

	[Tooltip("How closely the camera follows the car's position. The lower the value, the more the camera will lag behind.")]
    public float cameraStickiness = 10.0f;
    
    [Tooltip("How closely the camera matches the car's velocity vector. The lower the value, the smoother the camera rotations, but too much results in not being able to see where you're going.")]
    public float cameraRotationSpeed = 5.0f;

	float currCamRotationSpeed;

    void Awake()
    {		
        rootNode = GetComponent<Transform>();
        carLock = rootNode.parent.GetComponent<Transform>();
        carPhysics = carLock.parent.GetComponent<Rigidbody>();
		currCamRotationSpeed = cameraRotationSpeed;
    }

    void Start()
    {
        // Detach the camera so that it can move freely on its own.
        rootNode.parent = null;
    }

    void FixedUpdate()
    {
        Quaternion look;

        // Moves the camera to match the car's position.
        rootNode.position = Vector3.Lerp(rootNode.position, carLock.position, cameraStickiness * Time.fixedDeltaTime);


		float lookAngleComparison = Mathf.Acos(
			//Dot product
			((carLock.forward.x * carPhysics.velocity.x) + (carLock.forward.z * carPhysics.velocity.z))/ 
			(carLock.forward.magnitude * carPhysics.velocity.magnitude)) * Mathf.Rad2Deg;

		// If the car isn't moving, default to looking forwards. Prevents camera from freaking out with a zero velocity getting put into a Quaternion.LookRotation
		if (carPhysics.velocity.magnitude < rotationSpeedThreshold)
		{
			look = Quaternion.LookRotation(carLock.forward);
			currCamRotationSpeed = 2.0f;
		}

		//If the car is turning very fast, this will counteract the camera severely de-synching with the direction of the car
		else if (lookAngleComparison > rotationThreshold)
		{
			look = Quaternion.LookRotation(carLock.forward);
			currCamRotationSpeed = 2.0f;
			//Debug.Log("Angle Threshold: " + lookAngleComparison);
		}
		else {
			look = Quaternion.LookRotation(carPhysics.velocity.normalized);
			currCamRotationSpeed = cameraRotationSpeed;
			//Debug.Log("Velocity Look");
		}
        
        // Rotate the camera towards the velocity vector.
        look = Quaternion.Slerp(rootNode.rotation, look, currCamRotationSpeed * Time.fixedDeltaTime);                
        rootNode.rotation = look; 
    }
}
