using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TutorialDialogueUIScript : MonoBehaviour {

	[Serializable]
	public struct TutorialEntry {
		public string Text;
		[Tooltip("no sound if empty")]
		public string VoiceClip;
		public UnityEvent Events;
	}

	public static TutorialDialogueUIScript MainInstance;

	public TMP_Text text;
	public Button button;
	public TMP_Text buttonText;

	private TutorialEntry[] currentEntries;
	private UnityEvent closeEvents;

	private int currentIndex = 0;

	private void Awake() {
		MainInstance = this;
		button.onClick.AddListener(NextOrClose);
		Hide();
	}

	public void PopulateUI() {
		if (currentIndex >= currentEntries.Length)
			return;

		text.text = currentEntries[currentIndex].Text;
		if (currentIndex == currentEntries.Length - 1) {
			buttonText.text = "Close";
		} else {
			buttonText.text = "Next";
		}

		currentEntries[currentIndex].Events.Invoke();
		string voiceClip = currentEntries[currentIndex].VoiceClip;
		if (voiceClip != "") {
			SoundManager.PlaySound(voiceClip);
		}

	}

	public void NextOrClose() {
		if (currentIndex == currentEntries.Length - 1) {
			closeEvents.Invoke();
			Hide();
			return;
		}

		string oldVoiceClip = currentEntries[currentIndex].VoiceClip;
		if (oldVoiceClip != "") {
			// TODO: stop currently playing tutorial sound clip, so they dont overlap
			// SoundManager.
		}
		currentIndex++;
		PopulateUI();
	}

	public void Show(TutorialEntry[] entries, UnityEvent closeEvents) {
		if (!entries.Any())
			return;

		gameObject.SetActive(true);

		currentEntries = entries;
		currentIndex = 0;
		this.closeEvents = closeEvents;

		PopulateUI();
	}

	public void Hide() {
		gameObject.SetActive(false);
	}

}
