using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObstacleScript : MonoBehaviour {

	private ExplodeComponent exploderinoThingie;


	public ExplodeComponent ExplodeComponentOverride;

	public bool ResetToPrefab = false; // TODO: reset to prefab when respawning if this is true

	public Collider[] CollidersToDisable;

	// TODO: respawn timer
	// private float timer;

	private void Awake() {
		if (ExplodeComponentOverride) {
			exploderinoThingie = ExplodeComponentOverride;
		} else {
			exploderinoThingie = GetComponent<ExplodeComponent>();
		}
	}

	private void OnTriggerEnter(Collider other) {
		var car = other.GetComponent<SteeringScript>();
		if (!car)
			return;

		// TODO: if invulnerable
		if (car.IsInvulnerable)
			Explode();


	}

	public void Explode() {
		foreach (var item in CollidersToDisable)
			item.enabled = false;

		exploderinoThingie.Explode();
	}

	public void UndoExplode() {
		foreach (var item in CollidersToDisable)
			item.enabled = true;

		exploderinoThingie.UndoExplode();
	}

}
