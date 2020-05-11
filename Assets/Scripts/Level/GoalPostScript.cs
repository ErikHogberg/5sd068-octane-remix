﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPostScript : MonoBehaviour, IObserver<LevelPieceSuperClass> {

	public static GoalPostScript MainInstance;

	[HideInInspector]
	public LevelPieceSuperClass ParentSegment;

	public GameObject ContainerObject;

	private bool ready = true;

	private void Awake() {
		MainInstance = this;
		ContainerObject.SetActive(false);
		// if (ParentSegment)
		// 	ParentSegment.LeaveSegmentObservers.Add(this);
	}

	private void OnDestroy() {
		MainInstance = null;
	}

	private void Start() {

		// IDEA: show portal if only start or only end, show goalpost if both start and end

	}

	// TODO: set car, ground and obstacle collision layer settings to not count ground fin and flip trigger when entering goal post or portal
	private void OnTriggerEnter(Collider other) {
		if (!ready)
			return;

		SteeringScript.MainInstance.LapsCompleted++;
		print("Laps completed: " + SteeringScript.MainInstance.LapsCompleted);

		ready = false;
	}

	public void SetSegment(LevelPieceSuperClass segment) {

		if (ParentSegment)
			ParentSegment.LeaveSegmentObservers.Remove(this);

		ParentSegment = segment;
		ParentSegment.LeaveSegmentObservers.Add(this);

		ContainerObject.SetActive(true);

		ContainerObject.transform.position = segment.GoalSpot.position;
		ContainerObject.transform.rotation = segment.GoalSpot.rotation;
		ContainerObject.transform.localScale = segment.GoalSpot.localScale;
	}

	public static void SetInstanceSegment(LevelPieceSuperClass segment) {
		if (!MainInstance)
			return;

		MainInstance.SetSegment(segment);
	}

	// called when car leaves parent segment
	public void Notify(LevelPieceSuperClass segment) {
		ready = true;
	}

}
