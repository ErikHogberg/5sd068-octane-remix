using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SegmentFinishLineEditor : SegmentEditorSuperClass {

	public Toggle StartToggle;
	public Toggle EndToggle;

	public TMP_Text IsSelectedText;
	public TMP_Text IsNotAllowedText;

	protected override void ChildAwake() {

	}

	public override void UpdateUI() {

		// Debug.Log("start: "+ startse);

		StartToggle.isOn = currentSegment.isStart;
		StartToggle.interactable = !currentSegment.isStart && currentSegment.IsStartable;

		EndToggle.isOn = currentSegment.isEnd;
		EndToggle.interactable = !currentSegment.isEnd && currentSegment.IsEndable;

		IsSelectedText.enabled = true;
		if (currentSegment.isStart || currentSegment.isEnd) {
			IsSelectedText.text = "Choose another start or end segment to deselect this one";
		} else if (currentSegment.isStart) {
			IsSelectedText.text = "Choose another start segment to deselect this one";
		} else if (currentSegment.isEnd) {
			IsSelectedText.text = "Choose another end segment to deselect this one";
		} else {
			IsSelectedText.enabled = false;
		}

		IsNotAllowedText.enabled = true;
		if (!currentSegment.IsStartable && !currentSegment.IsEndable) {
			IsNotAllowedText.text = "This segment does not allow a start or finish line";
		} else if (!currentSegment.IsStartable) {
			IsNotAllowedText.text = "This segment does not allow a start line";
		} else if (!currentSegment.IsEndable) {
			IsNotAllowedText.text = "This segment does not allow a finish line";
		} else {
			IsNotAllowedText.enabled = false;
		}

	}

	public void StartOnToggle(bool value) {
		if (!currentSegment.IsStartable)
			return;
		currentSegment.isStart = value;
		UpdateUI();
	}

	public void EndOnToggle(bool value) {
		if (!currentSegment.IsEndable)
			return;
		currentSegment.isEnd = value;
		UpdateUI();
	}

}
