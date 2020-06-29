using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using System;

public class HighscoreEntryUIScript : MonoBehaviour {

	public TMP_Text PlayerNameText;
	public TMP_InputField RemixIDInput;
	public TMP_Text ScoreText;
	public TMP_Text TimeText;
	public TMP_Text CharacterText;

	public long highscoreEntryId;
	public HighscoreListUIScript ListScript;

	public void SetText(long entryId, string playerName, string remixId, long score, float time, CharacterSelected character) {
		highscoreEntryId = entryId;
		PlayerNameText.text = playerName;
		RemixIDInput.text = remixId;
		ScoreText.text = score.ToString();

		TimeSpan t = System.TimeSpan.FromSeconds(time);
		int milli = t.Milliseconds / 10;
		TimeText.text = TimerScript.TimeCalc(t.Hours)
			+ ":" + TimerScript.TimeCalc(t.Minutes)
			+ ":" + TimerScript.TimeCalc(t.Seconds)
			+ ":" + TimerScript.TimeCalc(milli);

		CharacterText.text = character.ToString();
	}

}
