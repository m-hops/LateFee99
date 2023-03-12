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



        public void React(GameObject from, GameObject targetObject, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            if (ReactionReferences != null)
                foreach (var reaction in ReactionReferences)
                    reaction.TryReact(EventParameters.Trigger(from, from, triggerObject, position).WithBegin(EventParameters.ParameterSet.Trigger(from, previousTriggerObject, position)));

            //if (!string.IsNullOrEmpty(ReactionOnTriggerObject))
            //    ReactionReference.TryReact(triggerObject, ReactionOnTriggerObject, targetObject, position, previousTriggerObject);
            //Events?.Invoke();
        }
        public bool TryReact(GameObject from, GameObject targetObject, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            if (CanReact(from, targetObject, triggerObject, position))
            {
                React(from, targetObject, triggerObject, position, previousTriggerObject);
                return true;
            }
            return false;
        }
        public bool CanReact(GameObject from, GameObject targetObject, GameObject triggerObject, Vector3 position, GameObject previousTriggerObject = null)
        {
            //if (!string.IsNullOrEmpty(ReactionOnTriggerObject) && !ReactionReference.CanReact(triggerObject, ReactionOnTriggerObject, targetObject, position, previousTriggerObject)) return false;
            return ReactionReferences == null 
                || ReactionReferences.Count == 0
                || ReactionReferences.Any(x => x.CanReact(
                    EventParameters.Trigger(from, from, triggerObject, position)
                    .WithBegin(EventParameters.ParameterSet.Trigger(from, previousTriggerObject, position))
                    ));
        }
    }
}