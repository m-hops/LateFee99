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
        public TargetType ObjectTargetType;

        
        #region /////////////////////////// Type Reaction
        public GameObject TargetObject;
        public string ReactionName;
        #endregion

        #region /////////////////////////// Type Event

        public UnityEvent Event;
        #endregion



        public void React(GameObject from, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            switch(ReactionType)
            {
                case Type.Reaction:
                    switch (ObjectTargetType)
                    {
                        case TargetType.Self:
                            React(from, ReactionName, triggerObject, position);
                            return;
                        case TargetType.Other:
                            React(TargetObject, ReactionName, triggerObject, position);
                            return;
                        case TargetType.TriggerObject:
                            if (triggerObject == null) return;
#if UNITY_EDITOR
                            if (!HasReaction(triggerObject, ReactionName))
                                Debug.LogWarning($"[{Time.frameCount}] Could not find reaction '{ReactionName}' on trigger object '{triggerObject.GetNameOrNull()}'. Triggered from '{from.GetNameOrNull()}' at position: {position}", from);
#endif
                            React(triggerObject, ReactionName, from, position);
                            return;
                        case TargetType.PreviousTriggerObject:
                            if (previousTriggerObject == null) return;
#if UNITY_EDITOR
                            if (!HasReaction(previousTriggerObject, ReactionName))
                                Debug.LogWarning($"[{Time.frameCount}] Could not find reaction '{ReactionName}' on previous trigger object '{previousTriggerObject.GetNameOrNull()}'. Triggered from '{from.GetNameOrNull()}' at position: {position}", from);
#endif
                            React(previousTriggerObject, ReactionName, from, position);
                            return;
                    }
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
                    switch (ObjectTargetType)
                    {
                        case TargetType.Self:
                            return CanReact(from, ReactionName, triggerObject, position);
                        case TargetType.Other:
                            return CanReact(TargetObject, ReactionName, triggerObject, position);
                        case TargetType.TriggerObject:
                            if (triggerObject == null) return true;
#if UNITY_EDITOR
                            if (!HasReaction(triggerObject, ReactionName))
                                Debug.LogWarning($"[{Time.frameCount}] Could not find reaction '{ReactionName}' on Trigger object '{triggerObject.GetNameOrNull()}'. Triggered from '{from.GetNameOrNull()}' at position: {position}", from);
#endif
                            return CanReact(triggerObject, ReactionName, from, position);
                        case TargetType.PreviousTriggerObject:
                            if (previousTriggerObject == null) return true;
#if UNITY_EDITOR
                            if (!HasReaction(previousTriggerObject, ReactionName))
                                Debug.LogWarning($"[{Time.frameCount}] Could not find reaction '{ReactionName}' on previous trigger object '{previousTriggerObject.GetNameOrNull()}'. Triggered from '{from.GetNameOrNull()}' at position: {position}", from);
#endif
                            return CanReact(previousTriggerObject, ReactionName, from, position);
                    }
                    break;
                case Type.Event:
                    return true;
            }
            return true;
        }



        public static bool HasReaction(GameObject obj, string reactionOrStateName)
        {
            return obj.GetComponents<Reaction>().Any(x => x.ReactionName == reactionOrStateName)
                || obj.GetComponents<ReactionState>().Any(x => x.StateName == reactionOrStateName);
        }
        public static bool CanReact(GameObject targetObject, string reactionOrStateName, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            int potentialReactCount = 0;
            foreach (var reaction in targetObject.GetComponents<Reaction>())
                if (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName)
                {
                    ++potentialReactCount;
                    if (reaction.CanReact(triggerObject, position))
                        return true;
                }
            foreach (var reaction in targetObject.GetComponents<ReactionState>())
                if (reaction.StateName == reactionOrStateName)
                {
                    ++potentialReactCount;
                    if (reaction.CanReact(triggerObject, position))
                        return true;
                }
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
        public static void React(GameObject targetObject, string reactionOrStateName, GameObject triggerObject, Vector3 position)
        {
            foreach (var reaction in targetObject.GetComponents<Reaction>())
                if (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName)
                    reaction.React(triggerObject, position);
            foreach (var reaction in targetObject.GetComponents<ReactionState>())
                if (reaction.StateName == reactionOrStateName)
                    reaction.React(triggerObject, position);
        }
    }
}