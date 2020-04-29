using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SegmentEditorSuperClass : MonoBehaviour {

	private static List<SegmentEditorSuperClass> segmentEditors = new List<SegmentEditorSuperClass>();

	protected LevelPieceSuperClass currentSegment;

	// TODO: separate editor super class for editing portals, in-between segments

	protected abstract void ChildAwake();

	protected void Awake() {
		segmentEditors.Add(this);
		ChildAwake();
	}

	protected void OnDestroy() {
		segmentEditors.Remove(this);
	}

	public abstract void UpdateUI();

	public void SetSegment(LevelPieceSuperClass segment) {
		currentSegment = segment;

		UpdateUI();
	}

	public static void SetSegmentsOnAll(LevelPieceSuperClass segment) {
		foreach (var editor in segmentEditors)
			editor.SetSegment(segment);
	}

}
