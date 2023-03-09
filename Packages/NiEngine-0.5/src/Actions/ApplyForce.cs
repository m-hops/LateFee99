using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie.Actions
{

    [Serializable, ClassPickerName("ApplyForce")]
    public class ApplyForce : Action
    {
        public GameObjectReference On;
        public PositionReference At;
        public DirectionReference Direction;
        public float Force;
        public GameObjectReference Opposit;
        public override void Act(Owner owner, EventParameters parameters)
        {
            var rb = On.GetTargetGameObject(parameters).GetComponent<Rigidbody>();
            var dir = Direction.GetDirection(parameters);
            Debug.Log($"Apply force to {rb.name}, dir={dir}, force={Force}");

            bool hasAt = At.TryGetPosition(parameters, out var at);
            if (hasAt)
                rb.AddForceAtPosition(dir * Force, at);
            else
                rb.AddForce(dir * Force);


            if (Opposit.TryGetTargetGameObject(parameters, out var opposit))
            {
                var oppositRb = opposit.GetComponent<Rigidbody>();
                if (hasAt)
                    oppositRb.AddForceAtPosition(dir * -Force, at);
                else
                    oppositRb.AddForce(dir * -Force);
            }

        }
    }
}