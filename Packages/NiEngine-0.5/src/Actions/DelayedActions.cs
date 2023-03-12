using System;
using UnityEngine;

namespace Nie.Actions
{

    [Serializable, ClassPickerName("Delayed Actions")]
    public class DelayedActions : Action, IUpdate
    {
        public float Seconds;
        public bool Repeat;

        public ActionSet Actions;

        [Tooltip("Will be set to Seconds when the state begin")]
        public float CurrentlyRemaining;

        public void Update(Owner owner, EventParameters parameters)
        {
            var nextTime = CurrentlyRemaining - Time.deltaTime;

            if (CurrentlyRemaining >= 0 && nextTime < 0)
            {
                Actions.Act(owner, parameters);
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
        public override void Act(Owner owner, EventParameters parameters)
        {
            CurrentlyRemaining = Seconds;
        }
    }
}