//#define NIE_DEBUG_REACTIONS
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    [System.Serializable]
    public struct ReactionReference
    {
        public enum Type
        {
            Reaction,
            Event,
        }
        public Type ReactionType;

        //[System.Obsolete]
        //public TargetType ObjectTargetType;
        //
        //[System.Obsolete]
        //public GameObject TargetObject;

        public GameObjectReference TargetObjectReference;

        #region /////////////////////////// Type Reaction
        public string ReactionName;
        #endregion

        #region /////////////////////////// Type Event

        public UnityEvent Event;
        public UnityEvent<GameObject> EventTrigger;
        #endregion


        public static bool IsActiveState(EventParameters parameters, string stateName)
        {
            Debug.Assert(parameters.Self != null);
            Debug.Assert(parameters.Current.From != null);
            bool hasPotential = false;
            foreach (var reactionState in parameters.Self.AllReactionState(stateName))
            {
                hasPotential = true;
                if (reactionState.IsActiveState)
                {
                    return true;
                }
            }
            foreach (var sm in parameters.Self.AllReactionStateMachine())
            {
                foreach (var g in sm.Groups)
                {
                    if (g.TryGetState(stateName, out var state))
                    {
                        hasPotential = true;
                        if (state.IsActiveState)
                        {
                            return true;
                        }
                    }
                }
            }
            return !hasPotential;
        }
        public static bool HasReaction(GameObject obj, string reactionOrStateName)
        {
            return obj.GetComponents<Reaction>().Any(x => x.enabled && x.ReactionName == reactionOrStateName)
                || obj.GetComponents<ReactionState>().Any(x => x.enabled && x.StateName == reactionOrStateName)
                || obj.GetComponents<ReactionStateMachine>().Any(x => x.enabled && x.HasState(new StateName(reactionOrStateName)));
        }

        public bool React(EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            Debug.Assert(parameters.Current.From != null);
            switch (ReactionType)
            {
                case Type.Reaction:
                    var obj = TargetObjectReference.GetTargetGameObject(parameters);
                    parameters = parameters.WithSelf(obj);
#if NIE_DEBUG_REACTIONS
                    Debug.Log($"Trigger Reaction {ReactionName}{parameters}.");
#endif
                    if (obj != null)
                        return React(ReactionName, parameters);
                    return false;

                case Type.Event:
                    Event?.Invoke();
                    return true;
            }
            return false;
        }
        public static bool React(string reactionOrStateName, EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            Debug.Assert(parameters.Current.From != null);
            foreach (var reaction in parameters.Self.GetComponents<Reaction>())
                if (reaction.enabled && (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName))
                {
                    reaction.React(parameters.Current.TriggerObject, parameters.Current.TriggerPosition);
                    return true;
                }
            foreach (var reaction in parameters.Self.GetComponents<ReactionState>())
                if (reaction.enabled && reaction.StateName == reactionOrStateName)
                {
                    reaction.React(parameters.Current.TriggerObject, parameters.Current.TriggerPosition);
                    return true;
                }
            foreach (var sm in parameters.Self.GetComponents<ReactionStateMachine>())
                if (sm.enabled)
                    if (sm.React(reactionOrStateName, parameters))
                        return true;
            return false;
        }

        public bool CanReact(EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            Debug.Assert(parameters.Current.From != null);
            switch (ReactionType)
            {
                case Type.Reaction:
                    var obj = TargetObjectReference.GetTargetGameObject(parameters);
                    parameters = parameters.WithSelf(obj);
                    if (obj != null)
                        CanReact( ReactionName, parameters);

                    break;
                case Type.Event:
                    return true;
            }
            return true;
        }
        public static bool CanReact(string reactionOrStateName, EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            Debug.Assert(parameters.Current.From != null);
            int potentialReactCount = 0;
            foreach (var reaction in parameters.Self.GetComponents<Reaction>())
                if (reaction.enabled && (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName))
                {
                    ++potentialReactCount;
                    if (reaction.CanReact(parameters.Current.TriggerObject, parameters.Current.TriggerPosition))
                        return true;
                }
            foreach (var reaction in parameters.Self.GetComponents<ReactionState>())
                if (reaction.enabled && reaction.StateName == reactionOrStateName)
                {
                    ++potentialReactCount;
                    if (reaction.CanReact(parameters.Current.TriggerObject, parameters.Current.TriggerPosition))
                        return true;
                }
            foreach (var sm in parameters.Self.GetComponents<ReactionStateMachine>())
                if (sm.enabled && sm.CanReact(reactionOrStateName, parameters))
                    return true;
            // can react when no potential reaction were found.
            if (potentialReactCount == 0) return true;
            return false;
        }

        public bool TryReact(EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            Debug.Assert(parameters.Current.From != null);
            if (CanReact(parameters))
            {
                React(parameters);
                return true;
            }
            return false;
        }













//        public bool React(GameObject from, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
//        {
//            switch(ReactionType)
//            {
//                case Type.Reaction:
//                    var parameters = new EventParameters()
//                    {
//                        Self = from,
//                        Current = EventParameters.ParameterSet.Trigger(from, triggerObject, position),
//                        OnBegin = EventParameters.ParameterSet.Trigger(null, previousTriggerObject, position)
//                    };
//                    Debug.Assert(parameters.Self != null);
//                    var obj = TargetObjectReference.GetTargetGameObject(parameters);
//                    parameters = parameters.WithSelf(obj);
//#if NIE_DEBUG_REACTIONS
//                    Debug.Log($"Trigger Reaction (old) {ReactionName}{parameters}.");
//#endif
//                    if (obj != null)
//                        return React(from, obj, ReactionName, triggerObject, position);

//                    return false;

//                case Type.Event:
//                    Event?.Invoke();
//                    return true;
//            }
//            return false;
//        }
            
//        public bool TryReact(GameObject from, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
//        {
//            if (CanReact(from, triggerObject, position, previousTriggerObject))
//            {
//                React(from, triggerObject, position, previousTriggerObject);
//                return true;
//            }
//            return false;
//        }


//        public bool CanReact(GameObject from, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
//        {
//            switch (ReactionType)
//            {
//                case Type.Reaction:
//                    var parameters = new EventParameters()
//                    {
//                        Self = from,
//                        Current = EventParameters.ParameterSet.Trigger(from, triggerObject, position),
//                        OnBegin = EventParameters.ParameterSet.Trigger(null, previousTriggerObject, position)
//                    };
//                    Debug.Assert(parameters.Self != null);
//                    var obj = TargetObjectReference.GetTargetGameObject(parameters);
//                    parameters = parameters.WithSelf(obj);
//                    if (obj != null)
//                        return CanReact(from, obj, ReactionName, triggerObject, position);

//                    break;
//                case Type.Event:
//                    return true;
//            }
//            return true;
//        }



//        public static bool CanReact(GameObject from, GameObject targetObject, string reactionOrStateName, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
//        {
//            int potentialReactCount = 0;
//            foreach (var reaction in targetObject.GetComponents<Reaction>())
//                if (reaction.enabled && (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName))
//                {
//                    ++potentialReactCount;
//                    if (reaction.CanReact(triggerObject, position))
//                        return true;
//                }
//            foreach (var reaction in targetObject.GetComponents<ReactionState>())
//                if (reaction.enabled && reaction.StateName == reactionOrStateName)
//                {
//                    ++potentialReactCount;
//                    if (reaction.CanReact(triggerObject, position))
//                        return true;
//                }
//            foreach (var sm in targetObject.GetComponents<ReactionStateMachine>())
//                if(sm.enabled && sm.CanReact(reactionOrStateName, new EventParameters()
//                    {
//                        Self = targetObject,
//                        Current = EventParameters.ParameterSet.Trigger(from, triggerObject, position),
//                        OnBegin = EventParameters.ParameterSet.Trigger(null, previousTriggerObject, position)
//                    }))
//                    return true;
//            // can react when no potential reaction were found.
//            if (potentialReactCount == 0) return true;
//            return false;
//        }
//        public static bool TryReact(GameObject from, GameObject targetObject, string reactionOrStateName, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
//        {
//            if (CanReact(from, targetObject, reactionOrStateName, triggerObject, position))
//            {
//                React(from, targetObject, reactionOrStateName, triggerObject, position);
//                return true;
//            }
//            return false;
//        }
//        public static bool React(GameObject from, GameObject targetObject, string reactionOrStateName, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
//        {
//            foreach (var reaction in targetObject.GetComponents<Reaction>())
//                if (reaction.enabled && (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName))
//                {
//                    reaction.React(triggerObject, position);
//                    return true;
//                }
//            foreach (var reaction in targetObject.GetComponents<ReactionState>())
//                if (reaction.enabled && reaction.StateName == reactionOrStateName)
//                {
//                    reaction.React(triggerObject, position);
//                    return true;
//                }
//            foreach (var sm in targetObject.GetComponents<ReactionStateMachine>())
//                if (sm.enabled)
//                    if (sm.React(reactionOrStateName, new EventParameters()
//                    {
//                        Self = targetObject,
//                        Current = EventParameters.ParameterSet.Trigger(from, triggerObject, position),
//                        OnBegin = EventParameters.ParameterSet.Trigger(null, previousTriggerObject, position)
//                    }))
//                        return true;
//            return false;
//        }
    }


    [System.Serializable]
    public struct StateReactionReference
    {
        public GameObjectReference TargetObjectReference;
        public string OnBegin;
        public string OnEnd;

        public static bool HasReaction(GameObject obj, string reactionOrStateName)
        {
            return obj.GetComponents<Reaction>().Any(x => x.enabled && x.ReactionName == reactionOrStateName)
                || obj.GetComponents<ReactionState>().Any(x => x.enabled && x.StateName == reactionOrStateName)
                || obj.GetComponents<ReactionStateMachine>().Any(x => x.enabled && x.HasState(new StateName(reactionOrStateName)));
        }

        public bool ReactOnBegin(EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            var obj = TargetObjectReference.GetTargetGameObject(parameters);
            parameters = parameters.WithSelf(obj);
#if NIE_DEBUG_REACTIONS
            Debug.Log($"Trigger Reaction OnBegin {OnBegin}{parameters}.");
#endif
            if (obj != null)
                return ReactionReference.React(OnBegin, parameters);
            return false;
        }
        public bool ReactOnEnd(EventParameters parameters)
        {
            Debug.Assert(parameters.Self != null);
            var obj = TargetObjectReference.GetTargetGameObject(parameters);
            parameters = parameters.WithSelf(obj);
#if NIE_DEBUG_REACTIONS
            Debug.Log($"Trigger Reaction OnEnd {OnEnd}{parameters}.");
#endif
            if (obj != null)
                return ReactionReference.React(OnEnd, parameters);
            return false;
        }
    }
}