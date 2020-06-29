using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreNotificationUIScript : MonoBehaviour {

	public static ScoreNotificationUIScript MainInstance = null;

	public ScoreNotificationInstanceUIScript NotificationTemplate;

	[Space]
	public Color DriftColor;
	public Color AirTimeColor;
	public Color BoostColor;
	public Color DodgeColor;

	private List<ScoreNotificationInstanceUIScript> Notifications = new List<ScoreNotificationInstanceUIScript>();

	void Start() {
		Notifications.Add(NotificationTemplate);
		NotificationTemplate.gameObject.SetActive(false);

		MainInstance = this;
	}

	public void Notify(ScoreSkill type, long score) {
		Color color;
		string message;
		switch (type) {
			case ScoreSkill.AIRTIME:
				message = "Air time:\t +" + score.ToString("0F");
				color = AirTimeColor;
				break;
			case ScoreSkill.BOOST:
				message = "Boost:\t +" + score.ToString("0F");
				color = BoostColor;
				break;
			case ScoreSkill.DODGE:
				message = "Dodge:\t +" + score.ToString("0F");
				color = DodgeColor;
				break;
			case ScoreSkill.DRIFT:
			default:
				message = "Drift:\t +" + score.ToString("0F");
				color = DriftColor;
				break;
		}

		foreach (var item in Notifications) {
			if (!item.running) {
				item.Show(transform.position, message, color);
				return;
			}
		}

		var newNotification = Instantiate(NotificationTemplate, transform).GetComponent<ScoreNotificationInstanceUIScript>();
		Notifications.Add(newNotification);
		newNotification.Show(transform.position, message, color);

	}

}
