using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObstacleScript : MonoBehaviour {

	public float RespawnTime = 3f;
	private float timer = -1f;

	public Collider[] CollidersToDisable;

	private ExplodeComponent exploderinoThingie;
	public ExplodeComponent ExplodeComponentOverride;


	private void Awake() {
		if (ExplodeComponentOverride) {
			exploderinoThingie = ExplodeComponentOverride;
		} else {
			exploderinoThingie = GetComponent<ExplodeComponent>();
		}
	}

	private void Update() {
		if (timer < 0)
			return;

		timer -= Time.deltaTime;

		if (timer < 0)
			UndoExplode();
	}

	private void OnTriggerEnter(Collider other) {
		var car = other.GetComponent<SteeringScript>();
		if (!car)
			return;

		// NOTE: invuln. check is also done in integrity script to negate damage
		if (car.IsInvulnerable)
			Explode();

	}

	public void Explode() {
		foreach (var item in CollidersToDisable)
			item.enabled = false;

		exploderinoThingie.Explode();

		timer = RespawnTime;
	}

	public void UndoExplode() {
		foreach (var item in CollidersToDisable)
			item.enabled = true;

		exploderinoThingie.UndoExplode();
	}

}
