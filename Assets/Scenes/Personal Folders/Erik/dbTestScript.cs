using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using static HighscoreManager;

public class dbTestScript : MonoBehaviour {

	public TMP_InputField NameInput;
	public TMP_InputField RemixInput;
	public TMP_InputField ScoreInput;
	public TMP_InputField TimeInput;
	public TMP_Dropdown CharacterDropdown;

	[Space]
	public GameObject ListEntryPrefab;

	public List<GameObject> ListEntries = new List<GameObject>();

	HighscoreManager.HighscoreList list;

	// public bool Insert = false;

	void Start() {
		list = new HighscoreList();
		// list = new HighscoreManager.HighscoreList();
		// list.Start(Insert);
		// list = null;
	}

	public void Submit() {
		string name = NameInput.text;
		string remix = RemixInput.text;
		int score = int.Parse(ScoreInput.text);
		int time = int.Parse(TimeInput.text);

		CharacterSelected character = CharacterSelected.NONE;
		switch (CharacterDropdown.options[CharacterDropdown.value].text) {
			case "Akash":
				character = CharacterSelected.AKASH;
				break;
			case "Michishige":
				character = CharacterSelected.MICHISHIGE;
				break;
			case "Ludwig":
				character = CharacterSelected.LUDWIG;
				break;
		}

		list.Insert(name, remix, score, time, character);
	}

	public void UpdateUI() {
		//TODO: get highscores
		// TODO: create/update list entries
	}


}
