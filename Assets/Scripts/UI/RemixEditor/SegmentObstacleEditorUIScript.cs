using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class SegmentObstacleEditorUIScript : MonoBehaviour {

	// TODO: show current obstacle on segment
	// TODO: set current obstacle on segment
	// IDEA: disable editing if obstacles are disallowed on segment

	TMP_Dropdown obstacleDropdown;

	protected void Awake() {
		obstacleDropdown = GetComponent<TMP_Dropdown>();
		obstacleDropdown.onValueChanged.AddListener(ApplyDropdown);
	}

	public void UpdateUI() {
		obstacleDropdown.ClearOptions();

		obstacleDropdown.options.Add(new TMP_Dropdown.OptionData("None"));

		var shownObject = LevelPieceSuperClass.CurrentSegment.Obstacles.ShownObject;

		int currentIndex = 0;
		for (int i = 0; i < LevelPieceSuperClass.CurrentSegment.Obstacles.objects.Count; i++) {
			var item = LevelPieceSuperClass.CurrentSegment.Obstacles.objects[i];
			obstacleDropdown.options.Add(new TMP_Dropdown.OptionData(item.Key));
			if (shownObject != null && shownObject.Key == item.Key)
				currentIndex = i + 1;
		}

		obstacleDropdown.value = currentIndex;
		ApplyDropdown();

		obstacleDropdown.RefreshShownValue();
		obstacleDropdown.interactable = true;

	}

	public void ApplyDropdown(int i) {
		LevelPieceSuperClass.CurrentSegment.Obstacles.UnhideObject(
			obstacleDropdown.options[i].text
		);
	}

	public void ApplyDropdown() {
		ApplyDropdown(obstacleDropdown.value);
	}
}
