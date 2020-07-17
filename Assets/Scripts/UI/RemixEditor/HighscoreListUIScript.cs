using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static HighscoreManager;

public class HighscoreListUIScript : MonoBehaviour {

	public TMP_InputField PlayerQueryInput;
	public TMP_InputField RemixQueryInput;

	public HighscoreEntryUIScript FirstListEntry;
	public GameObject ListContentParent;

	public GameObject SelectionErrorText;

	string selectedHighscoreRemix = null;

	List<HighscoreEntryUIScript> ListEntries = new List<HighscoreEntryUIScript>();

	HighscoreList dbList;
	bool timeOrdered = true;

	void Start() {
		Init();
	}

	private void OnEnable() {
		Init();
	}

	private void Init() {
		if (dbList == null)
			dbList = HighscoreManager.List;

		RemixQueryInput.text = LevelPieceSuperClass.GetRemixString();

		FirstListEntry.gameObject.SetActive(false);

		SelectionErrorText.SetActive(false);
		selectedHighscoreRemix = null;

		UpdateUI();
	}

	public void Toggle() {
		gameObject.SetActive(!gameObject.activeSelf);
	}

	public void SetOrdering(bool timeOrdered) {
		if (this.timeOrdered == timeOrdered) {
			return;
		}
		this.timeOrdered = timeOrdered;
		UpdateUI();
	}

	public void ClearFilters() {
		RemixQueryInput.text = "";
		PlayerQueryInput.text = "";
		UpdateUI();
	}

	public void FilterCurrentPlayerAndRemix() {
		RemixQueryInput.text = LevelPieceSuperClass.GetRemixString();
		PlayerQueryInput.text = NameInputInputScript.GetPlayerName();
		UpdateUI();
	}

	public void SelectHighscore(string remix) {
		Debug.Log("selected highscore with remix " + remix);
		selectedHighscoreRemix = remix;
	}

	public void DeselectHighscore() {
		selectedHighscoreRemix = null;
	}

	public void ChallengeHighscore() {
		if (selectedHighscoreRemix == null) {
			SelectionErrorText.SetActive(true);
			return;
		}

		LevelPieceSuperClass.LoadRemixFromString(selectedHighscoreRemix);
		ChangeSceneUIScript.MainInstance.SwapCurrentScene("PlayScene");
	}

	public void EditHighscoreRemix() {
		if (selectedHighscoreRemix == null) {
			SelectionErrorText.SetActive(true);
			return;
		}

		LevelPieceSuperClass.LoadRemixFromString(selectedHighscoreRemix);
		gameObject.SetActive(false);
	}

	public void FilterRemixBySelected() {
		if (selectedHighscoreRemix == null) {
			SelectionErrorText.SetActive(true);
			return;
		}

		RemixQueryInput.text = selectedHighscoreRemix;
		UpdateUI();
	}

	public void UpdateUI() {
		SelectionErrorText.SetActive(false);
		DeselectHighscore();

		string player = PlayerQueryInput.text;
		string remix = RemixQueryInput.text;

		bool queryPlayer = !(player == "" || player == "Player Query");
		bool queryRemix = !(remix == "" || remix == "Remix Query");

		Debug.Log("Query: player " + queryPlayer + ", remix " + queryRemix);


		List<HighscoreEntry> highscores = new List<HighscoreEntry>();

		if (queryRemix) {
			var matchingRemix = dbList.GetRemixByFreeText(remix);
			if (queryPlayer) {
				var matchingPlayers = dbList.GetPlayerByFreeText(player);
				foreach (var remixMatch in matchingRemix) {
					foreach (var playerMatch in matchingPlayers) {
						foreach (var highscore in dbList.GetHighscoresByRemixAndPlayer(remixMatch.EntryId, playerMatch.EntryId)) {
							highscores.Add(highscore);
						}
					}
				}
			} else {
				foreach (var remixMatch in matchingRemix) {
					foreach (var highscore in dbList.GetHighscoresByRemix(remixMatch.EntryId)) {
						highscores.Add(highscore);
					}
				}
			}
		} else {
			if (queryPlayer) {
				var matchingPlayers = dbList.GetPlayerByFreeText(player);
				foreach (var playerMatch in matchingPlayers) {
					foreach (var highscore in dbList.GetHighscoresByPlayer(playerMatch.EntryId)) {
						highscores.Add(highscore);
					}
				}
			} else {
				foreach (var highscore in dbList.GetAllHighscores()) {
					highscores.Add(highscore);
				}
			}
		}

		IEnumerable<HighscoreEntry> highscoreOrdered = timeOrdered ? highscores.OrderBy(e => e.Time) : highscores.OrderByDescending(e => e.Score);

		Dictionary<long, string> players = new Dictionary<long, string>();
		Dictionary<long, string> remixes = new Dictionary<long, string>();

		int entryCount = highscores.Count;

		foreach (var item in ListEntries) {
			item.gameObject.SetActive(false);
		}

		for (int i = 0; i < entryCount; i++) {
			if (i >= ListEntries.Count) {
				var entry = Instantiate(FirstListEntry.gameObject, ListContentParent.transform).GetComponent<HighscoreEntryUIScript>();
				entry.ListScript = this;
				ListEntries.Add(entry);
			}

			if (!players.ContainsKey(highscoreOrdered.ElementAt(i).PlayerEntryId)) {
				players.Add(highscoreOrdered.ElementAt(i).PlayerEntryId, dbList.GetPlayerByEntryId(highscoreOrdered.ElementAt(i).PlayerEntryId).Name);
			}
			if (!remixes.ContainsKey(highscoreOrdered.ElementAt(i).RemixEntryId)) {
				remixes.Add(highscoreOrdered.ElementAt(i).RemixEntryId, dbList.GetRemixByEntryId(highscoreOrdered.ElementAt(i).RemixEntryId).RemixId);
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

}
