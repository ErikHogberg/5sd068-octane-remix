using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;

[RequireComponent(typeof(Toggle))]
public class ObstacleListItem : MonoBehaviour
{
	private string itemName = "";
	private Toggle itemToggle;
	private TMP_Text itemLabel;
	private ObstacleListScript listReference;

    ObstacleListItem(string p_name, ObstacleListScript p_list) { itemName = p_name; listReference = p_list; }

	void Awake() { 
		itemToggle = GetComponent<Toggle>(); 
		itemLabel = transform.GetChild(1).GetComponent<TMP_Text>();
		TextColorAdjust();
	}

	public void SetName(string p_name) { itemName = p_name; itemLabel.text = p_name; }
	public void SetListReference(ObstacleListScript p_list) { listReference = p_list; }
	public void SetToggleGroup(ToggleGroup p_group) { itemToggle.group = p_group; }

	public void SetUpDownNav(Toggle upSelect, Toggle downSelect)
	{
		Navigation orgNav = itemToggle.navigation;
		orgNav.mode = Navigation.Mode.Explicit;
		orgNav.selectOnUp = upSelect;
		orgNav.selectOnDown = downSelect;
		itemToggle.navigation = orgNav;
	}
	public void SetLeftRightNav(Toggle leftSelect, Toggle rightSelect)
	{
		Navigation orgNav = itemToggle.navigation;
		orgNav.mode = Navigation.Mode.Explicit;
		orgNav.selectOnLeft = leftSelect;
		orgNav.selectOnRight = rightSelect;
		itemToggle.navigation = orgNav;
	}
	public void SetNavModeVertical()
    {
		Navigation orgNav = itemToggle.navigation;
		orgNav.mode = Navigation.Mode.Vertical;
		itemToggle.navigation = orgNav;
	}

	public Toggle GetToggle() { return itemToggle; }
	public TMP_Text GetText() { return itemLabel; }

	//Triggered onValueChanged
	public void TogglePing() {
		listReference.ReceiveTogglePing(itemName, itemToggle.isOn);
		TextColorAdjust();
	}
	private void TextColorAdjust() {
		if (itemToggle.isOn == true)
			itemLabel.color = new Color(itemLabel.color.r, itemLabel.color.g, itemLabel.color.b, 1f);
		else itemLabel.color = new Color(itemLabel.color.r, itemLabel.color.g, itemLabel.color.b, (45f / 255f));
	}
}
