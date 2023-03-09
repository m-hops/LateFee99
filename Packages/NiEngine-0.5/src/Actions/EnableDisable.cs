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

        GameObject TargetObject;
        bool WasActive;
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            var target = Target.GetTargetGameObject(parameters);
            if (target != null)
            {
                TargetObject = target;
                if (RevertAtEnd)
                    WasActive = target.activeSelf;
                target.SetActive(Enable);

            }
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            if (RevertAtEnd && TargetObject != null)
                TargetObject.SetActive(WasActive);
        }
    }
}