using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class SegmentObstacleEditorUIScript : SegmentEditorSuperClass {

	// TODO: show current obstacle on segment
	// TODO: set current obstacle on segment
	// IDEA: disable editing if obstacles are disallowed on segment

	TMP_Dropdown obstacleDropdown;

	protected override void ChildAwake() {
		obstacleDropdown = GetComponent<TMP_Dropdown>();
		obstacleDropdown.onValueChanged.AddListener(ApplyDropdown);
	}

	public override void UpdateUI() {
		obstacleDropdown.ClearOptions();

		obstacleDropdown.options.Add(new TMP_Dropdown.OptionData("None"));

		var shownObject = currentSegment.Obstacles.ShownObject;

		int currentIndex = 0;
		for (int i = 0; i < currentSegment.Obstacles.objects.Count; i++) {
			var item = currentSegment.Obstacles.objects[i];
			obstacleDropdown.options.Add(new TMP_Dropdown.OptionData(item.Key));
			if (shownObject != null && shownObject.Key == item.Key)
				currentIndex = i + 1;
		}

		obstacleDropdown.value = currentIndex;
		ApplyDropdown();

		obstacleDropdown.RefreshShownValue();
	}

	public void ApplyDropdown(int i) {
		currentSegment.Obstacles.UnhideObject(
			obstacleDropdown.options[i].text
		);
	}

	public void ApplyDropdown() {
		ApplyDropdown(obstacleDropdown.value);
	}
}
