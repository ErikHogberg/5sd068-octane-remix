using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeChildrenScript : ExplodeComponent {

	List<Rigidbody> childRBs = new List<Rigidbody>();

	public float ExplosionRadius = 1f;
	public float ExplosionForce = 1f;
	public float ExplosionUpForce = 1f;

	public Transform ExplosionCenter;

	public bool OnCollision = false;
	public bool OnTrigger = false;


	private void Awake() {
		GetComponentsInChildren<Rigidbody>(false, childRBs);
	}

	private void OnTriggerEnter(Collider other) {
		if (OnTrigger)
			Explode();
	}

	private void OnCollisionEnter(Collision other) {
		if (OnCollision)
			Explode();
	}

	public override void Explode() {
		foreach (var item in childRBs) {
			item.isKinematic = false;
			item.AddExplosionForce(
				ExplosionForce,
				ExplosionCenter.position,
				ExplosionRadius,
				ExplosionUpForce
			);
		}
	}

	public override void UndoExplode(){

	}

}
