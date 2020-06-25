using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class dbTestListItem : MonoBehaviour {
	public TMP_Text PlayerNameText;
	public TMP_Text RemixIdText;
	public TMP_Text ScoreText;
	public TMP_Text TimeText;
	public TMP_Text CharacterText;

	private long dbEntryId;
	public dbTestScript TestScript;

	public void SetText(long dbEntryId, string playerName, string remixId, long score, long time, CharacterSelected character) {
		PlayerNameText.text = playerName;
		RemixIdText.text = remixId;
		ScoreText.text = score.ToString();
		TimeText.text = time.ToString();
		CharacterText.text = character.ToString();

		this.dbEntryId = dbEntryId;
	}

	public void DeleteFromDb() {
		// FIXME: deletion always fails
		TestScript.DbList.RemoveHighscore(dbEntryId);
		Debug.Log("Deleting entry " + dbEntryId);
		TestScript.UpdateUI();
	}
}
