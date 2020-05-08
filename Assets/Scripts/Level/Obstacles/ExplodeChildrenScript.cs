using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component that takes all rigidbodies in its children, makes them non-kinematic, 
// and sends them flying with explosive force.
public class ExplodeChildrenScript : ExplodeComponent {

	List<Rigidbody> childRBs = new List<Rigidbody>();

	[Tooltip("How far around the explosion center the children will be affected by the explosion force")]
	public float ExplosionRadius = 1f;
	[Tooltip("How strong the explosion is")]
	public float ExplosionForce = 1f;
	[Tooltip("How much the explosion force should be adjusted upwards, regardless of position relative to explosion center")]
	public float ExplosionUpForce = 1f;

	[Tooltip("Optional explosion center object, will use the center of this object if left empty")]
	public Transform ExplosionCenter;

	[Tooltip("If the explosion should be triggered by collision with a collider on this object")]
	public bool OnCollision = false;
	[Tooltip("If the explosion should be triggered by trigger collision with a collider on this object")]
	public bool OnTrigger = false;


	private void Awake() {
		GetComponentsInChildren<Rigidbody>(false, childRBs);
		if (!ExplosionCenter)
			ExplosionCenter = transform;
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
