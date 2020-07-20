using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;

[RequireComponent(typeof(Toggle))]
public class ObstacleListItem : MonoBehaviour {
	public string ItemName { get; private set; } = "";
	public Toggle ItemToggle { get; private set; }
	public TMP_Text ItemLabel { get; private set; }
	private ObstacleListScript listReference;

	// ObstacleListItem(string p_name, ObstacleListScript p_list) { ItemName = p_name; listReference = p_list; }

	void Awake() {
		ItemToggle = GetComponent<Toggle>();
		ItemLabel = transform.GetChild(1).GetComponent<TMP_Text>();
		TextColorAdjust();
		// Debug.Log("obstacle list item awake");
	}

	public void SetName(string name) { ItemName = name; ItemLabel.text = name; }
	public void SetListReference(ObstacleListScript list) { listReference = list; }
	public void SetToggleGroup(ToggleGroup group) { ItemToggle.group = group; }

	public void SetUpDownNav(Toggle upSelect, Toggle downSelect) {
		Navigation orgNav = ItemToggle.navigation;
		orgNav.mode = Navigation.Mode.Explicit;
		orgNav.selectOnUp = upSelect;
		orgNav.selectOnDown = downSelect;
		ItemToggle.navigation = orgNav;
	}

	public void SetLeftRightNav(Toggle leftSelect, Toggle rightSelect) {
		Navigation orgNav = ItemToggle.navigation;
		orgNav.mode = Navigation.Mode.Explicit;
		orgNav.selectOnLeft = leftSelect;
		orgNav.selectOnRight = rightSelect;
		ItemToggle.navigation = orgNav;
	}

	public void SetNavModeVertical() {
		Navigation orgNav = ItemToggle.navigation;
		orgNav.mode = Navigation.Mode.Vertical;
		ItemToggle.navigation = orgNav;
	}

	//Triggered onValueChanged
	public void TogglePing() {
		listReference.ReceiveTogglePing(ItemName, ItemToggle.isOn);
		TextColorAdjust();
	}

	private void TextColorAdjust() {
		if (ItemToggle.isOn == true)
			ItemLabel.color = new Color(ItemLabel.color.r, ItemLabel.color.g, ItemLabel.color.b, 1f);
		else
			ItemLabel.color = new Color(ItemLabel.color.r, ItemLabel.color.g, ItemLabel.color.b, (45f / 255f));
	}
}
