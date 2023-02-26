using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Nie/Object/Constraints/SnapAllChildren")]
public class SnapAllChildren : MonoBehaviour
{
    void Update()
    {
        for(int i = 0; i != transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = Vector3.zero;
            transform.GetChild(i).localRotation = Quaternion.identity;
        }
    }
}
