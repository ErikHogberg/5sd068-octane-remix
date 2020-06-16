using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : MonoBehaviour {

	public Transform Exit;

	[Tooltip("Optional segment that the car needs to have made contact with immediatly before entering")]
	public LevelPieceSuperClass Segment;
	// TODO: dynamically set (and unset) exit portals segment to allow entry from the entry portal segment, instead of triggering a reset

	[HideInInspector]
	public List<IObserver<PortalScript>> Observers = new List<IObserver<PortalScript>>();

	private void OnTriggerEnter(Collider other) {

		if (Segment && !LevelPieceSuperClass.CheckCurrentSegment(Segment)) {
			LevelPieceSuperClass.ResetToCurrentSegment();
			return;
		}

		foreach (var item in Observers)
			item.Notify(this);

		var rb = other.attachedRigidbody;

		rb.MovePosition(Exit.position);
		rb.MoveRotation(Exit.rotation);

		rb.velocity = Exit.rotation * Vector3.forward * rb.velocity.magnitude;

		ResetTransition.StartTransition();

	}

}
