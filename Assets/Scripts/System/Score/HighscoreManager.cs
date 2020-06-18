using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public static class HighscoreManager {

	// TODO: player name, separate top ten for each player name

	public class HighscoreEntry {
		public int Score;
		public int Time;
		public CharacterSelected Character;
		// public string RemixID;
	}

	[Serializable]
	public class HighscoreListInstance {
		// IDEA: only save top 10 in each category

		// public static List<HighscoreEntry> Entries;
		public Dictionary<string, List<HighscoreEntry>> Entries = new Dictionary<string, List<HighscoreEntry>>();

		public static HighscoreListInstance LoadListFromFile(string path) {
			return JsonUtility.FromJson<HighscoreListInstance>(File.ReadAllText(path));
		}

		public void LoadListFromServer(string url) {

		}

		public void SaveListToFile(string path) {
			File.WriteAllText(SavePath, JsonUtility.ToJson(this, true));

		}

		public void SaveListToServer(string url) {

		}

		// Sort one list
		// NOTE: truncates list
		public void Sort(string remixId, bool skipKeyCheck = false) {
			if (!skipKeyCheck && !Entries.ContainsKey(remixId))
				return;

			Entries[remixId] = Entries[remixId].OrderByDescending(x => x.Score).Take(10).ToList();
		}

		// sort all lists
		public void Sort() {
			foreach (var item in Entries) {
				Sort(item.Key, true);
			}
		}

		public void AddScore(string remixId, HighscoreEntry entry){
			if (!Entries.ContainsKey(remixId))
				Entries.Add(remixId, new List<HighscoreEntry>());

			Entries[remixId].Add(entry);
			// TODO: sort and truncate
		}

		// TODO: search all entries for top 10 highest scores
		// IDEA: keep cached list, marked dirty on adding score to any list, update on get if dirty
		// public IEnumerable<(string, HighscoreEntry)> GetAllTimeHighscore() { }

	}

	public static HighscoreListInstance List = new HighscoreListInstance();

	public static string SavePath = Application.dataPath + "/highscore.json";

	// TODO: async/coroutine fn for getting entries

	

}
