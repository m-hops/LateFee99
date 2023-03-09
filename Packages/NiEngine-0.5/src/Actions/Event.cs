using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie.Actions
{

    [Serializable, ClassPickerName("Event")]
    public class Event : Action
    {
        public UnityEvent UnityEvent;
        public override void Act(Owner owner, EventParameters parameters)
        {
            UnityEvent?.Invoke();
        }
    }
}