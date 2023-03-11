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

        [Tooltip("Conditions to react")]
        public ReactionConditions Conditions;

        [Tooltip("Reaction executed when key has been pressed down.")]
        public ReactionList OnKeyDown;

        [Tooltip("Reaction executed when input is pressed.")]
        public ReactionList WhenKeyPressed;

        [Tooltip("Reaction executed when key has been released.")]
        public ReactionList OnKeyUp;

        public bool DebugLog;
        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;
        public GameObject TriggerObject => TriggerFromMainCamera ? Camera.main.gameObject : gameObject;
        public bool CanReact()
        {
            if(!enabled) return false;
            if (!Conditions.CanReactAll(gameObject, TriggerObject, transform.position, previousTriggerObjectIfExist: null)) return false;
            return true;
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode))
            {
                if(DebugLog)
                    Debug.Log($"[{Time.frameCount}] ReactOnInputKey '{name}' received OnKeyDown '{KeyCode}'", this);
                if (CanReact())
                {
                    OnKeyDown.TryReact(gameObject, TargetObject, TriggerObject, transform.position);
                    if(DebugLog)
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey '{name}' react OnKeyDown '{KeyCode}'", this);
                }
            }
            if (Input.GetKeyUp(KeyCode))
            {
                if (DebugLog)
                    Debug.Log($"[{Time.frameCount}] ReactOnInputKey '{name}' received OnKeyUp '{KeyCode}'", this);
                if (CanReact())
                {
                    OnKeyUp.TryReact(gameObject, TargetObject, TriggerObject, transform.position);
                    if (DebugLog)
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey '{name}' react OnKeyUp '{KeyCode}'", this);
                }
            }
            if (Input.GetKey(KeyCode))
            {
                if (DebugLog)
                    Debug.Log($"[{Time.frameCount}] ReactOnInputKey '{name}' received WhenKeyPressed '{KeyCode}'", this);
                if (CanReact())
                {
                    WhenKeyPressed.TryReact(gameObject, TargetObject, TriggerObject, transform.position);
                    if (DebugLog)
                        Debug.Log($"[{Time.frameCount}] ReactOnInputKey '{name}' react WhenKeyPressed '{KeyCode}'", this);
                }
            }
        }
    }
}