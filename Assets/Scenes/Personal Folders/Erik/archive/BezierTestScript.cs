using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierTestScript : MonoBehaviour {

	public Transform p0;
	public Transform p1;
	public Transform p2;
	public Transform p3;

	public List<Transform> Output;

	void UpdateBezier(){
		var points = Bezier.CubicBezierRender(p0.position, p1.position, p2.position, p3.position, Output.Count);

		for (int i = 0; i < points.Count; i++) {
			Output[i].position = points[i];
		}
	}

	// Start is called before the first frame update
	void Start() {
		UpdateBezier();
	}

	private void OnMouseDown() {
		print("click update bezier");
		UpdateBezier();
	}

}
