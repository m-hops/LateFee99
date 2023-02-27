using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    [System.Serializable]
    public struct StateCondition
    {
        public enum StateType
        {
            ReactionState,
            Animator,
            Ignore,
        }
        public StateType Type;
        public Animator Animator;
        public ReactionState ReactionState;
        public string State;
        public int StateHash;
        public bool CanReact(GameObject triggeringObject, Vector3 position)
        {
            switch (Type)
            {
                case StateType.Animator:
                    if (Animator != null)
                        if (Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != StateHash)
                            return false;
                    break;
                case StateType.ReactionState:
                    if (ReactionState != null && ReactionState.gameObject.TryGetReactionState(State, out var reactionState) && !reactionState.IsActiveState) 
                        return false;
                    break;
            }
            return true;
        }
    }

    [System.Serializable]
    public struct ReactionConditions
    {
        //public AnimatorStateReference MustBeInAnimatorState;
        //public ReactionStateReference MustBeInReactionState;
        public List<StateCondition> States;
        public bool CanReact(GameObject triggeringObject, Vector3 position)
        {
            //if (MustBeInAnimatorState.Animator != null)
            //    if (MustBeInAnimatorState.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != MustBeInAnimatorState.StateHash)
            //        return false;
            //if (MustBeInReactionState.Object != null && !MustBeInReactionState.IsActiveState) return false;

            foreach(var condition in States)
                if(!condition.CanReact(triggeringObject, position)) return false;
            return true;
        }
    }

}