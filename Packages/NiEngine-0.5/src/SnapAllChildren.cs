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
            var c = transform.GetChild(i);
            c.localPosition = Vector3.zero;
            c.localRotation = Quaternion.identity;
            if(c.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
        }
    }
}
