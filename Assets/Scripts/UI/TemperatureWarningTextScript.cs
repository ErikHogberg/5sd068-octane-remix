using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericTextFlashingScript))]
public class TemperatureWarningTextScript : MonoBehaviour {

	public static TemperatureWarningTextScript MainInstance;

	private GenericTextFlashingScript flashingScript;

	private void Awake() {
		flashingScript = GetComponent<GenericTextFlashingScript>();
		MainInstance = this;
		Hide();
	}

	private void OnDestroy() {
		MainInstance = null;
	}

	public static void SetFlashRate(float flashRate) {
		if (!MainInstance)
			return;

		MainInstance.flashingScript.SetFlashRate(flashRate);
	}

	public static void Hide() {
		if (MainInstance) 
			MainInstance.gameObject.SetActive(false);
	}

	public static void Show() {
		if (MainInstance) 
			MainInstance.gameObject.SetActive(true);
	}

}
