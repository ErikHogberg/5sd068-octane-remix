using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class CoDriverUIScript : MonoBehaviour {

	private static CoDriverUIScript mainInstance;

	public TMP_Text Text;
	public float CheckAheadDistanceSqr = 1;
	public float AngleThreshold = 20;

	public List<GameObject> Arrows;

	public static float CheckAheadDistanceStatic => mainInstance ? mainInstance.CheckAheadDistanceSqr : -1;

	// Start is called before the first frame update
	void Start() {
		mainInstance = this;
		if (Text)
			Text?.SetText("");
	}

	private void OnDestroy() {
		if (mainInstance == this)
			mainInstance = null;
	}

	public static void UpdateArrowsStatic(CenterlineScript.InternalCenterline line, int startIndex) {
		mainInstance?.UpdateArrows(line, startIndex);
	}

	public void UpdateArrows(CenterlineScript.InternalCenterline line, int startIndex) {
		if (line.LinePoints.Count < 2)
			return;

		var rotDeltas = CenterlineScript.GetGreatestRotationDeltasAhead(
			line,
			mainInstance.CheckAheadDistanceSqr,
			startIndex
		);

		StringBuilder outText = new StringBuilder("");

		Quaternion closestRot = Quaternion.identity;
		if (startIndex < line.LinePoints.Count - 2)
			closestRot = Quaternion.LookRotation(line.LinePoints[startIndex + 1] - line.LinePoints[startIndex], Vector3.up);
		else
			closestRot = Quaternion.LookRotation(line.LinePoints[startIndex] - line.LinePoints[startIndex - 1], Vector3.up);

		bool first = true;



		int i = 0;
		foreach ((_, _, var rot) in rotDeltas) {

			Quaternion.LookRotation(line.LinePoints[startIndex + 1] - line.LinePoints[startIndex], Vector3.up);
			float angle = Quaternion.Angle(closestRot, rot);
			if (angle > mainInstance.AngleThreshold) {
				if (first) {
					first = false;
				} else {
					outText.Append('\n');
				}
				outText.Append($"upcoming turn, {angle.ToString("000")} degrees");
			}

			if (i < Arrows.Count) {
				Arrows[i].SetActive(true);
				Arrows[i].transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, CenterlineScript.GetUIArrowDir(rot)));
			}

			i++;
		}

		for (; i < Arrows.Count; i++) {
			Arrows[i].SetActive(false);
		}

		mainInstance.Text?.SetText(outText);

	}

}
