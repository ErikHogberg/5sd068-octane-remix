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


		if (Mouse.current.rightButton.isPressed) {
			Vector3 delta = Mouse.current.delta.ReadValue(); //Input.mousePosition - oldMousePos;

			transform.Rotate(Vector3.up, delta.x * MouseSpeed * Time.deltaTime, Space.World);
			transform.Rotate(Vector3.right,-delta.y * MouseSpeed * Time.deltaTime, Space.Self);
		}

		if (Keyboard.current.iKey.isPressed )
			transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime, Space.Self);
		if (Keyboard.current.kKey.isPressed )
			transform.Translate(Vector3.back * MoveSpeed * Time.deltaTime, Space.Self);

		if (Keyboard.current.jKey.isPressed )
			transform.Translate(Vector3.left * MoveSpeed * Time.deltaTime, Space.Self);
		if (Keyboard.current.lKey.isPressed )
			transform.Translate(Vector3.right * MoveSpeed * Time.deltaTime, Space.Self);


	}

}
