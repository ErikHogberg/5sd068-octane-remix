﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(ToggleGroup))]
public class ObstacleListScript : MonoBehaviour {

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

	public GameObject ParentToHide;

	// private ObstacleListItem listItemPrefab;
	public ObstacleListItem ListItemTemplate;

	void Awake() {
		group = GetComponent<ToggleGroup>();
		// listItemPrefab = Resources.Load<ObstacleListItem>("ObstacleListItem");
		mainInstance = this;
	}

	// void OnDisable() { obstacleList.Clear(); }

	void Start() {
		ListItemTemplate.SetListReference(this);
		ListItemTemplate.SetToggleGroup(group);
		obstacleList.Add(ListItemTemplate);

		//So the obstacle list can register itself as a SegmentEditor in Awake() before first segment selection occurs
		if (SegmentListScript.listItems.Count > 0) {
			SegmentListScript.InitializeSegmentSelection(SegmentListScript.listItems[0]);
		}
		UpdateUI();
	}

	public static void Show(bool skipUpdate = false) {
		if (!mainInstance)
			return;

		GoalPostMenuScript.Hide();
		mainInstance.ParentToHide.SetActive(true);
		if (!skipUpdate) {
			mainInstance.UpdateUI();
		}
	}

	public static void Hide() {
		if (!mainInstance)
			return;

		mainInstance.ParentToHide.SetActive(false);
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
	public void UpdateUI() {

		currentListLayout.Clear();

		//For the currently selected segment, which obstacles are available?
		foreach (var item in LevelPieceSuperClass.CurrentSegment.Obstacles.objects) {
			currentListLayout.Add(item.Key);
		}

		//Fetch the recorded selected obstacle for the currently selected segment list item
		currentObstacleType = SegmentListScript.ReadCurrentItem().GetObstacle();

		int diff = currentListLayout.Count - obstacleList.Count;
		if (currentListLayout.Count > obstacleList.Count) {
			for (int i = 0; i < diff; i++) {
				AddNewListItem();
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

	public static void UpdateUIStatic(){
		mainInstance?.UpdateUI();
	}

	private void AddNewListItem() {
		// ObstacleListItem newItemObj = Instantiate(Resources.Load<ObstacleListItem>("ObstacleListItem"));
		ObstacleListItem newItemObj = Instantiate(ListItemTemplate);
		obstacleList.Add(newItemObj);
		newItemObj.transform.SetParent(gameObject.transform);
		newItemObj.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;

		newItemObj.SetListReference(this);
		newItemObj.SetToggleGroup(group);
	}

	private void SetListItem(ObstacleListItem item, string obstacleName) {
		item.SetName(obstacleName);
		item.ItemToggle.isOn = obstacleName == currentObstacleType;
	}

	public void ApplyObstacleSelection() {
		//UnityEngine.Debug.Log(SegmentListScript.ReadCurrentItem().GetText().text + " ApplyObstacle: " + currentObstacleType);
		LevelPieceSuperClass.CurrentSegment.Obstacles.UnhideObject(currentObstacleType);
	}

	//Showing a specific obstacle without actually recording it, used to avoid problems with
	//non-user initiated toggle-offs
	public void ApplyObstacleSelection(string name) {
		LevelPieceSuperClass.CurrentSegment.Obstacles.UnhideObject(name);
	}
}
