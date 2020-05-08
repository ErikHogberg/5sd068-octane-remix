using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BarUIScript))]
public class IntegrityUIScript : MonoBehaviour
{

	// TODO: List of instances, ordered by player index, for 2+ player split screen
	public static IntegrityUIScript MainInstance;

	private BarUIScript bar;
	[Tooltip("A secondary bar with the purpose of more clearly showing how much damage every hit does.")]
	public Image delayBar;
	[Tooltip("How many seconds should the delay bar wait before starting to move towards the actual bar's value.")]
	public float delaySeconds = 1.0f;
	[Tooltip("Once the delay bar starts draining, how fast should it drain?")]
	public float delayDrainSpeed = 4.0f;

	private BarUIScript delayBarScript;
	private bool draining = false;
	private IEnumerator currentDrain;

	void Awake() {
		MainInstance = this;
		bar = GetComponent<BarUIScript>();
		delayBarScript = delayBar.GetComponent<BarUIScript>();
	}

	void OnDestroy(){
		MainInstance = null;
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

	private float DelayPercent()
    {
		if (delayBarScript == null) { return delayBar.fillAmount; }
		else return delayBarScript.GetFillAmount();
	}

	private void SetPercent(float percent)
    {
		if (delayBarScript == null) delayBar.fillAmount = percent;
		else delayBarScript.SetBarPercentage(percent);
	}

	IEnumerator DelayBarDrain()
    {
		draining = true;
		yield return new WaitForSeconds(delaySeconds);
		while (DelayPercent() > (bar.GetFillAmount() + 0.001)) {
			float percent = Mathf.Lerp(DelayPercent(), bar.GetFillAmount(), delayDrainSpeed * Time.deltaTime);
			SetPercent(percent);
			yield return null;
		}
		SetPercent(bar.GetFillAmount());
		draining = false;
		yield break;
	}
}
