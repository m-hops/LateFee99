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
        public enum TargetType
        {
            Self,
            Other,
            TriggerObject,
            PreviousTriggerObject,
        }
        public StateType Type;
        [UnityEngine.Serialization.FormerlySerializedAs("ObjectTargetType2")]
        public TargetType ObjectTargetType;
        //[UnityEngine.Serialization.FormerlySerializedAs("ObjectTargetType2")]
        //public TargetType TargetObjectType;
        public Animator Animator;
        [System.Obsolete]
        public ReactionState ReactionState;

        public GameObject Other;
        public string State;
        public int StateHash;
        public bool CanReact(EventParameters parameters)
        {
            return CanReact(parameters.Self, parameters.TriggerObject, parameters.TriggerPosition, parameters.PreviousTriggerObject);
        }
        public bool CanReact(GameObject from, GameObject triggerObject, Vector3 position, GameObject previousTriggerObjectIfExist)
        {
            switch (Type)
            {
                case StateType.Animator:
                    if (Animator != null)
                        if (Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != StateHash)
                            return false;
                    break;
                case StateType.ReactionState:
                    GameObject target = null;
                    switch (ObjectTargetType)
                    {
                        case TargetType.Self:
                            target = from;
                            break;
                        case TargetType.Other:
                            target = Other;
                            break;
                        case TargetType.TriggerObject:
                            target = triggerObject;
                            break;
                        case TargetType.PreviousTriggerObject:
                            target = previousTriggerObjectIfExist;
                            break;
                    }
                    if(target != null)
                    {
                        bool hasPotential = false;
                        bool hasAnyActive = false;
                        foreach (var reactionState in target.AllReactionState(State))
                        {
                            hasPotential = true;
                            if (reactionState.IsActiveState)
                            {
                                hasAnyActive = true;
                                break;
                            }
                        }
                        foreach (var sm in target.AllReactionStateMachine())
                        {
                            foreach(var g in sm.Groups)
                            {
                                if(g.TryGetState(new StateName(State), out var state))
                                {
                                    hasPotential = true;
                                    if (state.IsActiveState)
                                    {
                                        hasAnyActive = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (hasPotential && !hasAnyActive)
                            return false;
                    }
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
        public bool CanReactAny(GameObject from, GameObject triggerObject, Vector3 position, GameObject previousTriggerObjectIfExist)
        {
            foreach (var condition in States)
                if (condition.CanReact(from, triggerObject, position, previousTriggerObjectIfExist)) return true;
            return false;
        }
        public bool CanReactAll(GameObject from, GameObject triggerObject, Vector3 position, GameObject previousTriggerObjectIfExist)
        {
            foreach (var condition in States)
                if (!condition.CanReact(from, triggerObject, position, previousTriggerObjectIfExist)) return false;
            return true;
        }
    }

}