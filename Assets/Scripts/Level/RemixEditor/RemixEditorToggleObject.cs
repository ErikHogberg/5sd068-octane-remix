using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class RemixEditorToggleObject : MonoBehaviour, IComparable<RemixEditorToggleObject>, IPointerDownHandler {
	public static List<RemixEditorToggleObject> Instances = new List<RemixEditorToggleObject>();

	[Tooltip("Order of the object in the remix editor list")]
	public int RemixEditorOrder;
	[Space]

	public GameObject ObjectToToggle;
	[Space]
	[Tooltip("Name for the object that will be shown in the remix editor list")]
	public string Name;
	public bool ToggleState => ObjectToToggle.activeSelf;

	private void Awake() {
		Instances.Add(this);
		Instances.Sort();
	}

	private void OnDestroy() {
		Instances.Remove(this);
	}

	public void OnPointerDown(PointerEventData eventData) {

		if (Mouse.current.leftButton.wasPressedThisFrame) {
			RemixMapScript.Select(this);
		}
	}

	public int CompareTo(RemixEditorToggleObject other) {
		return RemixEditorOrder - other.RemixEditorOrder;
	}
}
