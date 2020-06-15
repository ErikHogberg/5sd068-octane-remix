using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericFadeImageUIScript))]
public class SceneTransitionScript : MonoBehaviour {

	public static SceneTransitionScript MainInstance;

	GenericFadeImageUIScript fadeScript;
	public bool Running => fadeScript.Running;

	private void Awake() {
		fadeScript = GetComponent<GenericFadeImageUIScript>();
		MainInstance = this;
	}

	private void OnDestroy() {
		MainInstance = null;
	}

	public void StartTransition() {
		fadeScript.FadeIn();
	}

}
