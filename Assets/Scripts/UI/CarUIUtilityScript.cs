using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarUIUtilityScript : MonoBehaviour {

	public void FreezeCurrentCar() {
		SteeringScript.FreezeCurrentCar();
	}

	public void UnfreezeCurrentCar() {
		SteeringScript.UnfreezeCurrentCar();
	}

}
