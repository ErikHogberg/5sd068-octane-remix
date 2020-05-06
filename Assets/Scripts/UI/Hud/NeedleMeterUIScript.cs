﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
public class NeedleMeterUIScript : MonoBehaviour {

	public enum ColorState {
		NORMAL = 0,
		MAX,
		BOOST
	};

	private Image needle;
	private float initRotation;
	private Quaternion desiredRotation;

	private float targetPercent = 0.0f;
	private ColorState colorState = ColorState.NORMAL;
	// private Color targetColor = Color.white;
	private Color normalColor;

	[Tooltip("How many degrees the needle has turned when at max speed, clockwise")]
	[Range(0, 360)]
	public float MaxRotation = 110;

	[Tooltip("How fast the needle can fluctuate")]
	public float NeedleSpeed = 5.0f;

	public Color MaxColor = Color.red;
	public Color BoostColor = new Color(20.0f / 255.0f, 75.0f / 255.0f, 215.0f / 255.0f, 1);

	private void Awake() {
		needle = GetComponent<Image>();
		normalColor = needle.color;
		initRotation = transform.rotation.eulerAngles.z;
	}


	public void SetBarPercentage() {

		targetPercent = Mathf.Clamp(targetPercent, 0.0f, 1.1f);

		desiredRotation = Quaternion.Euler(0, 0, initRotation - MaxRotation * targetPercent);
		transform.localRotation = Quaternion.Slerp(transform.localRotation, desiredRotation, NeedleSpeed * Time.fixedDeltaTime);

	}

	public void ApplyColor() {
		if (colorState == ColorState.NORMAL) {
			needle.color = normalColor;//Color.white;
		} else if (colorState == ColorState.MAX) {
			needle.color = MaxColor;
		} else if (colorState == ColorState.BOOST) {
			needle.color = BoostColor;
		} else {
			needle.color = Color.grey;
		}
	}

	public void SetTargetPercent(float target) {
		targetPercent = target;
	}

	public void SetTargetColor(ColorState color) {
		colorState = color;
	}
}
