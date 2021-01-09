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

	void OnEnable() {
		controlPoints = serializedObject.FindProperty("ControlPoints");
	}

	private void OnSceneGUI() {

		CenterlineScript centerlineScript = (CenterlineScript)target;


		Handles.color = Color.blue;

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

		Handles.color = Color.gray;

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
		if (GUILayout.Button("Add")) {
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
