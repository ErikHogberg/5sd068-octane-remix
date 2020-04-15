using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserWheelControls : MonoBehaviour
{
	private GameObject Laser1;
	private GameObject Laser2;
	private GameObject Laser3;
	private GameObject Laser4;

	private bool active1 = true;

	void Start()
	{
		Laser1 = transform.Find("Laser1").gameObject;
		Laser2 = transform.Find("Laser2").gameObject;
		Laser3 = transform.Find("Laser3").gameObject;
		Laser4 = transform.Find("Laser4").gameObject;

	}

	public void LogHit()
    {
		Debug.Log("Laser Hit!");
    }
}
