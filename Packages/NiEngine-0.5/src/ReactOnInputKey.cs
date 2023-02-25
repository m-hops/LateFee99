using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    public class ReactOnInputKey : MonoBehaviour
    {

        [Header("Input:")]
        public KeyCode KeyCode;
        [Header("Conditions:")]
        public AnimatorStateReference MustBeInAnimatorState;
        public ReactionStateReference MustBeInReactionState;

        [Header("Reactions:")]
        public bool ReactOnKeyUp;
        public List<Reaction> Reactions;
        public List<ReactionStateReference> ReactionStates;

        [Header("Event:")]

        [SerializeField]
        [Tooltip("Event called when key has been pressed down.")]
        UnityEvent OnKeyDown;

        [SerializeField]
        [Tooltip("Event called when key has been released.")]
        UnityEvent OnKeyUp;

        [SerializeField]
        [Tooltip("Event called when input is pressed.")]
        UnityEvent WhenPressed;

        public bool CanReact()
        {
            if(!enabled) return false;
            if (MustBeInAnimatorState.Animator != null)
                if (MustBeInAnimatorState.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != MustBeInAnimatorState.StateHash)
                    return false;
            if (MustBeInReactionState.Object != null && !MustBeInReactionState.IsActiveState) return false;
            if (Reactions.Count > 0 && Reactions.All(x => !x.CanReact(gameObject, transform.position)) && ReactionStates.All(x => !x.CanReact(gameObject, transform.position)))
                return false;
            return true;
        }
        public void React()
        {
            foreach (var reaction in Reactions)
                reaction.TryReact(gameObject, transform.position);
            foreach (var reaction in ReactionStates)
                reaction.TryReact(gameObject, transform.position);
        }
        public bool TryReact()
        {
            if (CanReact())
            {
                React();
                return true;
            }
            return false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode))
            {
                if (!ReactOnKeyUp)
                    TryReact();
                OnKeyDown?.Invoke();
            }
            if (Input.GetKeyUp(KeyCode))
            {
                if (ReactOnKeyUp)
                    TryReact();
                OnKeyUp?.Invoke();
            }
            if (Input.GetKey(KeyCode))
            {
                WhenPressed?.Invoke();
            }
        }
    }
}