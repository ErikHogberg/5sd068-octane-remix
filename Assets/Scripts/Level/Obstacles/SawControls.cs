using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawControls : MonoBehaviour
{
	private GameObject sawBlade;
	private float currentSpeed;
	private bool start_stop_coroutine_running = false;

	[Tooltip("The maximum rotation speed of the saw blade")]
	public float sawMaxSpeed = 300.0f;

	[Tooltip("The time in seconds it takes for the saw blade to reach max speed after being still, and vice versa")]
	public float sawRevDuration = 1.0f;

	[Tooltip("Curve to represent the rate at which the saw revvs up to max speed after being still, and vice versa")]
	public AnimationCurve sawRevRate;


	void Start()
    {
		sawBlade = transform.Find("SawBlade").gameObject;
		StartRotating();
	}

	void FixedUpdate()
    {
		float rotate = currentSpeed * Time.deltaTime;
		sawBlade.transform.Rotate(0f, rotate, 0f, Space.Self);
	}

	public void LogHit()
    {
		Debug.Log("Saw hit!");
    }



	public void StartRotating() {
		if (!start_stop_coroutine_running)
			StartCoroutine(StartStop(true));
    }

	public void StopRotating() {
		if (!start_stop_coroutine_running)
			StartCoroutine(StartStop(false));
	}

	//start rotating = true 
	//stop rotating = false
	private IEnumerator StartStop(bool toggle)
	{
		start_stop_coroutine_running = true;
		float timer = 0.0f;

		if (toggle == true) {
			while (currentSpeed < sawMaxSpeed)
			{
				timer += Time.deltaTime;
				float percentOfMaxSpeed = sawRevRate.Evaluate(timer / sawRevDuration);
				currentSpeed = percentOfMaxSpeed * sawMaxSpeed;
				yield return null;
			}
			currentSpeed = sawMaxSpeed;
		} 
		else {
			while (currentSpeed > 0.0f)
			{
				timer += Time.deltaTime;
				float percentOfMaxSpeed = sawRevRate.Evaluate(1.0f - (timer / sawRevDuration));
				currentSpeed = percentOfMaxSpeed * sawMaxSpeed;
				yield return null;
			}
			currentSpeed = 0.0f;
		}
		start_stop_coroutine_running = false;
		yield break;
	}
}
