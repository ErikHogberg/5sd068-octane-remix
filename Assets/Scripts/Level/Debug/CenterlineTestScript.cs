using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CenterlineTestScript : MonoBehaviour {

	public CenterlineScript Centerline;
	public Color LineColor = Color.blue;

	// TODO: draw 3d arrow for showing delta rotation at distance ahead
	// IDEA: float field for distance ahead, optional gameobject field for object to rotate

	public float DistanceAhead = 1;
	public float ArrowLength = 5;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	void OnDrawGizmos() {
		if (Centerline) {
			Gizmos.color = LineColor;
			Gizmos.DrawLine(transform.position, Centerline.GetClosestPoint(transform.position));

			// if(Arrow){
			// Arrow.transform.localRotation = 
			Quaternion rot = Centerline.GetRotationDeltaAhead(transform.position, DistanceAhead, out int index);

			Gizmos.color = Color.green;
			if (index < 0) index = 0;
			Gizmos.DrawCube(Centerline.LinePoints[index], Vector3.one);
			Gizmos.DrawLine(transform.position,  transform.TransformPoint(rot *Vector3.forward * ArrowLength));
			// }
		}
	}
}
