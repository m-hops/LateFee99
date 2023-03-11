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

        public GameObject TargetObject => gameObject;
        public GameObject TriggerObject => TriggerFromMainCamera ? Camera.main.gameObject : gameObject;
        public bool CanReact()
        {
            if(!enabled) return false;

            var parameters = EventParameters.Trigger(gameObject, gameObject, TriggerObject, transform.position);
            if (DebugLog)
            {
                Debug.Log($"[{Time.frameCount}] ReactOnInputKey.CanReact '{name}' {parameters}");
                parameters = parameters.WithDebugTrace(new());
            }
            bool pass = NewConditions.Pass(new Owner(this), parameters);
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactOnInputKey.CanReact '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");

            return pass;
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode))
            {

                if (CanReact())
                {
                    var parameters = EventParameters.Trigger(gameObject, gameObject, TriggerObject, transform.position);
                    if (DebugLog)
                    {
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey.OnKeyDown '{name}' {parameters}");
                        parameters = parameters.WithDebugTrace(new());
                    }
                    NewOnKeyDown.OnBegin(new Owner(this), parameters);
                    if (DebugLog)
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey.OnKeyDown '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
                }
            }
            if (Input.GetKeyUp(KeyCode))
            {
                if (CanReact())
                {
                    var parameters = EventParameters.Trigger(gameObject, gameObject, TriggerObject, transform.position);
                    if (DebugLog)
                    {
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey.OnKeyUp '{name}' {parameters}");
                        parameters = parameters.WithDebugTrace(new());
                    }
                    NewOnKeyDown.OnEnd(new Owner(this), parameters);
                    NewOnKeyUp.Act(new Owner(this), parameters);
                    if (DebugLog)
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey.OnKeyUp '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
                }
            }
            if (Input.GetKey(KeyCode))
            {
                if (CanReact())
                {
                    var parameters = EventParameters.Trigger(gameObject, gameObject, TriggerObject, transform.position);
                    if (DebugLog)
                    {
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey.WhenKeyPressed '{name}' {parameters}");
                        parameters = parameters.WithDebugTrace(new());
                    }
                    NewWhenKeyPressed.Act(new Owner(this), parameters);
                    if (DebugLog)
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey.WhenKeyPressed '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
                }
            }
        }
    }
}