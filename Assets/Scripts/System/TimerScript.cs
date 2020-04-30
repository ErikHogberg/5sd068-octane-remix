using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class TimerScript : MonoBehaviour {
	
	private static TimerScript instance;
	public static TimerScript Instance {
		get {
			if (instance == null) { instance = Instantiate(Resources.Load<TimerScript>("Timer")); }
			return instance;
		}
	}

	private float timer = 0.0f;
	private bool running = false;

	private GameObject timerUI;
	private TMP_Text timerText;

	public void StartTimer() { running = true; Debug.Log("Timer Started!"); }
	public void StopTimer() { running = false; Debug.Log("Timer Stopped: " + timer.ToString("F2")); }
	public void ResetTimer() { timer = 0.0f; }
	public float GetTime() { return timer; }
	public TimeSpan GetTimeSpan() { return TimeSpan.FromSeconds(timer); }

	public void DisplayTime() {
		if (timerUI == null) {
			timerUI = Instantiate(Resources.Load<GameObject>("TimerUI"));
			timerUI.transform.SetParent(CanvasFinder.thisCanvas.transform, false);
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


	//To avoid jittery number updates on the UI
	private int updateCount = 0;
	private int updateInterval = 3;

	void Update() {
		if (running) {
			timer += Time.deltaTime;

			if (timerUI != null) {
				updateCount++;
				if (updateCount >= updateInterval) {
					TimeSpan t = TimeSpan.FromSeconds(timer);
					int milli = t.Milliseconds / 10;
					timerText.text = TimeCalc(t.Hours) + ":" + TimeCalc(t.Minutes) + ":" +
									 TimeCalc(t.Seconds) + ":" + TimeCalc(milli);
					updateCount = 0;
				}
			}
		}
	}

	private string TimeCalc(int nr) {
		string ret = "";
		if (nr <= 9) { ret = "0" + nr.ToString("F0"); } else if (nr >= 99f) ret = "00";
		else ret = nr.ToString("F0");
		return ret;
	}

}
