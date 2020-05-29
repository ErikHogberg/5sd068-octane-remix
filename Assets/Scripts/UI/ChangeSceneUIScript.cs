using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneUIScript : MonoBehaviour {

	public void StartScene(string sceneName){
		Time.timeScale = 1f;
		SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
	}

	public void Quit(){
		Application.Quit();
	}

}
