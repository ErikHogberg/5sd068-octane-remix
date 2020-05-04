using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PortalScript))]
public class GoalPortalScript : MonoBehaviour, IObserver<PortalScript> {

	private void Start() {
		GetComponent<PortalScript>().Observers.Add(this);
	}

	void Update() {

	}

	public void Notify(PortalScript portal){
		// TODO: add lap
	}

}
