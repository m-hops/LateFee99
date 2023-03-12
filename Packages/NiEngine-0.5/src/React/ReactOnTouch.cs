using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    [AddComponentMenu("Nie/Object/ReactOnTouch")]
    public class ReactOnTouch : MonoBehaviour
    {
        public bool DebugLog;

        public ConditionSet Conditions;

        public StateActionSet OnTouch;

        public ActionSet OnRelease;



        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;
        public bool CanTouch(TouchController by, Vector3 position)
        {
            if (!enabled) return false;

            var parameters = EventParameters.Trigger(gameObject, gameObject, by.gameObject, position);
            if (DebugLog)
                parameters = parameters.WithDebugTrace(new());
            bool pass = Conditions.Pass(new Owner(this), parameters);
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactOnTouch.CanTouch '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");

            return true;
        }

        public void Touch(TouchController by, Vector3 position)
        {
            var parameters = EventParameters.Trigger(gameObject, gameObject, by.gameObject, position);
            if (DebugLog)
                parameters = parameters.WithDebugTrace(new());
            OnTouch.OnBegin(new Owner(this), parameters);
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactOnTouch.Touch '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");

        }

        public void Release(TouchController by, Vector3 position)
        {
            var parameters = EventParameters.Trigger(gameObject, gameObject, by.gameObject);
            if (DebugLog)
                parameters = parameters.WithDebugTrace(new());
            OnTouch.OnEnd(new Owner(this), parameters);
            OnRelease.Act(new Owner(this), parameters);
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Grabbable.GrabBy '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
        }

    }
}