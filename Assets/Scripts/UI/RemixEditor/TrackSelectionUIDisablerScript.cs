using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackSelectionUIDisablerScript : MonoBehaviour {
	
	// TODO: make more generic
	
	void Start() {
		if (TrackSelectUIScript.SelectedTrack == "Short") {
			gameObject.SetActive(false);
		}
	}

}
