using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
public class NeedleMeterUIScript : MonoBehaviour {
	private Image needle;
	private float initRotation;

	[Tooltip("How many degrees the needle has turned when at max speed, clockwise")]
	[Range(0, 360)]
	public float MaxRotation = 90;

	void Start() {
		needle = GetComponent<Image>();
		initRotation = transform.rotation.eulerAngles.z;
		print("init rot: " + initRotation);
	}

	public void SetBarPercentage(float percentage) {
		// if (percentage > 1)
		// percentage = 1;

		if (percentage < 0)
			percentage = 0;

		transform.localRotation = Quaternion.Euler(0, 0, initRotation - MaxRotation * percentage);
	}

	public void SetColor(Color color) {
		needle.color = color;
	}
}
