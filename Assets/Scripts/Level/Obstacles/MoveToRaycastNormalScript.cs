using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveToRaycastNormalScript : MonoBehaviour {

	public enum UpDirection {
		Up,
		Down,
		Left,
		Right,
		Forwards,
		Backwards
	}

	public UpDirection ObjectUp = UpDirection.Up;

	public float Range = 10f;
	public LayerMask LaserLayerMask;

	// TODO: choose ray direction, relative to self?

	void Start() {

		Vector3 up = -transform.up;
		switch (ObjectUp) {
			case UpDirection.Up:
				// up = -transform.up;
				break;
			case UpDirection.Down:
				up = transform.up;
				break;
			case UpDirection.Left:
				up = transform.right;
				break;
			case UpDirection.Right:
				up = -transform.right;
				break;
			case UpDirection.Forwards:
				up = -transform.forward;
				break;
			case UpDirection.Backwards:
				up = transform.forward;
				break;
		}

		RaycastHit[] hits = Physics.RaycastAll(
			transform.position,
			up,
			Range,
			LaserLayerMask,
			QueryTriggerInteraction.Ignore
		);

		// Debug.DrawRay(transform.position, transform.forward);

		if (hits.Length < 1) {
			// Debug.LogWarning("Rock " + gameObject.name + " found no ground!");
			return;
		}

		float min = hits.Select(hit => hit.distance).Min();
		RaycastHit closest = hits.Where(hit => hit.distance == min).First();
		// Debug.Log("hit " + closest.rigidbody.gameObject.name);

		// closest.normal
		transform.position = closest.point;
		switch (ObjectUp) {
			case UpDirection.Up:
				transform.up = closest.normal;
				break;
			case UpDirection.Down:
				transform.up = -closest.normal;
				break;
			case UpDirection.Left:
				transform.right = -closest.normal;
				break;
			case UpDirection.Right:
				transform.right = closest.normal;
				break;
			case UpDirection.Forwards:
				transform.forward = closest.normal;
				break;
			case UpDirection.Backwards:
				transform.forward = -closest.normal;
				break;
		}
		// Debug.Log("Rock " + gameObject.name + " successfully moved to ground");


	}

}
