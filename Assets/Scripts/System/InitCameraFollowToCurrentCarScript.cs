using UnityEngine;

public class InitCameraFollowToCurrentCarScript : MonoBehaviour {

	void Start() {
		foreach (var item in GetComponents<IFollowScript>())
			item.SetFollowTarget(SteeringScript.MainInstance.transform);
	}

}
