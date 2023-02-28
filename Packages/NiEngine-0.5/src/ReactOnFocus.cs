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
        [Header("Conditions:")]

        [Tooltip("Conditions to execute the focus reaction")]
        public ReactionConditions Conditions;

        [Tooltip("Reaction executed when a FocusController looks at this GameObject.")]
        public ReactionList OnFocusReaction;

        [Tooltip("Reaction executed when a FocusController stops looking at this focused GameObject.")]
        public ReactionList OnUnfocusReaction;

        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;
        public bool CanFocus(FocusController by, Vector3 position)
        {
            if (!enabled) return false;
            if (!OnFocusReaction.CanReact(TargetObject, by.gameObject, position)) return false;
            if (!Conditions.CanReact(by.gameObject, position)) return false;
            return true;
        }
        public void Focus(FocusController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Focused By '{by.name}'", this);
            OnFocusReaction.TryReact(TargetObject, by.gameObject, position);
        }
        public void Unfocus(FocusController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Unfocused By '{by.name}'", this);
            OnUnfocusReaction.TryReact(TargetObject, by.gameObject, position);
        }
    }
}