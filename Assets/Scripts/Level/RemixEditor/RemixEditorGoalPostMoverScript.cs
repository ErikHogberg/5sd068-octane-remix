using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class RemixEditorGoalPostMoverScript : MonoBehaviour, IPointerDownHandler {

	public bool SetStart = false;
	public bool SetFinish = false;

	[Tooltip("Object that will be stretched between the goal post mover and the closest point on the centerline")]
	public GameObject ArrowBody;
	[Tooltip("Object that will be moved to closest point on the centerline and rotated towards it relative to the goal post mover")]
	public GameObject ArrowHead;
	[Tooltip("Object containing the finish line projector which will be placed on the closest position on the centerline")]
	public GameObject LineProjector;

	[Space]
	public CenterlineScript Centerline;
	bool moving = false;

	void Start() {
		if (!Centerline) Centerline = CenterlineScript.MainInstance;
		if (!Centerline) enabled = false;
		SetGoalPost();
	}

	void Update() {
		if (Mouse.current.leftButton.wasReleasedThisFrame)
			moving = false;

		if (moving) {
			Vector2 mouseDelta = Mouse.current.delta.ReadValue();
			transform.Translate(mouseDelta.x, 0, mouseDelta.y, Space.World);

			SetGoalPost();
		}
	}

	private void SetGoalPost() {
		Vector3 closestPos = Centerline.SetGoalPost(transform.position, SetStart, SetFinish, out var lineDir);

		Vector3 delta = closestPos - transform.position;
		Quaternion dir = Quaternion.FromToRotation(Vector3.forward, delta);
		float length = delta.magnitude;

		ArrowBody.transform.position = transform.position;
		ArrowBody.transform.rotation = dir;
		ArrowBody.transform.localScale = Vector3.one + Vector3.forward * length;

		ArrowHead.transform.rotation = dir;
		ArrowHead.transform.position = closestPos;

		LineProjector.transform.position = closestPos;
		LineProjector.transform.rotation = lineDir;
	}

	public void OnPointerDown(PointerEventData eventData) {
		if (Mouse.current.leftButton.wasPressedThisFrame) {
			moving = true;
		}
	}

}
