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
        [SerializeField]
        public List<ReactionReference> ReactionReferences;
        //[SerializeField]
        //public UnityEvent Events;
        //[SerializeField]
        //public string ReactionOnTriggeringObject;
        public void React(GameObject targetObject, GameObject triggeringObject, Vector3 position, GameObject previousTriggeringObject = null)
        {
            if(ReactionReferences != null)
                foreach (var reaction in ReactionReferences)
                    reaction.TryReact(targetObject, triggeringObject, position, previousTriggeringObject);

            //if (!string.IsNullOrEmpty(ReactionOnTriggeringObject))
            //    ReactionReference.TryReact(triggeringObject, ReactionOnTriggeringObject, targetObject, position, previousTriggeringObject);
            //Events?.Invoke();
        }
        public bool TryReact(GameObject targetObject, GameObject triggeringObject, Vector3 position, GameObject previousTriggeringObject = null)
        {
            if (CanReact(targetObject, triggeringObject, position))
            {
                React(targetObject, triggeringObject, position, previousTriggeringObject);
                return true;
            }
            return false;
        }
        public bool CanReact(GameObject targetObject, GameObject triggeringObject, Vector3 position, GameObject previousTriggeringObject = null)
        {
            //if (!string.IsNullOrEmpty(ReactionOnTriggeringObject) && !ReactionReference.CanReact(triggeringObject, ReactionOnTriggeringObject, targetObject, position, previousTriggeringObject)) return false;
            return ReactionReferences == null 
                || ReactionReferences.Count == 0
                || ReactionReferences.Any(x => x.CanReact(targetObject, triggeringObject, position, previousTriggeringObject));
        }
    }
}