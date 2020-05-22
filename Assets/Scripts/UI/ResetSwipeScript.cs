using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericSwipeMaskUIScript))]
public class ResetSwipeScript : ResetTransition {

	private GenericSwipeMaskUIScript swipeScript;

	protected override void Start(){
		swipeScript = GetComponent<GenericSwipeMaskUIScript>();
		base.Start();
	}

	public override void Notify(Camera snapshotCamera) {
        // fadeScript.FadeOut();
		base.Notify(snapshotCamera);
	}

}
