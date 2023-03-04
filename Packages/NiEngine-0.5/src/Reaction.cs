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
        public bool RunOnStart;

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
        [UnityEngine.Serialization.FormerlySerializedAs("MoveTriggeringObjectAt")]
        public Transform MoveTriggerObjectAt;

        [Tooltip("Will release this object if it has a Grabbable component and is currently grabbed")]
        public bool ReleaseGrabbed;

        public AnimatorStateReference PlayAnimatorState;

        [Header("Overrides:")]
        [Tooltip("If set, will execute the reaction on provided object instead of the object with this ReactionState.")]
        public GameObject ThisObject;
        [Tooltip("If set, will execute the reaction using provided object as the trigger object.")]
        [UnityEngine.Serialization.FormerlySerializedAs("TriggeringObject")]
        public GameObject TriggerObject;


        [Header("Events:")]
        [SerializeField]
        [Tooltip("Event called when the reaction happens. Parameters are (Reaction this, GameObject triggerObject)")]
        UnityEvent<Reaction, GameObject> OnReact;

        [SerializeField]
        [Tooltip("Event called when the reaction cooldown is over")]
        UnityEvent OnCooldownOver;

        float m_ReactionCooldown = 0;

        public Vector3 ReactionPosition => DefaultReactionPosition != null ? DefaultReactionPosition.position : transform.position;
        Vector3 GetReactionPosition(Vector3 receivedPosition) => DefaultReactionPosition != null ? DefaultReactionPosition.position : receivedPosition;

        public GameObject TargetObject => ThisObject != null ? TargetObject : gameObject;
        public GameObject GetOverriddenTriggerObject(GameObject triggerObject) => TriggerObject != null ? TriggerObject : triggerObject;
        private void Start()
        {
            if(RunOnStart)
                React(gameObject, transform.position);
        }
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
        public bool TryReact(GameObject triggerObject, Vector3 position)
        {
            triggerObject = GetOverriddenTriggerObject(triggerObject);
            if (!CanReact(triggerObject, position)) 
                return false;
            React(triggerObject, position);
            return true;
        }
        public void React(GameObject triggerObject, Vector3 position)
        {

            triggerObject = GetOverriddenTriggerObject(triggerObject);
            var thisObject = TargetObject;
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Reaction '{name}' reacts (TriggerObject: '{triggerObject.name}', position: {position}");

            if (MoveTriggerObjectAt != null && triggerObject != null)
            {
                triggerObject.transform.position = MoveTriggerObjectAt.transform.position;
                triggerObject.transform.rotation = MoveTriggerObjectAt.transform.rotation;
                if (triggerObject.TryGetComponent<Rigidbody>(out var rigidBody))
                {
                    rigidBody.velocity = Vector3.zero;
                    rigidBody.angularVelocity = Vector3.zero;
                }
                if (triggerObject.TryGetComponent<Grabbable>(out var grabbable))
                    grabbable.ReleaseIfGrabbed();
            }

            if (Spawn != null)
                Instantiate(Spawn, GetReactionPosition(position), Quaternion.identity);

            if (ReleaseGrabbed && thisObject.TryGetComponent<Grabbable>(out var grabbable2))
                grabbable2.ReleaseIfGrabbed();

            if (PlayAnimatorState.Animator != null)
                PlayAnimatorState.Animator.Play(PlayAnimatorState.StateHash);

            OnReact?.Invoke(this, triggerObject);


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