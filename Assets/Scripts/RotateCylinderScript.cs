using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCylinderScript : MonoBehaviour
{
    public float speed;
    public Vector3 Axis;

   

    // Update is called once per frame
    void Update()
    {
        transform.rotation *= Quaternion.Euler(Axis * speed * Time.deltaTime);

    }
}
