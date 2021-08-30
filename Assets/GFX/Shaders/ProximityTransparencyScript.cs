using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityTransparencyScript : MonoBehaviour {

	Material mat;

	public GameObject Target;

	void Start() {
		mat = GetComponent<MeshRenderer>().material;
	}

	void Update() {
		if (Target)
			mat.SetVector("proximityTarget", Target.transform.position);

	}

}
