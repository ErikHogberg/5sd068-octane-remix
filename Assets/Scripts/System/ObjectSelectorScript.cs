using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Hides and unhides objects in list to only show one at a time, decided on string key supplied
public class ObjectSelectorScript : MonoBehaviour {

	[Serializable]
	public struct ObjectIndex {
		public string Key;
		public GameObject Value;
	}

	public List<ObjectIndex> objects;

	public GameObject ShownObject { get; private set; }

	private void Awake() {
		foreach (var item in objects) {
			item.Value.SetActive(false);
		}
	}

	public void UnhideObject(string key) {
		if (ShownObject)
			ShownObject.SetActive(false);

		if (key == "")
			return;

		foreach (var gameObject in objects) {
			if (gameObject.Key == key) {
				ShownObject = gameObject.Value;
				ShownObject.SetActive(true);
				return;
			}
		}
	}

}
