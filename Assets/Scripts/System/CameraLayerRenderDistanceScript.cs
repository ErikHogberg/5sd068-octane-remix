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

	void Awake() {
		Camera camera = GetComponent<Camera>();

		float[] distances = new float[32];

		foreach (var item in LayerSettings) {
			int layerIndex = LayerMask.NameToLayer(item.LayerName);
			if (layerIndex < 0)
				continue;

			distances[layerIndex] = item.RenderDistance;
		}


		camera.layerCullDistances = distances;
	}

}
