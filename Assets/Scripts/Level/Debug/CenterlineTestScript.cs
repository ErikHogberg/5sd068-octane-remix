using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CenterlineTestScript : MonoBehaviour {

	public CenterlineScript Centerline;
	public Color LineColor = Color.blue;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	void OnDrawGizmos() {
		if (Centerline != null) {
			Gizmos.color = LineColor;
			Gizmos.DrawLine(transform.position, Centerline.GetClosestPoint(transform.position));
		}
	}
}
