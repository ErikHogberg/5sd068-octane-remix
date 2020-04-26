using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ObjectSelectorScript))]
public abstract class LevelPieceSuperClass : MonoBehaviour {

	public static List<LevelPieceSuperClass> Pieces = new List<LevelPieceSuperClass>();

	// TODO: comfortable way to reference obstacles avalable for placement
	// IDEA: obstacle registry, similar to list of sound effects with settings

	// IDEA: empty level segment type for optional spots for adding roads
	// IDEA: dynamic list of segment editing fields, only show the settings allowed for specific class, pushing fields from script every update

	// IDEA: option to disallow placing any obstacles on segment

	// IDEA: ability select multiple segments, shift click? show blank/custom message if same setting is different for some objects selected
	// IDEA: ability to group segments together, selecting and altering all segments at the same time
	// IDEA: when selecting multiple: list all avaliable settings, set them for only the segments that the settings can be applied for

	// [HideInInspector]
	public ObjectSelectorScript Obstacles { get; private set; }

	private void Awake() {
		Pieces.Add(this);
		Obstacles = GetComponent<ObjectSelectorScript>();

		Obstacles.UnhideObject("");
	}

	private void OnMouseOver() {

		if (EventSystem.current.IsPointerOverGameObject()) 	
			return;
		
		// print("clicked " + gameObject.name);

		if (Input.GetMouseButtonDown(0)) {
			// print("left click");
			RemixMapScript.SelectSegment(this);
		} 
		if (Input.GetMouseButtonDown(1)) {
			// print("right click");
			RemixMapScript.StartRotate();
		}
	}


}
