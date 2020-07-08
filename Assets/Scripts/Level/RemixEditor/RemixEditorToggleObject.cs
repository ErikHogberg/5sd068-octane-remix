using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RemixEditorToggleObject : MonoBehaviour {
	public static List<RemixEditorToggleObject> Instances = new List<RemixEditorToggleObject>();

	public GameObject ObjectToToggle;
	[Space]
	[Tooltip("Name for the object that will be shown in the remix editor list")]
	public string Name;
	public bool ToggleState => ObjectToToggle.activeSelf;

	private void Awake() {
		Instances.Add(this);
	}

	private void OnDestroy() {
		Instances.Remove(this);
	}

	private void OnMouseOver() {
		if (EventSystem.current.IsPointerOverGameObject())
			return;

		if (Input.GetMouseButtonDown(0)) {
			// TODO: remix editor list for toggled objects
			// RemixMapScript.SelectSegment(this);
		}

		if (Input.GetMouseButtonDown(1)) {
			RemixMapScript.StartRotate();
		}
	}
}
