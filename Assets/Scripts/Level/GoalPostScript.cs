﻿using UnityEngine;

public class GoalPostScript : MonoBehaviour, IObserver<LevelPieceSuperClass> {

	public static GoalPostScript MainInstance;

	public bool IsMainInstance = false;

	public GameObject ContainerObject;
	public GameObject PortalExit;
	public GameObject PortalEffects;

	private bool ready = true;

	private void Awake() {
		if (IsMainInstance) {
			MainInstance = this;
		} else {
			ContainerObject.SetActive(false);
		}
		// if (ParentSegment)
		// 	ParentSegment.LeaveSegmentObservers.Add(this);
		LevelPieceSuperClass.LeaveSegmentObservers.Add(this);
		print("leave observer registered");

		RemixEditorGoalPost.UpdateGoalPost();
	}

	private void OnDestroy() {
		if (IsMainInstance) {
			MainInstance = null;
		}
		LevelPieceSuperClass.LeaveSegmentObservers.Remove(this);
	}

	private bool readyOnAnyNextNotify = false;

	private void OnTriggerEnter(Collider other) {
		if (!ready) {

			print("goal post not ready!");
			return;
		}

		ready = false;

		if (!RemixEditorGoalPost.AttemptTransition(LevelPieceSuperClass.CurrentSegment)) {
			print("invalid goal post transition!");
			// LevelPieceSuperClass.ResetToCurrentSegment();
			ready = true;
			return;
		} else {
			print("any next notify set");
			readyOnAnyNextNotify = true;
		}

		// if (!LevelPieceSuperClass.CheckCurrentSegment(ParentSegment)) {
		// 	// Resets if entering from wrong segment

		// 	// LevelPieceSuperClass.ResetToCurrentSegment();

		// 	bool transitionSucceeded = ParentSegment.AttemptTransition();

		// 	if (!transitionSucceeded) {
		// 		return;
		// 	}
		// }

		SteeringScript.MainInstance.LapsCompleted++;
		print("Laps completed: " + SteeringScript.MainInstance.LapsCompleted + " <--");

	}

	// called when car leaves parent segment
	public void Notify(LevelPieceSuperClass segment) {

		if (readyOnAnyNextNotify) {
			print("any next notify used");
			ready = true;
			readyOnAnyNextNotify = false;
			return;
		}

		if (!RemixEditorGoalPost.CheckTransition(segment)) {
			return;
		}

		if (ready) {
			// Registers lap if the car somehow missed the goal post
			int oldLapsCompleted = SteeringScript.MainInstance.LapsCompleted;
			SteeringScript.MainInstance.LapsCompleted++;
			print("[Notify] Laps completed: " + SteeringScript.MainInstance.LapsCompleted + " <--");

			if (oldLapsCompleted < SteeringScript.MainInstance.LapsCompleted
			&& RemixEditorGoalPost.FinishSpot != RemixEditorGoalPost.StartSpot) {
				// RemixEditorGoalPost.AttemptTransition(LevelPieceSuperClass.CurrentSegment);
				RemixEditorGoalPost.MoveCarToStart();
				// RemixEditorGoalPost.MoveCarToStart();
				ready = false;
				readyOnAnyNextNotify = true;
				print("[Notify] any next notify set");
			}
		} else {
			ready = true;
			print("goal post ready");
		}

	}

	public void SetGoalPost(RemixEditorGoalPost goalPost) {
		PortalExit.SetActive(false);
		PortalEffects.SetActive(false);

		ContainerObject.transform.position = goalPost.GoalPost.transform.position;
		ContainerObject.transform.rotation = goalPost.GoalPost.transform.rotation;
		ContainerObject.transform.localScale = goalPost.GoalPost.transform.localScale;
	}

	public static void SetInstanceGoalPost(RemixEditorGoalPost goalPost) {
		MainInstance?.SetGoalPost(goalPost);
	}

	public void SetGoalPortal(RemixEditorGoalPost entrance, RemixEditorGoalPost exit) {
		PortalExit.SetActive(true);
		PortalEffects.SetActive(true);

		PortalExit.transform.position = exit.GoalPost.transform.position;
		PortalExit.transform.rotation = exit.GoalPost.transform.rotation;
		// NOTE: accounts for scaling using parent
		PortalExit.transform.localScale = Vector3.Scale(exit.GoalPost.transform.parent.parent.localScale, exit.GoalPost.transform.localScale);

		ContainerObject.transform.position = entrance.GoalPost.transform.position;
		ContainerObject.transform.rotation = entrance.GoalPost.transform.rotation;
		ContainerObject.transform.localScale = entrance.GoalPost.transform.localScale;
	}

	public static void SetInstanceGoalPortal(RemixEditorGoalPost entrance, RemixEditorGoalPost exit) {
		MainInstance?.SetGoalPortal(entrance, exit);
	}

}
