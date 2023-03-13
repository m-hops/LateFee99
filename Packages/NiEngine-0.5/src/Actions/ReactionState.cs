using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie.Actions
{

    [Serializable, ClassPickerName("ReactionState")]
    public class ReactionState : IStateAction
    {
        public StateReactionReference Reaction;
        public void OnBegin(Owner owner, EventParameters parameters)
        {
            Reaction.ReactOnBegin(parameters);
        }
        public void OnEnd(Owner owner, EventParameters parameters)
        {
            Reaction.ReactOnEnd(parameters.WithOnBeginTrigger());
        }
    }
}