using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    [AddComponentMenu("Nie/Object/ReactOnTouch")]
    public class ReactOnTouch : MonoBehaviour
    {
        public bool DebugLog;

        [Tooltip("Conditions to touch this touchable")]
        public ReactionConditions Conditions;

        [Tooltip("Reaction executed when a TouchController starts touching this object.")]
        public ReactionList OnTouch;

        [Tooltip("Reaction executed when a TouchController stops touching this object.")]
        public ReactionList OnRelease;

        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;
        public bool CanTouch(TouchController by, Vector3 position)
        {
            if (!enabled) return false;
            if (!Conditions.CanReactAll(gameObject, by.gameObject, position, previousTriggerObjectIfExist: null)) return false;
            if (!OnTouch.CanReact(gameObject, TargetObject, by.gameObject, position)) return false;
            return true;
        }

        public void Touch(TouchController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Touched By '{by.name}'");
            OnTouch.TryReact(gameObject, TargetObject, by.gameObject, position);
        }

        public void Release(TouchController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Released By '{by.name}'");
            OnRelease.TryReact(gameObject, TargetObject, by.gameObject, position);
        }

    }
}