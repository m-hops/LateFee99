using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{

    [Serializable]
    public struct GameObjectReference
    {
        public enum TypeEnum
        {
            Object,
            Self,
            TriggerObject,
            TriggerObjectOnBegin,
            ObjectFrom,
            ObjectFromOnBegin,
            FirstOfType,
            FirstWithTag,
            FirstWithName,
        }
        public TypeEnum Type;
        [SerializeField]
        GameObject ThisGameObject;
        [SerializeField]
        SerializableType<Component> ObjectType;
        [SerializeField]
        string String;

        public GameObject GetTargetGameObject(EventParameters eventParams)
        {
            if (!TryGetTargetGameObject(eventParams, out var go) || go == null)
            {
                switch (Type)
                {
                    case GameObjectReference.TypeEnum.Self:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference unable to find Self");
                        break;
                    case GameObjectReference.TypeEnum.ObjectFrom:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference unable to find From object");
                        break;
                    case GameObjectReference.TypeEnum.ObjectFromOnBegin:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference unable to find From object OnBegin");
                        break;
                    case GameObjectReference.TypeEnum.Object:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' unable to find Object", eventParams.Self);
                        break;
                    case GameObjectReference.TypeEnum.TriggerObject:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' could not find Trigger Object", eventParams.Self);
                        break;
                    case GameObjectReference.TypeEnum.TriggerObjectOnBegin:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' could not find Trigger Object OnBegin", eventParams.Self);
                        break;
                    case GameObjectReference.TypeEnum.FirstOfType:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' unable to find First Object Of Type '{String}'", eventParams.Self);
                        break;
                    case GameObjectReference.TypeEnum.FirstWithTag:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' unable to find First Object With Tag '{String}'", eventParams.Self);
                        break;
                    case GameObjectReference.TypeEnum.FirstWithName:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' unable to find First Object With Name '{String}'", eventParams.Self);
                        break;
                }
            }
            return go;
        }
        public bool TryGetTargetGameObject(EventParameters eventParams, out GameObject go)
        {
            switch (Type)
            {
                case GameObjectReference.TypeEnum.Self:
                    go = eventParams.Self;
                    return go != null;
                case GameObjectReference.TypeEnum.ObjectFrom:
                    go = eventParams.Current.From;
                    return go != null;
                case GameObjectReference.TypeEnum.ObjectFromOnBegin:
                    go = eventParams.OnBegin.From;
                    return go != null;
                case GameObjectReference.TypeEnum.Object:
                    go = ThisGameObject;
                    return go != null;
                case GameObjectReference.TypeEnum.TriggerObject:
                    go = eventParams.Current.TriggerObject;
                    return go != null;
                case GameObjectReference.TypeEnum.TriggerObjectOnBegin:
                    go = eventParams.OnBegin.TriggerObject;
                    return go != null;
                case GameObjectReference.TypeEnum.FirstOfType:
                    var obj = GameObject.FindObjectOfType(ObjectType);
                    go = default;
                    switch (obj)
                    {
                        case Component comp:
                            go = comp.gameObject;
                            break;
                        case GameObject goo:
                            go = goo;
                            break;
                    }
                    return go != null;
                case GameObjectReference.TypeEnum.FirstWithTag:
                    go = GameObject.FindGameObjectWithTag(String);
                    return go != null;
                case GameObjectReference.TypeEnum.FirstWithName:
                    go = GameObject.Find(String);
                    return go != null;
            }
            go = default;
            return false;
        }

#if UNITY_EDITOR
        public static GameObjectReference FromProperty(UnityEditor.SerializedProperty property)
        {
            return new GameObjectReference
            {
                Type = (TypeEnum)property.FindPropertyRelative("Type").intValue,
                ThisGameObject = (GameObject)property.FindPropertyRelative("ThisGameObject").objectReferenceValue,
                ObjectType = SerializableType<Component>.FromProperty(property.FindPropertyRelative("ObjectType")),
                String = property.FindPropertyRelative("String").stringValue,
            };

        }
#endif
    }
}