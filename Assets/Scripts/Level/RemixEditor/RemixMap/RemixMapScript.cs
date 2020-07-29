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

	public ObjectSelectorScript TabSelector;

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

		// TODO: selection highlight
		// TODO: only enable segment mouse picking in remix edit mode

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
			// print("rotating "+ delta);
			CameraPivot.transform.Rotate(Vector3.up, delta * CameraMouseSpeed, Space.World);

			mouseXBuffer = mouseX;

		}

	}

	public static void StartRotate() {
		if (!mainInstance)
			return;

		RemixCameraRotateScript.StopStatic();

		mainInstance.mouseDown = true;
		mainInstance.mouseXBuffer = Input.mousePosition.x;

	}

	public static void Select(LevelPieceSuperClass segment) {
		if (!mainInstance) {
			UnityEngine.Debug.Log("RemixMap: No main instance");
			return;
		}

		RemixCameraRotateScript.StopStatic();

		mainInstance.TabSelector.UnhideObject("Obstacles");

		LevelPieceSuperClass.CurrentSegment = segment;

		ObstacleListScript.Show(skipUpdate: true);
		// SegmentEditorSuperClass.SetSegmentsOnAll(segment);
		ObstacleListScript.UpdateUIStatic();
		RemixMenuCameraFocusScript.SetTarget(segment.transform);

	}

	public static void Select(RemixEditorGoalPost goalPost) {
		if (!mainInstance) {
			UnityEngine.Debug.Log("RemixMap: No main instance");
			return;
		}

		RemixCameraRotateScript.StopStatic();

		mainInstance.TabSelector.UnhideObject("GoalPosts");

		// GoalPostMenuScript.Show();
		// SegmentEditorSuperClass.SetSegmentsOnAll(segment);
		GoalSpotListScript.MainInstance?.SetToggleObject(goalPost);
		RemixMenuCameraFocusScript.SetTarget(goalPost.transform);

	}

	public static void Select(RemixEditorToggleObject toggleObject) {
		if (!mainInstance) {
			UnityEngine.Debug.Log("RemixMap: No main instance");
			return;
		}

		RemixCameraRotateScript.StopStatic();

		mainInstance.TabSelector.UnhideObject("ToggleObjects");

		// SegmentEditorSuperClass.SetSegmentsOnAll(segment);
		RemixMenuCameraFocusScript.SetTarget(toggleObject.transform);

	}

}
