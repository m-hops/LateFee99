using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie.Actions
{

    [Serializable, ClassPickerName("Enable")]
    public class EnableDisable : StateAction
    {
        public GameObjectReference Target;
        public bool Enable;
        public bool RevertAtEnd;
        [Serializable]
        public struct InternalState
        {
            public GameObject TargetObject;
            public bool WasActive;

        }
        public InternalState Internals;
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            var target = Target.GetTargetGameObject(parameters);
            if (target != null)
            {
                Internals.TargetObject = target;
                if (RevertAtEnd)
                    Internals.WasActive = target.activeSelf;
                target.SetActive(Enable);

            }
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            if (RevertAtEnd && Internals.TargetObject != null)
                Internals.TargetObject.SetActive(Internals.WasActive);
        }
    }
}