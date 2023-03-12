using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{

    [Serializable]
    public struct DirectionReference
    {

        public enum TypeEnum
        {
            Axis,
            Vector,
            Toward,
        }
        public enum AxisEnum
        {
            X,
            Y,
            Z
        }
        public TypeEnum Type;

        public PositionReference PositionFrom;
        public PositionReference PositionTo;

        public GameObjectReference Object;
        public AxisEnum Axis;

        public Vector3 GetDirection(EventParameters eventParams)
        {
            Vector3 vector = Vector3.zero;
            switch (Type)
            {
                case DirectionReference.TypeEnum.Axis:
                    var obj = Object.GetTargetGameObject(eventParams);
                    if(obj != null)
                    {
                        return Axis switch
                        {
                            AxisEnum.X => obj.transform.right,
                            AxisEnum.Y => obj.transform.up,
                            AxisEnum.Z => obj.transform.forward,
                            _ => throw new ArgumentOutOfRangeException(nameof(Axis), $"Not expected direction value: {Axis}"),
                        };
                    }
                    break;
                case DirectionReference.TypeEnum.Vector:
                    vector = PositionFrom.GetPosition(eventParams);
                    break;
                case DirectionReference.TypeEnum.Toward:
                    var posFrom = PositionFrom.GetPosition(eventParams);
                    var posTo = PositionTo.GetPosition(eventParams);
                    vector = (posFrom - posTo);
                    break;
            }
            var length = vector.magnitude;
            if (length > 0.00001f)
                return vector * (1 / length);

            Debug.LogWarning($"[{Time.frameCount}] DirectionReference vector is not a direction (lenght <= 0.0001). Returns Vector3.up");
            return Vector3.up;
        }
        public bool TryGet(EventParameters eventParams, out Vector3 direction)
        {
            Vector3 vector = Vector3.zero;
            switch (Type)
            {
                case DirectionReference.TypeEnum.Axis:
                    if (Object.TryGetTargetGameObject(eventParams, out var obj))
                    {
                        direction = Axis switch
                        {
                            AxisEnum.X => obj.transform.right,
                            AxisEnum.Y => obj.transform.up,
                            AxisEnum.Z => obj.transform.forward,
                            _ => throw new ArgumentOutOfRangeException(nameof(Axis), $"Not expected direction value: {Axis}"),
                        };
                        return true;
                    }
                    direction = Vector3.up;
                    return false;
                case DirectionReference.TypeEnum.Vector:
                    if(PositionFrom.TryGetPosition(eventParams, out var pos))
                    {
                        vector = pos;
                        break;
                    }
                    direction = Vector3.up;
                    return false;
                case DirectionReference.TypeEnum.Toward:
                    if (PositionFrom.TryGetPosition(eventParams, out var posFrom)
                        && PositionTo.TryGetPosition(eventParams, out var posTo))
                    {
                        vector = (posFrom - posTo);
                        break;
                    }
                    direction = Vector3.up;
                    return false;
            }
            var length = vector.magnitude;
            if (length > 0.00001f)
            {
                direction = vector * (1 / length);
                return true;
            }
            direction = Vector3.up;
            return false;
        }
    }
}