using System;
using UnityEngine;
using TMPro;

public class TimerScript : MonoBehaviour {

	private static TimerScript instance = null;
	public static TimerScript Instance => instance ?? (instance = Instantiate(Resources.Load<TimerScript>("Timer")));

	private float timer = 0.0f;
	private string timeTxt = "";
	private bool running = false;

	private GameObject timerUI;
	private TMP_Text timerText;

	[Tooltip("If the time counter should be instantiated as child of this object instead of the canvas")]
	public bool ParentCounterToThis = false;

	public void StartTimer() { running = true; Debug.Log("Timer Started!"); }
	public void StopTimer() { running = false; Debug.Log("Timer Stopped: " + timer.ToString("F2")); }
	public void ResetTimer() { timer = 0.0f; UnityEngine.Debug.Log("Timer Reset!"); }
	public void AddTime(float penalty) { timer += penalty; }
	public float TimeProgress => timer;
	public TimeSpan TimeSpanProgress => System.TimeSpan.FromSeconds(timer);

	private void Awake() {
		Debug.LogWarning("Created new Timer!");
		if (!instance) {
			instance = this;
		}

	}

	public void DisplayTime() {
		if (timerUI == null) {
			timerUI = Instantiate(Resources.Load<GameObject>("TimerUI"));
			if (ParentCounterToThis) {
				timerUI.transform.SetParent(this.transform, false);
			} else {
				timerUI.transform.SetParent(CanvasFinder.thisCanvas.transform, false);
			}
			timerText = timerUI.transform.GetChild(0).GetComponent<TMP_Text>();
		}
		timerUI.SetActive(true);
	}

	public void HideTime() {
		if (timerUI == null) {
			Debug.Log("TimerScript: Timer UI cannot be hidden because it has not yet been instantiated.");
			return;
		}
		timerUI.SetActive(false);
	}

	public float GetTimeNr() { return timer; }
	public string GetTimeTxt() { return timeTxt; }


	//To avoid jittery number updates on the UI
	private int updateCount = 0;
	private int updateInterval = 3;

	void Update() {
		if (running) {
			timer += UnityEngine.Time.deltaTime;

			if (timerUI != null) {
				updateCount++;
				if (updateCount >= updateInterval) {
					TimeSpan t = System.TimeSpan.FromSeconds(timer);
					int milli = t.Milliseconds / 10;
					timerText.text = TimeCalc(t.Hours) + ":" + TimeCalc(t.Minutes) + ":" +
									 TimeCalc(t.Seconds) + ":" + TimeCalc(milli);
					timeTxt = timerText.text;
					updateCount = 0;
					// Debug.LogWarning("has timer UI!");
				}
			}

		}
	}

	public static string TimeCalc(int nr, bool singleDigit = false) {
		string ret = "";
		if (nr <= 9) {
			if (singleDigit) {
				ret = nr.ToString("F0");
			} else {
				ret = "0" + nr.ToString("F0");
			}
		} else if (nr >= 99f) {
			if (singleDigit) {
				ret = "0";
			} else {
				ret = "00";
			}
		} else {
			ret = nr.ToString("F0");
		};

		return ret;
	}

}
