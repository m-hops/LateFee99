using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    [System.Serializable]
    public struct ReactionStateReference
    {
        public ReactionState Object;
        public string StateName;
        public bool IsActiveState
        {
            get
            {
                if (TryGetState(out var state))
                    return state.IsActiveState;
                return false;
            }
        }
        public string ActiveStateName
        {
            get
            {
                if (TryGetState(out var state))
                {
                    foreach (var reactionState in Object.GetComponents<ReactionState>())
                    {
                        if (reactionState.StateGroup == state.StateGroup && reactionState.IsActiveState)
                            return reactionState.StateName;
                    }
                }
                return null;
            }
        }
        public bool CanReact(GameObject triggeringObject, Vector3 position)
        {
            if (TryGetState(out var state))
                return state.CanReact(triggeringObject, position);
            return false;
        }
        public bool TryReact(GameObject triggeringObject, Vector3 position)
        {
            if (TryGetState(out var state))
                return state.TryReact(triggeringObject, position);
            return false;
        }
        public bool TryGetState(out ReactionState state)
        {
            foreach (var reactionState in Object.GetComponents<ReactionState>())
            {
                if (reactionState.StateName == StateName)
                {
                    state = reactionState;
                    return true;
                }
            }
            state = null;
            return false;
        }
    }
}