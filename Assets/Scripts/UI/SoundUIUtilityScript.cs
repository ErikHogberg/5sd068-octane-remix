using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundUIUtilityScript : MonoBehaviour {

	public void PlaySound(string soundName) {
		SoundManager.PlaySound(soundName);
	}

}
