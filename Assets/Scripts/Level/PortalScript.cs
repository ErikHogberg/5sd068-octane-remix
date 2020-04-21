using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : MonoBehaviour
{

	public Transform Exit;

	private void OnTriggerEnter(Collider other) {
		other.attachedRigidbody.MovePosition(Exit.position);
		other.attachedRigidbody.MoveRotation(Exit.rotation);
	}

}
