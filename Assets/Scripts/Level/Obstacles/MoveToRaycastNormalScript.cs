using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveToRaycastNormalScript : MonoBehaviour {

	public float Range = 10f;
	public LayerMask LaserLayerMask;

	// TODO: choose ray direction, relative to self?

	void Start() {
		RaycastHit[] hits = Physics.RaycastAll(
			transform.position,
			-transform.up,
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
		transform.up = closest.normal;
		// Debug.Log("Rock " + gameObject.name + " successfully moved to ground");


	}

}
