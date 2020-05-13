using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharSelectCanvas : MonoBehaviour {
	private static CharSelectCanvas instance;
	public static CharSelectCanvas Instance => instance ?? (instance = Instantiate(Resources.Load<CharSelectCanvas>("CharSelectCanvas")));


	private TMP_Text carName;
	private Image checkMark;
	private GameObject allUI;
	private GameObject btnNext;

	void Awake() {
		allUI = transform.GetChild(0).gameObject;
		btnNext = transform.GetChild(1).gameObject;
		carName = allUI.transform.GetChild(0).GetComponent<TMP_Text>();
		checkMark = carName.transform.GetChild(0).GetComponent<Image>();
	}

	public void SetText(string p_carName) { carName.text = p_carName; }
	public void SetCheck(bool toggle) {
		if (toggle == true) {
			checkMark.color = new Color(30f / 255f, 200f / 255f, 150f / 255f);
		} else { checkMark.color = new Color(185f / 255f, 185f / 255f, 185f / 255f); }
	}
	public void Activate(bool toggle) { allUI.SetActive(toggle); btnNext.SetActive(toggle); }

}
