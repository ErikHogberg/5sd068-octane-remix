using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericFollowPositionScript : MonoBehaviour {

	public GameObject ObjectToFollow;

	private Vector3 initPos;

	public bool AllowBuffer = false;
	public Vector3 Buffer;

	private void Start() {
		initPos = transform.position - ObjectToFollow.transform.position;
	}

	private void LateUpdate() {
		var targetPos = ObjectToFollow.transform.position + initPos;

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
			resultPos.y = targetPos.y + Buffer.x;
		} else if (delta.y > Buffer.y) {
			resultPos.y = targetPos.y - Buffer.x;
		}

		if (delta.z < -Buffer.z) {
			resultPos.z = targetPos.z + Buffer.x;
		} else if (delta.z > Buffer.z) {
			resultPos.z = targetPos.z - Buffer.x;
		}

		transform.position = resultPos;

	}

}
