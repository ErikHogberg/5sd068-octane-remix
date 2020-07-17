using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RemixMapEditButtonScript : MonoBehaviour {

	public LevelPieceSuperClass LevelPiece;

	private void SelectPiece() {

	}

	void Start() {
		GetComponent<Button>().onClick.AddListener(SelectPiece);
	}

}
