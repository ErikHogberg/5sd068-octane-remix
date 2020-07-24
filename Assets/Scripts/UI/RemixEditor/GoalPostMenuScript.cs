﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalPostMenuScript : MonoBehaviour {

	private static GoalPostMenuScript mainInstance;

	public Toggle StartToggle;
	public Toggle FinishToggle;

	private void Awake() {
		mainInstance = this;
	}

	public void UpdateUI() {
		bool isStart = RemixEditorGoalPost.StartSpot == GoalSpotListScript.CurrentGoalPost;
		StartToggle.SetIsOnWithoutNotify(isStart);
		// StartToggle.isOn = isStart;
		StartToggle.interactable = !isStart;

		bool isFinish = RemixEditorGoalPost.FinishSpot == GoalSpotListScript.CurrentGoalPost;
		// FinishToggle.isOn = isFinish;
		FinishToggle.SetIsOnWithoutNotify(isFinish);
		FinishToggle.interactable = !isFinish;
	}

	public static void Show() {
		if (!mainInstance)
			return;

		ObstacleListScript.Hide();
		mainInstance.gameObject.SetActive(true);
		mainInstance.UpdateUI();
	}

	public static void Hide() {
		if (!mainInstance)
			return;

		mainInstance.gameObject.SetActive(false);
	}

	public void OnStartToggle(bool value) {
		if (GoalSpotListScript.CurrentGoalPost == null)
			return;

		if (value) {
			RemixEditorGoalPost.StartSpot = GoalSpotListScript.CurrentGoalPost;
			StartToggle.interactable = false;
			RemixEditorGoalPost.UpdateGoalPost();
		}
	}

	public void OnFinishToggle(bool value) {
		if (GoalSpotListScript.CurrentGoalPost == null)
			return;

		if (value) {
			RemixEditorGoalPost.FinishSpot = GoalSpotListScript.CurrentGoalPost;
			FinishToggle.interactable = false;
			RemixEditorGoalPost.UpdateGoalPost();
		}
	}

}
