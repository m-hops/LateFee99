using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    [AddComponentMenu("Nie/Object/Reaction")]
    public class Reaction : MonoBehaviour
    {

        [Tooltip("Name of this reaction. If left empty, will react for every react call of this object")]
        public string ReactionName;
        [Tooltip("Print to console events caused by this Reaction")]
        public bool DebugLog = false;

#if UNITY_EDITOR
        [EditorCools.Button]
        public void Trigger()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                React();
        }
#endif

        [Header("Conditions:")]
        [Tooltip("Once this Reaction reacts, it cannot react again within the cooldown period, in seconds.")]
        public float ReactionCooldown = 0;

        [Header("Actions:")]
        [Tooltip("When reaction is activated, destroy this GameObject")]
        public bool DestroyGameObject;

        [Tooltip("If Destroy is checked, instantiate the provided GameObject in its place, with the same transform.")]
        public GameObject ReplaceWith;

        [Tooltip("If set, instantiate the provided GameObject at the reaction position")]
        public GameObject Spawn;

        [Tooltip("Default Reaction position, used when spawning the GameObject from the property 'Spawn'")]
        public Transform DefaultReactionPosition;

        [Tooltip("If set, move the GameObject that triggered this reaction. The GameObject may be null")]
        public Transform MoveTriggeringObjectAt;

        [Tooltip("Will release this object if it has a Grabbable component and is currently grabbed")]
        public bool ReleaseGrabbed;

        public AnimatorStateReference PlayAnimatorState;

        [Header("Overrides:")]
        [Tooltip("If set, will execute the reaction on provided object instead of the object with this ReactionState.")]
        public GameObject ThisObject;
        [Tooltip("If set, will execute the reaction using provided object as the triggering object.")]
        public GameObject TriggeringObject;


        [Header("Events:")]
        [SerializeField]
        [Tooltip("Event called when the reaction happens. Parameters are (Reaction this, GameObject triggeringObject)")]
        UnityEvent<Reaction, GameObject> OnReact;

        [SerializeField]
        [Tooltip("Event called when the reaction cooldown is over")]
        UnityEvent OnCooldownOver;

        float m_ReactionCooldown = 0;

        public Vector3 ReactionPosition => DefaultReactionPosition != null ? DefaultReactionPosition.position : transform.position;

        public GameObject TargetObject => ThisObject != null ? TargetObject : gameObject;
        public GameObject GetTargetTriggeringObject(GameObject triggeringObject) => TriggeringObject != null ? TriggeringObject : triggeringObject;
        private void Update()
        {
            if (m_ReactionCooldown > 0)
            {
                m_ReactionCooldown -= Time.deltaTime;
                if (m_ReactionCooldown < 0)
                    OnCooldownOver?.Invoke();
            }
        }

        public bool CanReact(GameObject from, Vector3 position)
        {
            return m_ReactionCooldown <= 0;
        }
        public void React() => React(null, ReactionPosition);
        public bool TryReact(GameObject triggeringObject, Vector3 position)
        {
            triggeringObject = GetTargetTriggeringObject(triggeringObject);
            if (!CanReact(triggeringObject, position)) 
                return false;
            React(triggeringObject, position);
            return true;
        }
        public void React(GameObject triggeringObject, Vector3 position)
        {

            triggeringObject = GetTargetTriggeringObject(triggeringObject);
            var thisObject = TargetObject;
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Reaction '{name}' reacts (triggeringObject: '{triggeringObject.name}', position: {position}");

            if (MoveTriggeringObjectAt != null && triggeringObject != null)
            {
                triggeringObject.transform.position = MoveTriggeringObjectAt.transform.position;
                triggeringObject.transform.rotation = MoveTriggeringObjectAt.transform.rotation;
                if (triggeringObject.TryGetComponent<Grabbable>(out var grabbable))
                    grabbable.ReleaseIfGrabbed();
            }

            if (Spawn != null)
                Instantiate(Spawn, position, Quaternion.identity);

            if (ReleaseGrabbed && thisObject.TryGetComponent<Grabbable>(out var grabbable2))
                grabbable2.ReleaseIfGrabbed();

            if (PlayAnimatorState.Animator != null)
                PlayAnimatorState.Animator.Play(PlayAnimatorState.StateHash);

            OnReact?.Invoke(this, triggeringObject);


            if (DestroyGameObject)
            {
                var pos = transform.position;
                var rot = transform.rotation;
                Destroy(thisObject);
                if (ReplaceWith != null)
                    Instantiate(ReplaceWith, pos, rot);
            }
            else if (ReactionCooldown > 0)
            {
                m_ReactionCooldown = ReactionCooldown;
            }
        }
    }
}