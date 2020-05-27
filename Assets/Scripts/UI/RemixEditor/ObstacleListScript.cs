using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(ToggleGroup))]
public class ObstacleListScript : SegmentEditorSuperClass
{
	public static Dictionary<string, ObstacleListItem> obstacleList = new Dictionary<string, ObstacleListItem>();
	private List<string> currentListLayout = new List<string>();
	private ToggleGroup group = null;

	private string obstacleNull = "None";
	private string currentObstacleType = "None";

	protected override void ChildAwake() { group = GetComponent<ToggleGroup>(); }

	void Start() {
		SegmentListScript.InitializeSegmentSelection(SegmentListScript.listItems[0]);
	}

	//Sent from ObstacleListItems, triggered by event 
	public void ReceiveTogglePing(string p_name, bool is_on) {
		if (is_on) currentObstacleType = p_name;
		if (!is_on && currentObstacleType == p_name)
			currentObstacleType = obstacleNull;
		ApplyObstacleSelection();
	}

	public override void UpdateUI() 
	{
		var shownObject = currentSegment.Obstacles.ShownObject;
		currentListLayout.Clear();
		bool noneCheck = true;

		//For the currently selected segment, which obstacles are available?
		for (int i = 0; i < currentSegment.Obstacles.objects.Count; i++) {
			var item = currentSegment.Obstacles.objects[i];
			currentListLayout.Add(item.Key);
			if (shownObject != null && shownObject.Key == item.Key) {
				currentObstacleType = item.Key;
				noneCheck = false;
			}
		} if (noneCheck) currentObstacleType = obstacleNull;

		//Creates list items based on all entries in CurrentListLayout
		//Should only be true the first time UpdateUI is run
		if (obstacleList.Count < 1) {
			foreach (string entry in currentListLayout) {
				NewListItem(entry);
			}
		}
		//Standard procedure for activating/deactivating list items
		else {
			//Deactivate all list items before checking which list items should be active
			foreach (KeyValuePair<string, ObstacleListItem> entry in obstacleList) {
				entry.Value.GetToggle().isOn = false;
				entry.Value.gameObject.SetActive(false);
            }
			foreach (string entry in currentListLayout) {
				//Activate all existing list items that represent obstacles available on selected segment
				if (obstacleList.ContainsKey(entry)) {
					obstacleList[entry].gameObject.SetActive(true);
					if (entry == currentObstacleType)
						obstacleList[entry].GetToggle().isOn = true;
				} 
				//If any not-previously-encountered obstacles are available, make new list items for them
				else {
					NewListItem(entry);
                }
			}
		}
		ApplyObstacleSelection();
	}

	private void NewListItem(string p_name) {
		ObstacleListItem newItemObj = Instantiate(Resources.Load<ObstacleListItem>("ObstacleListItem"));
		obstacleList.Add(p_name, newItemObj);
		newItemObj.transform.SetParent(gameObject.transform);
		newItemObj.gameObject.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);

		newItemObj.SetName(p_name);
		newItemObj.SetListReference(this);
		newItemObj.SetToggleGroup(group);
		if (p_name == currentObstacleType)
			newItemObj.GetToggle().isOn = true;
	}

	public void ApplySelection(string id) {
		currentSegment.Obstacles.UnhideObject(id);
	}

	public void ApplyObstacleSelection() {
		ApplySelection(currentObstacleType);
	}
}
