using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameCollision : MonoBehaviour {
	private void OnTriggerEnter(Collider other) {
		Debug.Log("Flame hit! " + other.transform.name);
	}
}
