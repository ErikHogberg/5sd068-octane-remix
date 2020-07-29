using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemixMenuCameraFocusScript : MonoBehaviour {

	private static List<RemixMenuCameraFocusScript> instances = new List<RemixMenuCameraFocusScript>();

	private float initZoom;
	private Vector3 initPos;
	public float Zoom;

	public Camera RemixMenuCamera;

	private static Transform lastTarget = null;

	private void Awake() {
		instances.Add(this);
		// remixMenuCamera = GetComponent<Camera>();
	}

	private void OnDestroy() {
		instances.Remove(this);
	}

	private void Start() {
		if (RemixMenuCamera.orthographic)
			initZoom = RemixMenuCamera.orthographicSize;
		else
			initZoom = transform.localPosition.magnitude;

		initPos = transform.position;
	}

	// TODO: interpolate movement

	private void ApplyZoom(float zoom) {
		if (RemixMenuCamera.orthographic) {
			RemixMenuCamera.orthographicSize = zoom;
			transform.position -= transform.forward * (RemixMenuCamera.farClipPlane - RemixMenuCamera.nearClipPlane) * .5f;
		} else {
			// var dir = Vector3.Normalize(transform.localPosition);
			// transform.localPosition = dir * zoom;
			// transform.position += transform.forward * zoom;
			// RemixMenuCamera.transform.localPosition = RemixMenuCamera.transform.localRotation.eulerAngles * zoom;
			// TODO: perspective zoom
		}

	}

	public static void SetTarget(Transform target) {
		
		// if (lastTarget != null && target == lastTarget) {
		// 	SetTarget();
		// 	return;
		// }

		lastTarget = target;

		// TODO: interpolate movement, and zoom
		foreach (var item in instances) {
			item.transform.position = target.position;
			item.ApplyZoom(item.Zoom);
		}
	}

	public static void SetTarget() {
		lastTarget = null;
		foreach (var item in instances) {
			item.transform.position = item.initPos;
			item.ApplyZoom(item.initZoom);
		}
	}

}
