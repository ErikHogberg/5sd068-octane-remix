using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;


public class LaserRaycastScript : MonoBehaviour {

	// TODO: alpha gradient fade out when hitting only air

	[Header("Required Objects")]
	public GameObject LaserCylinder;
	public ParticleSystem LaserParticleSystem;

	[Header("Settings")]
	public float Range = 100f;
	public LayerMask LaserLayerMask;
	public float DamagePerSecond = 100f;
	[Tooltip("Show laser even when not hitting anything?")]
	public bool ShowLaser = true;

	private Vector3 lastTarget;

	private bool HitLastFrame = true;

	void Update() {

		RaycastHit[] hits = Physics.RaycastAll(
			transform.position,
			transform.forward,
			Range,
			LaserLayerMask,
			QueryTriggerInteraction.Ignore
		);

		// Debug.DrawRay(transform.position, transform.forward);

		if (hits.Length == 0) {
			if (HitLastFrame) {
				HitLastFrame = false;
				LaserParticleSystem.Stop();
				if (ShowLaser) {
					Vector3 scale = LaserCylinder.transform.localScale;
					scale.z = Range * .5f;
					LaserCylinder.transform.localScale = scale;
				} else {
					LaserCylinder.SetActive(false);
				}
			}
			return;
		}

		if (!HitLastFrame) {
			HitLastFrame = true;
			LaserParticleSystem.Play();
			if (!ShowLaser) {
				LaserCylinder.SetActive(true);
			}
		}

		float min = hits.Select(hit => hit.distance).Min();
		RaycastHit closest = hits.Where(hit => hit.distance == min).First();
		// Debug.Log("hit " + closest.rigidbody.gameObject.name);

		{
			Vector3 scale = LaserCylinder.transform.localScale;
			scale.z = min * .5f;
			LaserCylinder.transform.localScale = scale;
		}

		LaserParticleSystem.transform.position = closest.point;

		// TODO: reduce getcomponent calls
		if(closest.collider.gameObject.TryGetComponent<TemperatureAndIntegrity>(out TemperatureAndIntegrity car)){
			car.LaserHit();
		}

	}
}
