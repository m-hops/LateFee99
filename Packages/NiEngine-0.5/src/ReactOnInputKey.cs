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

        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;
        public GameObject TriggerObject => TriggerFromMainCamera ? Camera.main.gameObject : gameObject;
        public bool CanReact()
        {
            if(!enabled) return false;
            if (!Conditions.CanReact(null, transform.position)) return false;
            return true;
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode))
            {
                if (CanReact())
                    OnKeyDown.TryReact(TargetObject, TriggerObject, transform.position);
            }
            if (Input.GetKeyUp(KeyCode))
            {
                if (CanReact())
                    OnKeyUp.TryReact(TargetObject, TriggerObject, transform.position);
            }
            if (Input.GetKey(KeyCode))
            {
                if (CanReact())
                    WhenKeyPressed.TryReact(TargetObject, TriggerObject, transform.position);
            }
        }
    }
}