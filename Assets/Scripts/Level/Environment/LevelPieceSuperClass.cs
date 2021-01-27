using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ObjectSelectorScript))]
public abstract class LevelPieceSuperClass : MonoBehaviour, IComparable<LevelPieceSuperClass> {

	public static List<LevelPieceSuperClass> Segments = new List<LevelPieceSuperClass>();

	public static LevelPieceSuperClass CurrentSegment = null;

	// [Tooltip("If entering this segment should change the speed profile of the car")]
	// public bool SetSpeedProfile = false;
	// [Tooltip("Which speed profile to change to")]
	// public int SpeedProfileIndex = 0;

	// IDEA: empty level segment type for optional spots for adding roads
	// IDEA: mark road as "hideable" to show a toggle in editor? too easy to miss that you have the option? 
	// IDEA: show separate list of hideable things, which can contain duplicates in other lists?

	// IDEA: ability select multiple segments, shift click? show blank/custom message if same setting is different for some objects selected
	// IDEA: ability to group segments together, selecting and altering all segments at the same time
	// IDEA: when selecting multiple: list all avaliable settings, set them for only the segments that the settings can be applied for

	public ObjectSelectorScript Obstacles { get; private set; }

	// public static List<IObserver<LevelPieceSuperClass>> LeaveSegmentObservers = new List<IObserver<LevelPieceSuperClass>>();

	// TODO: remove segment order field
	// TODO: figure out other way to decide order of segments in UI
	// IDEA: use position of segment to decide order. distance from start line?
	public int SegmentOrder;

	// sends new total number of segments
	public static List<IObserver<int>> AddSegmentObservers = new List<IObserver<int>>();

	private void Awake() {
		Segments.Add(this);
		Segments.Sort();

		foreach (var item in AddSegmentObservers)
			item.Notify(Segments.Count);

		Obstacles = GetComponent<ObjectSelectorScript>();
		if (!Obstacles) {
			Debug.LogError("no obstacle script found in " + name);
		}

	}

	private void OnMouseOver() {
		if (EventSystem.current.IsPointerOverGameObject())
			return;

		// print("clicked " + gameObject.name);

		if (Input.GetMouseButtonDown(0)) {
			// print("left click");
			RemixMapScript.Select(this);
		}
		if (Input.GetMouseButtonDown(1)) {
			// print("right click");
			RemixMapScript.StartRotate();
		}
	}

	public static bool CheckCurrentSegment(LevelPieceSuperClass segmentToCheck) {
		if (!CurrentSegment)
			return true;

		return CurrentSegment == segmentToCheck;
	}


	public static void ClearCurrentSegment(bool notifyLeaving = false) {
		// if (notifyLeaving && CurrentSegment) {
		// 	foreach (var observer in LeaveSegmentObservers)
		// 		observer.Notify(CurrentSegment);
		// }

		CurrentSegment = null;
	}

	public int CompareTo(LevelPieceSuperClass other) {
		return SegmentOrder - other.SegmentOrder;
	}

	public static string GetRemixString(bool printDebug = false) {
		int obstaclesPerIndex = 2;
		int indices = Segments.Count / obstaclesPerIndex + 1;
		byte[] obstacleChoices = new byte[indices];

		for (int i = 0; i < Segments.Count; i++) {

			int currentIndex = i / obstaclesPerIndex;
			int subindex = i % obstaclesPerIndex;
			int obstacleIndex = Segments[i].Obstacles.ShownIndex + 1;

			int value = obstacleIndex & 0b_0000_1111;
			if (subindex == 1) {
				value = (value << 4) & 0b_1111_0000;
			}

			obstacleChoices[currentIndex] = (byte)(obstacleChoices[currentIndex] | value);
		}

		string outString = Ecoji.Encode(obstacleChoices);

		return outString;
	}

	// loads obstacles choices from a base 64 number in a string, returns true if the string is valid and the process completed successfully
	public static bool LoadRemixFromString(string remixIDString) {

		// Debug.Log("loading id \"" + remixIDString + "\"");

		byte[] obstacleBytes;
		try {
			obstacleBytes = Ecoji.Decode(remixIDString);
		} catch (Ecoji.UnexpectedEndOfInputException) {
			return false;
			// throw;
		}

		if (obstacleBytes.Length * 2 < Segments.Count) {
			return false;
		}

		for (int i = 0; i < obstacleBytes.Length; i++) {
			int firstSegmentIndex = i * 2;
			int secondSegmentIndex = firstSegmentIndex + 1;

			if (firstSegmentIndex < Segments.Count) {
				int obstacleIndex = obstacleBytes[i] & 0b_0000_1111;
				ObjectSelectorScript obstacles = Segments[firstSegmentIndex].Obstacles;

				if (obstacleIndex > obstacles.Count) {
					return false;
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
		// SegmentEditorSuperClass.UpdateAllUI();
		ObstacleListScript.UpdateUIStatic();

		return true;
	}

}
