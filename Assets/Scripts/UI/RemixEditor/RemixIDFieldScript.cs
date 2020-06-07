using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RemixIDFieldScript : SegmentEditorSuperClass {

	public TMP_InputField TextField;
	public GameObject ErrorPopup;
	public GameObject ClipboardPopup;
	// TODO: successful load popup

	[Space]
	public bool GetIDOnSelect = false;

	protected override void ChildAwake() {
		ErrorPopup.SetActive(false);
		ClipboardPopup.SetActive(false);
	}

	public override void UpdateUI() {
		if (GetIDOnSelect) {
			GetID();
			// TODO: also update ID when changing obstacle on current segment
		}
	}

	public void GetID() {
		string id = LevelPieceSuperClass.GetBase64Remix();
		TextField.text = id;
		ErrorPopup.SetActive(false);
		ClipboardPopup.SetActive(true);
		GUIUtility.systemCopyBuffer = id;
	}

	public void SetID() {
		bool success = LevelPieceSuperClass.LoadRemixFromBase64(TextField.text);
		ErrorPopup.SetActive(!success);
		ClipboardPopup.SetActive(false);
	}

}
