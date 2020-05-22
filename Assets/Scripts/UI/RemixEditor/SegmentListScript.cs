using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;
using System.Dynamic;

public class SegmentListItemClass {
	private Toggle itemToggle;
	private TMP_Text itemLabel;
	private LevelPieceSuperClass segment;

	public SegmentListItemClass(LevelPieceSuperClass p_segment, Toggle p_toggle, TMP_Text p_text) {
		segment = p_segment; itemToggle = p_toggle; itemLabel = p_text;
    }

	public void SetText(string txt) { itemLabel.text = txt; }
	public void SetToggleGroup(ToggleGroup p_group) { itemToggle.group = p_group; }
	public void SetUpDownNav(Toggle upSelect, Toggle downSelect) {
		Navigation orgNav = itemToggle.navigation;
		orgNav.selectOnUp = upSelect;
		orgNav.selectOnDown = downSelect;
		itemToggle.navigation = orgNav;
    }
	public void SetLeftRightNav(Toggle leftSelect, Toggle rightSelect) {
		Navigation orgNav = itemToggle.navigation;
		orgNav.selectOnLeft = leftSelect;
		orgNav.selectOnRight = rightSelect;
		itemToggle.navigation = orgNav;
	}

	public Toggle GetToggle() { return itemToggle; }
	public TMP_Text GetText() { return itemLabel; }
	public LevelPieceSuperClass GetSegment() { return segment; }
}

[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(ToggleGroup))]
[RequireComponent(typeof(ScrollToSelected))]
public class SegmentListScript : SegmentEditorSuperClass
{
	private List<SegmentListItemClass> listItems = new List<SegmentListItemClass>();
	private GameObject listContent = null;
	private ScrollToSelected scrollMaster = null;
	private ToggleGroup group = null;

	[Tooltip("Should this script create a fresh new segment list every time it becomes enabled?")]
	public bool newListOnEnable = false;

	protected override void ChildAwake() { 
		listContent = GetComponent<ScrollRect>().content.gameObject;
		scrollMaster = GetComponent<ScrollToSelected>();
		group = GetComponent<ToggleGroup>();
	}

	void Start() {
		if (listItems.Count < 1) CreateSegmentList();
	}

	void OnEnable() { 
		if (listItems.Count < 1) CreateSegmentList();
		else {
			if (newListOnEnable) {
				DeleteSegmentList();
				CreateSegmentList();
			}
        }
	}

	void CreateSegmentList() {
		//Creating one list item for every segment currently registered
		for (int i = 0; i < LevelPieceSuperClass.Segments.Count; i++)
		{
			GameObject newItemObj = Instantiate(Resources.Load<GameObject>("SegmentListItem"));
			newItemObj.transform.SetParent(listContent.transform);
			newItemObj.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);

			Toggle newToggle = newItemObj.GetComponent<Toggle>();
			TMP_Text newText = newItemObj.transform.GetChild(1).GetComponent<TMP_Text>();
			LevelPieceSuperClass newSegment = LevelPieceSuperClass.Segments[i];

			SegmentListItemClass newItem = new SegmentListItemClass(newSegment, newToggle, newText);
			newItem.SetText("Segment " + (i + 1));
			listItems.Add(newItem);
		}
		group.SetAllTogglesOff();

		for (int i = 0; i < listItems.Count; i++) 
		{
			//Setting all list items in the same toggle group
			listItems[i].SetToggleGroup(group);

			//Registering the master script for smooth scrolling in every list item so they can adhere to it
			if (scrollMaster != null) {
				listItems[i].GetToggle().gameObject.GetComponent<SegmentListItemScrollPing>().RegisterScrollMaster(scrollMaster);
			}
			//Setting intra-list navigation relationships
			if (i == 0) {
				listItems[i].SetUpDownNav(listItems[listItems.Count - 1].GetToggle(), listItems[i + 1].GetToggle());
				listItems[i].GetToggle().isOn = true;
			}
			else if (i == listItems.Count - 1)
				listItems[i].SetUpDownNav(listItems[i - 1].GetToggle(), listItems[0].GetToggle());
			else
				listItems[i].SetUpDownNav(listItems[i - 1].GetToggle(), listItems[i + 1].GetToggle());
		}
	}

	void DeleteSegmentList()
    {
		if (listItems.Count < 1)
			UnityEngine.Debug.Log("SegmentListScript/DestroySegmentList: No list items to delete");
		else {
			for (int i = 0; i < listItems.Count; i++) {
				listItems[i] = null;
            }
			listItems.Clear();
		}
    }

	public override void UpdateUI() { }
}
