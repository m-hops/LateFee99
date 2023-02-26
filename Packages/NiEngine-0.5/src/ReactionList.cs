using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    [System.Serializable]
    public struct ReactionList
    {
        public List<ReactionReference> ReactionReferences;
        //public List<ReactionStateReference> SetReactionStates;
        public UnityEvent Events;
        public string ReactionOnTriggeringObject;
        public void React(GameObject targetObject, GameObject triggeringObject, Vector3 position)
        {
            foreach (var reaction in ReactionReferences)
                reaction.TryReact(triggeringObject, position);

            if (!string.IsNullOrEmpty(ReactionOnTriggeringObject))
                ReactionReference.TryReact(triggeringObject, ReactionOnTriggeringObject, targetObject, position);
            Events?.Invoke();
        }
        public bool TryReact(GameObject targetObject, GameObject triggeringObject, Vector3 position)
        {
            if (CanReact(targetObject, triggeringObject, position))
            {
                React(targetObject, triggeringObject, position);
                return true;
            }
            return false;
        }
        public bool CanReact(GameObject targetObject, GameObject triggeringObject, Vector3 position)
        {
            if (!string.IsNullOrEmpty(ReactionOnTriggeringObject) && !ReactionReference.CanReact(triggeringObject, ReactionOnTriggeringObject, targetObject, position)) return false;
            return ReactionReferences.Count == 0//+ SetReactionStates.Count == 0
                || ReactionReferences.Any(x => x.CanReact(triggeringObject, position));
                //|| SetReactionStates.Any(x => x.CanReact(triggeringObject, position));
        }
    }
}