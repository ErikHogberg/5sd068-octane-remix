using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RemixEditorGoalPost : MonoBehaviour {
	public static List<RemixEditorGoalPost> Instances = new List<RemixEditorGoalPost>();

	public static RemixEditorGoalPost StartSpot = null;
	public static RemixEditorGoalPost FinishSpot = null;

	[Tooltip("If this should be the default start line when entering remix editor for the first time, only one can be marked or there will be no guarantee of which will be used")]
	public bool InitStart = false;
	[Tooltip("If this should be the default finish line when entering remix editor for the first time, only one can be marked or there will be no guarantee of which will be used")]
	public bool InitFinish = false;

	[Space]

	public GoalPostScript GoalPost;
	// TODO: portal entrance and exit

	public GameObject ObjectToToggle;
	[Tooltip("Where the car starts when starting a run from this goal line")]
	public Transform SpawnSpot;

	private void Awake() {
		Instances.Add(this);

		if (InitStart)
			StartSpot = this;
		if (InitFinish)
			FinishSpot = this;

		// if (GoalPost.gameObject.activeSelf) {
		// 	StartSpot = this;
		// }
	}

	private void OnDestroy() {
		Instances.Remove(this);
	}

	private void OnMouseOver() {

		if (EventSystem.current.IsPointerOverGameObject())
			return;

		if (Input.GetMouseButtonDown(0)) {
			RemixMapScript.Select(this);
		}

		if (Input.GetMouseButtonDown(1)) {
			RemixMapScript.StartRotate();
		}
	}
}
