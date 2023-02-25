using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    public static class GameObjectExt
    {
        public static bool TryGetReactionState(this GameObject @this, string name, out ReactionState state)
        {
            foreach(var rs in @this.GetComponents<ReactionState>().Where(x=>x.StateName == name))
            {
                state = rs;
                return true;
            }
            state = null;
            return false;
        }
        public static bool TryGetReactionState(this GameObject @this, string name, string group, out ReactionState state)
        {
            foreach (var rs in @this.GetComponents<ReactionState>().Where(x => x.StateName == name && x.StateGroup == group))
            {
                state = rs;
                return true;
            }
            state = null;
            return false;
        }
    }
    [System.Serializable]
    public struct ReactionStateReference
    {
        public ReactionState Object;
        public string StateName;
        public bool IsActiveState
        {
            get
            {
                if (TryGetState(out var state))
                    return state.IsActiveState;
                return false;
            }
        }
        public string ActiveStateName
        {
            get
            {
                if(TryGetState(out var state))
                {
                    foreach (var reactionState in Object.GetComponents<ReactionState>())
                    {
                        if (reactionState.StateGroup == state.StateGroup && reactionState.IsActiveState)
                            return reactionState.StateName;
                    }
                }
                return null;
            }
        }
        public bool CanReact(GameObject triggeringObject, Vector3 position)
        {
            if (TryGetState(out var state))
                return state.CanReact(triggeringObject, position);
            return false;
        }
        public bool TryReact(GameObject triggeringObject, Vector3 position)
        {
            if(TryGetState(out var state))
                return state.TryReact(triggeringObject, position);
            return false;
        }
        public bool TryGetState(out ReactionState state)
        {
            foreach(var reactionState in Object.GetComponents<ReactionState>())
            {
                if(reactionState.StateName == StateName)
                {
                    state = reactionState;
                    return true;
                }
            }
            state = null;
            return false;
        }
    }
    public interface IReaction
    {
        bool CanReact(GameObject from, Vector3 position);
        bool TryReact(GameObject triggeringObject, Vector3 position);
        void React(GameObject triggeringObject, Vector3 position);
    }
    [AddComponentMenu("Nie/Object/ReactionState")]
    public class ReactionState : MonoBehaviour, IReaction
    {
        [Tooltip("Name of this state. Used when activating state.")]
        public string StateName;

        [Tooltip("This state is mutually exclusive will all ReactionState of the same group on this GameObject")]
        public string StateGroup;

        public bool IsInitialState;

        [Tooltip("Once this Reaction reacts, it cannot react again within the cooldown period, in seconds.")]
        public float ReactionCooldown = 0;


        [Header("Reaction:")]
        [Tooltip("If set, instantiate the provided GameObject at the reaction position")]
        public GameObject Spawn;

        [Tooltip("Default Reaction position, used when spawning the GameObject from the property 'Spawn'")]
        public Transform DefaultReactionPosition;

        public AnimatorStateReference PlayAnimatorState;

        [Header("Reaction on this object:")]
        public bool SetKinematic;
        public bool SetNonKinematic;
        bool m_PreviousKinematic;

        [Tooltip("Will release this object if it has a Grabbable component and is currently grabbed")]
        public bool ReleaseGrabbed;


        [Header("Reaction on Triggering Object:")]
        [Tooltip("If set, move the GameObject that triggered this reaction. The GameObject may be null")]
        public Transform MoveTriggeringObjectAt;
        [Tooltip("If set, activate the first ReactionState found of the provided name from the GameObject that triggered this reaction when this state is activated.")]
        public string ForceReactionStateOnTriggeringObjectOnBegin;
        [Tooltip("If set, activate the first ReactionState found of the provided name from the GameObject that triggered this reaction when this state is deactivated.")]
        public string ForceReactionStateOnTriggeringObjectOnEnd;


        [Header("Debug:")]
        [Tooltip("Print to console events caused by this Reaction")]
        public bool DebugLog = false;
        public bool IsActiveState;// { get; private set; }

        [SerializeField]
        [Tooltip("Event called when the reaction state begin. Parameters are (ReactionState this, GameObject triggeringObject)")]
        UnityEvent<ReactionState, GameObject> OnReactBegin;

        [SerializeField]
        [Tooltip("Event called when the reaction state ends. Parameters are (ReactionState this, GameObject triggeringObject)")]
        UnityEvent<ReactionState, GameObject> OnReactEnd;



        GameObject m_TriggeringObject;
        Vector3 m_TriggeredPosition;

        float m_ReactionCooldown = 0;

        public Vector3 ReactionPosition => DefaultReactionPosition != null ? DefaultReactionPosition.position : transform.position;
        void Start()
        {
            IsActiveState = IsInitialState;
#if UNITY_EDITOR
            if(IsInitialState)
                foreach (var state in gameObject.GetComponents<ReactionState>())
                    if (state != this && state.StateGroup == StateGroup && state.IsInitialState)
                        Debug.LogError($"Only one ReactionState of the same group can set as initial state. ReactionState on GameObject '{name}' : '{StateName}' and '{state.StateName}'");
#endif
        }
        private void Update()
        {
            if (m_ReactionCooldown > 0)
            {
                m_ReactionCooldown -= Time.deltaTime;
            }
        }

        public bool CanReact(GameObject from, Vector3 position)
        {
            return m_ReactionCooldown <= 0;
        }
        
        public bool TryReact(GameObject triggeringObject, Vector3 position)
        {
            if (!CanReact(triggeringObject, position))
                return false;
            ReactBegin(triggeringObject, position);
            return true;
        }
        public void ForceActivate() => ReactBegin(m_TriggeringObject, m_TriggeredPosition);
        public void React(GameObject triggeringObject, Vector3 position) => ReactBegin(triggeringObject, position);
        
        void ReactBegin(GameObject triggeringObject, Vector3 position)
        {
            foreach (var state in gameObject.GetComponents<ReactionState>())
                if (state.IsActiveState && state.StateGroup == StateGroup)
                    state.ReactEnd();
            
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactionState '{name}'.'{StateName}' ReactBegin (triggeringObject: '{triggeringObject.name}', position: {position}");
            
            IsActiveState = true;
            m_TriggeringObject = triggeringObject;
            m_TriggeredPosition = position;

            if (MoveTriggeringObjectAt != null && triggeringObject != null)
            {
                triggeringObject.transform.position = MoveTriggeringObjectAt.transform.position;
                triggeringObject.transform.rotation = MoveTriggeringObjectAt.transform.rotation;
                if (triggeringObject.TryGetComponent<Grabbable>(out var grabbable))
                    grabbable.ReleaseIfGrabbed();
            }

            if (Spawn != null)
                Instantiate(Spawn, position, Quaternion.identity);

            if (ReleaseGrabbed && TryGetComponent<Grabbable>(out var grabbable2))
                grabbable2.ReleaseIfGrabbed();

            if (PlayAnimatorState.Animator != null)
                PlayAnimatorState.Animator.Play(PlayAnimatorState.StateHash);

            if (SetKinematic && TryGetComponent<Rigidbody>(out var rigidBody))
            {
                m_PreviousKinematic = rigidBody.isKinematic;
                rigidBody.isKinematic = true;
            }
            else if (SetNonKinematic && TryGetComponent<Rigidbody>(out var rigidBody2))
            {
                m_PreviousKinematic = rigidBody2.isKinematic;
                rigidBody2.isKinematic = false;
            }

            if (!string.IsNullOrEmpty(ForceReactionStateOnTriggeringObjectOnBegin) && m_TriggeringObject != null)
                if (m_TriggeringObject.TryGetReactionState(ForceReactionStateOnTriggeringObjectOnBegin, out var state))
                    state.ForceActivate();

            OnReactBegin?.Invoke(this, m_TriggeringObject);

            if (ReactionCooldown > 0)
            {
                m_ReactionCooldown = ReactionCooldown;
            }
        }

        void ReactEnd()
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactionState '{name}'.'{StateName}' ReactEnd (triggeringObject: '{m_TriggeringObject.name}', position: {m_TriggeredPosition}");

            if ((SetKinematic || SetNonKinematic) && TryGetComponent<Rigidbody>(out var rigidBody))
            {
                rigidBody.isKinematic = m_PreviousKinematic;
            }

            if (!string.IsNullOrEmpty(ForceReactionStateOnTriggeringObjectOnEnd) && m_TriggeringObject != null)
                if (m_TriggeringObject.TryGetReactionState(ForceReactionStateOnTriggeringObjectOnEnd, out var state))
                    state.ForceActivate();

            OnReactEnd?.Invoke(this, m_TriggeringObject);

            IsActiveState = false;
            m_TriggeringObject = null;
            m_TriggeredPosition = Vector3.zero;
        }

    }
}