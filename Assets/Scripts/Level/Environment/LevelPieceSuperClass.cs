using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ObjectSelectorScript))]
public abstract class LevelPieceSuperClass : MonoBehaviour, IComparable<LevelPieceSuperClass> {

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

	[Tooltip("Override which segment was before this one, instead of assuming (segment order - 1)")]
	public bool OverridePreviousSegment = false;
	[Tooltip("Which segments were before this one, requires the override to be checked to be used")]
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
		Segments.Sort();

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

		if (!(SteeringScript.MainInstance?.EnableCheatMitigation ?? true)) {
			return true;
		}

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
		if (!other.CompareTag("Player") || currentSegment == this)
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

			SteeringScript.MainInstance?.Reset(currentSegment.RespawnSpot.position, currentSegment.RespawnSpot.rotation);
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

	public int CompareTo(LevelPieceSuperClass other) {
		// Compare using distance from origin
		/*
		float originDistance = transform.position.sqrMagnitude;
		float otherOriginDistance = other.transform.position.sqrMagnitude;
		float diff = originDistance - otherOriginDistance;
		if (diff > 0) {
			return 1;
		} else if (diff < 0) {
			return -1;
		} else {
			return 0;
		}
		// */

		// Compare using segment order
		return SegmentOrder - other.SegmentOrder;
	}

	// creates a base 64 number (in a string) representing the current obstacle choices of all segments
	public static string GetBase64Remix() {
		// string intString = "";

		// long obstacleChoices = 0;

		int obstaclesPerIndex = 2;
		int indices = Segments.Count / obstaclesPerIndex + 1;
		// int maxObstacleChoices = 8; // including none
		byte[] obstacleChoices = new byte[indices];

		for (int i = 0; i < Segments.Count; i++) {

			int currentIndex = i / obstaclesPerIndex;

			int subindex = i % obstaclesPerIndex;

			int obstacleIndex = Segments[i].Obstacles.ShownIndex + 1;
			int value = obstacleIndex & 0b_0000_1111;
			if (subindex == 1) {
				value = (value << 4) & 0b_1111_0000;
			}
			byte old = obstacleChoices[currentIndex];
			obstacleChoices[currentIndex] = (byte)(obstacleChoices[currentIndex] | value);

			// System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
			// nfi.NumberDecimalDigits = 8;
			// nfi

			// Debug.Log("index " + i + ", " + subindex + ": added " + Convert.ToString(value, 2) + " to " + Convert.ToString(old, 2) + " resulting in " + Convert.ToString(obstacleChoices[currentIndex], 2));
			Debug.Log("index " + i + " (" + currentIndex + ", " + subindex + "): added " +
				value
				// Convert.ToString(value, 2)
				+ " to " +
				old
				// Convert.ToString(old, 2)
				+ " resulting in " +
				obstacleChoices[currentIndex]
			// Convert.ToString(obstacleChoices[currentIndex], 2)
			);

			// byte chosenObstacle = (byte)(10*(i%25) + obstacleIndex); // 0 = no obstacle

			// long pow = 1;
			// for (int j = 0; j < byte.MaxValue / maxObstacleChoices; j++) {
			// 	pow *= maxObstacleChoices;
			// }

			// i/25;

			// Debug.Log("adding " + chosenObstacle + " with pow " + pow);
			// Debug.Log("adding " + chosenObstacle + " to slot " + index);
			// obstacleChoices += chosenObstacle * pow;

			// obstacleChoices[index] += chosenObstacle;
		}

		// foreach (var item in Segments) {
		// 	// NOTE: assumes single-digit entries, max 9 available obstacle choices
		// 	int chosenObstacle = item.Obstacles.ShownIndex + 1; // 0 = no obstacle
		// intString += chosenObstacle.ToString("D");
		// }

		// string outString = Convert.ToString(obstacleChoices, 64);

		// return   outString.ToString();
		string outString = System.Convert.ToBase64String(obstacleChoices);
		Debug.Log("pre compress: " + obstacleChoices.ToString() + ",\npost compress: " + outString);

		// return obstacleChoices.ToString();
		return outString;

	}

	// loads obstacles choices from a base 64 number in a string, returns true if the string is valid and the process completed successfully
	public static bool LoadRemixFromBase64(string remixBase64String) {
		// string outString = "";

		return true;
	}

}
