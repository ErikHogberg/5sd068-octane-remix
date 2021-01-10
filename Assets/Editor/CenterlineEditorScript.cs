using UnityEditor;
using UnityEngine;
using System.Collections;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
[CustomEditor(typeof(CenterlineScript))]
[CanEditMultipleObjects]
public class CenterlineEditorScript : Editor {
	SerializedProperty controlPoints;

	float handleSize = .04f;
	// int maxResolution = 100;
	bool overrideResolutionMax = false;

	bool addPointOnDrag = false;
	float distanceThreshold = 10f;

	Color mainLineHandleColor = Color.gray;
	Color forkLineHandleColor = new Color(.7f,.7f,.7f,1f);
	Color mainControlHandleColor = Color.blue;
	Color forkControlHandleColor = new Color(.2f,.2f,.7f,1f);

	void OnEnable() {
		controlPoints = serializedObject.FindProperty("ControlPoints");
	}

	private void OnSceneGUI() {

		CenterlineScript centerlineScript = (CenterlineScript)target;

		Handles.color = mainLineHandleColor;

		for (int i = 1; i < centerlineScript.LinePoints.Count - 1; i++) {
			EditorGUI.BeginChangeCheck();

			Transform handleTransform = centerlineScript.transform;
			Quaternion handleRotation = centerlineScript.transform.rotation;
			Vector3 pos = handleTransform.TransformPoint(centerlineScript.LinePoints[i]);
			pos = Handles.FreeMoveHandle(pos, handleRotation, handleSize, Vector3.one, Handles.DotHandleCap);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(centerlineScript, "Moved centerline control point");
				EditorUtility.SetDirty(centerlineScript);
				centerlineScript.LinePoints[i] = handleTransform.InverseTransformPoint(pos);
			}
		}

		Handles.color = forkLineHandleColor;

		for (int forkIndex = 0; forkIndex < centerlineScript.Forks.Count; forkIndex++) {
			for (int i = 1; i < centerlineScript.Forks[forkIndex].LinePoints.Count - 1; i++) {
				EditorGUI.BeginChangeCheck();

				Transform handleTransform = centerlineScript.transform;
				Quaternion handleRotation = centerlineScript.transform.rotation;
				Vector3 pos = handleTransform.TransformPoint(centerlineScript.Forks[forkIndex].LinePoints[i]);
				pos = Handles.FreeMoveHandle(pos, handleRotation, handleSize, Vector3.one, Handles.DotHandleCap);

				if (EditorGUI.EndChangeCheck()) {
					Undo.RecordObject(centerlineScript, "Moved centerline control point");
					EditorUtility.SetDirty(centerlineScript);
					centerlineScript.Forks[forkIndex].LinePoints[i] = handleTransform.InverseTransformPoint(pos);
				}
			}
		}

		Handles.color = mainControlHandleColor;

		for (int i = 0; i < centerlineScript.ControlPoints.Count; i++) {

			EditorGUI.BeginChangeCheck();

			Transform handleTransform = centerlineScript.transform;
			Quaternion handleRotation = centerlineScript.transform.rotation;
			Vector3 pos = handleTransform.TransformPoint(centerlineScript.ControlPoints[i]);
			pos = Handles.FreeMoveHandle(pos, handleRotation, handleSize, Vector3.one, Handles.DotHandleCap);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(centerlineScript, "Moved centerline control point");
				EditorUtility.SetDirty(centerlineScript);
				// IDEA: create new control point if start or end point is moved too far away from the next/previous point
				int count = centerlineScript.ControlPoints.Count;
				if (addPointOnDrag && i > 0 && i == count - 1) {
					if ((centerlineScript.ControlPoints[count - 1] - centerlineScript.ControlPoints[count - 2]).sqrMagnitude > distanceThreshold * distanceThreshold) {
						centerlineScript.ControlPoints.Add(centerlineScript.ControlPoints[count - 1]);
						// FIXME: handle holds second last point after point is added
						// TODO: add when dragging start point
						centerlineScript.ControlPoints[i + 1] = handleTransform.InverseTransformPoint(pos);
						centerlineScript.GenerateLinePoints();
						break;
					} else {
						centerlineScript.ControlPoints[i] = handleTransform.InverseTransformPoint(pos);
					}

				} else {

					centerlineScript.ControlPoints[i] = handleTransform.InverseTransformPoint(pos);
				}
				centerlineScript.GenerateLinePoints();
			}
		}

		Handles.color = forkControlHandleColor;

		for (int forkIndex = 0; forkIndex < centerlineScript.Forks.Count; forkIndex++) {
			for (int i = 0; i < centerlineScript.Forks[forkIndex].ControlPoints.Count; i++) {
				EditorGUI.BeginChangeCheck();

				Transform handleTransform = centerlineScript.transform;
				Quaternion handleRotation = centerlineScript.transform.rotation;
				Vector3 pos = handleTransform.TransformPoint(centerlineScript.Forks[forkIndex].ControlPoints[i]);
				pos = Handles.FreeMoveHandle(pos, handleRotation, handleSize, Vector3.one, Handles.DotHandleCap);

				if (EditorGUI.EndChangeCheck()) {
					Undo.RecordObject(centerlineScript, "Moved centerline control point");
					EditorUtility.SetDirty(centerlineScript);
					// IDEA: create new control point if start or end point is moved too far away from the next/previous point
					int count = centerlineScript.Forks[forkIndex].ControlPoints.Count;
					if (addPointOnDrag && i > 0 && i == count - 1) {
						if ((centerlineScript.Forks[forkIndex].ControlPoints[count - 1] - centerlineScript.Forks[forkIndex].ControlPoints[count - 2]).sqrMagnitude > distanceThreshold * distanceThreshold) {
							centerlineScript.Forks[forkIndex].ControlPoints.Add(centerlineScript.Forks[forkIndex].ControlPoints[count - 1]);
							// FIXME: handle holds second last point after point is added
							// TODO: add when dragging start point
							centerlineScript.Forks[forkIndex].ControlPoints[i + 1] = handleTransform.InverseTransformPoint(pos);
							centerlineScript.GenerateLinePoints();
							break;
						} else {
							centerlineScript.Forks[forkIndex].ControlPoints[i] = handleTransform.InverseTransformPoint(pos);
						}

					} else {

						centerlineScript.Forks[forkIndex].ControlPoints[i] = handleTransform.InverseTransformPoint(pos);
					}
					centerlineScript.GenerateLinePoints();
				}
			}
		}




	}



	public override void OnInspectorGUI() {
		serializedObject.Update();

		CenterlineScript centerlineScript = (CenterlineScript)target;

		EditorGUIUtility.labelWidth = 30;

		EditorGUI.BeginChangeCheck();

		for (int i = 0; i < centerlineScript.ControlPoints.Count; i++) {
			EditorGUILayout.BeginHorizontal();
			centerlineScript.ControlPoints[i] = EditorGUILayout.Vector3Field($"p{i}", centerlineScript.ControlPoints[i]);
			if (GUILayout.Button("Remove", GUILayout.Width(70))) {
				centerlineScript.ControlPoints.RemoveAt(i);
			}
			EditorGUILayout.EndHorizontal();
		}
		if (GUILayout.Button("Add Control Point")) {
			if (centerlineScript.ControlPoints.Count > 0) {
				centerlineScript.ControlPoints.Add(centerlineScript.ControlPoints[centerlineScript.ControlPoints.Count - 1]);
			} else {
				centerlineScript.ControlPoints.Add(Vector3.zero);
			}
		}

		EditorGUIUtility.labelWidth = 150;

		// EditorGUILayout.BeginHorizontal();
		if (overrideResolutionMax) {
			centerlineScript.Resolution = EditorGUILayout.IntField("Resolution/Line Count", centerlineScript.Resolution);
		} else {
			centerlineScript.Resolution = EditorGUILayout.IntSlider("Resolution/Line Count", centerlineScript.Resolution, 0, 250);
		}
		// FIXME: deselecting and selecting again will set the resolution back to below/at limit if it was above limit
		// overrideResolutionMax = EditorGUILayout.Toggle("Override max resolution", overrideResolutionMax);
		// EditorGUILayout.EndHorizontal();

		EditorGUI.indentLevel += 1;
		for (int forkIndex = 0; forkIndex < centerlineScript.Forks.Count; forkIndex++) {
			centerlineScript.Forks[forkIndex].Resolution = EditorGUILayout.IntSlider("Fork Resolution/Line Count", centerlineScript.Forks[forkIndex].Resolution, 0, 250);
			centerlineScript.Forks[forkIndex].StartIndex = EditorGUILayout.IntField("Start index", centerlineScript.Forks[forkIndex].StartIndex);
			if (centerlineScript.Forks[forkIndex].StartIndex >= centerlineScript.LinePoints.Count) {
				centerlineScript.Forks[forkIndex].StartIndex = centerlineScript.ControlPoints.Count - 1;
			} else if (centerlineScript.Forks[forkIndex].StartIndex < 0) {
				centerlineScript.Forks[forkIndex].StartIndex = 0;
			}

			for (int i = 0; i < centerlineScript.Forks[forkIndex].ControlPoints.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				centerlineScript.Forks[forkIndex].ControlPoints[i] = EditorGUILayout.Vector3Field($"p{i + 1}", centerlineScript.Forks[forkIndex].ControlPoints[i]);
				if (GUILayout.Button("Remove", GUILayout.Width(70))) {
					centerlineScript.Forks[forkIndex].ControlPoints.RemoveAt(i);
				}
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("Add Fork Control Point")) {
				if (centerlineScript.Forks[forkIndex].ControlPoints.Count > 0) {
					centerlineScript.Forks[forkIndex].ControlPoints.Add(centerlineScript.Forks[forkIndex].ControlPoints[centerlineScript.Forks[forkIndex].ControlPoints.Count - 1]);
				} else {
					centerlineScript.Forks[forkIndex].ControlPoints.Add(Vector3.zero);
				}
			}
		}

		if (GUILayout.Button("Add Fork")) {
			centerlineScript.Forks.Add(new CenterlineScript.InternalCenterline());
		}
		EditorGUI.indentLevel -= 1;


		if (EditorGUI.EndChangeCheck()) {
			centerlineScript.GenerateLinePoints();
			Undo.RecordObject(centerlineScript, "Changed centerline control points");
			EditorUtility.SetDirty(centerlineScript);
		}

		float oldHandleSize = handleSize;
		handleSize = EditorGUILayout.Slider("Handle Size", handleSize, 0.01f, 1f);

		if (oldHandleSize != handleSize) {
			SceneView.RepaintAll();
		}

		addPointOnDrag = EditorGUILayout.Toggle("Add new point by dragging", addPointOnDrag);
		distanceThreshold = EditorGUILayout.FloatField("Drag distance threshold", distanceThreshold);

		serializedObject.ApplyModifiedProperties();
	}

}
