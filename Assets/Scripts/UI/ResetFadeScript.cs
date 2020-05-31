using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResetTransition : MonoBehaviour, IObserver<Camera> {

	public static ResetTransition MainInstance;

	public RenderTexture renderTexture;

	private void Awake() {
		MainInstance = this;
	}

	private void OnDestroy() {
		MainInstance = null;
	}

	protected virtual void Start() {
		// fadeScript = GetComponent<GenericFadeImageUIScript>();
		SteeringScript.MainInstance.ResetObservers.Add(this);
	}

	public void TakeSnapshot(Camera snapshotCamera) {
		// var oldTarget = snapshotCamera.targetTexture;
		snapshotCamera.targetTexture = renderTexture;
		snapshotCamera.Render();
		snapshotCamera.targetTexture = null;
		// snapshotCamera.targetTexture = oldTarget;

	}

	public virtual void Notify(Camera snapshotCamera) {
		TakeSnapshot(snapshotCamera);
		// fadeScript.FadeOut();
	}

	public static void StartTransition(){
		MainInstance?.Notify(Camera.main);
	}
}

[RequireComponent(typeof(GenericFadeImageUIScript))]
public class ResetFadeScript : ResetTransition {
	private GenericFadeImageUIScript fadeScript;

	protected override void Start() {
		fadeScript = GetComponent<GenericFadeImageUIScript>();
		base.Start();
	}

	public override void Notify(Camera snapshotCamera) {
		fadeScript.FadeOut();
		base.Notify(snapshotCamera);
	}
}
