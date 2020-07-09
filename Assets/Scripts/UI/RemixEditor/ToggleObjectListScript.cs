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
public class ToggleObjectListScript : MonoBehaviour {

	public static ToggleObjectListScript MainInstance;

	public static List<ToggleObjectListItem> listItems = new List<ToggleObjectListItem>();
	private static ToggleObjectListItem currentItem;
	public static ToggleObjectListItem ReadCurrentItem() { return currentItem; }

	private static RemixEditorToggleObject currentToggleObject = null;

	public Button startButton;
	// public GameObject ListEntryTemplate;
	public ToggleObjectListItem ListEntryTemplate;

	// private GameObject listContent = null;
	private ScrollToSelected scrollMaster = null;
	private ToggleGroup group = null;

	void Awake() {
		MainInstance = this;
		
		// listContent = GetComponent<ScrollRect>().content.gameObject;
		scrollMaster = GetComponent<ScrollToSelected>();
		group = GetComponent<ToggleGroup>();
		// TimerScript.Instance.ResetTimer();
	}

	private void OnDestroy() {
		MainInstance = null;
	}

	void Start() {
		if (listItems.Count < 1) CreateSegmentList();
		ScrollToTop();
	}

	private void ScrollToTop() {
		GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
		if (listItems.Count > 0)
			EventSystem.current.SetSelectedGameObject(listItems[0].GetToggle().gameObject);
	}

	//Way of picking a segment #1
	//Sent from SegmentListItems, triggered by toggle event
	public void ReceiveTogglePing(ToggleObjectListItem item, bool isOn) {
		if (isOn) {
			if (currentToggleObject != item.GetSegment()) {
				currentItem = item;
				RemixMapScript.Select(item.GetSegment());
				UpdateStartButtonNav(currentItem.GetToggle());
			}
		}
	}
	//ATM, run by ObstacleListScript in its Start() so that it can register itself as a SegmentEditor 
	//in Awake() before first segmentselection occurs
	public static void InitializeSegmentSelection(ToggleObjectListItem item) {
		RemixMapScript.Select(item.GetSegment());
		currentItem = item;
		UpdateLeftNav();
	}

	public static void UpdateLeftNav() {
		foreach (ToggleObjectListItem item in listItems) {
			Toggle firstObstacle = ObstacleListScript.CurrentFirstItem().ItemToggle;
			item.SetLeftNav(firstObstacle);
		}
	}

	private void UpdateStartButtonNav(Toggle p_item) {
		Navigation orgNav = startButton.navigation;
		orgNav.mode = Navigation.Mode.Explicit;
		orgNav.selectOnLeft = p_item;
		orgNav.selectOnRight = p_item;
		startButton.navigation = orgNav;
	}

	void CreateSegmentList() {
		//Creating one list item for every segment currently registered

		if (LevelPieceSuperClass.Segments.Count < 1) {
			ListEntryTemplate.gameObject.SetActive(false);
			return;
		}

		ListEntryTemplate.SetSegment(LevelPieceSuperClass.Segments[0]);
		ListEntryTemplate.SetToggleGroup(group);
		ListEntryTemplate.SetListReference(this);
		ListEntryTemplate.GetScrollPinger().RegisterScrollMaster(scrollMaster);
		listItems.Add(ListEntryTemplate);

		// NOTE: skips first entry
		for (int i = 1; i < LevelPieceSuperClass.Segments.Count; i++) {
			//Instantiating a new list item
			// SegmentListItem newItemObj = Instantiate(Resources.Load<SegmentListItem>("SegmentListItem"));
			ToggleObjectListItem newItemObj = Instantiate(ListEntryTemplate);//.GetComponent<SegmentListItem>();
																			 // newItemObj.transform.SetParent(listContent.transform);
			newItemObj.transform.SetParent(ListEntryTemplate.transform.parent);
			newItemObj.GetComponent<RectTransform>().localScale = Vector3.one;

			//Giving data to new list item
			newItemObj.SetSegment(LevelPieceSuperClass.Segments[i]);
			newItemObj.SetToggleGroup(group);
			newItemObj.SetListReference(this);

			//Checking if a segment has a visible obstacle on them from before initialization
			// var shownObject = LevelPieceSuperClass.Segments[i].Obstacles.ShownObject;
			// if (shownObject != null && shownObject.Key != "")
			// newItemObj.UpdateObstacle(shownObject.Key);

			//Registering the master script for smooth scrolling in every list item so they can adhere to it
			newItemObj.GetScrollPinger().RegisterScrollMaster(scrollMaster);

			newItemObj.SetText("Segment " + (i + 1));
			listItems.Add(newItemObj);
		}
		group.SetAllTogglesOff();

		if (listItems.Count < 1) {
			return;
		}

		//Setting intra-list navigation relationships, for which all list items need to already exist
		UpdateStartButtonNav(listItems[0].GetToggle());
		for (int i = 0; i < listItems.Count; i++) {
			listItems[i].SetRightNav(startButton);

			if (i == 0) {
				listItems[i].SetUpDownNav(listItems[listItems.Count - 1].GetToggle(), listItems[i + 1].GetToggle());
				listItems[i].GetToggle().isOn = true;
			} else if (i == listItems.Count - 1)
				listItems[i].SetUpDownNav(listItems[i - 1].GetToggle(), listItems[0].GetToggle());
			else
				listItems[i].SetUpDownNav(listItems[i - 1].GetToggle(), listItems[i + 1].GetToggle());
		}
		currentItem = listItems[0];
	}

	public void UpdateUI() {

		if (currentToggleObject == null) {
			return;
		}

		//Way of picking a segment #2
		//Should only run when a segment is selected through clicking on them in the world
		if (currentToggleObject != currentItem.GetSegment()) {
			foreach (ToggleObjectListItem item in listItems) {
				if (item.GetSegment() == currentToggleObject) {
					string currentObstacleType = ObstacleListScript.ReadCurrentObstacleType();
					// Records which obstacle is currently selected for this segment, before switching to the new one
					// currentItem.UpdateObstacle(currentObstacleType);

					currentItem = item;
					currentItem.MarkAsSelected();
					UpdateStartButtonNav(currentItem.GetToggle());

					//Applying the new segment's recorded obstacle to the obstacle list
					ObstacleListScript.SegmentSwapObstacleRestoration(currentItem.GetObstacle());
					EventSystem.current.SetSelectedGameObject(currentItem.GetToggle().gameObject); // FIXME: segment not getting selected correctly
					break;
				}
			}
		}
	}

	public void SetToggleObject(RemixEditorToggleObject toggleObject) {
		currentToggleObject = toggleObject;
		UpdateUI();
	}
}
