using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateScript : MonoBehaviour {

	public Vector3 Axis = Vector3.up;
	public float Speed = 1f;
	public Space RelativeMode = Space.World;

	void Update() {

		transform.Rotate(Axis, Speed * Time.deltaTime, RelativeMode);

	}
}
