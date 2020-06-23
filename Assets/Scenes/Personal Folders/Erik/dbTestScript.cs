using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HighscoreManager;

public class dbTestScript : MonoBehaviour {

	// HighscoreManager.HighscoreList list;

	public bool Insert = false;

	void Start() {
		HighscoreList list = new HighscoreList();
		// list = new HighscoreManager.HighscoreList();
		list.Start(Insert);
		list = null;
	}

}
