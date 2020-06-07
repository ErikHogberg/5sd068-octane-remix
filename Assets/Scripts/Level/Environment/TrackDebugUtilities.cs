using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackDebugUtilities : MonoBehaviour {
	public void PrintRemixBase64() {
		Debug.Log("hash: " +
		LevelPieceSuperClass.GetBase64Remix()
		);
	}
}
