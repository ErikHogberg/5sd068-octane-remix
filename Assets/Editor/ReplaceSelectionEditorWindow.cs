using UnityEngine;
using UnityEditor;

public class ReplaceSelectionEditorWindow : EditorWindow {

	GameObject target;

	[MenuItem("Window/Replace Selection")]
	private static void ShowWindow() {
		var window = GetWindow<ReplaceSelectionEditorWindow>();
		window.titleContent = new GUIContent("Replace Selection");
		window.Show();
	}

	private void OnGUI() {
		target = (GameObject)EditorGUILayout.ObjectField(target, typeof(GameObject), true);
		
		GUILayout.Space(16);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Replace selection", GUILayout.Width(140), GUILayout.Height(40))) {
			// FIXME: doesnt preserve prefab link
			// ReplaceSelection();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	private void ReplaceSelection() {
		if (!target)
			return;

		var selection = Selection.GetTransforms(
			SelectionMode.TopLevel | SelectionMode.Editable
		);


		// Undo.RecordObjects(selection, "deleted selection");
		foreach (var item in selection) {
			var newItem = 
			// FIXME: prefabs not instantiated by prefab utility
			// (GameObject)PrefabUtility.InstantiatePrefab(target);
			Instantiate(target, item.position, item.rotation, item.parent);
			newItem.transform.localScale = item.localScale;

			Undo.RegisterCreatedObjectUndo(newItem, "created replacement object");
			Undo.DestroyObjectImmediate(item.gameObject);
		}

	}
}
