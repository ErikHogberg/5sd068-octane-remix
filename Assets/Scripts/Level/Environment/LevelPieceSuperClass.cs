using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ObjectSelectorScript))]
public abstract class LevelPieceSuperClass : MonoBehaviour {

	private const int allowedSegmentSkip = 0;

	public static List<LevelPieceSuperClass> Segments = new List<LevelPieceSuperClass>();

	protected static int startSegmentIndex = 0;
	protected static int EndSegmentIndex = 0;

	protected static int currentSegmentIndex = 0;

	// TODO: progress through whole track
	// IDEA: mark some tracks as reversing direction, to allow going back on the previous track and still progress

	[Tooltip("In which order this segment is expected, if a segment is too much out of order, then the car will be reset to the last segment")]
	public int SegmentOrder;
	[Tooltip("If resetting due to out of order segment should be ignored")]
	public bool OverrideSegmentOrderReset = false;
	// public bool OverridePreviousSegment = false;
	// public int PreviousSegment = 0;
	

	// IDEA: define an override "previous segment" instead of disabling all skip reset checks
	// IDEA: apply allowed skip number to previous segment overrides too

	public Transform RespawnSpot;

	// IDEA: empty level segment type for optional spots for adding roads
	// IDEA: dynamic list of segment editing fields, only show the settings allowed for specific class, pushing fields from script every update

	// IDEA: option to disallow placing any obstacles on segment

	// IDEA: ability select multiple segments, shift click? show blank/custom message if same setting is different for some objects selected
	// IDEA: ability to group segments together, selecting and altering all segments at the same time
	// IDEA: when selecting multiple: list all avaliable settings, set them for only the segments that the settings can be applied for

	public ObjectSelectorScript Obstacles { get; private set; }

	private void Awake() {
		Segments.Add(this);
		Obstacles = GetComponent<ObjectSelectorScript>();

		Obstacles.UnhideObject("");
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
		if (
			OverrideSegmentOrderReset || // if skip mitigation resetting is disabled on this segment
			SegmentOrder <= currentSegmentIndex + 1 + allowedSegmentSkip // if on next correct segment in allowed range
			&& SegmentOrder >= currentSegmentIndex - 1 - allowedSegmentSkip // if on previous correct segment in allowed range
			|| (currentSegmentIndex == Segments.Count - 1 && SegmentOrder == 0) // loop track
		) {
			currentSegmentIndex = SegmentOrder;
			print("current segment: " + currentSegmentIndex);
		} else {
			ResetToCurrentSegment();
		}
	}

	public static bool ResetToCurrentSegment() {
		LevelPieceSuperClass currentSegment = null;
		foreach (var segment in Segments) {
			if (segment.SegmentOrder == currentSegmentIndex) {
				currentSegment = segment;
				break;
			}
		}

		if (!currentSegment)
			return false;

		if (currentSegment.RespawnSpot) {
			SteeringScript.MainInstance.Reset(currentSegment.RespawnSpot.position, currentSegment.RespawnSpot.rotation);
		} else {
			SteeringScript.MainInstance.Reset(currentSegment.transform.position, currentSegment.transform.rotation);
		}

		return true;
	}

}
