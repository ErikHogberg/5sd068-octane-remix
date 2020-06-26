using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ScoreManager : MonoBehaviour {

	private static List<ScoreBoard> Boards = new List<ScoreBoard>();

	public static void GenerateScoreBoards(int amount, bool reset = false) {
		if (reset) {
			Boards.Clear();
		}

		for (int i = Boards.Count; i < amount; i++) {
			Boards.Add(new ScoreBoard());
		}
	}

	public static ScoreBoard Board(int index) {
		if (index <= (Boards.Count - 1)) {
			return Boards[index];
		} else {
			UnityEngine.Debug.Log("ScoreManager: Index does not exist in ScoreBoards list");
			return null;
		}
	}

	public static long GetGrandTotalScore(int index) {
		if (index <= (Boards.Count - 1)) {
			long[] three = Boards[index].GetAllThree();
			long grandTotal = 0;
			foreach (int nr in three) {
				grandTotal += nr;
			}
			return grandTotal;
		} else {
			UnityEngine.Debug.Log("ScoreManager: Index does not exist in ScoreBoards list");
			return 0;
		}
	}

	public static void ClearAllScores() {
		foreach (ScoreBoard board in Boards) {
			board.ClearScores();
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
