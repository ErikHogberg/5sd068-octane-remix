using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TilingSegmentBendingScript : MonoBehaviour {

	public Transform Target;
	// TODO: assume target magnitude is child of target
	public Transform TargetMagnitude;
	public Transform StartMagnitude;
	// TODO: start and end rotation

	public GameObject Segment;

	public float MinLength;
	public float MaxLength;

	[Tooltip("How many points/what resolution should be used for measuring curve length")]
	public int MeasureResolution = 8;

	public List<(GameObject, StraightLevelPieceScript)> segments = new List<(GameObject, StraightLevelPieceScript)>();

	private Vector3 CornerPos;
	private Vector3 EndMagnitudePos;
	private Vector3 StartMagnitudePos;


	void Start() {
		// TODO: disable if not run in editor
		// IDEA: option to keep script in game, for live editing of track shape
	}

	void Update() {
		if (!Target || !TargetMagnitude || !StartMagnitude) {
			return;
		}

		if (CornerPos == null
			|| TargetMagnitude == null
			|| StartMagnitude == null
			|| Target.localPosition != CornerPos
			|| TargetMagnitude.localPosition != EndMagnitudePos
			|| StartMagnitude.localPosition != StartMagnitudePos
		) {
			CornerPos = Target.localPosition;
			EndMagnitudePos = TargetMagnitude.localPosition;
			StartMagnitudePos = StartMagnitude.localPosition;
			UpdateCurve();
		}

	}


	private void UpdateCurve() {

		// TODO: get curve
		List<Vector3> points = Bezier.CubicBezierRender(
			transform.position,
			StartMagnitude.position,
			TargetMagnitude.position,
			Target.position,
			MeasureResolution
		);

		float distance = 0f;

		// TODO: measure length of curve
		for (int i = 1; i < points.Count; i++) {
			distance += Vector3.Distance(points[i - 1], points[i]);
		}

		// TODO: decide number of segments
		int minNumSegments = (int)(distance / MinLength);
		int maxNumSegments = (int)(distance / MaxLength);

		// TODO: check if current number of segments is within allowed range
		if (segments.Count < minNumSegments) {
			RefreshSegments(minNumSegments);
		} else if (segments.Count > maxNumSegments) {
			RefreshSegments(minNumSegments);
		}

		// TODO: place, bend and snap segments
		List<Vector3> segmentPoints = Bezier.CubicBezierRender(
			transform.position,
			StartMagnitude.position,
			TargetMagnitude.position,
			Target.position,
			segments.Count * 10
		);

	}

	private void RefreshSegments(int segmentCount) {

		Transform segmentContainer = transform.Find("New segments");

		if (!segmentContainer) {
			segmentContainer = new GameObject("New segments").transform;
			segmentContainer.parent = transform;
			segmentContainer.localPosition = Vector3.zero;
		}

		if (segments.Count < segmentCount) {
			for (int i = 0; i < segmentCount - segments.Count; i++) {
				GameObject newSegment = Instantiate(Segment, segmentContainer);
				segments.Add((newSegment, newSegment.transform.Find("RoadSegment27doublebone").GetComponentInChildren<StraightLevelPieceScript>()));
			}
		} else if (segments.Count > segmentCount) {
			for (int i = segments.Count - 1; i > segmentCount; i--) {
				// TODO: hide instead
				// DestroyImmediate(segments[i].Item1);
				segments[i].Item1.SetActive(false);
			}
			// segments.RemoveRange(segmentCount, segments.Count - segmentCount);
		}

	}

	/*
	private void BendBones() {

		if (!startObject) {
			Debug.LogWarning("Start object not assigned");
			return;
		}

		if (!endObject) {
			Debug.LogWarning("End object not assigned");
			return;
		}

		// BoneCollectionScript bones = Selection.activeTransform.GetComponent<BoneCollectionScript>();
		BoneCollectionScript bones = Selection.activeTransform.GetComponent<BoneCollectionScript>();

		if (!bones) {
			Debug.LogWarning("No bone collection script in selected object");
			return;
		}


		float endpointsDistance = Vector3.Distance(
			startObject.position,
			endObject.position
		);

		Vector3 centerMidpoint = Vector3.Lerp(startObject.position, endObject.position, .5f);

		Undo.RecordObjects(bones.Bones.Select(b => b.BoneTransform).ToArray(), "move bones");

		Undo.RecordObject(bones.transform, "move segment root");
		bones.transform.position = centerMidpoint;

		int boneCount = bones.Bones.Length;

		List<Vector3> points;
		{
			Vector3 start = startObject.position;
			Vector3 startDir = start - startObject.forward * startBezierMagnitude;
			Vector3 end = endObject.position;
			Vector3 endDir = end - endObject.forward * endBezierMagnitude;
			points = Bezier.CubicBezierRender(start, startDir, endDir, end, boneCount, 1,3);
		}

		if (points.Count != boneCount) {
			Debug.LogError("bone and bezier point count dont match");
			return;
		}

		for (int i = 0; i < points.Count; i++) {
			bones.Bones[i].BoneTransform.position = points[i];
		}

		for (int i = 0; i < boneCount; i++) {

			Transform nextBone;
			if (i == boneCount - 1)
				nextBone = endObject.transform;
			else
				nextBone = bones.Bones[i + 1].BoneTransform;

			var bone = bones.Bones[i];

			bone.BoneTransform.LookAt(nextBone, Vector3.Lerp(startObject.up, endObject.up, (float)i / boneCount));

			bone.BoneTransform.rotation *= Quaternion.FromToRotation(Vector3.forward, bone.Forward);
			bone.BoneTransform.rotation *= Quaternion.FromToRotation(Vector3.up, bone.Up);

		}

	}
	*/

}
