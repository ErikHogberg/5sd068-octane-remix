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
	private GameObject checkMarkSelected;
	private GameObject allUI;
	private GameObject btnNext;

	void Awake() {
		instance = this;
		allUI = transform.GetChild(0).gameObject;
		btnNext = transform.GetChild(1).gameObject;
		carName = allUI.transform.GetChild(0).GetComponent<TMP_Text>();
	}

	public void SetText(string p_carName) { carName.text = p_carName; }
	public void SetCheck(bool toggle) {
		
	}
	public void Activate(bool toggle) { allUI.SetActive(toggle); btnNext.SetActive(toggle); }


}
