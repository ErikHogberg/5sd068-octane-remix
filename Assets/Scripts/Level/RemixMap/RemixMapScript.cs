using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemixMapScript : MonoBehaviour {

	private static RemixMapScript mainInstance;

	public Camera MapCamera;
	public GameObject CameraPivot;

	public GameObject ButtonPrefab;
	public RawImage MapImage;

	public float CameraMouseSpeed = 1f;

	private bool mouseDown = false;
	private float mouseXBuffer = 0;

	private void Awake() {
		mainInstance = this;
	}

	private void OnDestroy() {
		mainInstance = null;
	}

	void Start() {

		// TODO: 3d mouse picking to select segment
		// TODO: selection highlight
		// IDEA: right click drag to rotate map
		// TODO: only enable segment mouse picking in remix edit mode
		// TODO: mouse pick-able boxes between segments for selecting transition, for adding portals, etc.
		// TODO: figure out more mechanics than portals that 
		// TODO: save remix		
		// TODO: load remix		

		// foreach (var piece in LevelPieceSuperClass.Pieces) {

		// 	Button button = Instantiate(
		// 		ButtonPrefab,
		// 		// MapCamera.transform.position - piece.transform.position,
		// 		// Quaternion.identity,
		// 		MapImage.transform
		// 	).GetComponent<Button>();

		// 	button.transform.localPosition = (piece.transform.position - MapCamera.transform.position) * MapImage.rectTransform.localScale.x;


		// 	// TODO: 2D pos, check if z is correct



		// }

	}

	private void Update() {

		if (Input.GetMouseButtonUp(1))
			mouseDown = false;

		if (mouseDown) {

			float mouseX = Input.mousePosition.x;
			float delta = mouseX - mouseXBuffer;

			CameraPivot.transform.Rotate(Vector3.up, delta * CameraMouseSpeed, Space.World);

			mouseXBuffer = mouseX;

		}

	}

	public static void StartRotate() {
		if (!mainInstance)
			return;

		mainInstance.mouseDown = true;
		mainInstance.mouseXBuffer = Input.mousePosition.x;

		// TODO: rotate camera
	}

	public static void SelectSegment(LevelPieceSuperClass segment) {
		if (!mainInstance)
			return;

		SegmentEditorSuperClass.SetSegmentsOnAll(segment);
		
		// TODO: populate menu, assign current segment reference for assignment
	}

}
