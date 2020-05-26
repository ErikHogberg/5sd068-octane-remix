﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SegmentListItem : MonoBehaviour
{
	private Toggle itemToggle;
	private TMP_Text itemLabel;
	private LevelPieceSuperClass segment;
	private SegmentListItemScrollPing scrollPing;
	private SegmentListScript listReference;

	void Awake() {
		itemToggle = GetComponent<Toggle>();
		itemLabel = transform.GetChild(1).GetComponent<TMP_Text>();
		scrollPing = GetComponent<SegmentListItemScrollPing>();
		TextColorAdjust();
	}

	public void SetText(string txt) { itemLabel.text = txt; }
	public void SetSegment(LevelPieceSuperClass p_segment) { segment = p_segment; }
	public void SetToggleGroup(ToggleGroup p_group) { itemToggle.group = p_group; }
	public void SetListReference(SegmentListScript p_list) { listReference = p_list; }

	public void SetUpDownNav(Toggle upSelect, Toggle downSelect)
	{
		Navigation orgNav = itemToggle.navigation;
		orgNav.selectOnUp = upSelect;
		orgNav.selectOnDown = downSelect;
		itemToggle.navigation = orgNav;
	}
	public void SetLeftRightNav(Toggle leftSelect, Toggle rightSelect)
	{
		Navigation orgNav = itemToggle.navigation;
		orgNav.selectOnLeft = leftSelect;
		orgNav.selectOnRight = rightSelect;
		itemToggle.navigation = orgNav;
	}

	public Toggle GetToggle() { return itemToggle; }
	public TMP_Text GetText() { return itemLabel; }
	public LevelPieceSuperClass GetSegment() { return segment; }
	public SegmentListItemScrollPing GetScrollPinger() { return scrollPing; }

	//Triggered onValueChanged
	public void TogglePing() {
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
		else itemLabel.color = new Color(itemLabel.color.r, itemLabel.color.g, itemLabel.color.b, (45f / 255f));
	}

}
