using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Hides and unhides objects in list to only show one at a time, decided on string key supplied
public class ObjectSelectorScript : MonoBehaviour {

	public string DefaultObject;

	[Serializable]
	public class ObjectIndex {
		public string Key;
		public GameObject Value;
	}

	public List<ObjectIndex> objects;

	[HideInInspector]
	public ObjectIndex ShownObject = null;

	private void Awake() {
		foreach (var item in objects)
			item.Value.SetActive(false);

		UnhideObject(DefaultObject);
	}

	public void UnhideObject(string key) {
		if (ShownObject != null && ShownObject.Value != null)
			ShownObject.Value.SetActive(false);

		if (key == "" || key == "None")
			return;

		foreach (var gameObject in objects) {
			if (gameObject.Key == key) {
				ShownObject = gameObject;
				ShownObject.Value.SetActive(true);
				return;
			}
		}
	}

	public void UnhideObject() {
		UnhideObject("");
		ShownObject = null;
	}

}
