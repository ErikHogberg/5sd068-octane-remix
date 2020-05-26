using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Diagnostics;
using System.Dynamic;

[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(ToggleGroup))]
[RequireComponent(typeof(ScrollToSelected))]
public class SegmentListScript : SegmentEditorSuperClass
{
	public static List<SegmentListItem> listItems = new List<SegmentListItem>();
	public static SegmentListItem currentItem;

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

	//Sent from SegmentListItems, triggered by event
	public void ReceiveTogglePing(SegmentListItem p_item, bool is_on) {
		if (is_on) {
			RemixMapScript.SelectSegment(p_item.GetSegment());
			currentItem = p_item;
		}
	}
	//ATM, run by ObstacleListScript in its Start() so that it can register itself as a SegmentEditor before first selection
	public static void InitializeSegmentSelection(SegmentListItem p_item) {
		RemixMapScript.SelectSegment(p_item.GetSegment());
		currentItem = p_item;
	}

	void CreateSegmentList() {
		//Creating one list item for every segment currently registered
		for (int i = 0; i < LevelPieceSuperClass.Segments.Count; i++)
		{
			//Instantiating a new list item
			SegmentListItem newItemObj = Instantiate(Resources.Load<SegmentListItem>("SegmentListItem"));
			newItemObj.transform.SetParent(listContent.transform);
			newItemObj.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);

			//Giving data to new list item
			newItemObj.SetSegment(LevelPieceSuperClass.Segments[i]);
			newItemObj.SetToggleGroup(group);
			newItemObj.SetListReference(this);
			//Registering the master script for smooth scrolling in every list item so they can adhere to it
			newItemObj.GetScrollPinger().RegisterScrollMaster(scrollMaster);

			newItemObj.SetText("Segment " + (i + 1));
			listItems.Add(newItemObj);
		}
		group.SetAllTogglesOff();

		for (int i = 0; i < listItems.Count; i++) {
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
		EventSystem.current.SetSelectedGameObject(listItems[0].GetToggle().gameObject);
	}

	void DeleteSegmentList() {
		if (listItems.Count < 1)
			UnityEngine.Debug.Log("SegmentListScript/DestroySegmentList: No list items to delete");
		else {
			for (int i = 0; i < listItems.Count; i++) {
				listItems[i] = null;
            }
			listItems.Clear();
		}
    }

	public override void UpdateUI() {
		if (currentSegment != currentItem.GetSegment()) {
			foreach (SegmentListItem item in listItems) { 
				if (item.GetSegment() == currentSegment)
                {
					currentItem = item;
					item.MarkAsSelected();
					EventSystem.current.SetSelectedGameObject(item.GetToggle().gameObject);
					break;
				}
			}
		}
	}
}
