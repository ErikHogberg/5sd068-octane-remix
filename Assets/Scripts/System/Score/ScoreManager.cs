using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour {

	private static List<ScoreBoard> Boards = new List<ScoreBoard>();

	public static void GenerateScoreBoards(int amount) {
		for (int i = 0; i < amount; i++) {
			Boards.Add(new ScoreBoard());
		}
	}

	public static ScoreBoard Board(int index) {
		if (index <= (Boards.Count - 1)) {
			return Boards[index];
		} else {
			Debug.Log("ScoreManager: Index does not exist in ScoreBoards list");
			return null;
		}
	}


	//Test code for anyone who wants it
	/*
		ScoreManager.GenerateScoreBoards(2);
		ScoreBoard boardOne = ScoreManager.Board(0);

		boardOne.AddRemix(500);
		boardOne.AddSkill(ScoreSkill.DRIFT, 1000);

		Debug.Log("Remix score: " + ScoreManager.Board(0).GetRemix());
		Debug.Log("Drift score: " + ScoreManager.Board(0).GetSkill(ScoreSkill.DRIFT));
	*/
}
