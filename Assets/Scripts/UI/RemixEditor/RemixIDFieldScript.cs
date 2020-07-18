using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RemixIDFieldScript : MonoBehaviour {

	public TMP_InputField TextField;
	public GameObject LoadErrorPopup;
	public GameObject LoadSuccessPopup;
	public GameObject ClipboardPopup;
	// TODO: successful load popup

	[Space]
	public bool CopyToClipboardOnGetID = true;
	[Space]
	public bool PrintDebug = false;

	protected void Awake() {
		LoadErrorPopup.SetActive(false);
		ClipboardPopup.SetActive(false);
		LoadSuccessPopup.SetActive(false);
	}

	public void GetID() {
		string id = LevelPieceSuperClass.GetRemixString(PrintDebug);
		TextField.text = id;

		LoadErrorPopup.SetActive(false);
		LoadSuccessPopup.SetActive(false);
		ClipboardPopup.SetActive(true);

		if (CopyToClipboardOnGetID)
			GUIUtility.systemCopyBuffer = id;
	}

	public void SetID() {
		string id = TextField.text;
		bool success = LevelPieceSuperClass.LoadRemixFromString(id);

		// Debug.Log("Loaded " + id + " " + success);
		
		LoadErrorPopup.SetActive(!success);
		LoadSuccessPopup.SetActive(success);
		ClipboardPopup.SetActive(false);
		
		ObstacleListScript.UpdateUIStatic();
	}

}
