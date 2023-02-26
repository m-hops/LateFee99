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
        public GameObject TargetObject;
        public string ReactionName;
        public void React(GameObject triggeringObject, Vector3 position)
            => React(TargetObject, ReactionName, triggeringObject, position);
        public bool TryReact(GameObject triggeringObject, Vector3 position)
            => TryReact(TargetObject, ReactionName, triggeringObject, position);
        public bool CanReact(GameObject triggeringObject, Vector3 position)
            => CanReact(TargetObject, ReactionName, triggeringObject, position);

        public static bool HasReaction(GameObject obj, string reactionOrStateName)
        {
            return obj.GetComponents<Reaction>().Any(x => x.ReactionName == reactionOrStateName)
                || obj.GetComponents<ReactionState>().Any(x => x.StateName == reactionOrStateName);
        }
        public static bool CanReact(GameObject targetObject, string reactionOrStateName, GameObject triggeringObject, Vector3 position)
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
        public static bool TryReact(GameObject targetObject, string reactionOrStateName, GameObject triggeringObject, Vector3 position)
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