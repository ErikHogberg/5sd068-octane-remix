using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
	[Tooltip("Is the scene that this gameobject is part of one of the menu scenes or not? " +
			"The only exception should be the PlayScene.")]
	public bool isMenu;
	public bool PrintDebug = false;

	void OnEnable() {
		if (isMenu) {
			SoundManager.StopLooping("playscene_music", PrintDebug);
			SoundManager.PlaySound("menu_music");
		}
		else {
			SoundManager.StopLooping("menu_music", PrintDebug);
			SoundManager.PlaySound("playscene_music");
		}
    }
	void OnDisable() {
		if (!isMenu) {
			SoundManager.StopAll();
		}
    }
}
