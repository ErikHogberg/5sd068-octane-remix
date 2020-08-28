using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TilingSegmentBendingScript : MonoBehaviour {

	public Transform Target;
	public Transform TargetMagnitude;
	public Transform StartMagnitude;

	public GameObject Segment;

	public float MinLength;
	public float MaxLength;

	void Start() {

	}

	void Update() {
		if (!Target || !TargetMagnitude || !StartMagnitude) {
			return;
		}

		// TODO: cache updating

		// TODO: get curve
		// TODO: measure length of curve
		// TODO: decide number of segments
		// TODO: place, bend and snap segments
	}
}
