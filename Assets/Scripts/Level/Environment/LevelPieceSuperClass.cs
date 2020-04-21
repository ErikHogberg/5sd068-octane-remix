using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelPieceSuperClass : MonoBehaviour {

	public static List<LevelPieceSuperClass> Pieces = new List<LevelPieceSuperClass>();

	// TODO: register pieces globally
	// TODO: manipulate registered pieces in UI

	// TODO: comfortable way to reference obstacles avalable for placement

	private void Awake() {
		Pieces.Add(this);
	}


}
