using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarRBHandler : MonoBehaviour
{
	public static CarRBHandler Instance;

	private Rigidbody rb;
	private bool timing = false;

	void OnEnable() { 
		Instance = this;
		rb = GetComponent<Rigidbody>();
	}


	public void FreezeRBPosition() {
		rb.constraints = RigidbodyConstraints.FreezePosition;
	}
	public void FreezeRBPosition(float duration) {
		if (timing == false) {
			FreezeRBPosition();
			StartCoroutine(Timer(duration));
		}
		else Debug.Log("CarHandler: Already freeze timing");
	}

	public void FreezeRBRotation() {
		rb.constraints = RigidbodyConstraints.FreezeRotation;
	}
	public void FreezeRBRotation(float duration) {
		if (timing == false) {
			FreezeRBRotation();
			StartCoroutine(Timer(duration));
		}
		else Debug.Log("CarHandler: Already freeze timing");
	}

	public void FreezeRB() {
		rb.constraints = RigidbodyConstraints.FreezeAll;
	}
	public void FreezeRB(float duration) {
		if (timing == false) {
			FreezeRB();
			StartCoroutine(Timer(duration));
		}
		else Debug.Log("CarHandler: Already freeze timing");
	}

	public void UnfreezeRB() {
		rb.constraints = RigidbodyConstraints.None;
	}

	IEnumerator Timer(float duration)
	{
		timing = true;
		yield return new WaitForSeconds(duration);
		UnfreezeRB();
		timing = false;
	}
}
