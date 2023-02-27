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
        public static string GetNameOrNull(this GameObject @this)
            => @this == null ? "<null>" : @this.name;
    }
    [AddComponentMenu("Nie/Object/ReactionState")]
    public class ReactionState : MonoBehaviour
    {
        [Tooltip("Name of this state. Used when activating state.")]
        public string StateName;
        [Tooltip("This state is mutually exclusive will all ReactionState of the same group on this GameObject")]
        public string StateGroup;
        [Tooltip("Will be in active state when the game object starts. Will not execute the reaction")]
        public bool IsInitialState;

        [Tooltip("Print to console events caused by this Reaction")]
        public bool DebugLog = false;

#if UNITY_EDITOR
        [Tooltip("If set, will draw at this location in the editor this state name when active.")]
        public Transform DebugDrawState;
#endif

        [Tooltip("Is currently activate and all other state of the same group are deactivated")]
        public bool IsActiveState;
#if UNITY_EDITOR
        [EditorCools.Button]
        public void SetActiveState()
        {
            if(UnityEditor.EditorApplication.isPlaying)
                ForceActivate();
        }
#endif
        [Header("Conditions:")]
        [Tooltip("Once this Reaction state activates, it cannot re-active again within the cooldown period, in seconds.")]
        public float Cooldown = 0;


        [Header("Actions:")]
        [Tooltip("If set, instantiate the provided GameObject at the reaction position")]
        public GameObject Spawn;

        [Tooltip("Default Reaction position, used when spawning the GameObject from the property 'Spawn'")]
        public Transform DefaultReactionPosition;

        //public AnimatorStateReference PlayAnimatorState;

        [Header("Actions on this object:")]
        public bool SetKinematic;
        public bool SetNonKinematic;
        bool m_PreviousKinematic;

        [Tooltip("Will release this object if it has a Grabbable component and is currently grabbed")]
        public bool ReleaseGrabbed;


        [Header("Actions on Triggering Object:")]
        [Tooltip("If set, Set the parent of the GameObject that triggered this reaction to this transform.")]
        public Transform AttachTriggeringObjectAt;
        [Tooltip("If checked and AttachTriggeringObjectAt is set, will set the new local position and rotation to 0 from the new parent the triggering object is being attached to.")]
        public bool MoveToParentOrigin;
        Transform m_PreviousAttachedObject;

        //[Tooltip("If set, activate the first ReactionState found of the provided name from the GameObject that triggered this reaction when this state is activated.")]
        //public string OnBeginForceState;
        //[Tooltip("If set, activate the first ReactionState found of the provided name from the GameObject that triggered this reaction when this state is deactivated.")]
        //public string OnEndForceState;


        [Header("Overrides:")]
        [Tooltip("If set, will execute the reaction on provided object instead of the object with this ReactionState.")]
        public GameObject ThisObject;
        [Tooltip("If set, will execute the reaction using provided object as the triggering object.")]
        public GameObject TriggeringObject;


        [SerializeField]
        [Tooltip("Reaction executed when this ReactionState gets activated")]
        ReactionList TriggerOnBegin;

        [SerializeField]
        [Tooltip("Reaction executed when this ReactionState gets deactivated")]
        ReactionList TriggerOnEnd;



        GameObject m_TriggeringObject;
        Vector3 m_TriggeredPosition;

        float m_ReactionCooldown = 0;
        bool m_IsReactingBegin = false;
        bool m_IsReactingEnd = false;
        public Vector3 ReactionPosition => DefaultReactionPosition != null ? DefaultReactionPosition.position : transform.position;

        Vector3 GetReactionPosition(Vector3 receivedPosition) => DefaultReactionPosition != null ? DefaultReactionPosition.position : receivedPosition;
        public GameObject TargetObject => ThisObject != null ? TargetObject : gameObject;
        public GameObject GetTargetTriggeringObject(GameObject triggeringObject) => TriggeringObject != null ? TriggeringObject : triggeringObject;
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
#if UNITY_EDITOR
        
        TextMesh DebugLabel = null;
        void CheckDebugLabel()
        {
            
            if(EditorMenu.DrawStatesLabel && IsActiveState && DebugDrawState != null && DebugLabel == null)
            {
                DebugLabel = GameObject.Instantiate(EditorMenu.DebugLabelAsset, DebugDrawState).GetComponent<TextMesh>();
                DebugLabel.hideFlags = HideFlags.HideAndDontSave;
                DebugLabel.transform.localPosition = Vector3.zero;
                DebugLabel.transform.localRotation = Quaternion.identity;
                var parentScale = DebugDrawState.lossyScale;
                var scale = EditorMenu.DebugLabelAsset.transform.localScale;
                DebugLabel.transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
                DebugLabel.text = StateName;
            }
            if ((!EditorMenu.DrawStatesLabel || !IsActiveState) && DebugLabel != null)
            {
                if(UnityEditor.EditorApplication.isPlaying)
                    Destroy(DebugLabel.gameObject);
                else
                    DestroyImmediate(DebugLabel.gameObject);
                DebugLabel = null;
            }
        }
        void OnDrawGizmos()
        {
            CheckDebugLabel();
            if (UnityEditor.EditorApplication.isPlaying && EditorMenu.DrawStatesLabel) return;
            if (EditorMenu.DrawStatesGizmos && DebugDrawState != null && IsActiveState)
                UnityEditor.Handles.Label(DebugDrawState.position, StateName);
        }
#endif
        private void Update()
        {
#if UNITY_EDITOR
            CheckDebugLabel();
#endif
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
            triggeringObject = GetTargetTriggeringObject(triggeringObject);
            if (!CanReact(triggeringObject, position))
                return false;
            ReactBegin(triggeringObject, position);
            return true;
        }
        public void ForceActivate()
        {
            if (IsActiveState) return;
            ReactBegin(m_TriggeringObject, m_TriggeredPosition);
        }
        public void ForceActivateState(string stateName)
        {
            foreach (var state in gameObject.GetComponents<ReactionState>())
                if (state.StateName == stateName)
                {
                    state.React(null, state.gameObject.transform.position);
                    //return true;
                }
            //return false;
        }
        public void React(GameObject triggeringObject, Vector3 position)
        {
            if (IsActiveState) return;
            ReactBegin(triggeringObject, position);
        }
        
        void ReactBegin(GameObject triggeringObject, Vector3 position)
        {
            if (m_IsReactingBegin)
            {
                Debug.LogWarning($"[{Time.frameCount}] reaction state '{StateName}' begin on object '{gameObject.GetNameOrNull()}' is being triggered twice in the same reaction sequence. Look for infinite reaction loops. Triggered by '{triggeringObject.GetNameOrNull()}' at position: {position}", gameObject);
                return;
            }
            m_IsReactingBegin = true;
            foreach (var state in gameObject.GetComponents<ReactionState>())
                if (state.IsActiveState && state.StateGroup == StateGroup)
                    state.ReactEnd(triggeringObject, position);
            
            triggeringObject = GetTargetTriggeringObject(triggeringObject);
            var thisObject = TargetObject;

            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactionState '{name}'.'{StateName}' ReactBegin (triggeringObject: '{(triggeringObject == null ? "<null>" : triggeringObject.name)}', position: {position}");
            
            IsActiveState = true;
            m_TriggeringObject = triggeringObject;
            m_TriggeredPosition = position;

            if (AttachTriggeringObjectAt != null && triggeringObject != null)
            {
                if (triggeringObject.TryGetComponent<Grabbable>(out var grabbable))
                    grabbable.ReleaseIfGrabbed();
                m_PreviousAttachedObject = triggeringObject.transform.parent;
                triggeringObject.transform.parent = AttachTriggeringObjectAt.transform;
                if (MoveToParentOrigin)
                {
                    triggeringObject.transform.localRotation = Quaternion.identity;
                    triggeringObject.transform.localPosition = Vector3.zero;
                }
            }

            if (Spawn != null)
                Instantiate(Spawn, GetReactionPosition(position), Quaternion.identity);

            if (ReleaseGrabbed && thisObject.TryGetComponent<Grabbable>(out var grabbable2))
                grabbable2.ReleaseIfGrabbed();

            //if (PlayAnimatorState.Animator != null)
            //    PlayAnimatorState.Animator.Play(PlayAnimatorState.StateHash);

            if (SetKinematic && thisObject.TryGetComponent<Rigidbody>(out var rigidBody))
            {
                m_PreviousKinematic = rigidBody.isKinematic;
                rigidBody.isKinematic = true;
            }
            else if (SetNonKinematic && thisObject.TryGetComponent<Rigidbody>(out var rigidBody2))
            {
                m_PreviousKinematic = rigidBody2.isKinematic;
                rigidBody2.isKinematic = false;
            }

            //if (!string.IsNullOrEmpty(OnBeginForceState) && m_TriggeringObject != null)
            //    if (m_TriggeringObject.TryGetReactionState(OnBeginForceState, out var state))
            //        state.ForceActivate();
            //    else
            //        Debug.LogWarning($"[{Time.frameCount}] ReactionState '{name}'.'{StateName}' cannot find ReactionState '{OnBeginForceState}' to force on triggering object '{(m_TriggeringObject == null ? "<null>" : m_TriggeringObject.name)}' at position: {m_TriggeredPosition}");

            TriggerOnBegin.TryReact(gameObject, triggeringObject, position);

            if (Cooldown > 0)
            {
                m_ReactionCooldown = Cooldown;
            }
            m_IsReactingBegin = false;
        }

        void ReactEnd(GameObject triggeringObject, Vector3 position)
        {
            if (m_IsReactingEnd)
            {
                Debug.LogWarning($"[{Time.frameCount}] reaction state '{StateName}' end on object '{gameObject.GetNameOrNull()}' is being triggered twice in the same reaction sequence. Look for infinite reaction loops. Triggered by '{triggeringObject.GetNameOrNull()}' at position: {position}", gameObject);
                return;
            }
            m_IsReactingEnd = true;
            var thisObject = TargetObject;

            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactionState '{name}'.'{StateName}' ReactEnd (triggeringObject: '{(m_TriggeringObject == null ? "<null>" : m_TriggeringObject.name)}', position: {m_TriggeredPosition}");

            if ((SetKinematic || SetNonKinematic) && thisObject.TryGetComponent<Rigidbody>(out var rigidBody))
            {
                rigidBody.isKinematic = m_PreviousKinematic;
            }

            if (AttachTriggeringObjectAt != null && m_TriggeringObject != null)
                m_TriggeringObject.transform.parent = m_PreviousAttachedObject;


            //if (!string.IsNullOrEmpty(OnEndForceState) && m_TriggeringObject != null)
            //    if (m_TriggeringObject.TryGetReactionState(OnEndForceState, out var state))
            //        state.ForceActivate();
            //    else //if(DebugLog)
            //        Debug.LogWarning($"[{Time.frameCount}] ReactionState '{name}'.'{StateName}' cannot find ReactionState '{OnEndForceState}' to force on triggering object '{(m_TriggeringObject == null ? "<null>" : m_TriggeringObject.name)}' at position: {m_TriggeredPosition}");

            TriggerOnEnd.TryReact(gameObject, triggeringObject, position, m_TriggeringObject);
            IsActiveState = false;
            m_TriggeringObject = null;
            m_TriggeredPosition = Vector3.zero;
            m_IsReactingEnd = false;
        }

    }
}