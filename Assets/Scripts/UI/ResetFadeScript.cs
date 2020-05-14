using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericFadeImageUIScript))]
public class ResetFadeScript : MonoBehaviour, IObserver<Camera> {

	public RenderTexture renderTexture;
	// public Camera snapshotCamera;

	private GenericFadeImageUIScript fadeScript;


	private void Start() {
		fadeScript = GetComponent<GenericFadeImageUIScript>();
		SteeringScript.MainInstance.ResetObservers.Add(this);		
	}

	public void TakeSnapshot(Camera snapshotCamera) {
		// var oldTarget = snapshotCamera.targetTexture;
		snapshotCamera.targetTexture = renderTexture;
		snapshotCamera.Render();
		snapshotCamera.targetTexture = null;
		// snapshotCamera.targetTexture = oldTarget;

	}

	public void Notify(Camera snapshotCamera) {
        TakeSnapshot(snapshotCamera);
        fadeScript.FadeOut();
	}

}
