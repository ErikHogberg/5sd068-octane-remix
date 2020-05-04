using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//A script to allow any instantiated UI elements to find the scene's canvas
[RequireComponent(typeof(Canvas))]
public class CanvasFinder : MonoBehaviour
{
	public static Canvas thisCanvas;

	void Awake() { thisCanvas = GetComponent<Canvas>(); }
}
