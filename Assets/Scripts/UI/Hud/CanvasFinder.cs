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

	void OnEnable() {
		AssignCanvas();
    }
	void OnDisable() {
		thisCanvas = null;
		Debug.Log("CanvasFinder: " + gameObject.name + " is not thisCanvas anymore, thisCanvas is now null.");
	}

	public void AssignCanvas()
    {
		thisCanvas = GetComponent<Canvas>();
		//Debug.Log(gameObject.name + " is thisCanvas");
	}

	public static void SetThisCanvas(Canvas p_canvas)
    {
		thisCanvas = p_canvas;
		//Debug.Log(p_canvas.gameObject.name + " is set to thisCanvas");
	}
}
