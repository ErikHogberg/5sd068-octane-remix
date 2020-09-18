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

	private List<GameObject> segments;

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
			|| Target.localPosition != CornerPos
			|| TargetMagnitude.localPosition != EndMagnitudePos
			|| StartMagnitude.localPosition != StartMagnitudePos
		) {
			CornerPos = Target.localPosition;
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
			10
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


	}

	private void RefreshSegments(int segmentCount) {
		if (segments.Count < segmentCount) {
			for (int i = 0; i < segmentCount - segments.Count; i++) {
				segments.Add(Instantiate(Segment));
			}
		} else if (segments.Count > segmentCount) {
			for (int i = segments.Count; i > segmentCount; i--) {
				DestroyImmediate(segments[i]);
			}
			segments.RemoveRange(segmentCount, segments.Count - segmentCount);
		}

	}
}
