using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(ToggleGroup))]
public class ObstacleListScript : SegmentEditorSuperClass {

	private static ObstacleListScript mainInstance;

	public static List<ObstacleListItem> obstacleList = new List<ObstacleListItem>();
	private static List<string> currentListLayout = new List<string>();
	private static ToggleGroup group = null;

	private const string obstacleNull = "None";
	private static string currentObstacleType = "None";

	public static string ReadCurrentObstacleType() {
		//Only checking for "turned off" obstacles when recording on segment avoids a lot of potential bugs
		if (!group.AnyTogglesOn())
			currentObstacleType = obstacleNull;

		return currentObstacleType;
	}

	private ObstacleListItem listItemPrefab;

	protected override void ChildAwake() {
		group = GetComponent<ToggleGroup>();
		listItemPrefab = Resources.Load<ObstacleListItem>("ObstacleListItem");
		mainInstance = this;
	}

	void OnDisable() { obstacleList.Clear(); }

	void Start() {
		//So the obstacle list can register itself as a SegmentEditor in Awake() before first segment selection occurs
		SegmentListScript.InitializeSegmentSelection(SegmentListScript.listItems[0]);
		UpdateUI();
	}

	//Sent from ObstacleListItems, triggered by toggle event 
	public void ReceiveTogglePing(string name, bool isOn) {
		if (isOn) {
			currentObstacleType = name;
			ApplyObstacleSelection();
		} else {
			//Avoiding issues with non-user initiated toggle-offs
			ApplyObstacleSelection(obstacleNull);
		}
	}

	//Runs from SegmentListScript.UpdateUI() when new segment is selected, to display its recorded obstacle selection
	public static void SegmentSwapObstacleRestoration(string name) {
		if (name != "") {
			currentObstacleType = name;
		}
	}

	public static ObstacleListItem CurrentFirstItem() {
		// return obstacleList[currentListLayout[0]];
		return obstacleList[0];
	}

	//Runs whenever a new segment is selected
	public override void UpdateUI() {

		// FIXME: resets obstacles loaded by id

		currentListLayout.Clear();

		//For the currently selected segment, which obstacles are available?
		foreach (var item in currentSegment.Obstacles.objects) {
			currentListLayout.Add(item.Key);
		}

		//Fetch the recorded selected obstacle for the currently selected segment list item
		currentObstacleType = SegmentListScript.ReadCurrentItem().GetObstacle();

		//Creates list items based on all entries in CurrentListLayout
		//Should only be true the first time UpdateUI is run

		// obstacleList.Clear();
		// foreach (string entry in currentListLayout) {
		// 	AddNewListItem(entry);
		// }

		int diff = currentListLayout.Count - obstacleList.Count;
		if (currentListLayout.Count > obstacleList.Count) {
			for (int i = 0; i < diff; i++) {
				AddNewListItem("Error, please select next segment");
			}
		}

		for (int i = 0; i < obstacleList.Count; i++) {
			if (i < currentListLayout.Count) {
				obstacleList[i].gameObject.SetActive(true);
				SetListItem(obstacleList[i], currentListLayout[i]);

				Toggle currentSegmentItem = SegmentListScript.ReadCurrentItem().GetToggle();

				int prevIndex = i - 1;
				if (i == 0)
					prevIndex = currentListLayout.Count - 1;

				int nextIndex = i + 1;
				if (i == currentListLayout.Count - 1)
					nextIndex = 0;

				obstacleList[i].SetUpDownNav(obstacleList[prevIndex].ItemToggle, obstacleList[nextIndex].ItemToggle);
				obstacleList[i].SetLeftRightNav(currentSegmentItem, currentSegmentItem);
			} else {
				obstacleList[i].gameObject.SetActive(false);
			}
		}

		// ApplyObstacleSelection();
		SegmentListScript.UpdateLeftNav();
	}

	private void AddNewListItem(string obstacleName) {
		ObstacleListItem newItemObj = Instantiate(Resources.Load<ObstacleListItem>("ObstacleListItem"));
		// ObstacleListItem newItemObj = Instantiate(listItemPrefab);
		// obstacleList.Add(obstacleName, newItemObj);
		obstacleList.Add(newItemObj);
		newItemObj.transform.SetParent(gameObject.transform);
		newItemObj.gameObject.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);

		newItemObj.SetListReference(this);
		newItemObj.SetToggleGroup(group);

		// newItemObj.SetName(p_name);
		// if (p_name == currentObstacleType)
		// 	newItemObj.GetToggle().isOn = true;

		// SetListItem(newItemObj, obstacleName);
	}

	private void SetListItem(ObstacleListItem item, string obstacleName) {
		item.SetName(obstacleName);
		item.ItemToggle.isOn = obstacleName == currentObstacleType;
	}

	public void ApplyObstacleSelection() {
		//UnityEngine.Debug.Log(SegmentListScript.ReadCurrentItem().GetText().text + " ApplyObstacle: " + currentObstacleType);
		currentSegment.Obstacles.UnhideObject(currentObstacleType);
	}

	//Showing a specific obstacle without actually recording it, used to avoid problems with
	//non-user initiated toggle-offs
	public void ApplyObstacleSelection(string p_name) {
		currentSegment.Obstacles.UnhideObject(p_name);
	}
}
