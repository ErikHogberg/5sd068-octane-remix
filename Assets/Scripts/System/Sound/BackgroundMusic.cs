using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
	public bool isMenu;

	void OnEnable() {
		if (isMenu) {
			SoundManager.StopLooping("playscene_music");
			SoundManager.PlaySound("menu_music");
		}
		else {
			SoundManager.StopLooping("menu_music");
			SoundManager.PlaySound("playscene_music");
		}
    }
	void OnDisable() {
		if (!isMenu) {
			SoundManager.StopAll();
		}
    }
}
