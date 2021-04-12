using UnityEditor;
using UnityEngine;
using System.Collections;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
[CustomEditor(typeof(CenterlineScript))]
[CanEditMultipleObjects]
public class CenterlineEditorScript : Editor {

	SerializedProperty controlPoints;

	CenterlineScript centerlineScript;

	float controlHandleSize = .4f;
	float lineHandleSize = .1f;
	// int maxResolution = 100;
	// bool overrideResolutionMax = false;

	bool addPointOnDrag = false;
	float distanceThreshold = 10f;

	Color mainLineHandleColor = Color.gray;
	Color forkLineHandleColor = new Color(.8f, .8f, .7f, 1f);
	Color mainControlHandleColor = Color.blue;
	Color forkControlHandleColor = new Color(.3f, .2f, .6f, 1f);

	public bool UseAnchors = false;

	bool drawControlPointLines = true;

	CenterlineScript.InternalCenterline selectedLine = null;

	void OnEnable() {
		controlPoints = serializedObject.FindProperty("ControlPoints");
	}

	static void DrawLine(CenterlineScript.InternalCenterline line, Transform transform, int depth = 0) {
		if (depth > CenterlineScript.MAX_DEPTH) {
			Debug.LogError("Scene GUI line draw recursion too deep");
			return;
		}

		for (int i = 1; i < line.ControlPoints.Count; i++) {
			UnityEditor.Handles.DrawLine(
				transform.TransformPoint(line.ControlPoints[i - 1]),
				transform.TransformPoint(line.ControlPoints[i])
			);
		}

		foreach (var fork in line.Forks) {
			DrawLine(fork, transform, depth + 1);
		}
	}

	void DrawLineHandles(CenterlineScript.InternalCenterline line, int depth = 0) {
		if (depth > CenterlineScript.MAX_DEPTH) {
			Debug.LogError("Line handle draw recursion too deep");
			return;
		}

		for (int i = 1; i < line.LinePoints.Count - 1; i++) {
			EditorGUI.BeginChangeCheck();

			Transform handleTransform = centerlineScript.transform;
			Quaternion handleRotation = centerlineScript.transform.rotation;
			Vector3 pos = handleTransform.TransformPoint(line.LinePoints[i]);
			pos = Handles.FreeMoveHandle(pos, handleRotation, lineHandleSize, Vector3.one, Handles.DotHandleCap);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(centerlineScript, "Moved centerline control point");
				EditorUtility.SetDirty(centerlineScript);
				line.LinePoints[i] = handleTransform.InverseTransformPoint(pos);
			}
		}

		foreach (var fork in line.Forks) {
			DrawLineHandles(fork, depth + 1);
		}
	}

	void DrawControlHandles(CenterlineScript.InternalCenterline line, int depth = 0) {

		if (depth > CenterlineScript.MAX_DEPTH) {
			Debug.LogError("Control handle draw recursion too deep");
			return;
		}

		for (int i = 0; i < line.ControlPoints.Count; i++) {

			EditorGUI.BeginChangeCheck();

			Transform handleTransform = centerlineScript.transform;
			Quaternion handleRotation = centerlineScript.transform.rotation;
			Vector3 pos = handleTransform.TransformPoint(line.ControlPoints[i]);

			if (UseAnchors)
				pos = Handles.PositionHandle(pos, Quaternion.identity);
			else
				pos = Handles.FreeMoveHandle(pos, handleRotation, controlHandleSize, Vector3.one, Handles.DotHandleCap);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(centerlineScript, "Moved centerline control point");
				EditorUtility.SetDirty(centerlineScript);
				// IDEA: create new control point if start or end point is moved too far away from the next/previous point
				int count = line.ControlPoints.Count;
				if (addPointOnDrag && i > 0 && i == count - 1) {
					if ((line.ControlPoints[count - 1] - line.ControlPoints[count - 2]).sqrMagnitude > distanceThreshold * distanceThreshold) {
						line.ControlPoints.Add(line.ControlPoints[count - 1]);
						// FIXME: handle holds second last point after point is added
						// TODO: add when dragging start point
						line.ControlPoints[i + 1] = handleTransform.InverseTransformPoint(pos);
						centerlineScript.GenerateLinePoints();
						break;
					} else {
						line.ControlPoints[i] = handleTransform.InverseTransformPoint(pos);
					}

				} else {

					line.ControlPoints[i] = handleTransform.InverseTransformPoint(pos);
				}
				centerlineScript.GenerateLinePoints();
			}
		}

		foreach (var fork in line.Forks) {
			DrawControlHandles(fork, depth + 1);
		}
	}

	private void OnSceneGUI() {

		centerlineScript = (CenterlineScript)target;

		if (drawControlPointLines) {
			UnityEditor.Handles.color = Color.blue;
			DrawLine(centerlineScript.MainCenterline, centerlineScript.transform);
		}

		Handles.color = mainLineHandleColor;
		DrawLineHandles(centerlineScript.MainCenterline);

		Handles.color = mainControlHandleColor;
		DrawControlHandles(centerlineScript.MainCenterline);

	}


	void DrawLineInspector(CenterlineScript.InternalCenterline line, CenterlineScript.InternalCenterline parent, int depth = 1) {
		EditorGUILayout.BeginHorizontal();
		// EditorGUILayout.LabelField($"Fork depth {depth}");
		line.Name = EditorGUILayout.TextField("Name", line.Name);

		bool wasSelected = selectedLine != null && line == selectedLine;
		bool isSelected = EditorGUILayout.Toggle("selected", wasSelected);
		if (!wasSelected && isSelected) {
			selectedLine = line;
		}

		if (parent != null) {
			if (GUILayout.Button("Remove Fork")) {
				parent.Forks.Remove(line);
				return;
			}
		}

		EditorGUILayout.EndHorizontal();

		line.Resolution = EditorGUILayout.IntSlider("Fork Resolution/Line Count", line.Resolution, 2, 250);
		if (parent != null) {
			line.StartIndex = EditorGUILayout.IntField("Start index", line.StartIndex);
			if (line.StartIndex >= parent.LinePoints.Count) {
				line.StartIndex = parent.LinePoints.Count - 1;
			} else if (line.StartIndex < 0) {
				line.StartIndex = 0;
			}
		}

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField($"Rejoin line: {(line.RejoinLine != null ? line.RejoinLine.Name : "none")}");
		if (GUILayout.Button("Join selected line") && selectedLine != null) {
			line.RejoinLine = selectedLine;
		}
		EditorGUILayout.EndHorizontal();
		line.RejoinIndex = EditorGUILayout.IntField("Rejoin index", line.RejoinIndex);
		if (line.RejoinIndex < 0) line.RejoinIndex = 0;

		for (int i = line.ControlPoints.Count - 1; i >= 0; i--) {
			// for (int i = 0; i < line.ControlPoints.Count; i++) {
			EditorGUILayout.BeginHorizontal();
			line.ControlPoints[i] = EditorGUILayout.Vector3Field($"p{i + 1}", line.ControlPoints[i]);
			if (GUILayout.Button("Remove", GUILayout.Width(70))) {
				line.ControlPoints.RemoveAt(i);
				EditorGUILayout.EndHorizontal();
				return;
			}
			EditorGUILayout.EndHorizontal();
		}
		if (GUILayout.Button("Add Fork Control Point")) {
			if (line.ControlPoints.Count > 0) {
				line.ControlPoints.Add(line.ControlPoints[line.ControlPoints.Count - 1]);
			} else {
				line.ControlPoints.Add(Vector3.zero);
			}
		}

		line.ForksInspectorFoldState = EditorGUILayout.Foldout(line.ForksInspectorFoldState, "Forks");
		if (line.ForksInspectorFoldState) {
			EditorGUI.indentLevel += 1;
			// foreach (var fork in line.Forks) {
			for (int i = line.Forks.Count - 1; i >= 0; i--) {
				DrawLineInspector(line.Forks[i], line, depth + 1);
				// if(line.Forks[i] == null) line.Forks.RemoveAt(i);
			}
			EditorGUI.indentLevel -= 1;

			if (GUILayout.Button($"Add Fork (depth {depth})")) {
				line.Forks.Add(new CenterlineScript.InternalCenterline());
			}
		}
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		CenterlineScript centerlineScript = (CenterlineScript)target;

		EditorGUIUtility.labelWidth = 30;


		EditorGUIUtility.labelWidth = 200;
		UseAnchors = EditorGUILayout.Toggle("Use Anchors", UseAnchors);
		EditorGUILayout.LabelField($"Selected line: {(selectedLine != null ? selectedLine.Name : "none")}");

		EditorGUI.BeginChangeCheck();

		DrawLineInspector(centerlineScript.MainCenterline, null);

		if (EditorGUI.EndChangeCheck()) {
			centerlineScript.GenerateLinePoints();
			Undo.RecordObject(centerlineScript, "Changed centerline through inspector");
			EditorUtility.SetDirty(centerlineScript);
		}

		EditorGUI.BeginChangeCheck();

		controlHandleSize = EditorGUILayout.Slider("Control Point Handle Size", controlHandleSize, 0.01f, 5f);
		lineHandleSize = EditorGUILayout.Slider("Line Point Handle Size", lineHandleSize, 0.01f, 5f);

		mainLineHandleColor = EditorGUILayout.ColorField("Main Line Handle Color", mainLineHandleColor);
		forkLineHandleColor = EditorGUILayout.ColorField("Fork Line Handle Color", forkLineHandleColor);
		mainControlHandleColor = EditorGUILayout.ColorField("Main Control Point Handle Color", mainControlHandleColor);
		forkControlHandleColor = EditorGUILayout.ColorField("Fork Control Point Handle Color", forkControlHandleColor);
		drawControlPointLines = EditorGUILayout.Toggle("Draw Control Point Lines", drawControlPointLines);

		if (EditorGUI.EndChangeCheck())
			SceneView.RepaintAll();

		addPointOnDrag = EditorGUILayout.Toggle("Add new point by dragging", addPointOnDrag);
		if (addPointOnDrag)
			distanceThreshold = EditorGUILayout.FloatField("Drag distance threshold", distanceThreshold);

		serializedObject.ApplyModifiedProperties();
	}

}
