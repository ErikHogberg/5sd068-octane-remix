using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(ToggleGroup))]
public class ObstacleListScript : SegmentEditorSuperClass {
	// public static Dictionary<string, ObstacleListItem> obstacleList = new Dictionary<string, ObstacleListItem>();
	public static List<ObstacleListItem> obstacleList = new List<ObstacleListItem>();
	private static List<string> currentListLayout = new List<string>();
	private static ToggleGroup group = null;

	private static string obstacleNull = "None";
	private static string currentObstacleType = "None";

	public static string ReadCurrentObstacleType() {
		//Only checking for "turned off" obstacles when recording on segment avoids a lot of potential bugs
		if (group.AnyTogglesOn()) {
			return currentObstacleType;
		} else {
			currentObstacleType = obstacleNull;
			return currentObstacleType;
		}
	}

	private ObstacleListItem listItemPrefab;

	protected override void ChildAwake() {
		group = GetComponent<ToggleGroup>();
		listItemPrefab = Resources.Load<ObstacleListItem>("ObstacleListItem");
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
		}
		//Avoiding issues with non-user initiated toggle-offs
		else DisplayObstacle(obstacleNull);
	}

	//Runs from SegmentListScript.UpdateUI() when new segment is selected, to display its recorded obstacle selection
	public static void SegmentSwapObstacleRestoration(string p_name) {
		if (p_name != "") {
			currentObstacleType = p_name;
		}
	}

	public static ObstacleListItem CurrentFirstItem() {
		// return obstacleList[currentListLayout[0]];
		return obstacleList[0];
	}

	//Runs whenever a new segment is selected
	public override void UpdateUI() {
		currentListLayout.Clear();
		//For the currently selected segment, which obstacles are available?
		for (int i = 0; i < currentSegment.Obstacles.objects.Count; i++) {
			var item = currentSegment.Obstacles.objects[i];
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

		// FIXME: index out of range exception after adding list items, on first time iterating and setting item settings

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

				obstacleList[i].SetUpDownNav(obstacleList[prevIndex].GetToggle(), obstacleList[nextIndex].GetToggle());
				obstacleList[i].SetLeftRightNav(currentSegmentItem, currentSegmentItem);
			} else {
				obstacleList[i].gameObject.SetActive(false);
			}
		}


		/*
		if (obstacleList.Count < 1) {
			foreach (string entry in currentListLayout) {
				AddNewListItem(entry);
			}
		}

		//Standard procedure for activating/deactivating list items
		else {
			//Deactivate all list items before checking which list items should be active
			foreach (KeyValuePair<string, ObstacleListItem> entry in obstacleList) {
				entry.Value.GetToggle().isOn = false;
				entry.Value.gameObject.SetActive(false);
			}

			for (int i = 0; i < currentListLayout.Count; i++) {

				//Activate all existing list items that represent obstacles available on selected segment
				string entry = currentListLayout[i];
				if (obstacleList.ContainsKey(entry)) {
					obstacleList[entry].gameObject.SetActive(true);
					if (entry == currentObstacleType)
						obstacleList[entry].GetToggle().isOn = true;
				}
				//If any not-previously-encountered obstacles are available, make new list items for them
				else { AddNewListItem(entry); }

				//Navigation bindings
				int prevIndex, nextIndex;
				if (i == 0) prevIndex = currentListLayout.Count - 1;
				else prevIndex = i - 1;
				if (i == currentListLayout.Count - 1) nextIndex = 0;
				else nextIndex = i + 1;

				//Updating navigation bindings so up-down takes you to the previous/next active list item
				string prevObstacleKey = currentListLayout[prevIndex];
				string nextObstacleKey = currentListLayout[nextIndex];

				if (obstacleList.TryGetValue(prevObstacleKey, out ObstacleListItem prevValue)
				 && obstacleList.TryGetValue(nextObstacleKey, out ObstacleListItem nextValue)) {
					Toggle prevItem = prevValue.GetToggle();
					Toggle nextItem = nextValue.GetToggle();
					obstacleList[entry].SetUpDownNav(prevItem, nextItem);
					Debug.LogWarning("Found obstacles " + prevObstacleKey + " and " + nextObstacleKey);

				} else {
					Debug.LogWarning(
						" missing obstacle " + prevObstacleKey + ": " + obstacleList.ContainsKey(prevObstacleKey)
						 + " or " + nextObstacleKey + ": " + obstacleList.ContainsKey(nextObstacleKey) + " on segment \""
						+ currentSegment.gameObject.name + "\""
						+ " in " + currentSegment.transform.parent.parent.parent.gameObject.name
						+ " in " + currentSegment.transform.parent.parent.parent.parent.gameObject.name
					);
					Debug.Log("listing obstacle entries");
					foreach (var item in obstacleList) {
						Debug.Log("obstacle entry: " + item);
					}
					Debug.Log("no more obstacle entries");
					Debug.Log("listing current list layout");
					foreach (var item in currentListLayout) {
						Debug.Log("list entry: " + item);
					}
					Debug.Log("no more list entries");

					return;
				}


				//Updating navigation bindings so left-right always takes you to current segment list item
				Toggle currentSegmentItem = SegmentListScript.ReadCurrentItem().GetToggle();
				obstacleList[entry].SetLeftRightNav(currentSegmentItem, currentSegmentItem);
			}
		}
		// */


		ApplyObstacleSelection();
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
		SetListItem(newItemObj, obstacleName);
	}

	private void SetListItem(ObstacleListItem item, string obstacleName) {
		item.SetName(obstacleName);
		if (obstacleName == currentObstacleType)
			item.GetToggle().isOn = true;
	}

	public void ApplyObstacleSelection() {
		//UnityEngine.Debug.Log(SegmentListScript.ReadCurrentItem().GetText().text + " ApplyObstacle: " + currentObstacleType);
		currentSegment.Obstacles.UnhideObject(currentObstacleType);
	}

	//Showing a specific obstacle without actually recording it, used to avoid problems with
	//non-user initiated toggle-offs
	public void DisplayObstacle(string p_name) {
		currentSegment.Obstacles.UnhideObject(p_name);
	}
}
