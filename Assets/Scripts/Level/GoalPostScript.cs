using UnityEngine;

public class GoalPostScript : MonoBehaviour {

	public static GoalPostScript MainInstance;

	public bool IsMainInstance = false;

	public GameObject ContainerObject;
	public GameObject PortalExit;
	public GameObject PortalEffects;

	[Space]
	[Tooltip("If centerline placement should be raycasted")]
	public bool Raycast = true;
	[Tooltip("Which layers placement raycast reacts to")]
	public LayerMask HitMask;


	private void Awake() {
		if (IsMainInstance) {
			MainInstance = this;
		} else {
			ContainerObject.SetActive(false);
		}

		// RemixEditorGoalPost.UpdateGoalPost();
	}

	private void OnDestroy() {
		if (IsMainInstance) {
			MainInstance = null;
		}
		// LevelPieceSuperClass.LeaveSegmentObservers.Remove(this);
	}

	private void Start() {
		if (CenterlineScript.MainInstance) {
			MoveTo(CenterlineScript.MainInstance.FinishLine, CenterlineScript.MainInstance.FinishIndex);
		}
	}

	// private void OnTriggerEnter(Collider other) {
	// 	// notify/check the centerline system for finish line crossing validity
	// 	if (other.TryGetComponent<SteeringScript>(out SteeringScript steeringScript)) {
	// 		bool cross = steeringScript.progressScript.ValidateFinishCrossing(out bool shouldReset);

	// 		if (cross) {
	// 			SteeringScript.MainInstance.LapsCompleted++;
	// 			print("Laps completed: " + SteeringScript.MainInstance.LapsCompleted + " <--");
	// 		}

	// 		if (shouldReset) {
	// 			steeringScript.progressScript.ResetTransform();
	// 			steeringScript.CallResetEvents();
	// 		}
	// 	}
	// }

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

	public void MoveTo(CenterlineScript.InternalCenterline line, int index) {
		Vector3 pos = CenterlineScript.GetLinePointPos(line, index);
		Quaternion rot = CenterlineScript.GetLinePointRot(line, index);

		if (Raycast) {
			Ray ray = new Ray(pos, Vector3.down);
			if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, HitMask)) {
				pos = hitInfo.point;
			}
		}

		ContainerObject.transform.position = pos;
		ContainerObject.transform.rotation = rot;
	}

	public static void MoveToStatic(CenterlineScript.InternalCenterline line, int index) {
		MainInstance?.MoveTo(line, index);
	}

}
