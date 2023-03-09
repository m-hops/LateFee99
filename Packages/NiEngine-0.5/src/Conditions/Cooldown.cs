using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie.Conditions
{

    [Serializable, ClassPickerName("Cooldown")]
    public class ConditionCooldown : Condition, IStateObserver, IUpdate, IInitialize
    {
        [Tooltip("In Seconds")]
        public float TimeInSeconds;
        float TimeLeft;
        public override bool Pass(Owner owner, EventParameters parameters)
        {
            return TimeLeft <= 0;
        }
        void IUpdate.Update(Owner owner)
        {
            TimeLeft -= Time.deltaTime;
        }
        void IInitialize.Initialize(Owner owner)
        {
            TimeLeft = TimeInSeconds;
        }
        void IStateObserver.OnBegin(Owner owner, EventParameters parameters)
        {

        }
        void IStateObserver.OnEnd(Owner owner, EventParameters parameters)
        {
            TimeLeft = TimeInSeconds;
        }
    }
}