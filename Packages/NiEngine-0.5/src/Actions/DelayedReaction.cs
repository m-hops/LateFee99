using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie.Actions
{

    [Serializable, ClassPickerName("Delayed Reaction")]
    public class DelayedReaction : StateAction, IUpdate
    {
        public float Seconds;
        public bool Repeat;

        public ReactionList Reaction;

        [Tooltip("Will be set to Seconds when the state begin")]
        public float CurrentlyRemaining;

        public void Update(Owner owner, EventParameters parameters)
        {
            var nextTime = CurrentlyRemaining - Time.deltaTime;

            if (CurrentlyRemaining >= 0 && nextTime < 0)
            {
                Debug.Log($"DelayedReaction on State '{owner.State?.StateName.Name}' {owner.StateMachine?.name} {parameters}");
                Reaction.React(parameters);
                if (Repeat)
                    CurrentlyRemaining = Seconds;
                else
                    CurrentlyRemaining = nextTime;
            }
            else
            {
                CurrentlyRemaining = nextTime;
            }
        }
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            CurrentlyRemaining = Seconds;
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
        }
    }
}