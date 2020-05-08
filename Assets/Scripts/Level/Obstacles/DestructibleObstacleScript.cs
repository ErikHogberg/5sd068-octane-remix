using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObstacleScript : MonoBehaviour {

	private ExplodeComponent exploderinoThingie;

	// public float required

	[SerializeField]
	public ExplodeComponent ExplodeComponentOverride;

	private void Awake() {
		if (ExplodeComponentOverride) {
			exploderinoThingie = ExplodeComponentOverride;
		} else {
			exploderinoThingie = GetComponent<ExplodeComponent>();
		}
	}


}
