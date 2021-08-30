using UnityEngine;
using UnityEngine.EventSystems;

public class GameOverPanel : MonoBehaviour, IObserver<int> {

	// NOTE: temporary, max laps setting will probably be moved somewhere else once other scripts start using it
	public int MaxLaps = 3;

	private static GameOverPanel mainInstance;

	private void Awake() {
		mainInstance = this;
	}

	public static void SetCar() {
		if (mainInstance)
			SteeringScript.MainInstance.LapCompletedObservers.Add(mainInstance);
	}

	void Start() {
		// SteeringScript.MainInstance.LapCompletedObservers.Add(this);
		// Debug.LogWarning("game over steering: " + SteeringScript.MainInstance.gameObject.name);
		gameObject.SetActive(false);
	}

	// bool init = false;
	// private void Update() {
	// 	if (init) {
	// 		return;
	// 	}

	// 	SteeringScript.MainInstance.LapCompletedObservers.Add(this);
	// 	init = true;		
	// }

	public void Notify(int lapsCompleted) {
		if (lapsCompleted == MaxLaps) {
			// TODO: disable car controls
			TimerScript.Instance.StopTimer();
			ScoreManager.Board(0).StopScoreCollecting();

			HighscoreManager.List.Insert(
				NameInputInputScript.GetPlayerName(), // "No Name",
				LevelPieceSuperClass.GetRemixString(),
				ScoreManager.GetGrandTotalScore(0),
				TimerScript.Instance.GetTimeNr(),
				CharacterSelection.GetPick(0)
			);
			gameObject.SetActive(true);
			EventSystem.current.SetSelectedGameObject(gameObject.transform.GetChild(2).gameObject);
		}
		
	}

}
