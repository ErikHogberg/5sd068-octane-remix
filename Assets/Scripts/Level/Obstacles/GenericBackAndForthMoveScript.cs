using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericBackAndForthMoveScript : MonoBehaviour {

	private enum MoveState {
		MovingForth,
		ReachedEnd,
		MovingBack,
		ReachedStart,
	}

	MoveState state = MoveState.MovingForth;

	[Tooltip("the other location to move between, the first location being the start location of the object")]
	public Transform Target;

	Vector3 initPos;
	Vector3 targetInitPos;

	[Tooltip("How long it takes to move between the 2 locations")]
	[Min(0.01f)]
	public float MoveTime = 1;
	[Tooltip("How long the object will pause at the 2 locations")]
	[Min(0.01f)]
	public float PauseTime = 1;

	[Tooltip("How the velocity changes during movement")]
	public AnimationCurve MoveCurve = AnimationCurve.Linear(0, 0, 1, 1);
	public bool FlipCurveOnReturn = true;

	float timer = 0;

	public bool AffectedBySloMo = true;

	void Start() {
		initPos = transform.position;

		if (!Target) {
			Debug.LogError("Target not assigned for moving object!");
			enabled = false;
			return;
		}
		targetInitPos = Target.position;
	}

	void Update() {

		if (AffectedBySloMo) {
			timer += Time.deltaTime;
		} else {
			timer += Time.unscaledDeltaTime;
		}

		float percentage = 0;
		switch (state) {
			case MoveState.MovingForth:
				percentage = timer / MoveTime;
				percentage = MoveCurve.Evaluate(percentage);
				if (timer > MoveTime) {
					timer = 0;
					state = MoveState.ReachedEnd;
					percentage = 1;
				}
				break;
			case MoveState.ReachedEnd:
				percentage = 1;
				if (timer > PauseTime) {
					timer = 0;
					state = MoveState.MovingBack;
				}
				break;
			case MoveState.MovingBack:
				percentage = 1f - (timer / MoveTime);
				if (FlipCurveOnReturn) {
					percentage = 1 - MoveCurve.Evaluate(1 - percentage);
				} else {
					percentage = MoveCurve.Evaluate(percentage);
				}
				if (timer > MoveTime) {
					timer = 0;
					state = MoveState.ReachedStart;
					percentage = 0;
				}
				break;
			case MoveState.ReachedStart:
				percentage = 0;
				if (timer > PauseTime) {
					timer = 0;
					state = MoveState.MovingForth;
				}
				break;
		}

		transform.position = Vector3.Lerp(initPos, targetInitPos, percentage);
	}
}
