using UnityEngine;

public interface IFollowScript {
	void SetFollowTarget(Transform target);
}

public class GenericFollowPositionScript : MonoBehaviour, IFollowScript {

	public Transform Target;

	private Vector3 initPos;

	public bool AllowBuffer = false;
	public Vector3 Buffer;

	private void Start() {
		initPos = transform.position - Target.transform.position;
	}

	private void LateUpdate() {
		var targetPos = Target.transform.position + initPos;

		if (!AllowBuffer) {
			transform.position = targetPos;
			return;
		}

		Vector3 delta = targetPos - transform.position;
		Vector3 resultPos = transform.position;

		if (delta.x < -Buffer.x) {
			resultPos.x = targetPos.x + Buffer.x;
		} else if (delta.x > Buffer.x) {
			resultPos.x = targetPos.x - Buffer.x;
		}

		if (delta.y < -Buffer.y) {
			resultPos.y = targetPos.y + Buffer.y;
		} else if (delta.y > Buffer.y) {
			resultPos.y = targetPos.y - Buffer.y;
		}

		if (delta.z < -Buffer.z) {
			resultPos.z = targetPos.z + Buffer.z;
		} else if (delta.z > Buffer.z) {
			resultPos.z = targetPos.z - Buffer.z;
		}

		transform.position = resultPos;

	}

	public void SetFollowTarget(Transform target) {
		Target = target;
	}

}
