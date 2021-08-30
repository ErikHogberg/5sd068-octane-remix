using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BoidsVFXTestScript : MonoBehaviour {

	public VisualEffect BoidsVFX;

	private Vector3 oldPos;

	void Start() {
		if(!BoidsVFX){
			enabled = false;
			Debug.LogWarning("no vfx assigned!");
			return;
		}

		oldPos = transform.position;
		UpdateVFX();
	}

	void Update() {
		if (oldPos != transform.position) {
			oldPos = transform.position;
			UpdateVFX();
		}
	}

	void UpdateVFX() {
		BoidsVFX.SetVector3("target", oldPos);
	}

}
