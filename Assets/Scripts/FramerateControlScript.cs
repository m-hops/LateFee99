using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FramerateControlScript : MonoBehaviour
{
    public int FrameRate = 30;

    void Start()
    {
        Application.targetFrameRate = FrameRate;
    }
 
}
