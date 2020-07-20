using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ToggleObjectListItem : MonoBehaviour {
	private Toggle itemToggle;
	private TMP_Text itemLabel;

	// private string segmentSelectedObstacle;
	private RemixEditorToggleObject toggleObject;
	private ListItemScrollPing scrollPing;
	private ToggleObjectListScript listReference;

	void Awake() {
		itemToggle = GetComponent<Toggle>();
		itemLabel = transform.GetChild(1).GetComponent<TMP_Text>();
		scrollPing = GetComponent<ListItemScrollPing>();
		// segmentSelectedObstacle = "None";
		TextColorAdjust();
	}

	public void SetText(string txt) { itemLabel.text = txt; }
	public void SetToggleObject(RemixEditorToggleObject toggleObject) { this.toggleObject = toggleObject; }
	public void SetListReference(ToggleObjectListScript list) { listReference = list; }

	public void SetUpDownNav(Toggle upSelect, Toggle downSelect) {
		Navigation orgNav = itemToggle.navigation;
		orgNav.selectOnUp = upSelect;
		orgNav.selectOnDown = downSelect;
		itemToggle.navigation = orgNav;
	}

	public void SetLeftNav(Toggle leftSelect) {
		Navigation orgNav = itemToggle.navigation;
		orgNav.selectOnLeft = leftSelect;
		itemToggle.navigation = orgNav;
	}

	public void SetRightNav(Button rightSelect) {
		Navigation orgNav = itemToggle.navigation;
		orgNav.selectOnRight = rightSelect;
		itemToggle.navigation = orgNav;
	}

	public Toggle GetToggle() { return itemToggle; }
	public TMP_Text GetText() { return itemLabel; }
	public RemixEditorToggleObject GetToggleObject() { return toggleObject; }
	public ListItemScrollPing GetScrollPinger() { return scrollPing; }

	//Triggered onValueChanged
	public void TogglePing() {
		// toggleObject.ToggleState = itemToggle.isOn;
		toggleObject.ObjectToToggle.SetActive(itemToggle.isOn);
		listReference.ReceiveTogglePing(this, itemToggle.isOn);
		TextColorAdjust();
	}

	public void MarkAsSelected() {
		itemToggle.isOn = true;
		TextColorAdjust();
	}

	private void TextColorAdjust() {
		if (itemToggle.isOn == true)
			itemLabel.color = new Color(itemLabel.color.r, itemLabel.color.g, itemLabel.color.b, 1f);
		else
			itemLabel.color = new Color(itemLabel.color.r, itemLabel.color.g, itemLabel.color.b, (45f / 255f));
	}

}
