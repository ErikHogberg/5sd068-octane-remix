using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMaskScript : MonoBehaviour {

	private static CameraMaskScript mainInstance;
	// IDEA: camera list for splitscreen, etc.

	[Serializable]
	public struct CameraMaskTableRow {
		public string ID;
		public LayerMask CameraMask;
	}

	public List<CameraMaskTableRow> CameraMaskTable;

	private LayerMask defaultMask;

	private Camera attachedCamera;

	private void Awake() {
		mainInstance = this;

		attachedCamera = GetComponent<Camera>();
		defaultMask = attachedCamera.cullingMask;
		// IDEA: set event mask too?
	}

	private void OnDestroy() {
		mainInstance = null;
	}

	public bool SetMask(string key) {
		foreach (var maskRow in CameraMaskTable) {
			if (maskRow.ID == key) {
				attachedCamera.cullingMask = maskRow.CameraMask;
				return true;
			}
		}

		return false;
	}

	public static bool SetMaskStatic(string key) {
		if (mainInstance == null)
			return false;

		return mainInstance.SetMask(key);
	}

}
