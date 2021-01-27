using System.Collections.Generic;
using UnityEngine;

public static class Bezier {

	// get 1 point on curve
	public static Vector3 CubicBezierEval(Vector3 start, Vector3 startDir, Vector3 endDir, Vector3 end, float t) {
		t = Mathf.Clamp(t, 0, .999f);

		Vector3 result =
			Mathf.Pow(1 - t, 3) * start // (1-t)^3*P0
			+ 3 * Mathf.Pow(1 - t, 2) * t * startDir // 3*(1-t)^2*P1
			+ 3 * (1 - t) * Mathf.Pow(t, 2) * endDir // 3*(1-t) *t^2*P2
			+ Mathf.Pow(t, 3) * end // (t)^3*P3
		;

		return result;
	}

	public static Vector3 CubicBezierEval(List<Vector3> points, float t) {
		t = Mathf.Clamp(t, 0, .999f);

		if (points.Count == 4) {
			return CubicBezierEval(points[0], points[1], points[2], points[3], t);
		} else if (points.Count > 4) {
			List<Vector3> newPoints = new List<Vector3>();
			for (int i = 1; i < points.Count; i++) {
				newPoints.Add(Vector3.Lerp(points[i - 1], points[i], t));
			}
			return CubicBezierEval(newPoints, t);
		} else {
			// TODO: error
			return Vector3.zero;
		}

		// Vector3 result =
		// 	Mathf.Pow(1 - t, 3) * start // (1-t)^3*P0
		// 	+ 3 * Mathf.Pow(1 - t, 2) * t * startDir // 3*(1-t)^2*P1
		// 	+ 3 * (1 - t) * Mathf.Pow(t, 2) * endDir // 3*(1-t) *t^2*P2
		// 	+ Mathf.Pow(t, 3) * end // (t)^3*P3
		// ;

		// return result;
	}

	// get all points on curve, points do not count or include start and end
	public static List<Vector3> CubicBezierRender(Vector3 start, Vector3 startDir, Vector3 endDir, Vector3 end, int numPoints, int startOffset = 0, int endOffset = 0) {
		var outList = new List<Vector3>();

		// if (numPoints <= startOffset + endOffset) {
		// TODO: error, return safe value
		// }

		for (int i = 0; i < numPoints; i++) {

			float t;
			// if (withEndpoints) {
			// t = ((float)i) / ((float)numPoints - 1);
			t = ((float)i + startOffset) / ((float)numPoints - 1 + endOffset);
			// Debug.Log("bezier t: " + t);
			// } else {
			// t = ((float)i + 1) / ((float)numPoints + 1);
			// }
			Vector3 point = CubicBezierEval(start, startDir, endDir, end, t);
			outList.Add(point);
		}

		return outList;
	}

	public static List<Vector3> CubicBezierRender(List<Vector3> controlPoints, int numPoints, int startOffset = 0, int endOffset = 0) {
		var outList = new List<Vector3>();

		// if (numPoints <= startOffset + endOffset) {
		// TODO: error, return safe value
		// }

		for (int i = 0; i < numPoints; i++) {

			float t;
			t = ((float)i + startOffset) / ((float)numPoints - 1 + endOffset);
			Vector3 point = CubicBezierEval(controlPoints, t);
			outList.Add(point);
		}

		return outList;
	}
}
