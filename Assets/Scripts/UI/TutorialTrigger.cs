using UnityEngine;
using UnityEngine.Events;

public class TutorialTrigger : MonoBehaviour {
	public TutorialDialogueUIScript.TutorialEntry[] TutorialEntries;
	[Header("Events when window closes")]
	public UnityEvent CloseEvents;

	private void OnTriggerEnter(Collider other) {
		TutorialDialogueUIScript.MainInstance?.Show(TutorialEntries, CloseEvents);
	}
}
