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
        //public string ReactionOnTriggerObject;
        public void React(EventParameters parameters)
        {
            if (ReactionReferences != null)
                foreach (var reaction in ReactionReferences)
                    reaction.TryReact(parameters);

            //if (!string.IsNullOrEmpty(ReactionOnTriggerObject))
            //    ReactionReference.TryReact(triggerObject, ReactionOnTriggerObject, targetObject, position, previousTriggerObject);
            //Events?.Invoke();
        }
        public bool CanReact(EventParameters parameters)
        {
            //if (!string.IsNullOrEmpty(ReactionOnTriggerObject) && !ReactionReference.CanReact(triggerObject, ReactionOnTriggerObject, targetObject, position, previousTriggerObject)) return false;
            return ReactionReferences == null
                || ReactionReferences.Count == 0
                || ReactionReferences.Any(x => x.CanReact(parameters));
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



        public void React(GameObject targetObject, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            if (ReactionReferences != null)
                foreach (var reaction in ReactionReferences)
                    reaction.TryReact(targetObject, triggerObject, position, previousTriggerObject);

            //if (!string.IsNullOrEmpty(ReactionOnTriggerObject))
            //    ReactionReference.TryReact(triggerObject, ReactionOnTriggerObject, targetObject, position, previousTriggerObject);
            //Events?.Invoke();
        }
        public bool TryReact(GameObject targetObject, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            if (CanReact(targetObject, triggerObject, position))
            {
                React(targetObject, triggerObject, position, previousTriggerObject);
                return true;
            }
            return false;
        }
        public bool CanReact(GameObject targetObject, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            //if (!string.IsNullOrEmpty(ReactionOnTriggerObject) && !ReactionReference.CanReact(triggerObject, ReactionOnTriggerObject, targetObject, position, previousTriggerObject)) return false;
            return ReactionReferences == null 
                || ReactionReferences.Count == 0
                || ReactionReferences.Any(x => x.CanReact(targetObject, triggerObject, position, previousTriggerObject));
        }
    }
}