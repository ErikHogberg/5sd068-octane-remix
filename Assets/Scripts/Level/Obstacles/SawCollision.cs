using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawCollision : MonoBehaviour
{
	private SawControls saw;

	void Start()
	{
		saw = transform.parent.GetComponent<SawControls>();

	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Saw hit! " + other.transform.name);

	}
}
