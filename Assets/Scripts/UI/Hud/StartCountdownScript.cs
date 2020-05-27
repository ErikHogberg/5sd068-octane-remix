using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartCountdownScript : MonoBehaviour {

	public TMP_Text SecondText;
	public TMP_Text MillisecondText;
	public TMP_Text NotificationText;

	float timer = 3;
	bool running = false;

	void Start() {
		SteeringScript.FreezeCurrentCar();
		UpdateUI();
	}

	void Update() {
		if (!running)
			return;

		timer -= Time.unscaledDeltaTime;
		// Debug.Log("counting down: " + timer + ", " + Time.deltaTime.ToString("000000"));

		UpdateUI();

		if (timer < 0) {
			UINotificationSystem.Notify("Go!", Color.green, 2);
			running = false;
			gameObject.SetActive(false);
			SteeringScript.UnfreezeCurrentCar();
		}

	}

	void UpdateUI() {
		SecondText.text = timer.ToString("0");
		MillisecondText.text = "." + ((timer % 1f) * 100f).ToString("00");
	}

	public void StartCountdown() {
		running = true;
	}

	public void StartPenaltyCountdown() {
		NotificationText.text = "Reset Penalty";
		running = true;
		gameObject.SetActive(true);

	}

}
