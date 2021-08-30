using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class CenterlineMinimapScript : MonoBehaviour {

	public RenderTexture renderTexture;

	void Start() {

	}

	void Update() {

	}

	public void UpdateRenderTexture(Quaternion dir, IEnumerable<Vector3> points) {

		if (!renderTexture) return;


		RenderTexture.active = renderTexture;
		// texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		// for (int i = 0; i < renderTexture.width * .2f; i++)
		// 	for (int j = 0; j < renderTexture.height; j++) {
		// 		texture.SetPixel((at + i), j, new Color(1, 0, 0));
		// 	}
		// texture.Apply();
		// RenderTexture.active = null;
		// renderTexture.set

		Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, DefaultFormat.HDR, TextureCreationFlags.None);

		Vector3 lastPoint = Vector3.zero;
		bool first = true;
		foreach (var point in points) {
			if (first) {
				first = false;
				lastPoint = dir * point;
				continue;
			}

			Vector3 newPoint = dir * point;

			for (int i = 0; i < 10; i++) {
				Vector3 pos = Vector3.Lerp(lastPoint, newPoint, 10f / i);
				Vector2 projectedPos = Vector3.ProjectOnPlane(pos, Vector3.up);
				texture.SetPixel((int)projectedPos.x, (int)projectedPos.y, Color.white);
			}
			texture.Apply();

			lastPoint = newPoint;
		}


	}

}
