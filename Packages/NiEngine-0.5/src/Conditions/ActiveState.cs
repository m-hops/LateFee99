using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie.Conditions
{


    [Serializable, ClassPickerName("State")]
    public class ConditionActiveState : Condition
    {
        [Tooltip("In Seconds")]
        public GameObjectReference Target;
        public string State;
        public override bool Pass(Owner owner, EventParameters parameters)
        {
            var obj = Target.GetTargetGameObject(parameters);
            if(obj != null)
            {
                parameters = parameters.WithSelf(obj);
                return ReactionReference.IsActiveState(parameters, State);
            }
            return false;
        }
    }
}