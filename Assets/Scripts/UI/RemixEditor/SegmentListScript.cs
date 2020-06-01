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
public class SegmentListScript : SegmentEditorSuperClass {
	public Button startButton;
	public static List<SegmentListItem> listItems = new List<SegmentListItem>();
	private static SegmentListItem currentItem;
	public static SegmentListItem ReadCurrentItem() { return currentItem; }

	private GameObject listContent = null;
	private ScrollToSelected scrollMaster = null;
	private ToggleGroup group = null;

	[Tooltip("Should this script create a fresh new segment list every time it becomes enabled?")]
	public bool newListOnEnable = false;

	protected override void ChildAwake() {
		listContent = GetComponent<ScrollRect>().content.gameObject;
		scrollMaster = GetComponent<ScrollToSelected>();
		group = GetComponent<ToggleGroup>();
		TimerScript.Instance.ResetTimer();
	}
	void Start() {
		if (listItems.Count < 1) CreateSegmentList();
		ScrollToTop();
	}
	void OnEnable() {
		Init();
	}

	private void OnDisable() {
		DeleteSegmentList();
	}

	// bool hasBeenInit = false;
	private void Init() {

		// if (hasBeenInit || LevelPieceSuperClass.Segments.Count < 1) {
		// 	return;
		// }

		// hasBeenInit = true;

		if (listItems.Count < 1) CreateSegmentList();
		else {
			if (newListOnEnable) {
				DeleteSegmentList();
				CreateSegmentList();
			}
		}

		// UpdateUI();
	}

	// private void Update() {
	// 	Init();
	// }

	private void ScrollToTop() {
		GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
		EventSystem.current.SetSelectedGameObject(listItems[0].GetToggle().gameObject);
	}

	//Way of picking a segment #1
	//Sent from SegmentListItems, triggered by toggle event
	public void ReceiveTogglePing(SegmentListItem p_item, bool is_on) {
		if (is_on) {
			if (currentSegment != p_item.GetSegment()) {
				RemixMapScript.SelectSegment(p_item.GetSegment());
				currentItem = p_item;
				UpdateStartButtonNav(currentItem.GetToggle());
			}
		}
	}
	//ATM, run by ObstacleListScript in its Start() so that it can register itself as a SegmentEditor 
	//in Awake() before first segmentselection occurs
	public static void InitializeSegmentSelection(SegmentListItem p_item) {
		RemixMapScript.SelectSegment(p_item.GetSegment());
		currentItem = p_item;
		UpdateLeftNav();
	}

	public static void UpdateLeftNav() {
		foreach (SegmentListItem item in listItems) {
			Toggle firstObstacle = ObstacleListScript.CurrentFirstItem().GetToggle();
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
		for (int i = 0; i < LevelPieceSuperClass.Segments.Count; i++) {
			//Instantiating a new list item
			SegmentListItem newItemObj = Instantiate(Resources.Load<SegmentListItem>("SegmentListItem"));
			newItemObj.transform.SetParent(listContent.transform);
			newItemObj.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);

			//Giving data to new list item
			newItemObj.SetSegment(LevelPieceSuperClass.Segments[i]);
			newItemObj.SetToggleGroup(group);
			newItemObj.SetListReference(this);

			//Checking if a segment has a visible obstacle on them from before initialization
			var shownObject = LevelPieceSuperClass.Segments[i].Obstacles.ShownObject;
			if (shownObject != null && shownObject.Key != "")
				newItemObj.UpdateObstacle(shownObject.Key);

			//Registering the master script for smooth scrolling in every list item so they can adhere to it
			newItemObj.GetScrollPinger().RegisterScrollMaster(scrollMaster);

			newItemObj.SetText("Segment " + (i + 1));
			listItems.Add(newItemObj);
		}
		group.SetAllTogglesOff();

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

		if (currentSegment == null) {
			return;
		}

		//Way of picking a segment #2
		//Should only run when a segment is selected through clicking on them in the world
		if (currentSegment != currentItem.GetSegment()) {
			foreach (SegmentListItem item in listItems) {
				if (item.GetSegment() == currentSegment) {
					string currentObstacleType = ObstacleListScript.ReadCurrentObstacleType();
					//Records which obstacle is currently selected for this segment, before switching to the new one
					currentItem.UpdateObstacle(currentObstacleType);
					currentItem = item;
					currentItem.MarkAsSelected();
					UpdateStartButtonNav(currentItem.GetToggle());

					//Applying the new segment's recorded obstacle to the obstacle list
					ObstacleListScript.SegmentSwapObstacleRestoration(currentItem.GetObstacle());
					EventSystem.current.SetSelectedGameObject(currentItem.GetToggle().gameObject);
					break;
				}
			}
		}
	}
}
