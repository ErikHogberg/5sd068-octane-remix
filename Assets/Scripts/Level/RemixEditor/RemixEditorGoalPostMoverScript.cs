using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemixEditorGoalPostMoverScript : MonoBehaviour {

	public bool SetStart = false;
	public bool SetFinish = false;

	[Tooltip("Object that will be stretched between the goal post mover and the closest point on the centerline")]
	public GameObject ArrowBody;
	[Tooltip("Object that will be moved to closest point on the centerline and rotated towards it relative to the goal post mover")]
	public GameObject ArrowHead;
	[Tooltip("Object containing the finish line projector which will be placed on the closest position on the centerline")]
	public GameObject LineProjector;

	void Start() {

	}

	void Update() {

	}
}
