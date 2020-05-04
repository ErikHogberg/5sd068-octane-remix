using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RespawnTriggerScript : MonoBehaviour {

	private void OnTriggerEnter(Collider other) {
		LevelPieceSuperClass.ResetToCurrentSegment();
	}

}
