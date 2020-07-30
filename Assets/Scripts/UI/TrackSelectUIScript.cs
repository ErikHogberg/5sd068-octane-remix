using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrackSelectUIScript : MonoBehaviour {
	public static string SelectedTrack = "Long";

	public void SetSelectedTrack(string track) {
		SelectedTrack = track;
	}

}
