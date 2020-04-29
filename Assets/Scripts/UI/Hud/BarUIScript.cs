using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class BarUIScript : MonoBehaviour {

	private Image barFill;

	void Start() {
		barFill = GetComponent<Image>();
		if (barFill.sprite == null) {
			Debug.Log("BarUIScript: " + transform.parent.name + " has no assigned bar sprite.");
		} else {
			barFill.type = Image.Type.Filled;
			barFill.fillMethod = Image.FillMethod.Horizontal;
			barFill.fillOrigin = (int)Image.OriginHorizontal.Left;
		}
	}

	public float GetFillAmount() { return barFill.fillAmount; }

	public void SetBarPercentage(float percentage) {
		if (percentage > 1)
			percentage = 1;

		if (percentage < 0)
			percentage = 0;

		if (barFill.sprite == null) {
			Vector3 scale = transform.localScale;
			scale.x = percentage;
			transform.localScale = scale;
		} else {
			barFill.fillAmount = percentage;
		}
	}

	public void SetColor(Color color) {
		barFill.color = color;
	}

}
