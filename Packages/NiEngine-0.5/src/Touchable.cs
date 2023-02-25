using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    [AddComponentMenu("Nie/Object/Touchable")]
    public class Touchable : MonoBehaviour
    {
        public bool DebugLog;
        [Header("Conditions:")]
        public AnimatorStateReference MustBeInAnimatorState;
        public ReactionStateReference MustBeInReactionState;

        [Header("On Touch:")]
        public List<Reaction> Reactions;
        public List<ReactionStateReference> ReactionStates;
        [SerializeField]
        [Tooltip("Event called when a ToucherController touches this Touchable.")]
        UnityEvent<Touchable, ToucherController> OnTouch;

        [Header("On Release:")]
        public List<Reaction> OnReleaseReactions;
        public List<ReactionStateReference> OnReleaseReactionStates;
        [SerializeField]
        [Tooltip("Event called when a ToucherController stops touching this Touchable.")]
        UnityEvent<Touchable, ToucherController> OnRelease;

        [Header("On Focus:")]
        public List<Reaction> OnFocusReactions;
        public List<ReactionStateReference> OnFocusReactionStates;
        [SerializeField]
        [Tooltip("Event called when a ToucherController looks at this Touchable before touching it.")]
        UnityEvent<Touchable, ToucherController> OnFocus;

        [Header("On Unfocus:")]
        public List<Reaction> OnUnfocusReactions;
        public List<ReactionStateReference> OnUnfocusReactionStates;
        [SerializeField]
        [Tooltip("Event called when a ToucherController either stops looking at this Touchable or has touched it after focusing on it.")]
        UnityEvent<Touchable, ToucherController> OnUnfocus;

        public bool CanTouch(ToucherController by, Vector3 position)
        {
            if (!enabled) return false;
            if (Reactions.Count > 0 && Reactions.All(x => !x.CanReact(by.gameObject, position)) && ReactionStates.All(x => !x.CanReact(by.gameObject, position)))
                return false;
            if (MustBeInAnimatorState.Animator != null)
                if (MustBeInAnimatorState.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != MustBeInAnimatorState.StateHash)
                    return false;
            if (MustBeInReactionState.Object != null && !MustBeInReactionState.IsActiveState) return false;
            return true;
        }
        public void Touch(ToucherController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Touched By '{by.name}'");

            foreach (var reaction in Reactions)
                reaction.TryReact(by.gameObject, position);
            foreach (var reaction in ReactionStates)
                reaction.TryReact(by.gameObject, position);

            OnTouch?.Invoke(this, by);
        }
        public void Release(ToucherController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Released By '{by.name}'");
            foreach (var reaction in OnReleaseReactions)
                reaction.TryReact(by.gameObject, position);
            foreach (var reaction in OnReleaseReactionStates)
                reaction.TryReact(by.gameObject, position);
            OnRelease?.Invoke(this, by);
        }
        public void Focus(ToucherController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Focused By '{by.name}'");
            foreach (var reaction in OnFocusReactions)
                reaction.TryReact(by.gameObject, position);
            foreach (var reaction in OnFocusReactionStates)
                reaction.TryReact(by.gameObject, position);

            OnFocus?.Invoke(this, by);
        }
        public void Unfocus(ToucherController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Unfocused By '{by.name}'");
            foreach (var reaction in OnUnfocusReactions)
                reaction.TryReact(by.gameObject, position);
            foreach (var reaction in OnUnfocusReactionStates)
                reaction.TryReact(by.gameObject, position);
            OnUnfocus?.Invoke(this, by);
        }
    }
}