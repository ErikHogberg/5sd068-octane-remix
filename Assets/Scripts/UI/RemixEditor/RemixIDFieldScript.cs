using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RemixIDFieldScript : SegmentEditorSuperClass {

	public TMP_InputField TextField;
	public GameObject LoadErrorPopup;
	public GameObject LoadSuccessPopup;
	public GameObject ClipboardPopup;
	// TODO: successful load popup

	[Space]
	public bool CopyToClipboardOnGetID = true;
	public bool GetIDOnSelect = false;
	[Space]
	public bool PrintDebug = false;

	protected override void ChildAwake() {
		LoadErrorPopup.SetActive(false);
		ClipboardPopup.SetActive(false);
		LoadSuccessPopup.SetActive(false);
	}

	public override void UpdateUI() {
		if (GetIDOnSelect) {
			GetID();
			// TODO: also update ID when changing obstacle on current segment
		}
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
		
		SegmentEditorSuperClass.UpdateAllUI();
	}

}
