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
public class GoalSpotListScript : MonoBehaviour {

	public static GoalSpotListScript MainInstance;

	public static List<GoalSpotListItem> listItems = new List<GoalSpotListItem>();
	private static GoalSpotListItem currentItem;
	public static GoalSpotListItem ReadCurrentItem() { return currentItem; }

	public static RemixEditorGoalPost CurrentGoalPost = null;

	public Button startButton;
	// public GameObject ListEntryTemplate;
	public GoalSpotListItem ListEntryTemplate;

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
	public void ReceiveTogglePing(GoalSpotListItem item, bool isOn) {
		if (isOn) {
			if (CurrentGoalPost != item.GetGoalPost()) {
				currentItem = item;
				RemixMapScript.Select(item.GetGoalPost());
				UpdateStartButtonNav(currentItem.GetToggle());
			}
		}
	}
	//ATM, run by ObstacleListScript in its Start() so that it can register itself as a SegmentEditor 
	//in Awake() before first segmentselection occurs
	public static void InitializeSegmentSelection(GoalSpotListItem item) {
		RemixMapScript.Select(item.GetGoalPost());
		currentItem = item;
		UpdateLeftNav();
	}

	public static void UpdateLeftNav() {
		foreach (GoalSpotListItem item in listItems) {
			Toggle firstObstacle = ObstacleListScript.CurrentFirstItem().ItemToggle;
			item.SetLeftNav(firstObstacle);
		}
	}

	private void UpdateStartButtonNav(Toggle item) {
		Navigation orgNav = startButton.navigation;
		orgNav.mode = Navigation.Mode.Explicit;
		orgNav.selectOnLeft = item;
		orgNav.selectOnRight = item;
		startButton.navigation = orgNav;
	}

	void SetEntry(GoalSpotListItem entry, RemixEditorGoalPost goalPost, int index) {
		entry.SetGoalPost(goalPost);
		entry.SetToggleGroup(group);
		entry.SetListReference(this);
		entry.GetScrollPinger().RegisterScrollMaster(scrollMaster);
		entry.SetText("Line " + (index + 1));
		listItems.Add(entry);
	}

	void CreateSegmentList() {
		//Creating one list item for every segment currently registered

		if (RemixEditorGoalPost.Instances.Count < 1) {
			ListEntryTemplate.gameObject.SetActive(false);
			return;
		}

		ListEntryTemplate.gameObject.SetActive(true);
		SetEntry(ListEntryTemplate, RemixEditorGoalPost.Instances[0], 1);

		// NOTE: skips first entry
		for (int i = 1; i < RemixEditorGoalPost.Instances.Count; i++) {
			//Instantiating a new list item
			// SegmentListItem newItemObj = Instantiate(Resources.Load<SegmentListItem>("SegmentListItem"));
			GoalSpotListItem newItemObj = Instantiate(ListEntryTemplate);//.GetComponent<SegmentListItem>();

			// newItemObj.transform.SetParent(listContent.transform);
			newItemObj.transform.SetParent(ListEntryTemplate.transform.parent);
			newItemObj.GetComponent<RectTransform>().localScale = Vector3.one;

			SetEntry(newItemObj, RemixEditorGoalPost.Instances[i], i + 1);
		}

		group.SetAllTogglesOff();

		if (listItems.Count < 1) 
			return;

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

		if (CurrentGoalPost == null) {
			return;
		}

		//Way of picking a segment #2
		//Should only run when a segment is selected through clicking on them in the world
		if (CurrentGoalPost != currentItem.GetGoalPost()) {
			foreach (GoalSpotListItem item in listItems) {
				if (item.GetGoalPost() == CurrentGoalPost) {
					string currentObstacleType = ObstacleListScript.ReadCurrentObstacleType();
					// Records which obstacle is currently selected for this segment, before switching to the new one
					// currentItem.UpdateObstacle(currentObstacleType);

					currentItem = item;
					currentItem.MarkAsSelected();
					UpdateStartButtonNav(currentItem.GetToggle());

					//Applying the new segment's recorded obstacle to the obstacle list
					// ObstacleListScript.SegmentSwapObstacleRestoration(currentItem.GetObstacle());
					EventSystem.current.SetSelectedGameObject(currentItem.GetToggle().gameObject); // FIXME: segment not getting selected correctly
					break;
				}
			}
		}
	}

	public void SetToggleObject(RemixEditorGoalPost goalPost) {
		CurrentGoalPost = goalPost;
		GoalPostMenuScript.Show();
		// UpdateUI();
	}

}
