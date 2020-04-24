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

		foreach (var item in currentSegment.Obstacles.objects)
			obstacleDropdown.options.Add(new TMP_Dropdown.OptionData(item.Key));

		obstacleDropdown.value = 0;
		ApplyDropdown();
		obstacleDropdown.RefreshShownValue();
	}

	public void ApplyDropdown(int i) {
		currentSegment.Obstacles.UnhideObject(
			//currentSegment.Obstacles.objects[i].Key
			obstacleDropdown.options[i].text
		);
	}

	public void ApplyDropdown() {
		ApplyDropdown(obstacleDropdown.value);
	}
}
