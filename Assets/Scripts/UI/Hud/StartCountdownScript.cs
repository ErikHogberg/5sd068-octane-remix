using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StartCountdownScript : MonoBehaviour {

	private static StartCountdownScript mainInstance;

	public static bool IsShown => mainInstance?.gameObject.activeInHierarchy ?? false;

	public TMP_Text SecondText;
	public TMP_Text MillisecondText;
	public TMP_Text NotificationText;
	public Button StartButton;

	[Space]
	public InputActionReference GasBinding;
	public float MaxGasSpeed = 1f;

	[Space]
	public NeedleMeterUIScript GasNeedle;

	float timer = 3;
	bool running = false;

	float gasBuffer = 0;
	float currentGas = 0;

	bool firstStart = true;

	private void Awake() {
		mainInstance = this;
		// Debug.Log("gas action init");

		GasBinding.action.performed += Gas;
		GasBinding.action.canceled += Gas;

		GasBinding.action.Enable();
	}

	private void Gas(InputAction.CallbackContext c) {
		gasBuffer = c.ReadValue<float>();
		// Debug.Log("gas buffer input: " + gasBuffer);
	}

	private void OnEnable() {
		// Debug.Log("gas action enabled");
		GasBinding.action.Enable();
	}

	// private void OnDisable() {
	// Debug.Log("gas action disabled");
	// GasBinding.action.Disable();
	// }

	private void OnDestroy() {
		mainInstance = null;
	}

	void Start() {
		SteeringScript.FreezeCurrentCar();
		TimerScript.Instance.DisplayTime();
		UpdateUI();
	}

	void Update() {
		if (firstStart) {
			// Debug.Log("input system updated");
			// InputSystem.Update();

			currentGas = Mathf.MoveTowards(currentGas, gasBuffer, MaxGasSpeed * Time.unscaledDeltaTime);
			GasNeedle.SetTargetPercent(currentGas);

			if (!running && currentGas >= 1f - float.Epsilon) {
				StartCountdown();
			}

			if (running) {
				GasNeedle.SetTargetColor(NeedleMeterUIScript.ColorState.MAX);
			} else if (GasBinding.action.triggered) {
				GasNeedle.SetTargetColor(NeedleMeterUIScript.ColorState.BOOST);
			} else {
				GasNeedle.SetTargetColor(NeedleMeterUIScript.ColorState.NORMAL);
			}
			GasNeedle.ApplyColor();
			GasNeedle.UpdateBarPercentage();
		}

		if (!running)
			return;

		timer -= Time.unscaledDeltaTime;
		// Debug.Log("counting down: " + timer + ", " + Time.deltaTime.ToString("000000"));



		UpdateUI();

		if (timer < 0) {
			running = false;

			if (firstStart) {
				UINotificationSystem.Notify("Go!", Color.green, 2);
				TimerScript.Instance.StartTimer();
			}
			firstStart = false;
			GasNeedle?.transform.parent.gameObject.SetActive(false);
			StartButton?.gameObject.SetActive(false);

			gameObject.SetActive(false);
			if (!PauseScript.IsPaused)
				SteeringScript.UnfreezeCurrentCar();
		}

	}

	void UpdateUI() {
		SecondText.text = timer.ToString("0");
		MillisecondText.text = "." + ((timer % 1f) * 100f).ToString("00");
	}

	public void StartCountdown() {
		// firstStart = false;
		// GasNeedle?.transform.parent.gameObject.SetActive(false);
		// StartButton?.gameObject.SetActive(false);
		running = true;
	}

	public void StartPenaltyCountdown(float time = 3f) {
		SteeringScript.FreezeCurrentCar();
		// Physics.Simulate(.1f);
		NotificationText.text = "Reset Penalty";
		timer = time;
		running = true;
		TimerScript.Instance.AddTime(time);
		gameObject.SetActive(true);
	}

	public static void StartPenaltyCountdownStatic(float time = 3f) {
		mainInstance?.StartPenaltyCountdown(time);
	}

}
