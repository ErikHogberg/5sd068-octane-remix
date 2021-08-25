using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowFPSFollowScript : MonoBehaviour {
	public GameObject Target;

    public float FPS = 12f;

    private float timer = -1f;

	void LateUpdate() {

        transform.position = Target.transform.position;

        if(timer < 0){
            timer += 1f/FPS;

            transform.rotation = Target.transform.rotation;

        }
        timer -= Time.deltaTime;

	}
}
