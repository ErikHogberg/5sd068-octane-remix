using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SegmentEditorSuperClass : MonoBehaviour {

	private static List<SegmentEditorSuperClass> segmentEditors = new List<SegmentEditorSuperClass>();

	// TODO: make static?
	protected LevelPieceSuperClass currentSegment = null;

	// TODO: separate editor super class for toggling objects without segments, such as ramps

	protected abstract void ChildAwake();

	protected void Awake() {
		segmentEditors.Add(this);
		ChildAwake();
	}

	protected void OnDestroy() {
		segmentEditors.Remove(this);
	}

	public abstract void UpdateUI();

	public static void UpdateAllUI() {
		foreach (var item in segmentEditors) 
			item.UpdateUI();
	}

	public void SetSegment(LevelPieceSuperClass segment) {
		currentSegment = segment;
		UpdateUI();
	}

	public static void SetSegmentsOnAll(LevelPieceSuperClass segment) {
		foreach (var editor in segmentEditors)
			editor.SetSegment(segment);
	}

}
