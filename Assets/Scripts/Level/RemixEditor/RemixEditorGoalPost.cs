using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class RemixEditorGoalPost : MonoBehaviour, IComparable<RemixEditorGoalPost> {
	public static List<RemixEditorGoalPost> Instances = new List<RemixEditorGoalPost>();

	public static RemixEditorGoalPost StartSpot = null;
	public static RemixEditorGoalPost FinishSpot = null;

	[Tooltip("Order of the object in the remix editor list")]
	public int RemixEditorOrder;
	[Space]

	[Tooltip("If this should be the default start line when entering remix editor for the first time, only one can be marked or there will be no guarantee of which will be used")]
	public bool InitStart = false;
	[Tooltip("If this should be the default finish line when entering remix editor for the first time, only one can be marked or there will be no guarantee of which will be used")]
	public bool InitFinish = false;

	[Space]

	public Transform GoalPost;
	// TODO: portal entrance and exit
	// IDEA: have same object for goal post and goal portal entrance, just toggle effect volume and use different ontrigger branches, place goal post with disabled trigger as portal exit?

	[Tooltip("Which previous segments the player is allowed to cross this finish line from")]
	public int[] AllowedPreviousSegments;
	[Tooltip("Where the car starts when starting a run from this goal line")]
	public Transform SpawnSpot;

	private void Awake() {
		Instances.Add(this);
		Instances.Sort();

		if (InitStart)
			StartSpot = this;
		if (InitFinish)
			FinishSpot = this;

		// if (GoalPost.gameObject.activeSelf) {
		// 	StartSpot = this;
		// }

		if (FinishSpot)
			UpdateGoalPost();

		GoalPost.gameObject.SetActive(false);
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

	public static bool CheckTransition(LevelPieceSuperClass targetSegment) {
		if (!targetSegment) {
			print("target segment null in check transition");
			return false;
		}

		if (!FinishSpot) {
			print("finish spot null in check transition");
		}

		// TODO: use centerline to check transition instead
		// return FinishSpot?.AllowedPreviousSegments.Contains(targetSegment.SegmentOrder) ?? true;
		return true;
	}

	public static bool AttemptTransition(LevelPieceSuperClass targetSegment) {
		if (targetSegment == null)
			return true;

		bool success = CheckTransition(targetSegment);

		if (success) {
			if (FinishSpot != StartSpot) {
				MoveCarToStart();
			}
		} else {
			// LevelPieceSuperClass.ResetToCurrentSegment();
		}

		return success;
	}

	public static void MoveCarToStart() {
		SteeringScript.MainInstance?.Teleport(StartSpot.SpawnSpot.position, StartSpot.SpawnSpot.rotation);
		LevelPieceSuperClass.ClearCurrentSegment(notifyLeaving: false);
	}

	public static void UpdateGoalPost() {
		if (StartSpot && !FinishSpot)
			FinishSpot = StartSpot;
		if (!StartSpot && FinishSpot)
			StartSpot = FinishSpot;

		if (!FinishSpot) {
			// Debug.LogError("no finish spot assigned");
			return;
		}

		if (StartSpot == FinishSpot) {
			// Single finish line
			GoalPostScript.SetInstanceGoalPost(StartSpot);
		} else {
			// Portal
			GoalPostScript.SetInstanceGoalPortal(FinishSpot, StartSpot);
		}
	}

	public int CompareTo(RemixEditorGoalPost other) {
		return RemixEditorOrder - other.RemixEditorOrder;
	}
}
