using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GenericFirstPersonFreeRoamScript : MonoBehaviour {

	public float MoveSpeed = 100f;
	public float MouseSpeed = 100f;

	private bool mouseDown = false;

	Vector3 oldMousePos = Vector3.zero;

	private void Update() {

		if (Input.GetMouseButtonDown(1)) {
			mouseDown = true;
		} else if (Input.GetMouseButtonUp(1)) {
			mouseDown = false;
		}

		if (mouseDown) {
			Vector3 delta = Input.mousePosition - oldMousePos;

			transform.Rotate(Vector3.up, delta.x * MouseSpeed * Time.deltaTime, Space.World);
			transform.Rotate(Vector3.right,-delta.y * MouseSpeed * Time.deltaTime, Space.Self);
		}

		if (Input.GetKey(KeyCode.I))
			transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime, Space.Self);
		if (Input.GetKey(KeyCode.K))
			transform.Translate(Vector3.back * MoveSpeed * Time.deltaTime, Space.Self);

		if (Input.GetKey(KeyCode.J))
			transform.Translate(Vector3.left * MoveSpeed * Time.deltaTime, Space.Self);
		if (Input.GetKey(KeyCode.L))
			transform.Translate(Vector3.right * MoveSpeed * Time.deltaTime, Space.Self);

		oldMousePos = Input.mousePosition;

	}

}
