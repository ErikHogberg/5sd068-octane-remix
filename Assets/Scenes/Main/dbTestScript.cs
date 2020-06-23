using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dbTestScript : MonoBehaviour {

	// HighscoreManager.HighscoreList list;

	void Start() {
		HighscoreList list = new HighscoreList();
		// list = new HighscoreManager.HighscoreList();
		list.Start();
		list = null;
	}

}
