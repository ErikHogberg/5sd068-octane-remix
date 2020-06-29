using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum ScoreSkill {
	DRIFT = 0,
	DODGE,
	AIRTIME,
	BOOST
}

public class Score {
	private long score = 0;
	public void AddScore(long add) { score += add; }
	public long GetScore() { return score; }
	public void ClearScore() { score = 0; }
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

	public void AddScore(ScoreSkill p_type, long add) {
		skillScoreTypes[p_type].AddScore(add);
	}

	public long GetScore(ScoreSkill p_type) {
		return skillScoreTypes[p_type].GetScore();
	}

	public long GetScoreTotal() {
		long total = 0;
		foreach (KeyValuePair<ScoreSkill, Score> skill in skillScoreTypes) {
			total += skill.Value.GetScore();
		}
		return total;
	}

	public Dictionary<ScoreSkill, long> GetSkillScores() {
		Dictionary<ScoreSkill, long> all = new Dictionary<ScoreSkill, long>();
		foreach (KeyValuePair<ScoreSkill, Score> skill in skillScoreTypes) {
			all.Add(skill.Key, skill.Value.GetScore());
		}
		return all;
	}

	public void ClearSkillScores() {
		foreach (KeyValuePair<ScoreSkill, Score> skillScore in skillScoreTypes) {
			skillScore.Value.ClearScore();
		}
	}
}

public class ScoreBoard {

	private Score remix = new Score();
	private Score time = new Score();
	private SkillScore skill = new SkillScore();

	private bool collectingScore = true;

	public ScoreBoard() {
		remix = new Score();
		time = new Score();
		skill = new SkillScore();
	}

	public void StopScoreCollecting() {
		collectingScore = false;
	}

	public void AddRemix(long add) {
		if (collectingScore)
			remix.AddScore(add);
	}
	public void AddTime(long add) {
		if (collectingScore)
			time.AddScore(add);
	}
	public void AddSkill(ScoreSkill type, long add) {
		if (collectingScore)
			skill.AddScore(type, add);
	}

	public void ClearScores() { remix.ClearScore(); time.ClearScore(); skill.ClearSkillScores(); }

	public long GetRemix() { return remix.GetScore(); }
	public long GetTime() { return time.GetScore(); }
	public long GetSkill(ScoreSkill type) { return skill.GetScore(type); }
	public long GetSkillTotal() { return skill.GetScoreTotal(); }

	public long[] GetAllThree() {
		long[] all = new long[3];
		all[0] = GetRemix();
		all[1] = GetTime();
		all[2] = GetSkillTotal();
		return all;
	}

	public Dictionary<ScoreSkill, long> GetAllSkillScores() {
		return skill.GetSkillScores();
	}
}
