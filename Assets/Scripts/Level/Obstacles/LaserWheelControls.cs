using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserWheelControls : MonoBehaviour
{
	public enum RotationDimension {
		xAxis = 0,
		yAxis,
		zAxis
	}

	/*private GameObject Laser1;
	private GameObject Laser2;
	private GameObject Laser3;
	private GameObject Laser4;*/

	[Header("Lasers")]
	[Tooltip("Game objects containing everything for making laser beams visible")]
	public List<GameObject> LaserBeams;

	[Tooltip("Game objects containing colliders for laser beams")]
	public List<GameObject> LaserColliders;

	[Tooltip("Determines whether the lasers are visble and collidable. To change during runtime, use function with this same name.")]
	public bool lasersActive = true;


	[Header("Wheel Rotation")]
	[Tooltip("Determines whether the laser wheel is spinning")]
	public bool rotationActive = true;

	[Tooltip("Around which axis the wheel rotates")]
	public RotationDimension rotationDimension = RotationDimension.xAxis;

	[Tooltip("Use the world axes when rotating wheel, as opposed to local axes")]
	public bool useWorldSpace = true;

	[Tooltip("The speed at which the laser wheel rotates")]
	public float rotationSpeed = 10.0f;

	void Start()
	{
		/*Laser1 = transform.Find("Laser1").gameObject;
		Laser2 = transform.Find("Laser2").gameObject;
		Laser3 = transform.Find("Laser3").gameObject;
		Laser4 = transform.Find("Laser4").gameObject;*/

		if (LaserBeams.Count > 0 && LaserColliders.Count > 0) {
			if (lasersActive) {
				foreach (GameObject item in LaserBeams) {
					item.SetActive(true);
				}
				foreach (GameObject item in LaserColliders) {
					item.SetActive(true);
				}
			}
			else {
				foreach (GameObject item in LaserBeams) {
					item.SetActive(false);
				}
				foreach (GameObject item in LaserColliders) {
					item.SetActive(false);
				}
			}
		} else { Debug.Log("LaserWheelControls: No items in either LaserBeams list or LaserColliders list"); }
	}

	public void LogHit()
    {
		Debug.Log("Laser Hit!");
    }

	public void FixedUpdate()
    {
		if (rotationActive)
		{
			float rotate = rotationSpeed * Time.deltaTime;
			if (useWorldSpace)
			{
				if (rotationDimension == RotationDimension.xAxis)
					transform.Rotate(rotate, 0f, 0f, Space.World);
				else if (rotationDimension == RotationDimension.yAxis)
					transform.Rotate(0f, rotate, 0f, Space.World);
				else
					transform.Rotate(0f, 0f, rotate, Space.World);
			}
			else
			{
				if (rotationDimension == RotationDimension.xAxis)
					transform.Rotate(rotate, 0f, 0f, Space.Self);
				else if (rotationDimension == RotationDimension.yAxis)
					transform.Rotate(0f, rotate, 0f, Space.Self);
				else
					transform.Rotate(0f, 0f, rotate, Space.Self);
			}
		}
	}

	public void LasersActive(bool toggle)
    {
		if (LaserBeams.Count > 0 && LaserColliders.Count > 0) {
			foreach (GameObject item in LaserBeams)
			{
				item.SetActive(toggle);
			}
			foreach (GameObject item in LaserColliders)
			{
				item.SetActive(toggle);
			}
		} else { Debug.Log("LaserWheelControls: No items in either LaserBeams list or LaserColliders list"); }
	}

}
