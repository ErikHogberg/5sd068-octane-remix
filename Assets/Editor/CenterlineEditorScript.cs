using UnityEditor;
using UnityEngine;
using System.Collections;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
[CustomEditor(typeof(CenterlineScript))]
[CanEditMultipleObjects]
public class CenterlineEditorScript : Editor {
	SerializedProperty controlPoints;

	float controlHandleSize = .4f;
	float lineHandleSize = .1f;
	// int maxResolution = 100;
	bool overrideResolutionMax = false;

	bool addPointOnDrag = false;
	float distanceThreshold = 10f;

	Color mainLineHandleColor = Color.gray;
	Color forkLineHandleColor = new Color(.8f, .8f, .7f, 1f);
	Color mainControlHandleColor = Color.blue;
	Color forkControlHandleColor = new Color(.3f, .2f, .6f, 1f);

	bool showPoints = true;
	bool showForks = false;

	void OnEnable() {
		controlPoints = serializedObject.FindProperty("ControlPoints");
	}

	private void OnSceneGUI() {

		CenterlineScript centerlineScript = (CenterlineScript)target;

		Handles.color = mainLineHandleColor;

		for (int i = 1; i < centerlineScript.MainCenterline.LinePoints.Count - 1; i++) {
			EditorGUI.BeginChangeCheck();

			Transform handleTransform = centerlineScript.transform;
			Quaternion handleRotation = centerlineScript.transform.rotation;
			Vector3 pos = handleTransform.TransformPoint(centerlineScript.MainCenterline.LinePoints[i]);
			pos = Handles.FreeMoveHandle(pos, handleRotation, lineHandleSize, Vector3.one, Handles.DotHandleCap);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(centerlineScript, "Moved centerline control point");
				EditorUtility.SetDirty(centerlineScript);
				centerlineScript.MainCenterline.LinePoints[i] = handleTransform.InverseTransformPoint(pos);
			}
		}

		Handles.color = forkLineHandleColor;

		for (int forkIndex = 0; forkIndex < centerlineScript.Forks.Count; forkIndex++) {
			for (int i = 1; i < centerlineScript.Forks[forkIndex].LinePoints.Count - 1; i++) {
				EditorGUI.BeginChangeCheck();

				Transform handleTransform = centerlineScript.transform;
				Quaternion handleRotation = centerlineScript.transform.rotation;
				Vector3 pos = handleTransform.TransformPoint(centerlineScript.Forks[forkIndex].LinePoints[i]);
				pos = Handles.FreeMoveHandle(pos, handleRotation, lineHandleSize, Vector3.one, Handles.DotHandleCap);

				if (EditorGUI.EndChangeCheck()) {
					Undo.RecordObject(centerlineScript, "Moved centerline control point");
					EditorUtility.SetDirty(centerlineScript);
					centerlineScript.Forks[forkIndex].LinePoints[i] = handleTransform.InverseTransformPoint(pos);
				}
			}
		}

		Handles.color = mainControlHandleColor;

		for (int i = 0; i < centerlineScript.MainCenterline.ControlPoints.Count; i++) {

			EditorGUI.BeginChangeCheck();

			Transform handleTransform = centerlineScript.transform;
			Quaternion handleRotation = centerlineScript.transform.rotation;
			Vector3 pos = handleTransform.TransformPoint(centerlineScript.MainCenterline.ControlPoints[i]);
			pos = Handles.FreeMoveHandle(pos, handleRotation, controlHandleSize, Vector3.one, Handles.DotHandleCap);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(centerlineScript, "Moved centerline control point");
				EditorUtility.SetDirty(centerlineScript);
				// IDEA: create new control point if start or end point is moved too far away from the next/previous point
				int count = centerlineScript.MainCenterline.ControlPoints.Count;
				if (addPointOnDrag && i > 0 && i == count - 1) {
					if ((centerlineScript.MainCenterline.ControlPoints[count - 1] - centerlineScript.MainCenterline.ControlPoints[count - 2]).sqrMagnitude > distanceThreshold * distanceThreshold) {
						centerlineScript.MainCenterline.ControlPoints.Add(centerlineScript.MainCenterline.ControlPoints[count - 1]);
						// FIXME: handle holds second last point after point is added
						// TODO: add when dragging start point
						centerlineScript.MainCenterline.ControlPoints[i + 1] = handleTransform.InverseTransformPoint(pos);
						centerlineScript.GenerateLinePoints();
						break;
					} else {
						centerlineScript.MainCenterline.ControlPoints[i] = handleTransform.InverseTransformPoint(pos);
					}

				} else {

					centerlineScript.MainCenterline.ControlPoints[i] = handleTransform.InverseTransformPoint(pos);
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
				pos = Handles.FreeMoveHandle(pos, handleRotation, controlHandleSize, Vector3.one, Handles.DotHandleCap);

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

		EditorGUIUtility.labelWidth = 200;

		// EditorGUILayout.BeginHorizontal();
		if (overrideResolutionMax) {
			centerlineScript.MainCenterline.Resolution = EditorGUILayout.IntField("Resolution/Line Count", centerlineScript.MainCenterline.Resolution);
		} else {
			centerlineScript.MainCenterline.Resolution = EditorGUILayout.IntSlider("Resolution/Line Count", centerlineScript.MainCenterline.Resolution, 0, 250);
		}
		// FIXME: deselecting and selecting again will set the resolution back to below/at limit if it was above limit
		// overrideResolutionMax = EditorGUILayout.Toggle("Override max resolution", overrideResolutionMax);
		// EditorGUILayout.EndHorizontal();

		showPoints = EditorGUILayout.Foldout(showPoints, "Control Points");
		if (showPoints) {
			for (int i = 0; i < centerlineScript.MainCenterline.ControlPoints.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				centerlineScript.MainCenterline.ControlPoints[i] = EditorGUILayout.Vector3Field($"p{i}", centerlineScript.MainCenterline.ControlPoints[i]);
				if (GUILayout.Button("Remove", GUILayout.Width(70))) {
					centerlineScript.MainCenterline.ControlPoints.RemoveAt(i);
				}
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("Add Control Point")) {
				if (centerlineScript.MainCenterline.ControlPoints.Count > 0) {
					centerlineScript.MainCenterline.ControlPoints.Add(centerlineScript.MainCenterline.ControlPoints[centerlineScript.MainCenterline.ControlPoints.Count - 1]);
				} else {
					centerlineScript.MainCenterline.ControlPoints.Add(Vector3.zero);
				}
			}
		}

		showForks = EditorGUILayout.Foldout(showForks, "Forks");
		if (showForks) {
			EditorGUI.indentLevel += 1;
			for (int forkIndex = 0; forkIndex < centerlineScript.Forks.Count; forkIndex++) {
				centerlineScript.Forks[forkIndex].Resolution = EditorGUILayout.IntSlider("Fork Resolution/Line Count", centerlineScript.Forks[forkIndex].Resolution, 0, 250);
				centerlineScript.Forks[forkIndex].StartIndex = EditorGUILayout.IntField("Start index", centerlineScript.Forks[forkIndex].StartIndex);
				if (centerlineScript.Forks[forkIndex].StartIndex >= centerlineScript.MainCenterline.LinePoints.Count) {
					centerlineScript.Forks[forkIndex].StartIndex = centerlineScript.MainCenterline.ControlPoints.Count - 1;
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
		}


		if (EditorGUI.EndChangeCheck()) {
			centerlineScript.GenerateLinePoints();
			Undo.RecordObject(centerlineScript, "Changed centerline control points");
			EditorUtility.SetDirty(centerlineScript);
		}

		EditorGUI.BeginChangeCheck();

		controlHandleSize = EditorGUILayout.Slider("Control Point Handle Size", controlHandleSize, 0.01f, 1f);
		lineHandleSize = EditorGUILayout.Slider("Line Point Handle Size", lineHandleSize, 0.01f, 1f);

		mainLineHandleColor = EditorGUILayout.ColorField("Main Line Handle Color", mainLineHandleColor);
		forkLineHandleColor = EditorGUILayout.ColorField("Fork Line Handle Color", forkLineHandleColor);
		mainControlHandleColor = EditorGUILayout.ColorField("Main Control Point Handle Color", mainControlHandleColor);
		forkControlHandleColor = EditorGUILayout.ColorField("Fork Control Point Handle Color", forkControlHandleColor);

		if (EditorGUI.EndChangeCheck())
			SceneView.RepaintAll();

		addPointOnDrag = EditorGUILayout.Toggle("Add new point by dragging", addPointOnDrag);
		if (addPointOnDrag)
			distanceThreshold = EditorGUILayout.FloatField("Drag distance threshold", distanceThreshold);

		serializedObject.ApplyModifiedProperties();
	}

}
