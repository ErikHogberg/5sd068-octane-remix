using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceWeatherDetectionScript : MonoBehaviour
{
    // private void OnCollisionEnter(Collision other) {
    //     print("enter tag: " + other.gameObject.tag);
    // }

    // private void OnCollisionStay(Collision other) {
    //     print("stay tag: " + other.gameObject.tag);
        
    // }

    private void OnTriggerEnter(Collider other) {
        print("trigger enter tag: " + other.gameObject.tag);
    }

    // private void OnTriggerStay(Collider other) {
        // print("trigger stay tag: " + other.gameObject.tag);
    // }
}
