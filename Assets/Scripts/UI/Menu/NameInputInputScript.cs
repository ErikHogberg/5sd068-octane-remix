using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameInputInputScript : MonoBehaviour {

	public static string PlayerName = "";

	public TMP_InputField NameInput;

	void Start() {
		NameInput.text = PlayerName;
	}

	// Method made for input field OnValueChange
	public void SetPlayerName(string newPlayerName) {
		PlayerName = newPlayerName;
	}

	public static string GetPlayerName() {
		if (PlayerName == "") {
			return "No Name";
		}

		return PlayerName;
	}

}
