using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DebugUIScript : MonoBehaviour {

	public static DebugUIScript MainInstance;

	public List<TMP_Text> TextBoxes;

	private void Start() {
		MainInstance = this;
	}

	public void SetText(IEnumerable<string> text) {
		if(text.Count() != TextBoxes.Count) {
			return;
		}

		foreach ((TMP_Text box, string textValue) in TextBoxes.Zip(text, (b, t) => (b,t))) {
			box.text = textValue;
		}
	}

	public void SetText(string text, int index) {
		TextBoxes[index].text = text;
	}

}
