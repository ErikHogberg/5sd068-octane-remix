using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class BarUIScript : MonoBehaviour {

	private Image barFill;
	private MaskedBarFunctionality maskedBar;
	private Gradient3Colors gradient;

	void Awake() {
		barFill = GetComponent<Image>();
		maskedBar = GetComponent<MaskedBarFunctionality>();
		gradient = GetComponent<Gradient3Colors>();

		if (barFill.sprite == null) {
			Debug.Log("BarUIScript: " + transform.parent.name + " has no assigned bar sprite.");
		} else {
			barFill.type = Image.Type.Filled;
			barFill.fillMethod = Image.FillMethod.Horizontal;
			barFill.fillOrigin = (int)Image.OriginHorizontal.Left;
		}
	}

	public float GetFillAmount() {
		if (maskedBar == null) return barFill.fillAmount;
		else return maskedBar.GetPercent();
	}

	public void SetBarPercentage(float percentage) {
		percentage = Mathf.Clamp01(percentage);

		if (barFill.sprite == null) {
			Vector3 scale = transform.localScale;
			scale.x = percentage;
			transform.localScale = scale;
		} else {
			if (maskedBar == null) barFill.fillAmount = percentage;
			else maskedBar.SetPos(percentage);
		}
		if (gradient != null) SetColor(gradient.ColorFromPercent(percentage));
	}

	public void SetColor(Color color) {
		barFill.color = color;
	}

}
