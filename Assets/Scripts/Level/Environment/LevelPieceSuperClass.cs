using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ObjectSelectorScript))]
public abstract class LevelPieceSuperClass : MonoBehaviour {

	private const int allowedSegmentSkip = 0;

	public static List<LevelPieceSuperClass> Segments = new List<LevelPieceSuperClass>();

	protected static LevelPieceSuperClass startSegment = null;
	protected static LevelPieceSuperClass endSegment = null;

	protected static LevelPieceSuperClass currentSegment = null;

	// TODO: progress through whole track
	// IDEA: mark some tracks as reversing direction, to allow going back on the previous track and still progress

	[Tooltip("In which order this segment is expected, if a segment is too much out of order, then the car will be reset to the last segment")]
	public int SegmentOrder;
	// [Tooltip("If resetting due to out of order segment should be ignored")]
	// public bool OverrideSegmentOrderReset = false;

	[Tooltip("If this is the default start segment, make sure only one is marked, or a random one will be selected")]
	public bool InitStart = false;
	[Tooltip("If this is the default end/finish segment, make sure only one is marked, or a random one will be selected")]
	public bool InitEnd = false;

	[Tooltip("If a start goal post can be placed here")]
	public bool IsStartable = true;
	[Tooltip("If an end goal portal can be placed here")]
	public bool IsEndable = true;

	public bool isStart {
		get {
			if (!startSegment) {
				startSegment = this;
				GoalPostScript.SetInstanceSegment(startSegment);
				Debug.Log("start segment is null");
			}
			return startSegment == this;
		}
		set {
			if (value) {
				startSegment = this;
				GoalPostScript.SetInstanceSegment(startSegment);
			}
		}
	}
	public bool isEnd {
		get {
			if (!endSegment) {
				endSegment = this;
				GoalPostScript.SetInstanceSegment(endSegment);
				Debug.Log("end segment is null");
			}
			return endSegment == this;
		}
		set {
			if (value) {
				endSegment = this;
				GoalPostScript.SetInstanceSegment(endSegment);
			}
		}
	}

	[Tooltip("Override which segment was before this one, instead of assuming segment order - 1")]
	public bool OverridePreviousSegment = false;
	[Tooltip("Which segments were before this one, requires the override to be checked to be used")]
	// public int PreviousSegment = 0;
	public List<int> PreviousSegments;

	[Tooltip("Which segment the car will land on when resetting at this segment, this segment if null")]
	public LevelPieceSuperClass SegmentOnReset;

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

	public List<IObserver<LevelPieceSuperClass>> LeaveSegmentObservers = new List<IObserver<LevelPieceSuperClass>>();


	private void Awake() {
		Segments.Add(this);

		Obstacles = GetComponent<ObjectSelectorScript>();

		// Obstacles.UnhideObject("");

		if (InitStart) {
			// startSegment = this;
			isStart = true;
			// endSegment = this;
			// GoalPostScript.SetInstanceSegment(this);
			// UpdateGoalPost();
		}

		if (InitEnd) {
			isEnd = true;
			// endSegment = this;
			// UpdateGoalPost();
		}
	}

	private void Start() {

		if (isEnd || isStart) {
			UpdateGoalPost();
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

	public static bool ResetToStart() {
		if (!startSegment)
			return false;

		currentSegment = startSegment;
		ResetToCurrentSegment();

		return true;
	}


	// If a transition to this segment is allowed
	public bool CheckValidProgression() {
		int currentSegmentSkip = allowedSegmentSkip;

		if (OverrideSegmentSkip)
			currentSegmentSkip = CustomSegmentSkip;

		bool validProgression = !currentSegment;
		if (OverridePreviousSegment)
			validProgression = validProgression || PreviousSegments.Contains(currentSegment.SegmentOrder);
		else
			validProgression = validProgression
				|| (SegmentOrder <= currentSegment.SegmentOrder + 1 + currentSegmentSkip
					&& SegmentOrder > currentSegment.SegmentOrder);

		return validProgression;
	}

	public bool AttemptTransition() {
		bool validProgression = CheckValidProgression();
		if (validProgression) {
			if (currentSegment)
				foreach (var observer in currentSegment.LeaveSegmentObservers)
					observer.Notify(currentSegment);

			currentSegment = this;
			print("current segment: " + currentSegment.SegmentOrder);
		} else {
			UINotificationSystem.Notify("Illegal shortcut!", Color.yellow, 1.5f);
			ResetToCurrentSegment();
		}

		return validProgression;
	}

	private void OnTriggerEnter(Collider other) {
		if (currentSegment == this)
			return;

		AttemptTransition();
	}

	public static bool CheckCurrentSegment(LevelPieceSuperClass segmentToCheck) {
		if (!currentSegment)
			return true;

		return currentSegment == segmentToCheck;
	}

	public static bool ResetToCurrentSegment() {
		if (!currentSegment)
			return false;

		if (currentSegment.RespawnSpot) {
			if (currentSegment.SegmentOnReset)
				currentSegment = currentSegment.SegmentOnReset;

			SteeringScript.MainInstance.Reset(currentSegment.RespawnSpot.position, currentSegment.RespawnSpot.rotation);
		} else {
			return false;
		}

		return true;
	}

	public void UpdateGoalPost() {

		if (!startSegment)
			startSegment = this;
		if (!endSegment)
			endSegment = this;

		if (startSegment == this && endSegment == this) {
			GoalPostScript.SetInstanceSegment(this);
		} else {
			// TODO: spawn portals at ends instead
		}
	}

	public static void ClearCurrentSegment() {
		currentSegment = null;
	}

}
