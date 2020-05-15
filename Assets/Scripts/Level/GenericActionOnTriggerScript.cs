using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericActionOnTriggerScript : MonoBehaviour
{
	// public GameObject trailerCamera;

	public UnityEvent Actions;

	private void OnCollisionEnter(Collision other) {

		Actions.Invoke();

	}

	private void OnTriggerEnter(Collider other) {
		Actions.Invoke();
		
	}

}
