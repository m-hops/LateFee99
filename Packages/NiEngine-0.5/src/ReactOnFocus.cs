using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    [AddComponentMenu("Nie/Object/ReactOnFocus")]
    public class ReactOnFocus : MonoBehaviour
    {
        public bool DebugLog;
        public bool ShowHand = true;
        //public bool ReactOnColliderObject = true;
        public bool SendFocusToRigidBodyObject = true;

        //[UnityEngine.Serialization.FormerlySerializedAs("NewConditions")]
        public ConditionSet NewConditions;

        //[UnityEngine.Serialization.FormerlySerializedAs("NewOnFocus")]
        public StateActionSet NewOnFocus;

        //[UnityEngine.Serialization.FormerlySerializedAs("NewOnUnfocus")]
        public ActionSet NewOnUnfocus;

        EventParameters MakeEvent(GameObject trigger, Vector3 position)
        {
            var parameters = EventParameters.Trigger(gameObject, gameObject, trigger, position);
            if (DebugLog)
                parameters = parameters.WithDebugTrace(new());
            return parameters;
        }

        public bool CanReact(FocusController by, Vector3 position)
        {
            if (!enabled) return false;
            var parameters = MakeEvent(by.gameObject, position);
            bool pass = CanReact(parameters);
            if (parameters.HasTraces)
                Debug.Log($"[{Time.frameCount}] ReactOnFocus.CanReact '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
            return pass;
        }

        public bool CanReact(EventParameters parameters)
        {
            if (!enabled) return false;
            bool pass = NewConditions.Pass(new Owner(this), parameters);
            return pass;
        }

        public void Focus(FocusController by, Vector3 position)
        {
            var parameters = MakeEvent(by.gameObject, position);
            NewOnFocus.OnBegin(new Owner(this), parameters);
            if (parameters.HasTraces)
                Debug.Log($"[{Time.frameCount}] ReactOnFocus.OnFocus '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
        }

        public void Unfocus(FocusController by, Vector3 position)
        {
            var parameters = MakeEvent(by.gameObject, position);
            NewOnFocus.OnEnd(new Owner(this), parameters);
            NewOnUnfocus.Act(new Owner(this), parameters);
            if (parameters.HasTraces)
                Debug.Log($"[{Time.frameCount}] ReactOnFocus.OnUnfocus '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
        }
    }
}