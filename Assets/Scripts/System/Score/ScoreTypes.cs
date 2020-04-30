using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum ScoreSkill {
	DRIFT = 0,
	DODGE,
	AIRTIME
}

public class Score {
	private int score = 0;
	public void AddScore(int add) { score += add; }
	public int GetScore() { return score; }
}

public class SkillScore {
	
	private Dictionary<ScoreSkill, Score> skillScoreTypes = new Dictionary<ScoreSkill, Score>();

	public SkillScore() {
		//Making a list entry for every different type in the ScoreSkill enum
		foreach (int type in Enum.GetValues(typeof(ScoreSkill))) {
			Score newScore = new Score();
			skillScoreTypes.Add((ScoreSkill)type, newScore);
		}
	}

	public void AddScore(ScoreSkill p_type, int add) {
		skillScoreTypes[p_type].AddScore(add);
	}

	public int GetScore(ScoreSkill p_type) {
		return skillScoreTypes[p_type].GetScore();
	}

	public int GetScoreTotal() {
		int total = 0;
		foreach (KeyValuePair<ScoreSkill, Score> skill in skillScoreTypes) {
			total += skill.Value.GetScore();
		}
		return total;
	}

	public Dictionary<ScoreSkill, int> GetSkillScores() {
		Dictionary<ScoreSkill, int> all = new Dictionary<ScoreSkill, int>();
		foreach (KeyValuePair<ScoreSkill, Score> skill in skillScoreTypes) {
			all.Add(skill.Key, skill.Value.GetScore());
		}
		return all;
	}
}

public class ScoreBoard {

	private Score remix = new Score();
	private Score time = new Score();
	private SkillScore skill = new SkillScore();

	public ScoreBoard() {
		remix = new Score();
		time = new Score();
		skill = new SkillScore();
	}

	public void AddRemix(int add) { remix.AddScore(add); }
	public void AddTime(int add) { time.AddScore(add); }
	public void AddSkill(ScoreSkill type, int add) { skill.AddScore(type, add); }

	public int GetRemix() { return remix.GetScore(); }
	public int GetTime() { return time.GetScore(); }
	public int GetSkill(ScoreSkill type) { return skill.GetScore(type); }
	public int GetSkillTotal() { return skill.GetScoreTotal(); }

	public int[] GetAllThree() {
		int[] all = new int[3];
		all[0] = GetRemix();
		all[1] = GetTime();
		all[2] = GetSkillTotal();
		return all;
	}
	public Dictionary<ScoreSkill, int> GetAllSkillScores() {
		return skill.GetSkillScores();
	}
}
