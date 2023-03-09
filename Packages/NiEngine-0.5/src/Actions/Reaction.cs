using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie.Actions
{

    [Serializable, ClassPickerName("Reaction")]
    public class Reaction : Action
    {
        public ReactionReference Reference;
        public override void Act(Owner owner, EventParameters parameters)
        {
            Reference.React(parameters);
        }
    }
}