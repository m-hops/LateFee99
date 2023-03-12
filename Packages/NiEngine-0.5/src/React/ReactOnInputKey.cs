using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    public class ReactOnInputKey : MonoBehaviour
    {

        [Header("Input:")]
        public KeyCode KeyCode;
        public bool TriggerFromMainCamera = true;


        //[UnityEngine.Serialization.FormerlySerializedAs("NewConditions")]
        public ConditionSet NewConditions;

        //[UnityEngine.Serialization.FormerlySerializedAs("NewOnKeyDown")]
        public StateActionSet NewOnKeyDown;

        //[UnityEngine.Serialization.FormerlySerializedAs("NewWhenKeyPressed")]
        public ActionSet NewWhenKeyPressed;

        //[UnityEngine.Serialization.FormerlySerializedAs("NewOnKeyUp")]
        public ActionSet NewOnKeyUp;

        public bool DebugLog;
        bool m_ReactedOnDown;
        public GameObject TargetObject => gameObject;
        public GameObject TriggerObject => TriggerFromMainCamera ? Camera.main.gameObject : gameObject;
        public bool CanReact()
        {
            if(!enabled) return false;

            var parameters = MakeEvent();
            bool pass = CanReact(parameters);
            if (parameters.HasTraces)
                Debug.Log($"[{Time.frameCount}] ReactOnInputKey.CanReact '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");

            return pass;
        }
        public bool CanReact(EventParameters parameters)
        {
            if (!enabled) return false;
            bool pass = NewConditions.Pass(new Owner(this), parameters);
            return pass;
        }
        EventParameters MakeEvent()
        {
            var parameters = EventParameters.Trigger(gameObject, gameObject, TriggerObject, transform.position);
            if (DebugLog)
                parameters = parameters.WithDebugTrace(new());
            return parameters;
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode))
            {
                var parameters = MakeEvent();
                if (CanReact(parameters))
                {
                    m_ReactedOnDown = true;
                    NewOnKeyDown.OnBegin(new Owner(this), parameters);
                    if (parameters.HasTraces)
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey.OnKeyDown '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
                }
            }
            if (Input.GetKeyUp(KeyCode))
            {
                var parameters = MakeEvent();
                if (m_ReactedOnDown || CanReact(parameters))
                {
                    m_ReactedOnDown = false;
                    NewOnKeyDown.OnEnd(new Owner(this), parameters);
                    NewOnKeyUp.Act(new Owner(this), parameters);
                    if (parameters.HasTraces)
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey.OnKeyUp '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
                }
            }
            if (Input.GetKey(KeyCode))
            {
                var parameters = MakeEvent();
                if (CanReact(parameters))
                {
                    NewWhenKeyPressed.Act(new Owner(this), parameters);
                    if (parameters.HasTraces)
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey.WhenKeyPressed '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
                }
            }
        }
    }
}