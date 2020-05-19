using UnityEngine;

public class TutorialTrigger : MonoBehaviour {
	public TutorialDialogueUIScript.TutorialEntry[] TutorialEntries;

	private void OnTriggerEnter(Collider other) {
		TutorialDialogueUIScript.MainInstance?.Show(TutorialEntries);
	}
}
