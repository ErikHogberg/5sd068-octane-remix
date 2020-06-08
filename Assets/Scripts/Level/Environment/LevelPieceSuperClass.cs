﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
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
	public static string GetRemixString(bool printDebug = false) {
		int obstaclesPerIndex = 2;
		int indices = Segments.Count / obstaclesPerIndex + 1;
		byte[] obstacleChoices = new byte[indices];

		for (int i = 0; i < Segments.Count; i++) {

			int currentIndex = i / obstaclesPerIndex;
			int subindex = i % obstaclesPerIndex;
			int obstacleIndex = Segments[i].Obstacles.ShownIndex + 1;

			// FIXME: no obstacle choice is not recorded
			int value = obstacleIndex & 0b_0000_1111;
			if (subindex == 1) {
				value = (value << 4) & 0b_1111_0000;
			}

			// byte old = obstacleChoices[currentIndex];
			obstacleChoices[currentIndex] = (byte)(obstacleChoices[currentIndex] | value);

			if (printDebug) {
				Debug.Log("index " + i + " (" + currentIndex + ", " + subindex + "): added " +
					value
					// Convert.ToString(value, 2)

					// + " to " +
					// old
					// Convert.ToString(old, 2)

					+ " resulting in " +
					obstacleChoices[currentIndex]
				// Convert.ToString(obstacleChoices[currentIndex], 2)
				);
			}
		}

		string outString = System.Convert.ToBase64String(obstacleChoices);
		// string outString = Encoding.UTF8.GetString(obstacleChoices, 0, obstacleChoices.Length);
		
		if (printDebug) {
			string outString2 = BitConverter.ToString(obstacleChoices);
			string outString3 = Encoding.UTF8.GetString(obstacleChoices, 0, obstacleChoices.Length);
			string outString4 = Encoding.ASCII.GetString(obstacleChoices, 0, obstacleChoices.Length);
			string outString5 = Encoding.Unicode.GetString(obstacleChoices, 0, obstacleChoices.Length);
			string outString6 = Encoding.BigEndianUnicode.GetString(obstacleChoices, 0, obstacleChoices.Length);
			string outString7 = Encoding.Default.GetString(obstacleChoices, 0, obstacleChoices.Length);
			string outString8 = Encoding.UTF32.GetString(obstacleChoices, 0, obstacleChoices.Length);

			Debug.Log("print string types:");
			Debug.Log("base64: " + outString);
			Debug.Log("bitconverter: " + outString2);
			Debug.Log("utf8: " + outString3);
			Debug.Log("ascii: " + outString4);
			Debug.Log("unicode: " + outString5);
			Debug.Log("big endian unicode: " + outString6);
			Debug.Log("default: " + outString7);
			Debug.Log("utf32: " + outString8);
			Debug.Log("utf7: " + Encoding.UTF7.GetString(obstacleChoices, 0, obstacleChoices.Length));

			Debug.Log(

				"pre convert: " + BitConverter.ToString(obstacleChoices) +
				",\npost convert: " + outString
			);

			byte[] undoString = System.Convert.FromBase64String(outString);
			// byte[] undoString = Encoding.UTF8.GetBytes(outString);

			Debug.Log(
				"convert back: " + BitConverter.ToString(undoString)
			);
		}

		return outString;
	}

	// loads obstacles choices from a base 64 number in a string, returns true if the string is valid and the process completed successfully
	public static bool LoadRemixFromString(string remixBase64String) {
		// string outString = "";

		// remixBase64String.
		Debug.Log("loading id \"" + remixBase64String + "\"");

		// src for base 64 string validation: https://stackoverflow.com/questions/6309379/how-to-check-for-a-valid-base64-encoded-string
		remixBase64String = remixBase64String.Trim();
		bool valid = (remixBase64String.Length % 4 == 0) && Regex.IsMatch(remixBase64String, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

		if (!valid) {
			Debug.LogWarning("invalid input string: not base 64 string");
			return false;
		}

		byte[] obstacleBytes = System.Convert.FromBase64String(remixBase64String);

		if (obstacleBytes.Length * 2 < Segments.Count) {
			return false;
		}

		// TODO: validate input more

		// FIXME: new obstacles not applied, old ones applied instead (how??)

		for (int i = 0; i < obstacleBytes.Length; i++) {
			int firstSegmentIndex = i * 2;
			int secondSegmentIndex = firstSegmentIndex + 1;

			if (firstSegmentIndex < Segments.Count) {
				int obstacleIndex = obstacleBytes[i] & 0b_0000_1111;
				ObjectSelectorScript obstacles = Segments[firstSegmentIndex].Obstacles;

				if (obstacleIndex > obstacles.Count) {
					return false;
				}

				int prevObject = obstacles.ShownIndex;
				string unhidObstacle = "none";
				if (obstacleIndex > 0) {
					unhidObstacle = obstacles.objects[obstacleIndex - 1].Key;
				}

				if (prevObject != obstacleIndex - 1) {
					Debug.Log("[first] segment " + (firstSegmentIndex + 1)
						+ ": unhid obstacle " + (obstacleIndex - 1)
						+ " (" + unhidObstacle + "), prev: "
						+ prevObject
						+ " (" + (obstacles.ShownObject?.Key ?? "none") + ")"
					);
				}

				if (obstacleIndex > 0) {
					obstacles.UnhideObject(obstacleIndex - 1);
				} else {
					obstacles.UnhideObject();
				}
			} else {
				break;
			}

			if (secondSegmentIndex < Segments.Count) {
				int obstacleIndex = obstacleBytes[i] >> 4;
				ObjectSelectorScript obstacles = Segments[secondSegmentIndex].Obstacles;

				if (obstacleIndex > obstacles.Count) {
					return false;
				}

				int prevObject = obstacles.ShownIndex;
				if (prevObject != obstacleIndex - 1) {
					// Debug.Log("[second] segment " + secondSegmentIndex + ": unhid obstacle " + obstacleIndex);
					// Debug.Log("[second] segment " + secondSegmentIndex + ": unhid obstacle " + (obstacleIndex - 1) + ", prev: " + prevObject);

					string unhidObstacle = "none";
					if (obstacleIndex > 0) {
						unhidObstacle = obstacles.objects[obstacleIndex - 1].Key;
					}
					Debug.Log("[second] segment " + (secondSegmentIndex + 1)
						+ ": unhid obstacle " + (obstacleIndex - 1)
						+ " (" + unhidObstacle + "), prev: "
						+ prevObject
						+ " (" + (obstacles.ShownObject?.Key ?? "none") + ")"
					);
				}

				if (obstacleIndex > 0) {
					obstacles.UnhideObject(obstacleIndex - 1);
				} else {
					obstacles.UnhideObject();
				}
			} else {
				break;
			}

		}

		// TODO: update remix editor obstacle list
		SegmentEditorSuperClass.UpdateAllUI();

		return true;
	}

}
