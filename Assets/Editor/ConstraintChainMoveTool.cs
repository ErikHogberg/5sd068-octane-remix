
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("Constraint Chain Move Tool", typeof(ConstraintToolComponent))]
public class ConstraintChainMoveTool : EditorTool {
	// Serialize this value to set a default value in the Inspector.
	[SerializeField]
	Texture2D m_ToolIcon;

	GUIContent m_IconContent;

	void OnEnable() {
		m_IconContent = new GUIContent() {
			image = m_ToolIcon,
			text = "Constrained Move Tool",
			tooltip = "Constrained Move Tool"
		};
	}

	public override GUIContent toolbarIcon {
		get { return m_IconContent; }
	}

	// This is called for each window that your tool is active in. Put the functionality of your tool here.
	public override void OnToolGUI(EditorWindow window) {
		EditorGUI.BeginChangeCheck();

		Vector3 position = Tools.handlePosition;

		using (new Handles.DrawingScope(Color.green)) {
			position = Handles.Slider(position, Vector3.right);
			position = Handles.Slider(position, Vector3.up);
			position = Handles.Slider(position, Vector3.forward);
		}

		if (EditorGUI.EndChangeCheck()) {
			Vector3 delta = position - Tools.handlePosition;

			Undo.RecordObjects(Selection.transforms, "Move Platform");

			// IDEA: restrict to move one selection at a time to simplify tool?
			foreach (var transform in Selection.transforms){
				transform.position += delta;

				var constraintComponent = transform.GetComponent<ConstraintToolComponent>();
				if (constraintComponent) {
					constraintComponent.EnforceConstraints();
				}
			}
		}
	}
}
