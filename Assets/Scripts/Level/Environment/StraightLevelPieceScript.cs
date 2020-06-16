using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightLevelPieceScript : LevelPieceSuperClass {

	[Header("Bones")]

	public Transform FrontParent;
	public Transform RearParent;

	public Transform FrontLeftBone;
	public Transform FrontRightBone;

	public Transform RearLeftBone;
	public Transform RearRightBone;

	[Tooltip("Bones on the left side, in order. 0 is front, grows towards rear")]
	public List<Transform> LeftBones;
	[Tooltip("Bones on the right side, in order. 0 is front, grows towards rear")]
	public List<Transform> RightBones;

}
