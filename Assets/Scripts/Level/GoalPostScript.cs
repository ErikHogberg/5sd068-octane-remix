using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPostScript : MonoBehaviour {

	public static GoalPostScript MainInstance;

	public LevelPieceSuperClass ParentSegment;
	public GameObject ContainerObject;

	private void Awake() {
		MainInstance = this;
		ContainerObject.SetActive(false);
	}

	private void OnDestroy() {
		MainInstance = null;
	}

	private void Start() {

		// IDEA: show portal if only start or only end, show goalpost if both start and end

	}

	private void OnCollisionEnter(Collision other) {
		// TODO: check if lap is valid
		// TODO: disable until player leaves segment
	}

	public static void SetSegment(LevelPieceSuperClass segment) {
		if (!MainInstance)
			return;

		MainInstance.ParentSegment = segment;

		MainInstance.ContainerObject.SetActive(true);

		MainInstance.ContainerObject.transform.position = segment.GoalSpot.position;
		MainInstance.ContainerObject.transform.rotation = segment.GoalSpot.rotation;
		MainInstance.ContainerObject.transform.localScale = segment.GoalSpot.localScale;

	}

}
