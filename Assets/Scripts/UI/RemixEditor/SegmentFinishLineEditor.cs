using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SegmentFinishLineEditor : SegmentEditorSuperClass {

	public Toggle StartToggle;
	public Toggle EndToggle;

	public TMP_Text text;

	protected override void ChildAwake() {

	}

	public override void UpdateUI() {
		
		StartToggle.isOn = currentSegment.isStart;
		StartToggle.interactable = !currentSegment.isStart && currentSegment.IsStartable;
		
		EndToggle.isOn = currentSegment.isEnd;
		EndToggle.interactable = !currentSegment.isEnd && currentSegment.IsEndable;

		if (currentSegment.isStart || currentSegment.isEnd) {
			text.enabled = true;
		} else {
			text.enabled = false;
		}

	}

	public void StartOnToggle(bool value) {
		if (!currentSegment.IsStartable)
			return;
		currentSegment.isStart = true;
	}

	public void EndOnToggle(bool value) {
		if (!currentSegment.IsEndable)
			return;
		currentSegment.isEnd = true;
	}

}
