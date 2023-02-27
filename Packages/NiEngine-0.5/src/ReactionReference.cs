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
            TriggeringObject,
            PreviousTriggeringObject,
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



        public void React(GameObject from, GameObject triggeringObject, Vector3 position, GameObject previousTriggeringObject = null)
        {
            switch(ReactionType)
            {
                case Type.Reaction:
                    switch (ObjectTargetType)
                    {
                        case TargetType.Self:
                            React(from, ReactionName, triggeringObject, position);
                            return;
                        case TargetType.Other:
                            React(TargetObject, ReactionName, triggeringObject, position);
                            return;
                        case TargetType.TriggeringObject:
                            if (triggeringObject == null) return;
#if UNITY_EDITOR
                            if (!HasReaction(triggeringObject, ReactionName))
                                Debug.LogWarning($"[{Time.frameCount}] Could not find reaction '{ReactionName}' on triggering object '{triggeringObject.GetNameOrNull()}'. Triggered from '{from.GetNameOrNull()}' at position: {position}", from);
#endif
                            React(triggeringObject, ReactionName, from, position);
                            return;
                        case TargetType.PreviousTriggeringObject:
                            if (previousTriggeringObject == null) return;
#if UNITY_EDITOR
                            if (!HasReaction(previousTriggeringObject, ReactionName))
                                Debug.LogWarning($"[{Time.frameCount}] Could not find reaction '{ReactionName}' on previous triggering object '{previousTriggeringObject.GetNameOrNull()}'. Triggered from '{from.GetNameOrNull()}' at position: {position}", from);
#endif
                            React(previousTriggeringObject, ReactionName, from, position);
                            return;
                    }
                    return;
                case Type.Event:
                    Event?.Invoke();
                    return;
            }
        }
            
        public bool TryReact(GameObject from, GameObject triggeringObject, Vector3 position, GameObject previousTriggeringObject = null)
        {
            if (CanReact(from, triggeringObject, position, previousTriggeringObject))
            {
                React(from, triggeringObject, position, previousTriggeringObject);
                return true;
            }
            return false;
        }

        public bool CanReact(GameObject from, GameObject triggeringObject, Vector3 position, GameObject previousTriggeringObject = null)
        {
            switch (ReactionType)
            {
                case Type.Reaction:
                    switch (ObjectTargetType)
                    {
                        case TargetType.Self:
                            return CanReact(from, ReactionName, triggeringObject, position);
                        case TargetType.Other:
                            return CanReact(TargetObject, ReactionName, triggeringObject, position);
                        case TargetType.TriggeringObject:
                            if (triggeringObject == null) return true;
#if UNITY_EDITOR
                            if (!HasReaction(triggeringObject, ReactionName))
                                Debug.LogWarning($"[{Time.frameCount}] Could not find reaction '{ReactionName}' on triggering object '{triggeringObject.GetNameOrNull()}'. Triggered from '{from.GetNameOrNull()}' at position: {position}", from);
#endif
                            return CanReact(triggeringObject, ReactionName, from, position);
                        case TargetType.PreviousTriggeringObject:
                            if (previousTriggeringObject == null) return true;
#if UNITY_EDITOR
                            if (!HasReaction(previousTriggeringObject, ReactionName))
                                Debug.LogWarning($"[{Time.frameCount}] Could not find reaction '{ReactionName}' on previous triggering object '{previousTriggeringObject.GetNameOrNull()}'. Triggered from '{from.GetNameOrNull()}' at position: {position}", from);
#endif
                            return CanReact(previousTriggeringObject, ReactionName, from, position);
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
        public static bool CanReact(GameObject targetObject, string reactionOrStateName, GameObject triggeringObject, Vector3 position, GameObject previousTriggeringObject = null)
        {
            int potentialReactCount = 0;
            foreach (var reaction in targetObject.GetComponents<Reaction>())
                if (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName)
                {
                    ++potentialReactCount;
                    if (reaction.CanReact(triggeringObject, position))
                        return true;
                }
            foreach (var reaction in targetObject.GetComponents<ReactionState>())
                if (reaction.StateName == reactionOrStateName)
                {
                    ++potentialReactCount;
                    if (reaction.CanReact(triggeringObject, position))
                        return true;
                }
            // can react when no potential reaction were found.
            if (potentialReactCount == 0) return true;
            return false;
        }
        public static bool TryReact(GameObject targetObject, string reactionOrStateName, GameObject triggeringObject, Vector3 position, GameObject previousTriggeringObject = null)
        {
            if (CanReact(targetObject, reactionOrStateName, triggeringObject, position))
            {
                React(targetObject, reactionOrStateName, triggeringObject, position);
                return true;
            }
            return false;
        }
        public static void React(GameObject targetObject, string reactionOrStateName, GameObject triggeringObject, Vector3 position)
        {
            foreach (var reaction in targetObject.GetComponents<Reaction>())
                if (string.IsNullOrEmpty(reaction.ReactionName) || reaction.ReactionName == reactionOrStateName)
                    reaction.React(triggeringObject, position);
            foreach (var reaction in targetObject.GetComponents<ReactionState>())
                if (reaction.StateName == reactionOrStateName)
                    reaction.React(triggeringObject, position);
        }
    }
}