using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Billboard : MonoBehaviour
{
    public bool AutoTargetMainCamera = true;
    public GameObject Target;

    public enum LockAxisEnum
    {
        None,
        X,
        Y,
        // TODO
        //OtherY,
    }
    [Tooltip("Only rotate around this axis")]
    public LockAxisEnum LockAxis = LockAxisEnum.Y;


    //[Tooltip("If LockAxis is set to 'OtherY'. Will lock on the Y axis of this other transform object.")]
    //public Transform Other;

    // Start is called before the first frame update
    void Start()
    {
        if (AutoTargetMainCamera)
        {
            Target = Camera.main.gameObject;
        }   
    }

    void Update()
    {
        LookAt();
    }


    void LookAt()
    {
        float3 target = Target.transform.position;
        float3 here = transform.position;
        var diff = target - here;
        float3 lockAxis = float3.zero;
        switch (LockAxis)
        {
            case LockAxisEnum.X:
                {
                    diff.x = 0;
                    lockAxis = new float3(1, 0, 0);
                    var dir = math.normalize(diff);
                    //Debug.DrawLine(transform.position, dir, Color.blue);
                    var up = math.cross(dir, lockAxis);
                    var upL = math.length(up);
                    if (upL < 0.001) return;
                    up = up * math.rcp(upL);
                    //Debug.DrawLine(transform.position, up, Color.green);
                    transform.rotation = quaternion.LookRotation(dir, up);
                    //var dir = math.normalize(diff);
                    //var up = math.cross(dir, lockAxis);
                    //var upL = math.length(up);
                    //if (upL < 0.001) return;
                    //up = up * math.rcp(upL);
                    //quaternion.LookRotation(dir, up);
                    break;
                }
            case LockAxisEnum.Y:
                {
                    diff.y = 0;
                    lockAxis = new float3(0, 1, 0);
                    var dir4Y = math.normalize(diff);
                    transform.rotation = quaternion.LookRotation(dir4Y, new float3(0, 1, 0));
                    return;
                }
            //case LockAxisEnum.OtherY:
            //    lockAxis = Other.transform.up;
            //    float3 xAxis = Other.transform.right;
            //    float3 zAxis = Other.transform.forward;
            //    float3 xPos = math.dot(diff, xAxis);
            //    float3 zPos = math.dot(diff, zAxis);
            //    diff = xPos + zPos;
            //    break;
        }
    }
}
