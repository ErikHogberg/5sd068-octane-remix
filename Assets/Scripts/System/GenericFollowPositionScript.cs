using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericFollowPositionScript : MonoBehaviour {

	public GameObject ObjectToFollow;

	private Vector3 initPos;

	private void Start() {
		initPos = transform.position - ObjectToFollow.transform.position;
	}

	private void LateUpdate() {
		transform.position = ObjectToFollow.transform.position + initPos;
	}

}
