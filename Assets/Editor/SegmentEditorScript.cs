using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SegmentScript))]
[CanEditMultipleObjects]
public class SegmentEditorScript : Editor {

	SerializedProperty LevelPieceScript;
	float handleSize = 0.4f;

	void OnEnable() {
		// cellSize = serializedObject.FindProperty("CellSize");
		LevelPieceScript = serializedObject.FindProperty("LevelPieceScript");
	}


	private void OnSceneGUI() {
		SegmentScript segmentScript = target as SegmentScript;
		StraightLevelPieceScript levelPieceScript = segmentScript.LevelPieceScript;

		Handles.color = Color.white;

		for (int i = 1; i < levelPieceScript.LeftBones.Count; i++) {

			Handles.DrawLine(
				// segmentScript.transform.TransformPoint(levelPieceScript.LeftBones[i - 1].position),
				// segmentScript.transform.TransformPoint(levelPieceScript.LeftBones[i].position)
				// levelPieceScript.transform.TransformPoint( levelPieceScript.LeftBones[i - 1].localPosition),
				// levelPieceScript.transform.TransformPoint(levelPieceScript.LeftBones[i].localPosition)

				levelPieceScript.LeftBones[i - 1].position,
				levelPieceScript.LeftBones[i].position
			);

		}

		for (int i = 0; i < levelPieceScript.LeftBones.Count; i++) {

			EditorGUI.BeginChangeCheck();

			Transform handleTransform = levelPieceScript.LeftBones[i];
			Quaternion handleRotation = levelPieceScript.LeftBones[i].rotation;
			Vector3 pos = levelPieceScript.LeftBones[i].position;
			//handleTransform.TransformPoint(centerlineScript.ControlPoints[i]);
			// pos = Handles.DoPositionHandle(pos, handleRotation);
			// pos = Handles.PositionHandle(pos, handleRotation);
			pos = Handles.FreeMoveHandle(pos, handleRotation, handleSize, Vector3.one, Handles.DotHandleCap);
			// TODO: rotate forward dir towards next bone
			// TODO: rotate left dir towards bone on other side
			// IDEA: toggle option for disabling bone auto-rotation

			if (EditorGUI.EndChangeCheck()) {
				// FIXME: undo entry not registering
				Undo.RecordObject(segmentScript, "Segment script change");
				Undo.RecordObject(levelPieceScript, "Level piece change");
				EditorUtility.SetDirty(segmentScript);
				EditorUtility.SetDirty(levelPieceScript);
				levelPieceScript.LeftBones[i].position = pos;//handleTransform.InverseTransformPoint(pos);
			}
		}

	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		LevelPieceScript = serializedObject.FindProperty("LevelPieceScript");

		SegmentScript segmentScript = (SegmentScript)target;

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PropertyField(LevelPieceScript);

		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(segmentScript, "Segment Script Change");
			EditorUtility.SetDirty(segmentScript);
		}
		
		EditorGUI.BeginChangeCheck();

		handleSize = EditorGUILayout.Slider("Handle size", handleSize, 0, 10);
		if (EditorGUI.EndChangeCheck()) {
			// Repaint();
			SceneView.RepaintAll();
		}

		// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}


	void SnapSegments() {

		SegmentScript segmentScript = (SegmentScript)target;
		StraightLevelPieceScript middleSegment = segmentScript.LevelPieceScript;


		// float leftEndpointsDistance = Vector3.Distance(
		// 	startSegment.FrontLeftBone.position,
		// 	endSegment.RearRightBone.position
		// );

		// float rightEndpointsDistance = Vector3.Distance(
		// 	startSegment.FrontRightBone.position,
		// 	endSegment.RearLeftBone.position
		// );

		// Vector3 centerMidpoint = Vector3.Lerp(startSegment.FrontParent.position, endSegment.RearParent.position, .5f);


		// TODO: scale each bone length (only 1 axis) to match distance to next bone
		// TODO: evenly scale width and height of bone, interpolating between start- and end-point scale

		// Move root
		// middleSegment.transform.parent.parent.position = centerMidpoint;

		// middleSegment.FrontParent.position = endSegment.RearParent.position;
		// middleSegment.FrontParent.rotation = endSegment.RearParent.rotation;

		// middleSegment.RearParent.position = startSegment.FrontParent.position;
		// middleSegment.RearParent.rotation = startSegment.FrontParent.rotation;


		// float leftSpacing = leftEndpointsDistance / middleSegment.LeftBones.Count;
		// float rightSpacing = rightEndpointsDistance / middleSegment.RightBones.Count;

		// Vector3 midpointUp = Quaternion.Slerp(
		// 	Quaternion.Euler(startSegment.FrontParent.forward),
		// 	Quaternion.Euler(endSegment.RearParent.forward),
		// 	.5f
		// ).eulerAngles;

		// List<Vector3> leftPoints;
		// {
		// 	Vector3 leftStart = startSegment.FrontLeftBone.position;
		// 	Vector3 leftStartDir = leftStart - startSegment.FrontLeftBone.up.normalized * leftStartBezierMagnitude;
		// 	Vector3 leftEnd = endSegment.RearLeftBone.position;
		// 	Vector3 leftEndDir = leftEnd - endSegment.RearLeftBone.right.normalized * leftEndBezierMagnitude;
		// 	leftPoints = Bezier.CubicBezierRender(leftStart, leftStartDir, leftEndDir, leftEnd, middleSegment.LeftBones.Count);
		// }

		// List<Vector3> rightPoints;
		// {
		// 	Vector3 rightStart = startSegment.FrontRightBone.position;
		// 	Vector3 rightStartDir = rightStart - startSegment.FrontRightBone.up.normalized * rightStartBezierMagnitude;
		// 	Vector3 rightEnd = endSegment.RearRightBone.position;
		// 	Vector3 rightEndDir = rightEnd + endSegment.RearRightBone.right.normalized * rightEndBezierMagnitude;
		// 	rightPoints = Bezier.CubicBezierRender(rightStart, rightStartDir, rightEndDir, rightEnd, middleSegment.RightBones.Count);
		// }

		// IDEA: separate script/fn for updating bone length and rotation according to distance to next bone


		// if (leftPoints.Count != middleSegment.LeftBones.Count || rightPoints.Count != middleSegment.RightBones.Count) {
		// 	Debug.LogError("bone and bezier point count dont match");
		// 	return;
		// }

		// for (int i = 0; i < leftPoints.Count; i++) {
		// 	middleSegment.LeftBones[leftPoints.Count - i - 1].position = leftPoints[i];
		// }

		// for (int i = 0; i < rightPoints.Count; i++) {
		// 	middleSegment.RightBones[rightPoints.Count - i - 1].position = rightPoints[i];
		// }

		// if (middleSegment.LeftBones.Count == middleSegment.RightBones.Count) {
		// 	for (int i = 0; i < middleSegment.LeftBones.Count; i++) {

		// 		Transform nextLeftBone;
		// 		Transform nextRightBone;
		// 		if (i == middleSegment.LeftBones.Count - 1) {
		// 			nextLeftBone = middleSegment.RearLeftBone;
		// 			nextRightBone = middleSegment.RearRightBone;
		// 		} else {
		// 			nextLeftBone = middleSegment.LeftBones[i + 1];
		// 			nextRightBone = middleSegment.RightBones[i + 1];
		// 		}

		// 		var leftBone = middleSegment.LeftBones[i];
		// 		var rightBone = middleSegment.RightBones[i];

		// 		leftBone.LookAt(nextLeftBone, (leftBone.position - rightBone.position).normalized);
		// 		leftBone.Rotate(Vector3.right * 90, Space.Self);
		// 		leftBone.Rotate(Vector3.up * -90, Space.Self);

		// 		rightBone.LookAt(nextRightBone, (leftBone.position - rightBone.position).normalized);
		// 		rightBone.Rotate(Vector3.right * 90, Space.Self);
		// 		rightBone.Rotate(Vector3.up * -90, Space.Self);

		// 	}
		// }

	}

}
