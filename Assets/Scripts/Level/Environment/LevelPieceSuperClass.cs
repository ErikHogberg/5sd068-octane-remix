﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ObjectSelectorScript))]
public abstract class LevelPieceSuperClass : MonoBehaviour {

	private const int allowedSegmentSkip = 0;

	public static List<LevelPieceSuperClass> Segments = new List<LevelPieceSuperClass>();

	protected static LevelPieceSuperClass startSegment;
	protected static LevelPieceSuperClass endSegment;

	protected static LevelPieceSuperClass currentSegment;

	// TODO: progress through whole track
	// IDEA: mark some tracks as reversing direction, to allow going back on the previous track and still progress

	[Tooltip("In which order this segment is expected, if a segment is too much out of order, then the car will be reset to the last segment")]
	public int SegmentOrder;
	// [Tooltip("If resetting due to out of order segment should be ignored")]
	// public bool OverrideSegmentOrderReset = false;

	public bool isStart = false;

	[Tooltip("Override which segment was before this one, instead of assuming segment order - 1")]
	public bool OverridePreviousSegment = false;
	[Tooltip("Which segments were before this one, requires the override to be checked to be used")]
	// public int PreviousSegment = 0;
	public List<int> PreviousSegments;

	[Tooltip("Override how many segments are allowed to be skipped when entering this segment")]
	public bool OverrideSegmentSkip = false;
	[Tooltip("How many segments are allowed to be skipped when entering this segment, 0 means only the exact previous segment is allowed")]
	[Min(0)]
	public int CustomSegmentSkip = 0;
	// NOTE: will not check overrides of previous segments other than for this segment
	// IDEA: define list of multiple allowed previous segments?

	public Transform RespawnSpot;
	public Transform GoalSpot;

	// IDEA: empty level segment type for optional spots for adding roads
	// IDEA: dynamic list of segment editing fields, only show the settings allowed for specific class, pushing fields from script every update

	// IDEA: option to disallow placing any obstacles on segment, just dont add any obstacles to object selector?

	// IDEA: ability select multiple segments, shift click? show blank/custom message if same setting is different for some objects selected
	// IDEA: ability to group segments together, selecting and altering all segments at the same time
	// IDEA: when selecting multiple: list all avaliable settings, set them for only the segments that the settings can be applied for

	public ObjectSelectorScript Obstacles { get; private set; }

	private void Awake() {
		Segments.Add(this);
		
		Obstacles = GetComponent<ObjectSelectorScript>();

		Obstacles.UnhideObject("");
	}

	private void Start() {
		if (isStart) {
			GoalPostScript.SetSegment(this);
			// UpdateGoalPost();	
		}
	}

	private void OnMouseOver() {

		if (EventSystem.current.IsPointerOverGameObject())
			return;

		// print("clicked " + gameObject.name);

		if (Input.GetMouseButtonDown(0)) {
			// print("left click");
			RemixMapScript.SelectSegment(this);
		}
		if (Input.GetMouseButtonDown(1)) {
			// print("right click");
			RemixMapScript.StartRotate();
		}
	}

	// TODO: fade effect when respawning

	private void OnTriggerEnter(Collider other) {

		int currentSegmentSkip = allowedSegmentSkip;

		if (OverrideSegmentSkip)
			currentSegmentSkip = CustomSegmentSkip;

		if (
			// NOTE: possible false positive when using override?
			!currentSegment
			||currentSegment == this
			||
			(
				OverridePreviousSegment
				&& PreviousSegments.Contains(currentSegment.SegmentOrder)
			)
			|| (
				SegmentOrder <= currentSegment.SegmentOrder + 1 + currentSegmentSkip // if on next correct segment in allowed range
				&& SegmentOrder >= currentSegment.SegmentOrder - 1 - currentSegmentSkip
			) 
		) {
			currentSegment = this;
			print("current segment: " + currentSegment.SegmentOrder);
		} else {
			ResetToCurrentSegment();
		}
	}

	public static bool CheckCurrentSegment(LevelPieceSuperClass segmentToCheck){
		if (!currentSegment)
			return true;

		return currentSegment == segmentToCheck;
	}

	public static bool ResetToCurrentSegment() {
		// LevelPieceSuperClass currentSegment = null;
		// foreach (var segment in Segments) {
		// 	if (segment.SegmentOrder == LevelPieceSuperClass.currentSegment.SegmentOrder) {
		// 		currentSegment = segment;
		// 		break;
		// 	}
		// }

		if (!currentSegment) {
			// SteeringScript.MainInstance.Reset();
			return false;
		}

		if (currentSegment.RespawnSpot) {
			SteeringScript.MainInstance.Reset(currentSegment.RespawnSpot.position, currentSegment.RespawnSpot.rotation);
		} else {
			// SteeringScript.MainInstance.Reset(currentSegment.transform.position, currentSegment.transform.rotation);
			// SteeringScript.MainInstance.Reset();
			return false;
		}

		return true;
	}

	public void UpdateGoalPost() {
		if (startSegment == endSegment) {
			GoalPostScript.SetSegment(this);
		} else {
			// TODO: spawn portals at ends instead
		}
	}

}
