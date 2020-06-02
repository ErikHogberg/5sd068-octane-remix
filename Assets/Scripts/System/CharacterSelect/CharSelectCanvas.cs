using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharSelectCanvas : MonoBehaviour {
	private static CharSelectCanvas instance;
	public static CharSelectCanvas Instance => instance ?? (instance = Instantiate(Resources.Load<CharSelectCanvas>("CharSelectCanvas")));

	private CharacterSelection charSelectReference = null;

	private TMP_Text carName;
	private GameObject characterInfo;
	private RectTransform arrowLeft;
	private RectTransform arrowRight;
	private GameObject allUI;
	private GameObject btnNext;

	private Dictionary<CharacterSelected, GameObject> characterItems = new Dictionary<CharacterSelected, GameObject>();

	void Awake() {
		instance = this;
		allUI = transform.GetChild(0).gameObject;
		carName = allUI.transform.GetChild(0).GetComponent<TMP_Text>();
		characterInfo = allUI.transform.GetChild(1).gameObject;
		arrowLeft = allUI.transform.GetChild(2).GetComponent<RectTransform>();
		arrowRight = allUI.transform.GetChild(3).GetComponent<RectTransform>();
		btnNext = transform.GetChild(1).gameObject;

		characterItems.Add(CharacterSelected.AKASH, characterInfo.transform.GetChild(0).gameObject);
		characterItems.Add(CharacterSelected.MICHISHIGE, characterInfo.transform.GetChild(1).gameObject);
		characterItems.Add(CharacterSelected.LUDWIG, characterInfo.transform.GetChild(2).gameObject);
	}

	public void SetText(string p_carName) { carName.text = p_carName; }

	public void SetCharacter(CharacterSelected p_name) {
		if (characterItems.ContainsKey(p_name)) {
			foreach (KeyValuePair<CharacterSelected, GameObject> item in characterItems) {
				if (item.Key != p_name) { item.Value.SetActive(false); }
				else { item.Value.SetActive(true); }
            }
        } else { UnityEngine.Debug.Log("CharSelectCanvas/SetCharacter: No character item for " + p_name.ToString()); }
    }
	public void Activate(bool toggle) { allUI.SetActive(toggle); btnNext.SetActive(toggle); }


}
