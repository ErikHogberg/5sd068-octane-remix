using UnityEngine;
using UnityEditor;
using System.Collections;

class SetWorldEditorWindow : EditorWindow {

	Transform target;

	bool rotate = true;
	Vector3 rotationOffset = Vector3.zero;


	[MenuItem("Window/Set world position to other")]
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(SetWorldEditorWindow));
	}

	void OnGUI() {
		GUILayout.Label("Move", EditorStyles.boldLabel);
		target = (Transform)EditorGUILayout.ObjectField(target, typeof(Transform), true);

		rotate = EditorGUILayout.Toggle("Also rotate?", rotate);
		rotationOffset = EditorGUILayout.Vector3Field("Rotation offset", rotationOffset);

		if (GUILayout.Button("Move to selection")) {
			MoveTransformToSelected();
		}

	}

	void MoveTransformToSelected() {
		var selected = Selection.activeGameObject;

		if (!selected) {
			Debug.LogWarning("No object to move selected in scene");
			return;
		}

		if (!target) {
			Debug.LogWarning("No target object assigned in window");
			return;
		}


		Undo.RecordObject(selected.transform, "Moved object using custom tool");
		// Undo.RegisterCompleteObjectUndo(selected.transform, "Moved object using custom tool");
		selected.transform.position = target.position;
		if (rotate)
			selected.transform.rotation = target.rotation * Quaternion.Euler(rotationOffset);

	}
}
