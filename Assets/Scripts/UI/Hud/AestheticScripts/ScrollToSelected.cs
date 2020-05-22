using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollToSelected : MonoBehaviour
{
	protected ScrollRect scrollRect;
	protected RectTransform contentPanel;

	private bool scrolling = false;
	private Vector2 goalPosition;
	private float scrollSpeed = 5f;
	private float accuracy = 0.05f;

	void Awake() { 
		scrollRect = GetComponent<ScrollRect>();
		contentPanel = scrollRect.content;
	}

	public void SnapTo(RectTransform target) {
		//Canvas.ForceUpdateCanvases(); 
		goalPosition =
			(Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
			- (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
		goalPosition.x = contentPanel.anchoredPosition.x;
		scrolling = true;
	}

	void Update()
    {
		if (scrolling) {
			contentPanel.anchoredPosition = 
				Vector2.Lerp(contentPanel.anchoredPosition, goalPosition, scrollSpeed * Time.deltaTime);

			//Check if goalPosition has been reached (closely enough)
			Vector2 pos = contentPanel.anchoredPosition;
			if (pos.x > goalPosition.x - accuracy && pos.x < goalPosition.x + accuracy) {
				if (pos.y > goalPosition.y - accuracy && pos.y < goalPosition.y + accuracy) {
					contentPanel.anchoredPosition = goalPosition;
					scrolling = false;
				}
			}
		}
    }
}
