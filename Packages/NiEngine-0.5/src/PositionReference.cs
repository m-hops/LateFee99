using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{

    [Serializable]
    public struct PositionReference
    {

        public enum TypeEnum
        {
            Self,
            AtPosition,
            AtTriggerPosition,
            AtPreviousTriggerPosition,
            AtGameObject,
        }
        public TypeEnum Type;
        public Vector3 AtPosition;
        public GameObjectReference AtTransform;


        public Vector3 GetPosition(EventParameters eventParams)
        {
            if (!TryGetPosition(eventParams, out var go))
            {
                switch (Type)
                {
                    case PositionReference.TypeEnum.Self:
                        Debug.LogWarning($"[{Time.frameCount}] PositionReference unable to find Self");
                        break;
                    case PositionReference.TypeEnum.AtPosition:
                        break;
                    case PositionReference.TypeEnum.AtTriggerPosition:
                        break;
                    case PositionReference.TypeEnum.AtPreviousTriggerPosition:
                        break;
                }
            }
            return go;
        }
        public bool TryGetPosition(EventParameters eventParams, out Vector3 position)
        {
            switch (Type)
            {
                case PositionReference.TypeEnum.Self:
                    position = eventParams.Self.transform.position;
                    return true;
                case PositionReference.TypeEnum.AtPosition:
                    position = AtPosition;
                    return true;
                case PositionReference.TypeEnum.AtGameObject:
                    if (AtTransform.TryGetTargetGameObject(eventParams, out var obj))
                    {
                        position = obj.transform.position;
                        return true;
                    }
                    else
                    {
                        position = eventParams.Self.transform.position;
                    }
                    return false;
                case PositionReference.TypeEnum.AtTriggerPosition:
                    position = eventParams.TriggerPosition;
                    break;
                case PositionReference.TypeEnum.AtPreviousTriggerPosition:
                    position = eventParams.PreviousTriggerPosition;
                    break;
            }
            position = default;
            return false;
        }
    }
}