using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timescaleScript : MonoBehaviour
{
    
    public float Timescale = 1f;
    
    void Start()
    {
        Time.timeScale = Timescale;
    }

}
