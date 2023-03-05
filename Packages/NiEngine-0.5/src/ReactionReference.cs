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
        public enum TargetType
        {
            Self,
            Other,
            TriggerObject,
            PreviousTriggerObject,
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


        public static bool HasReaction(GameObject obj, string reactionOrStateName)
        {
            return obj.GetComponents<Reaction>().Any(x => x.enabled && x.ReactionName == reactionOrStateName)
                || obj.GetComponents<ReactionState>().Any(x => x.enabled && x.StateName == reactionOrStateName)
                || obj.GetComponents<ReactionStateMachine>().Any(x => x.enabled && x.HasState(new StateName(reactionOrStateName)));
        }

        public void React(EventParameters parameters)
        {
            switch (ReactionType)
            {
                case Type.Reaction:
                    var obj = TargetObjectReference.GetTargetGameObject(parameters);
                    if (obj != null)
                        React(obj, ReactionName, parameters);
                    return;

                case Type.Event:
                    Event?.Invoke();
                    return;
            }
        }
        public static void React(GameObject targetObject, string reactionOrStateName, EventParameters parameters)
        {
            foreach (var reaction in targetObject.GetComponents<Reaction>())
                if (reaction.enabled && (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName))
                    reaction.React(parameters.TriggerObject, parameters.TriggerPosition);
            foreach (var reaction in targetObject.GetComponents<ReactionState>())
                if (reaction.enabled && reaction.StateName == reactionOrStateName)
                    reaction.React(parameters.TriggerObject, parameters.TriggerPosition);
            foreach (var sm in targetObject.GetComponents<ReactionStateMachine>())
                if(sm.enabled)
                    sm.React(reactionOrStateName, parameters);
        }

        public bool CanReact(EventParameters parameters)
        {
            switch (ReactionType)
            {
                case Type.Reaction:
                    var obj = TargetObjectReference.GetTargetGameObject(parameters);
                    if (obj != null)
                        CanReact(obj, ReactionName, parameters);

                    break;
                case Type.Event:
                    return true;
            }
            return true;
        }
        public static bool CanReact(GameObject targetObject, string reactionOrStateName, EventParameters parameters)
        {
            int potentialReactCount = 0;
            foreach (var reaction in targetObject.GetComponents<Reaction>())
                if (reaction.enabled && (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName))
                {
                    ++potentialReactCount;
                    if (reaction.CanReact(parameters.TriggerObject, parameters.TriggerPosition))
                        return true;
                }
            foreach (var reaction in targetObject.GetComponents<ReactionState>())
                if (reaction.enabled && reaction.StateName == reactionOrStateName)
                {
                    ++potentialReactCount;
                    if (reaction.CanReact(parameters.TriggerObject, parameters.TriggerPosition))
                        return true;
                }
            foreach (var sm in targetObject.GetComponents<ReactionStateMachine>())
                if (sm.enabled && sm.CanReact(reactionOrStateName, parameters))
                    return true;
            // can react when no potential reaction were found.
            if (potentialReactCount == 0) return true;
            return false;
        }

        public bool TryReact(EventParameters parameters)
        {
            if (CanReact(parameters))
            {
                React(parameters);
                return true;
            }
            return false;
        }













        public void React(GameObject from, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            switch(ReactionType)
            {
                case Type.Reaction:
                    var obj = TargetObjectReference.GetTargetGameObject(new EventParameters()
                    {
                        Self = from,
                        TriggerObject = triggerObject,
                        PreviousTriggerObject = previousTriggerObject,
                        TriggerPosition = position,
                        PreviousTriggerPosition = position,
                    });
                    if(obj != null)
                        React(obj, ReactionName, triggerObject, position);

                    return;
                    
                case Type.Event:
                    Event?.Invoke();
                    return;
            }
        }
            
        public bool TryReact(GameObject from, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            if (CanReact(from, triggerObject, position, previousTriggerObject))
            {
                React(from, triggerObject, position, previousTriggerObject);
                return true;
            }
            return false;
        }


        public bool CanReact(GameObject from, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            switch (ReactionType)
            {
                case Type.Reaction:
                    var obj = TargetObjectReference.GetTargetGameObject(new EventParameters()
                    {
                        Self = from,
                        TriggerObject = triggerObject,
                        PreviousTriggerObject = previousTriggerObject,
                        TriggerPosition = position,
                        PreviousTriggerPosition = position,
                    });
                    if (obj != null)
                        CanReact(obj, ReactionName, triggerObject, position);

                    break;
                case Type.Event:
                    return true;
            }
            return true;
        }



        public static bool CanReact(GameObject targetObject, string reactionOrStateName, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            int potentialReactCount = 0;
            foreach (var reaction in targetObject.GetComponents<Reaction>())
                if (reaction.enabled && (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName))
                {
                    ++potentialReactCount;
                    if (reaction.CanReact(triggerObject, position))
                        return true;
                }
            foreach (var reaction in targetObject.GetComponents<ReactionState>())
                if (reaction.enabled && reaction.StateName == reactionOrStateName)
                {
                    ++potentialReactCount;
                    if (reaction.CanReact(triggerObject, position))
                        return true;
                }
            foreach (var sm in targetObject.GetComponents<ReactionStateMachine>())
                if(sm.enabled && sm.CanReact(reactionOrStateName, new EventParameters()
                    {
                        Self = targetObject,
                        TriggerObject = triggerObject,
                        PreviousTriggerObject = previousTriggerObject,
                        TriggerPosition = position,
                        PreviousTriggerPosition = position,
                    }))
                    return true;
            // can react when no potential reaction were found.
            if (potentialReactCount == 0) return true;
            return false;
        }
        public static bool TryReact(GameObject targetObject, string reactionOrStateName, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            if (CanReact(targetObject, reactionOrStateName, triggerObject, position))
            {
                React(targetObject, reactionOrStateName, triggerObject, position);
                return true;
            }
            return false;
        }
        public static void React(GameObject targetObject, string reactionOrStateName, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            foreach (var reaction in targetObject.GetComponents<Reaction>())
                if (reaction.enabled && (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName))
                    reaction.React(triggerObject, position);
            foreach (var reaction in targetObject.GetComponents<ReactionState>())
                if (reaction.enabled && reaction.StateName == reactionOrStateName)
                    reaction.React(triggerObject, position);
            foreach (var sm in targetObject.GetComponents<ReactionStateMachine>())
                if(sm.enabled)
                    sm.React(reactionOrStateName, new EventParameters()
                    {
                        Self = targetObject,
                        TriggerObject = triggerObject,
                        PreviousTriggerObject = previousTriggerObject,
                        TriggerPosition = position,
                        PreviousTriggerPosition = position,
                    });
        }
    }
}