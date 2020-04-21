using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemixMapScript : MonoBehaviour {

	public Camera MapCamera;

	public GameObject ButtonPrefab;
	public RawImage MapImage;

	void Start() {

		foreach (var piece in LevelPieceSuperClass.Pieces) {

			Button button = Instantiate(
				ButtonPrefab,
				// MapCamera.transform.position - piece.transform.position,
				// Quaternion.identity,
				MapImage.transform
			).GetComponent<Button>();
			
			button.transform.localPosition = (piece.transform.position - MapCamera.transform.position) * MapImage.rectTransform.localScale.x;
			

			// TODO: 2D pos, check if z is correct



		}

	}

}
