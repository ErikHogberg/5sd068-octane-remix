using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CharacterSelected {
	NONE = 0,
	AKASH,
	MICHISHIGE,
	LUDWIG
}

[System.Serializable]
public class CharSelectEntry {
	[Tooltip("The name that will show up in the UI for this car.")]
	public string carName;

	[Tooltip("The selection tag associated with this car.")]
	public CharacterSelected carTag;

	[Tooltip("The game object in the character select environment which contain the purely visual car model.")]
	public GameObject carModel;
}

public class CharacterSelection : MonoBehaviour {
	private static CharacterSelection _i;
	public static CharacterSelection i {
		get {
			if (_i == null) { _i = Instantiate(Resources.Load<CharacterSelection>("CharacterSelectEnvironment")); }
			return _i;
		}
	}

	//Player index + their character choice
	private static Dictionary<int, CharacterSelected> choices = new Dictionary<int, CharacterSelected>();

	[Tooltip("Start the scene with the character select environment active or not.")]
	public bool startAsActive = false;

	[Header("Data")]
	[Tooltip("The data for all character select entries.")]
	public CharSelectEntry[] charSelectData;

	
	//To make sure the actual player objects don't do anything while in character selection
	private List<GameObject> playerObjects;

	//Index for easily storing what car we're currently viewing
	private int currViewIndex = 0;

	private Canvas mainCanvas;
	private Camera charSelectCamera;
	private GameObject charSelectVisuals;

	//To be able to seamlessly return to whatever we were doing before hopping into char select
	private Dictionary<Camera, bool> cameraStatusMemory;

	void Awake() {
		if (_i == null) _i = this;
		playerObjects = new List<GameObject>();

		mainCanvas = CanvasFinder.thisCanvas;
		cameraStatusMemory = new Dictionary<Camera, bool>();
		// choices = new Dictionary<int, CharacterSelected>();
		UINavInput.i.AddUINavListener(UIMode.CHARSELECT);

		if (transform.childCount > 2) {
			Debug.Log("CharacterSelection: Root object has more than its intended 2 children");
			return;
		} else {
			foreach (Transform child in transform) {
				if (child.GetComponent<Camera>() != null)
					charSelectCamera = child.gameObject.GetComponent<Camera>();
				else charSelectVisuals = child.gameObject;
			}
			charSelectCamera.enabled = false;
			charSelectVisuals.SetActive(false);
			CharSelectCanvas.i.Activate(false);
		}
	}
	void Start() {
		MakePick(0, CharacterSelected.NONE);
		if (startAsActive)
			ActivateCharSelect(true);
	}

	//TODO: Make it button-activate
	//Sets the current choice of character for a specified player index
	public void MakePick(int playerIndex, CharacterSelected pick) {
		if (choices.ContainsKey(playerIndex)) {
			choices[playerIndex] = pick;
		} else {
			choices.Add(playerIndex, pick);
		}
		VisualSetDisplay();
		AnnouncePick(playerIndex);
	}
	public static CharacterSelected GetPick(int playerIndex) {
		if (playerIndex >= choices.Count) 
			return CharacterSelected.NONE;
		
		return choices[playerIndex];
	}

	public CharacterSelected CurrentIndex() { return charSelectData[currViewIndex].carTag; }

	//Goes either left or right through the array of available cars
	public void SwapDisplayCar(bool goLeft) {
		charSelectData[currViewIndex].carModel.SetActive(false);

		if (goLeft) currViewIndex -= 1;
		else currViewIndex += 1;

		if (currViewIndex < 0) currViewIndex = charSelectData.Length - 1;
		else if (currViewIndex >= charSelectData.Length) currViewIndex = 0;

		VisualSetDisplay();
	}
	//Swaps immediately to the available car at the specified index
	public void SetDisplayCar(int index) {
		charSelectData[currViewIndex].carModel.SetActive(false);
		currViewIndex = index;
		VisualSetDisplay();
	}

	private void VisualSetDisplay() {
		if (charSelectData[currViewIndex].carTag == choices[0])
			CharSelectCanvas.i.SetCheck(true);
		else CharSelectCanvas.i.SetCheck(false);

		CharSelectCanvas.i.SetText(charSelectData[currViewIndex].carName);
		charSelectData[currViewIndex].carModel.SetActive(true);
	}
	private void AnnouncePick(int index) {
		string car_n = "";
		foreach (CharSelectEntry entry in charSelectData) {
			if (entry.carTag == choices[index]) { car_n = entry.carName; }
		}
		if (car_n == "") car_n = "no car yet";
		Debug.Log("CharacterSelection: Player " + index + " picked " + car_n + "!");
	}

	public void ActivateCharSelect(bool toggle) {
		playerObjects.Add(SteeringScript.MainInstance.gameObject);
		foreach (GameObject obj in playerObjects) { obj.SetActive(!toggle); }
		CanvasFinder.thisCanvas.gameObject.SetActive(!toggle);
		UINavInput.i.SetUINavMode(UIMode.CHARSELECT);

		if (toggle == true) {
			Camera[] cameraList = Camera.allCameras;
			foreach (Camera cam in cameraList) {
				cameraStatusMemory.Add(cam, cam.enabled);
				cam.enabled = !toggle;
			}
			UINavInput.i.Activate();
			CanvasFinder.SetThisCanvas(CharSelectCanvas.i.GetComponent<Canvas>());
		} else {
			foreach (KeyValuePair<Camera, bool> cam in cameraStatusMemory) {
				cam.Key.enabled = cam.Value;
			}
			cameraStatusMemory.Clear();
			UINavInput.i.Deactivate();
		}
		charSelectVisuals.SetActive(toggle);
		charSelectCamera.enabled = toggle;
		CharSelectCanvas.i.Activate(toggle);

		for (int i = 0; i < charSelectData.Length; i++) {
			//Sorts choices dictionary by key and picks out the item with lowest-value key
			if (charSelectData[i].carTag == choices.OrderBy(j => j.Key).First().Value) {
				CharSelectCanvas.i.SetText(charSelectData[i].carName);
				currViewIndex = i;
			}
		}
	}

}
