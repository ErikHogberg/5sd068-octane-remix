using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLayerRenderDistanceScript : MonoBehaviour {

	[Serializable]
	public struct LayerRenderDistancePair {
		public string LayerName;
		public float RenderDistance;
	}

	public LayerRenderDistancePair[] LayerSettings;

	public bool PrintDebug = false;

	void Start() {
		Camera camera = GetComponent<Camera>();

		float[] distances = new float[32];

		foreach (var item in LayerSettings) {
			int layerIndex = LayerMask.NameToLayer(item.LayerName);
			if (layerIndex < 0) {
				if (PrintDebug)
					Debug.Log("[Render distance] Layer " + item.LayerName + " not found ");

				continue;
			}

			distances[layerIndex] = item.RenderDistance;
			if (PrintDebug)
				Debug.Log("Successfully set render distance of layer " + item.LayerName + " to " + item.RenderDistance);
		}

		camera.layerCullDistances = distances;
	}

}
