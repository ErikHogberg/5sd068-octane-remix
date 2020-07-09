using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RemixEditorGoalPost : MonoBehaviour {
	public static List<RemixEditorGoalPost> Instances = new List<RemixEditorGoalPost>();
	public static RemixEditorGoalPost StartSpot;
	public static RemixEditorGoalPost FinishSpot;

	public GoalPostScript GoalPost;
	public GameObject ObjectToToggle;
	[Tooltip("Where the car starts when starting a run from this goal line")]
	public Transform SpawnSpot;

	private void Awake() {
		Instances.Add(this);

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
