using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SoundCollisionTrigger : MonoBehaviour
{
	[Tooltip("The name/ID of the sound that should be played upon collision with this gameobject.")]
	public string soundName = "Blank";
	[Tooltip("Should the colliders of this gameobject and its children be triggers or not?")]
	public bool triggerCollider = true;

	//Array for all colliders on object
	private Collider[] col;

	private void Awake() {
		col = transform.GetComponentsInChildren<Collider>();
		foreach (Collider item in col) {
			item.isTrigger = triggerCollider;
		}
		//Debug.Log("Colliders: " + col.Length);
    }

	private void OnTriggerEnter(Collider other)
    {
		if (triggerCollider)
			SoundManager.PlaySound(soundName);
    }

	private void OnCollisionEnter(Collision collision)
    {
		if (!triggerCollider)
			SoundManager.PlaySound(soundName);
	}

}
