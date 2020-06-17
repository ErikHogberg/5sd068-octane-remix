using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HighscoreManager {

	public class HighscoreEntry {
		public int Score;
		public int Time;
		public CharacterSelected Character;
		public string RemixID;
	}

	public static List<HighscoreEntry> Entries;

	public static void LoadHighscoreList(){
		
	}

}
