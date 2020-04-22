using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentObstacleEditorUIScript : SegmentEditorSuperClass {

	// TODO: show current obstacle on segment
	// TODO: set current obstacle on segment
	// IDEA: disable editing if obstacles are disallowed on segment

	

	public override void UpdateUI() {
		GetComponent<TMPro.TMP_Text>().text = currentSegment.gameObject.name;
	}

	public void SetDropdown(){

	}

}
