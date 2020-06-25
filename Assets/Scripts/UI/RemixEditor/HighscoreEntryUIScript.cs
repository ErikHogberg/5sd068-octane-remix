using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class HighscoreEntryUIScript : MonoBehaviour {

	public TMP_Text PlayerNameText;
	public TMP_InputField RemixIDInput;
	public TMP_Text ScoreText;
	public TMP_Text TimeText;
	public TMP_Text CharacterText;

	public long highscoreEntryId;
	public HighscoreListUIScript ListScript;

	public void SetText(long entryId, string playerName, string remixId, long score, long time, CharacterSelected character) {
		highscoreEntryId = entryId;
		PlayerNameText.text = playerName;
		RemixIDInput.text = remixId;
		ScoreText.text = score.ToString();
		TimeText.text = time.ToString();
		CharacterText.text = character.ToString();
	}


}
