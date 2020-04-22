using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMaskScript : MonoBehaviour {

	public static CameraMaskScript MainInstance;
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
        attachedCamera = GetComponent<Camera>();
        defaultMask = attachedCamera.cullingMask;
        // IDEA: set event mask too?
    }

	public bool SetMask(string key) {
		foreach (var maskRow in CameraMaskTable) {
			if (maskRow.ID == key) {
                
                return true;
			}
		}

		return false;
	}


}
