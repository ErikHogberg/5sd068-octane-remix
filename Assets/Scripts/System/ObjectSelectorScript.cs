﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Hides and unhides objects in list to only show one at a time, decided on string key supplied
public class ObjectSelectorScript : MonoBehaviour {

	public string DefaultObject;
	public bool UseEnabledAsDefault = false;

	[Serializable]
	public class ObjectEntry {
		public string Key;
		public GameObject Value;
	}

	public List<ObjectEntry> objects;

	[HideInInspector]
	public ObjectEntry ShownObject = null;
	public int ShownIndex {
		get {
			if (ShownObject == null)
				return -1;

			for (int i = 0; i < objects.Count; i++) {
				if (objects[i] == ShownObject) {
					return i;
				}
			}

			return -1; // should be error
		}
	}

	private void Awake() {
		foreach (var item in objects) {
			if (UseEnabledAsDefault && (item?.Value.activeInHierarchy ?? false))
				DefaultObject = item.Key;

			item.Value.SetActive(false);
		}

		UnhideObject(DefaultObject);
	}

	public void UnhideObject(int index) {
		if (ShownObject != null && ShownObject.Value != null)
			ShownObject.Value.SetActive(false);

		if (index < objects.Count) {
			ShownObject = objects[index];
			ShownObject.Value.SetActive(true);
		}
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
