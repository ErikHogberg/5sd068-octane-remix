using UnityEngine;

public class CarCam2 : MonoBehaviour {

	[Tooltip("Max allowed yaw difference between camera car direction, in degrees")]
	public float MaxYaw = 45f;

	// public float TurnSpeed = 1f;
	public float FollowSpeed = 1f;
	// TODO: velocity curve for turn/follow speed increase

	public Vector3 Buffer;

	public SteeringScript car;
	public Transform CameraFocus;

	public Transform Parent;


	Vector3 initPos;

	void Start() {
		initPos = Quaternion.Inverse(CameraFocus.rotation) * (transform.position - CameraFocus.position);

		transform.parent = Parent;
	}

	void FixedUpdate() {

		// }

		// void LateUpdate() {
		var carForward = CameraFocus.forward;
		var velocityForward = car.Velocity;

		float steering = car.GetSteering();


		Vector3 targetPos = CameraFocus.rotation * initPos;// + CameraFocus.position;

		// transform.position = targetPos;
		
		// TODO: counteract/subtract car turn delta from camera turn follow

		transform.position = Vector3.MoveTowards(transform.position, targetPos + CameraFocus.position, FollowSpeed * Time.deltaTime);

		// Vector3 currentPos = transform.position;
		Vector3 currentPos = Quaternion.Inverse(CameraFocus.rotation) * (transform.position - CameraFocus.position);
		currentPos.x = Mathf.Clamp(currentPos.x, initPos.x - Buffer.x, initPos.x + Buffer.x);
		currentPos.y = Mathf.Clamp(currentPos.y, initPos.y - Buffer.y, initPos.y + Buffer.y);
		currentPos.z = Mathf.Clamp(currentPos.z, initPos.z - Buffer.z, initPos.z + Buffer.z);
		// transform.position = currentPos;
		transform.position = CameraFocus.rotation * currentPos + CameraFocus.position;

		transform.LookAt(CameraFocus);

		// transform.RotateAround(CameraFocus.position, CameraFocus.up, TurnSpeed*Time.deltaTime);



	}
}
