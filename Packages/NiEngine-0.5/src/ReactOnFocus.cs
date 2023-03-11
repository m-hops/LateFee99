using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    [AddComponentMenu("Nie/Object/ReactOnFocus")]
    public class ReactOnFocus : MonoBehaviour
    {
        public bool DebugLog;
        public bool ShowHand = true;
        //public bool ReactOnColliderObject = true;
        public bool SendFocusToRigidBodyObject = true;
        [Header("Conditions:")]

        [Tooltip("Conditions to execute the focus reaction")]
        public ReactionConditions Conditions;

        [Tooltip("Reaction executed when a FocusController looks at this GameObject.")]
        public ReactionList OnFocusReaction;

        [Tooltip("Reaction executed when a FocusController stops looking at this focused GameObject.")]
        public ReactionList OnUnfocusReaction;

        [Header("Run-Time state:")]
        [Tooltip("Object that triggered the focus. Used as 'previous trigger object'")]
        public GameObject FocusedByTriggerObject;
        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;
        public bool CanFocus(FocusController by, Vector3 position)
        {
            if (!enabled) return false;
            if (!Conditions.CanReactAll(gameObject, by.gameObject, position, previousTriggerObjectIfExist:null)) return false;
            if (!OnFocusReaction.CanReact(gameObject, TargetObject, by.gameObject, position)) return false;
            return true;
        }
        public void Focus(FocusController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Focused By '{by.name}'", this);
            if(OnFocusReaction.TryReact(gameObject, TargetObject, by.gameObject, position))
                FocusedByTriggerObject = by.gameObject;
        }
        public void Unfocus(FocusController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Unfocused By '{by.name}'", this);
            OnUnfocusReaction.TryReact(gameObject, TargetObject, by.gameObject, position, previousTriggerObject: FocusedByTriggerObject);
            FocusedByTriggerObject = null;
        }
    }
}