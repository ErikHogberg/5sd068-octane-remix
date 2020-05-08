using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gradient3Colors : MonoBehaviour
{
	public Color color1;
	public Color color2;
	public Color color3;

	private Color currColor;

	public Color ColorFromPercent(float percent)
    {
		float rA, gA, bA;
		float rB, gB, bB;
		float rDiff, gDiff, bDiff;
		float rFinal, gFinal, bFinal;
		float effectivePercent;

		if (percent < 0.5f) {
			rA = color1.r; gA = color1.g; bA = color1.b;
			rB = color2.r; gB = color2.g; bB = color2.b;
			effectivePercent = percent / 0.5f;
		}
		else {
			rA = color2.r; gA = color2.g; bA = color2.b;
			rB = color3.r; gB = color3.g; bB = color3.b;
			effectivePercent = (percent - 0.5f) / 0.5f;
		}
		rDiff = rB - rA; gDiff = gB - gA; bDiff = bB - bA;

		rFinal = rA + (rDiff * effectivePercent);
		gFinal = gA + (gDiff * effectivePercent);
		bFinal = bA + (bDiff * effectivePercent);
		Color finalColor = new Color(rFinal, gFinal, bFinal);
		currColor = finalColor;
		return finalColor;
	} 
}
