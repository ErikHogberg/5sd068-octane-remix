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
	}

	private void OnDestroy() {
		if (IsMainInstance) {
			MainInstance = null;
		}
		LevelPieceSuperClass.LeaveSegmentObservers.Remove(this);
	}

	private void Start() {

		// IDEA: show portal if only start or only end, show goalpost if both start and end

	}

	private void OnTriggerEnter(Collider other) {
		if (!ready) {

			// FIXME: goal post only triggering new lap half the time, reporting not ready on other half

			print("goal post not ready!");
			return;
		}

		ready = false;

		if (!RemixEditorGoalPost.AttemptTransition(LevelPieceSuperClass.CurrentSegment)) {
			print("invalid goal post transition!");
			// LevelPieceSuperClass.ResetToCurrentSegment();
			ready = true;
			return;
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
		print("Laps completed: " + SteeringScript.MainInstance.LapsCompleted + "<--");

	}

	// called when car leaves parent segment
	public void Notify(LevelPieceSuperClass segment) {

		if (!RemixEditorGoalPost.CheckTransition(segment)) {
			return;
		}

		if (ready) {
			// Registers lap if the car somehow missed the goal post
			int oldLapsCompleted = SteeringScript.MainInstance.LapsCompleted;
			SteeringScript.MainInstance.LapsCompleted++;

			// FIXME: illegal shortcut on first segment after teleport, despite current segment being cleared
			if (oldLapsCompleted < SteeringScript.MainInstance.LapsCompleted && RemixEditorGoalPost.FinishSpot != RemixEditorGoalPost.StartSpot) {
				RemixEditorGoalPost.MoveCarToStart();
			}
		}

		ready = true;
		print("goal post ready");
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
