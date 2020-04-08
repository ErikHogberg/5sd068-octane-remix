using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class BarScript : MonoBehaviour {

	private Image barFill;

	void Start() {
		barFill = GetComponent<Image>();
	}

	public void SetBarPercentage(float percentage) {
		if (percentage > 1)
			percentage = 1;

		if (percentage < 0)
			percentage = 0;


		Vector3 scale = transform.localScale;
		scale.x = percentage;
		transform.localScale = scale;
	}

	public void SetColor(Color color) {
		barFill.color = color;
	}

}
