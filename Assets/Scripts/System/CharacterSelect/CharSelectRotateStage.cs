using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSelectRotateStage : MonoBehaviour
{
	public float rotationSpeed = 10.0f;

    void Update()
    {
		transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
	}
}
