using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BarUIScript))]
public class IntegrityUIScript : MonoBehaviour
{
	private BarUIScript bar;
	[Tooltip("A secondary bar with the purpose of more clearly showing how much damage every hit does.")]
	public Image delayBar;
	[Tooltip("How many seconds should the delay bar wait before starting to move towards the actual bar's value.")]
	public float delaySeconds = 1.0f;
	[Tooltip("Once the delay bar starts draining, how fast should it drain?")]
	public float delayDrainSpeed = 4.0f;

	private bool draining = false;
	private IEnumerator currentDrain;

	void Awake() {
		bar = GetComponent<BarUIScript>();
	}

	public void SetIntegPercentage(float percentage) {
		if (bar == null) {
			Debug.Log("IntegrityeUIScript: " + transform.parent.name + " is missing a BarUIScript component");
			return;
		}

		bar.SetBarPercentage(percentage);
		if (draining) {
			StopCoroutine(currentDrain);
			draining = false;
		}
		currentDrain = DelayBarDrain();
		StartCoroutine(currentDrain);

		/*if (color.HasValue) 
			bar.SetColor(color.Value);*/
	}

	IEnumerator DelayBarDrain()
    {
		draining = true;
		yield return new WaitForSeconds(delaySeconds);
		while (delayBar.fillAmount > (bar.GetFillAmount() + 0.001)) {
			delayBar.fillAmount = Mathf.Lerp(delayBar.fillAmount, bar.GetFillAmount(), delayDrainSpeed * Time.deltaTime);
			yield return null;
		}
		delayBar.fillAmount = bar.GetFillAmount();
		draining = false;
		yield break;
	}
}
