using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterlineScript : MonoBehaviour {

	public List<Vector3> ControlPoints;

	public List<Vector3> LinePoints;

	public int Resolution = 10;

	// TODO: get closest point to curve relative to given point
	// TODO: generate co-driver calls based on angle delta of set distance ahead of closest point


	public void GenerateLinePoints() {
		int controlPointCount = ControlPoints.Count;
		if (controlPointCount < 2)
			return;

		if (LinePoints == null) {
			LinePoints = new List<Vector3>();
		} else {
			LinePoints.Clear();
		}

		if (controlPointCount == 2) {
			LinePoints.Add(ControlPoints[0]);
			LinePoints.Add(ControlPoints[1]);
			return;
		}

		// IDEA: choose curve resolution

		if (controlPointCount == 3) {
			LinePoints = Bezier.CubicBezierRender(
				ControlPoints[0],
				ControlPoints[1],
				ControlPoints[1],
				ControlPoints[2],
				Resolution
			);
		}

		if (controlPointCount == 4) {
			LinePoints = Bezier.CubicBezierRender(
				ControlPoints[0],
				ControlPoints[1],
				ControlPoints[2],
				ControlPoints[3],
				Resolution
			);
		}

	}

}
