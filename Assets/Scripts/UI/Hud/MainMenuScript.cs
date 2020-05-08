using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A script for anything we'd want to happen on the main menu screen
public class MainMenuScript : MonoBehaviour
{
	public static MainMenuScript Instance;
	private ChangeSceneUIScript scenes;

	void Awake() {
		Instance = this;
		scenes = GetComponent<ChangeSceneUIScript>();
	}
	
	public void StartBtnClick(string sceneName){
		scenes.StartScene(sceneName);
	}

	public void ExitBtnClick() {
		scenes.Quit();
    }
}
