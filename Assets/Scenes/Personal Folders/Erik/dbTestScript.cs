﻿using System.Linq;
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
	public TMP_InputField PlayerQueryInput;
	public TMP_InputField RemixQueryInput;

	[Space]
	public dbTestListItem FirstListEntry;
	public GameObject ListContentParent;

	List<dbTestListItem> ListEntries;

	public HighscoreManager.HighscoreList DbList;

	public bool OrderedHighscore = false;

	public void SetOrdered (bool value){
		OrderedHighscore = value;
	}

	// public bool Insert = false;

	void Start() {
		DbList = new HighscoreList();
		// list = new HighscoreManager.HighscoreList();
		// list.Start(Insert);
		// list = null;

		ListEntries = new List<dbTestListItem>();
		ListEntries.Add(FirstListEntry);
		// FirstListEntry.gameObject.SetActive(false);

		UpdateUI();
	}

	public void Submit() {
		string name = NameInput.text;
		string remix = RemixInput.text;
		if (name == "" || remix == "" || ScoreInput.text == "" || TimeInput.text == "") {
			Debug.Log("empty input field");
			return;
		}

		int score = int.Parse(ScoreInput.text);
		int time = int.Parse(TimeInput.text);

		// Debug.Log("parsed ints: score " + score + ", time " + time);

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

		DbList.Insert(name, remix, score, time, character);

		UpdateUI();
	}

	public void UpdateUI() {
		// TODO: get highscores
		// TODO: create/update list entries

		string player = PlayerQueryInput.text;
		string remix = RemixQueryInput.text;

		bool queryPlayer = !(player == "" || player == "Player Query");
		bool queryRemix = !(remix == "" || remix == "Remix Query");

		Debug.Log("Query: player " + queryPlayer + ", remix " + queryRemix);


		List<HighscoreEntry> highscores = new List<HighscoreEntry>();

		if (queryRemix) {
			var matchingRemix = DbList.GetRemixByFreeText(remix);
			if (queryPlayer) {
				var matchingPlayers = DbList.GetPlayerByFreeText(player);
				foreach (var remixMatch in matchingRemix) {
					foreach (var playerMatch in matchingPlayers) {
						foreach (var highscore in DbList.GetHighscoresByRemixAndPlayer(remixMatch.EntryId, playerMatch.EntryId)) {
							highscores.Add(highscore);
						}
					}
				}
			} else {
				foreach (var remixMatch in matchingRemix) {
					foreach (var highscore in DbList.GetHighscoresByRemix(remixMatch.EntryId)) {
						highscores.Add(highscore);
					}
				}
			}
		} else {
			if (queryPlayer) {
				var matchingPlayers = DbList.GetPlayerByFreeText(player);
				foreach (var playerMatch in matchingPlayers) {
					foreach (var highscore in DbList.GetHighscoresByPlayer(playerMatch.EntryId)) {
						highscores.Add(highscore);
					}
				}
			} else {
				foreach (var highscore in DbList.GetAllHighscores()) {
					highscores.Add(highscore);
				}
			}
		}

		IEnumerable<HighscoreEntry> highscoreOrdered = OrderedHighscore ? highscores.OrderBy(e => e.Score) : highscores.AsEnumerable();

		Dictionary<long, string> players = new Dictionary<long, string>();
		Dictionary<long, string> remixes = new Dictionary<long, string>();

		int entryCount = highscores.Count;

		foreach (var item in ListEntries) {
			item.gameObject.SetActive(false);
		}

		for (int i = 0; i < entryCount; i++) {
			if (i >= ListEntries.Count) {
				var entry = Instantiate(FirstListEntry.gameObject, ListContentParent.transform).GetComponent<dbTestListItem>();
				entry.TestScript = this;
				ListEntries.Add(entry);
			}

			if (!players.ContainsKey(highscoreOrdered.ElementAt(i).PlayerEntryId)) {
				players.Add(highscoreOrdered.ElementAt(i).PlayerEntryId, DbList.GetPlayerByEntryId(highscoreOrdered.ElementAt(i).PlayerEntryId).Name);
			}
			if (!remixes.ContainsKey(highscoreOrdered.ElementAt(i).RemixEntryId)) {
				remixes.Add(highscoreOrdered.ElementAt(i).RemixEntryId, DbList.GetRemixByEntryId(highscoreOrdered.ElementAt(i).RemixEntryId).RemixId);
			}


			ListEntries[i].gameObject.SetActive(true);
			// TODO: assign highscore data to entry
			ListEntries[i].SetText(
				highscoreOrdered.ElementAt(i).EntryId,
				players[highscoreOrdered.ElementAt(i).PlayerEntryId],
				remixes[highscoreOrdered.ElementAt(i).RemixEntryId],
				highscoreOrdered.ElementAt(i).Score,
				highscoreOrdered.ElementAt(i).Time,
				highscoreOrdered.ElementAt(i).Character
			);
		}

	}

	public void Clear() {
		DbList.ClearAllTables();
		UpdateUI();
	}


}
