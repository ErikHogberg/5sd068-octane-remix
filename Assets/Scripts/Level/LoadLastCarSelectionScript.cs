using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectSelectorScript))]
public class LoadLastCarSelectionScript : MonoBehaviour {

	// public bool Enable = true;

	public bool LoadIfNone = true;
	public string CarToLoadIfNone = "Ludwig";

	void Start() {
		var selector = GetComponent<ObjectSelectorScript>();

		CharacterSelected selectedCar = CharacterSelection.GetPick(0);

		switch (selectedCar) {
			case CharacterSelected.AKASH:
				selector.UnhideObject("Akash");
				break;
			case CharacterSelected.LUDWIG:
				selector.UnhideObject("Ludwig");
				break;
			case CharacterSelected.MICHISHIGE:
				selector.UnhideObject("Michishige");
				break;
			case CharacterSelected.NONE:
				if (LoadIfNone) 
					selector.UnhideObject(CarToLoadIfNone);
				break;
		}

		// RemixEditorGoalPost.MoveCarToStart();

		Debug.Log("current car: " + SteeringScript.MainInstance.gameObject.name);
		LapCounterUIScript.SetCar();
		GameOverPanel.SetCar();

	}

}
