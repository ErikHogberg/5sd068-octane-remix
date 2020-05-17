using UnityEngine;

public class CarCam2 : MonoBehaviour {

	// [Tooltip("Max allowed yaw difference between camera car direction, in degrees")]
	// public float MaxYaw = 45f;

	// public float TurnSpeed = 1f;
	[Tooltip("How fast the camera moves towards straight behind the car")]
	public float FollowSpeed = 10f;
	// TODO: velocity curve for turn/follow speed increase

	[Space]

	[Tooltip("How many degrees the camera is rotates when steering is at max angle")]
	public float SteeringMax = 20f;
	[Tooltip("How fast the camera follow the steering angle when the steering angle is at max angle")]
	public float SteeringFollowSpeed = 2f;
	[Tooltip("How much of the steering follow speed is used when the steering is 0")]
	[Range(0, 1)]
	public float MinSteeringFollowMultiplier = 0.2f;
	[Tooltip("If the steering rotation is around car up axis instead of world up axis")]
	public bool SteeringRelativeToCar = true;

	[Space]

	[Tooltip("How much the camera is allowed to move horizontally")]
	[Min(0)]
	public float HorizontalBuffer;
	[Tooltip("How much the camera is allowed to move vertically, min and max")]
	public Vector2 VerticalBuffer;
	[Tooltip("How much the camera is allowed to zoom, in and out")]
	public Vector2 ZoomBuffer;

	[Space]

	public SteeringScript Car;
	public Transform CameraPivot;
	public Transform CameraLookat;

	public Transform Parent;

	Vector3 initPos;
	float steeringBuffer = 0;

	void Start() {
		initPos = Quaternion.Inverse(CameraPivot.rotation) * (transform.position - CameraPivot.position);

		if (Parent) {
			transform.parent = Parent;
		} else {
			transform.parent = Car.transform.parent;
		}
	}

	void LateUpdate() {

		var carForward = CameraPivot.forward;
		var velocityForward = Car.Velocity;

		float steering = Car.GetSteering();

		Vector3 targetPos = CameraPivot.rotation * initPos;

		// TODO: counteract/subtract car turn delta from camera turn follow
		// TODO: reduce boost pad rotation change camera snapping
		// IDEA: add snapping threshold for detecting jerky motion, use different logic in those cases

		// TODO: separate follow speed for each axis

		transform.position = Vector3.MoveTowards(transform.position, targetPos + CameraPivot.position, FollowSpeed * Time.deltaTime);

		Vector3 currentPos = Quaternion.Inverse(CameraPivot.rotation) * (transform.position - CameraPivot.position);
		currentPos.x = Mathf.Clamp(currentPos.x, initPos.x - HorizontalBuffer, initPos.x + HorizontalBuffer);
		currentPos.y = Mathf.Clamp(currentPos.y, initPos.y + VerticalBuffer.x, initPos.y + VerticalBuffer.y);
		currentPos.z = Mathf.Clamp(currentPos.z, initPos.z + ZoomBuffer.x, initPos.z + ZoomBuffer.y);
		transform.position = CameraPivot.rotation * currentPos + CameraPivot.position;

		transform.LookAt(CameraLookat);

		// steeringBuffer = Mathf.MoveTowardsAngle(steeringBuffer, steering, SteeringFollowSpeed * Time.deltaTime);
		steeringBuffer = Mathf.LerpAngle(steeringBuffer, steering, SteeringFollowSpeed * Mathf.Clamp(Mathf.Abs(steering), MinSteeringFollowMultiplier, 1f) * Time.deltaTime);
		if (SteeringRelativeToCar) {
			transform.rotation = Quaternion.AngleAxis(SteeringMax * steeringBuffer, CameraPivot.up) * transform.rotation;
		} else {
			transform.rotation = Quaternion.AngleAxis(SteeringMax * steeringBuffer, Vector3.up) * transform.rotation;
		}

		// transform.RotateAround(CameraFocus.position, CameraFocus.up, TurnSpeed*Time.deltaTime);


	}
}
