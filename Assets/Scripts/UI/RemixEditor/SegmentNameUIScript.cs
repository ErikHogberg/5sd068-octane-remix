using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class SegmentNameUIScript : SegmentEditorSuperClass {

	// TODO: show current obstacle on segment
	// TODO: set current obstacle on segment
	// IDEA: disable editing if obstacles are disallowed on segment

	protected override void ChildAwake() {

	}

	public override void UpdateUI() {
		GetComponent<TMP_Text>().text = LevelPieceSuperClass.CurrentSegment.gameObject.name;
	}

	public void SetDropdown() {

	}

}
