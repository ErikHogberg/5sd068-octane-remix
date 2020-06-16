using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeToBlack : MonoBehaviour
{
	private static FadeToBlack instance;
	public static FadeToBlack Instance => instance ?? (instance = Instantiate(Resources.Load<FadeToBlack>("FadeToBlack")));

	//A black image that should be large enough to cover the entire screen
	private Image blackScreen;
	private GameObject blackScreenObj;

	//The time that the fade should take to complete
	private float fadeTime = 2.0f;
	private float timer = 0.0f;

	private bool fading = false;
	private bool toBlack = true;

	//These variables are mostly to save on characters and make the code more readable
	private Color cFull;
	private Color cEmpty;

	private void MakeBlackScreen() {
		blackScreenObj = Instantiate(Resources.Load<GameObject>("BlackScreen"));
		blackScreenObj.transform.SetParent(CanvasFinder.thisCanvas.transform, false);
		blackScreen = blackScreenObj.GetComponent<Image>();
		cFull = new Color(blackScreen.color.r, blackScreen.color.g, blackScreen.color.b, 1f);
		cEmpty = new Color(blackScreen.color.r, blackScreen.color.g, blackScreen.color.b, 0f);
	}

	//To instantly set the black screen as fully opaque or fully transparent
	public void SetFull(bool p_toBlack)
    {
		if (blackScreen == null) MakeBlackScreen();
		if (p_toBlack == true)
			blackScreen.color = cFull;
		else blackScreen.color = cEmpty;
	}

	//Start a fade, if p_toBlack == true, it will transition from transparent to black, 
	//and from black to transparent if p_toBlack == false
	public void StartFadeToBlack(bool p_toBlack, float p_fadeTime)
    {
		if (!fading) {
			if (blackScreen == null) MakeBlackScreen();

			if (p_toBlack) toBlack = true;
			else toBlack = false;

			fading = true;
			fadeTime = p_fadeTime;
			timer = fadeTime;
		} else { Debug.Log("FadeToBlack: Is already fading!"); }
	}

	void Update()
    {
		if (fading && blackScreen != null) {
			if (timer > 0f) {
				timer -= Time.deltaTime;
				float percent = (timer / fadeTime);

				if (toBlack) {
					blackScreen.color = new Color(cFull.r, cFull.g, cFull.b, 1f - percent);
				} else {
					blackScreen.color = new Color(cEmpty.r, cEmpty.g, cEmpty.b, percent);
				}
			} else {
				fading = false;
				timer = 0.0f;
            }
        }
    }
	//If other scripts want to check if a fade is currently happening
	public bool IsFading() { return fading; }
}
