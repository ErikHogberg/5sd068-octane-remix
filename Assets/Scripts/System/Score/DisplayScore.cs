using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayScore : MonoBehaviour
{
	private TMP_Text scoreText;
	private int updateInterval = 5;
	private int updateCount = 0;

	void Awake() { 
		scoreText = transform.GetChild(0).GetComponent<TMP_Text>(); 
		scoreText.text = "0";
		ScoreManager.GenerateScoreBoards(1);
	}
	void OnEnable() {
		ScoreManager.ClearAllScores();
	}

	void Update() {
		if (updateCount >= updateInterval) {
			UpdateScore();
			updateCount = 0;
		}
		updateCount++;
	}

	private void UpdateScore() {
		scoreText.text = ScoreManager.GetGrandTotalScore(0).ToString();
	}
}
