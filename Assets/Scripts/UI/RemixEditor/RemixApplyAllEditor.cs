using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RemixApplyAllEditor : MonoBehaviour {

	public TMP_Dropdown ObstacleDropdown;

	void Start() {
		RefreshDropdown();
	}

	public void RefreshDropdown() {
		List<string> obstacles = new List<string>();
		obstacles.Add("None");

		foreach (var segment in LevelPieceSuperClass.Segments) {
			foreach (var obstacleEntry in segment.Obstacles.objects) {
				if (!obstacles.Contains(obstacleEntry.Key)) {
					obstacles.Add(obstacleEntry.Key);
				}
			}
		}

		ObstacleDropdown.ClearOptions();
		ObstacleDropdown.AddOptions(obstacles);
	}

	public void Apply() {
		string selectedObstacle = ObstacleDropdown.options[ObstacleDropdown.value].text;

		// apply dropdown selection to all applicable segments, hide all obstacles on unapplicable
		foreach (var segment in LevelPieceSuperClass.Segments) {
			segment.Obstacles.UnhideObject(selectedObstacle);
		}

		SegmentEditorSuperClass.UpdateAllUI();
	}

	public void ShuffleAll() {
		// TODO: randomize obstacles on all segments
	}

}
