using UnityEngine;
using UnityEditor;
using System.Collections;

class DistributeObjects : EditorWindow {

	Transform startTransform;
	Transform endTransform;


	// bool rotate = false;

	[MenuItem("Window/Distribute Objects")]
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(DistributeObjects));
	}

	void OnGUI() {
		GUILayout.Label("Even distribution tool!");
		GUILayout.Label("Select 1+ objects");
		// GUILayout.Label("Last 2 objects decide start and end.");

		GUILayout.BeginHorizontal();
		GUILayout.Label("Start:");
		startTransform = (Transform)EditorGUILayout.ObjectField(startTransform, typeof(Transform), true);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("End:");
		endTransform = (Transform)EditorGUILayout.ObjectField(endTransform, typeof(Transform), true);
		GUILayout.EndHorizontal();

		// GUILayout.Space(8);
		// rotate = EditorGUILayout.Toggle("Also rotate?", rotate);
		GUILayout.Space(16);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Distribute!", GUILayout.Width(165), GUILayout.Height(32))) {
			DistributeSelection();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

	}

	void DistributeSelection() {
		var selection = Selection.GetTransforms(
			SelectionMode.TopLevel | SelectionMode.Editable
		);

		if (selection.Length < 1) {
			Debug.LogWarning("Too few objects");
			return;
		}


		// TODO: assign start and end to last 2 selected instead of name order
		// Transform startTransform = selection[selection.Length - 2];
		// Transform endTransform = selection[selection.Length - 1];

		for (
			int i = 0; 
			i < selection.Length;// - 2; 
			i++) {
			Transform currentTransform = selection[i];
			Undo.RecordObject(currentTransform, "Moved object using custom tool");
			currentTransform.position = Vector3.Lerp(startTransform.position, endTransform.position, (1f / (selection.Length + 1)) * (i + 1));
			// TODO: rotate
		}

		Debug.Log("distributed " + selection.Length + " objects between " + startTransform.name + " and " + endTransform.name);

	}
}
