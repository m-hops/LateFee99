using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Nie
{
    
    public static class GameObjectExt
    {
        public static IEnumerable<ReactionState> AllReactionState(this GameObject @this, string name)
        {
            foreach (var rs in @this.GetComponents<ReactionState>().Where(x => x.enabled && x.StateName == name))
                yield return rs;
        }
        public static IEnumerable<ReactionStateMachine> AllReactionStateMachine(this GameObject @this)
            => @this.GetComponents<ReactionStateMachine>().Where(x=> x.enabled );

        public static IEnumerable<ReactionState> AllReactionState(this GameObject @this, string name, string group)
        {
            foreach (var rs in @this.GetComponents<ReactionState>().Where(x => x.enabled && x.StateName == name && x.StateGroup == group))
                yield return rs;
        }
        public static bool TryGetFirstReactionState(this GameObject @this, string name, out ReactionState state)
        {
            foreach(var rs in @this.GetComponents<ReactionState>().Where(x=> x.enabled && x.StateName == name))
            {
                state = rs;
                return true;
            }
            state = null;
            return false;
        }
        public static bool TryGetFirstReactionState(this GameObject @this, string name, string group, out ReactionState state)
        {
            foreach (var rs in @this.GetComponents<ReactionState>().Where(x => x.enabled && x.StateName == name && x.StateGroup == group))
            {
                state = rs;
                return true;
            }
            state = null;
            return false;
        }
        public static string GetNameOrNull(this GameObject @this)
            => @this == null ? "<null>" : @this.name;
        public static string GetNameOrNull(this MonoBehaviour @this)
            => @this == null ? "<null>" : @this.name;
        public static GameObject GetGameObjectOrNull(this Component @this)
            => @this == null ? null : @this.gameObject;
    }
    [AddComponentMenu("Nie/Object/ReactionState")]
    public class ReactionState : MonoBehaviour
    {
        public interface IObserver
        {
            void OnBegin(ReactionState state, GameObject from, GameObject triggerObject, Vector3 position);
            void OnEnd(ReactionState state, GameObject from, GameObject triggerObject, GameObject previousTriggerObject, Vector3 position);
        }
        List<IObserver> m_Observers = new();
        public void AddObserver(IObserver o) => m_Observers.Add(o);
        public void RemoveObserver(IObserver o) => m_Observers.Remove(o);
        [Tooltip("Name of this state. Used when activating state.")]
        public string StateName;
        [Tooltip("This state is mutually exclusive will all ReactionState of the same group on this GameObject")]
        public string StateGroup;
        [System.Obsolete("Set active state in run-time fields")]
        [Tooltip("Will be in active state when the game object starts. Will not execute the reaction")]
        public bool IsInitialState;

        [Tooltip("Print to console events caused by this Reaction")]
        public bool DebugLog = false;

#if UNITY_EDITOR
        [Tooltip("If set, will draw at this location in the editor this state name when active.")]
        public Transform DebugDrawState;
#endif

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
        [Tooltip("if set, will reactive with new trigger objects while being already active with an old trigger object.")]
        public bool ReactivateOnNewTrigger = true;


        [Header("On Begin Actions:")]
        [Tooltip("If set, instantiate the provided GameObject at the reaction position")]
        public GameObject Spawn;

        [Tooltip("Default Reaction position, used when spawning the GameObject from the property 'Spawn'")]
        public Transform DefaultReactionPosition;

        //public AnimatorStateReference PlayAnimatorState;

        [Header("Actions on this object:")]
        [Tooltip("Will set back to previous value when state ends")]
        public bool SetKinematic;
        [Tooltip("Will set back to previous value when state ends")]
        public bool SetNonKinematic;

        [Tooltip("Will release this object if it has a Grabbable component and is currently grabbed")]
        public bool ReleaseGrabbed;


        [Header("Actions on Trigger Object:")]
        [Tooltip("If set, Set the parent of the GameObject that triggered this reaction to this transform.")]
        [UnityEngine.Serialization.FormerlySerializedAs("AttachTriggeringObjectAt")]
        public Transform AttachTriggerObjectAt;
        [Tooltip("If checked and AttachTriggerObjectAt is set, will set the new local position and rotation to 0 from the new parent the trigger object is being attached to.")]
        public bool MoveToParentOrigin;
        public bool DetachOnEnd = true;

        public Transform MoveToOnEnd;



        [SerializeField]
        [Tooltip("Reaction executed when this ReactionState gets activated")]
        ReactionList TriggerOnBegin;

        [Header("On End Actions:")]

        [SerializeField]
        [Tooltip("Reaction executed when this ReactionState gets deactivated")]
        ReactionList TriggerOnEnd;

        //[Tooltip("If set, activate the first ReactionState found of the provided name from the GameObject that triggered this reaction when this state is activated.")]
        //public string OnBeginForceState;
        //[Tooltip("If set, activate the first ReactionState found of the provided name from the GameObject that triggered this reaction when this state is deactivated.")]
        //public string OnEndForceState;

        public ReactionList DelayedReaction;
        [Tooltip("In second.")]
        public float Delay;

        [Header("Overrides:")]
        [Tooltip("If set, will execute the reaction on provided object instead of the object with this ReactionState.")]
        public GameObject OverrideThisObject;
        
        [Tooltip("If set, will execute the reaction using provided object as the trigger object.")]
        public GameObject OverrideTriggerObject;




        [Header("Run-time state:")]
        [Tooltip("Is currently activate and all other state of the same group are deactivated")]
        public bool IsActiveState;

        [Tooltip("The trigger object when this state was activated")]
        public GameObject CurrentTriggerObject;
        [Tooltip("The trigger position when this state was activated")]
        public Vector3 CurrentTriggerPosition;
        [Tooltip("Cool down time before this state can be (re)activated")]
        public float CurrentReactionCooldown = 0;
        //[Tooltip("The Transform the trigger object was detached from if 'Attach Trigger Object At' is set. It will reattach to it if 'Detach On End' is set.")]
        //public Transform PreviousAttachedObject;
        [Tooltip("Current Idle Time")]
        public float CurrentIdleTime = 0;

        [UnityEngine.Serialization.FormerlySerializedAs("m_PreviousKinematic")]
        public bool PreviousKinematic;

        int m_BeginReactionDepth = 0;
        int m_EndReactionDepth = 0;

        public Vector3 ReactionPosition => DefaultReactionPosition != null ? DefaultReactionPosition.position : transform.position;

        Vector3 GetReactionPosition(Vector3 receivedPosition) => DefaultReactionPosition != null ? DefaultReactionPosition.position : receivedPosition;
        public GameObject TargetObject => OverrideThisObject != null ? TargetObject : gameObject;
        public GameObject GetOverriddenTrigger(GameObject triggerObject) => OverrideTriggerObject != null ? OverrideTriggerObject : triggerObject;
        void Start()
        {
            if (IsInitialState)
            {
                Debug.LogWarning("Using obsolete feature 'ReactionState.IsInitialState'", this);
                IsActiveState = IsInitialState;
            }
#if UNITY_EDITOR
            
            if(IsActiveState)
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
                var obj = GameObject.Instantiate(EditorMenu.DebugLabelAsset, DebugDrawState);
                DebugLabel = obj.GetComponent<TextMesh>();
                obj.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
                DebugLabel.transform.localPosition = Vector3.zero;
                DebugLabel.transform.localRotation = Quaternion.identity;
                var parentScale = DebugDrawState.lossyScale;
                var scale = EditorMenu.DebugLabelAsset.transform.localScale;
                DebugLabel.transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
                DebugLabel.text = StateName;
            }
            if ((!EditorMenu.DrawStatesLabel || !IsActiveState || DebugDrawState == null) && DebugLabel != null)
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
            if (IsActiveState)
            {
                var NextIdleTime = CurrentIdleTime + Time.deltaTime;

                if (NextIdleTime >= Delay && CurrentIdleTime < Delay)
                    DelayedReaction.React(gameObject, CurrentTriggerObject, CurrentTriggerPosition, CurrentTriggerObject);

                CurrentIdleTime = NextIdleTime;
            }
            if (CurrentReactionCooldown > 0)
            {
                CurrentReactionCooldown -= Time.deltaTime;
            }
        }

        public bool CanReact(GameObject triggerObject, Vector3 position)
        {
            //if (IsActiveState && !ReactivateOnNewTrigger) return false;
            //if (IsActiveState && CurrentTriggerObject == triggerObject) return false;
            if (m_BeginReactionDepth > 100)
            {
                Debug.LogWarning($"[{Time.frameCount}] reaction state '{StateName}' is requested to begin on object '{gameObject.GetNameOrNull()}' after 100 begin loop. Look for infinite reaction loops. Triggered by '{triggerObject.GetNameOrNull()}' at position: {position}", gameObject);
                return false;
            }
            if (m_EndReactionDepth > 100)
            {
                Debug.LogWarning($"[{Time.frameCount}] reaction state '{StateName}' is requested to begin on object '{gameObject.GetNameOrNull()}' after 100 end loop. Look for infinite reaction loops. Triggered by '{triggerObject.GetNameOrNull()}' at position: {position}", gameObject);
                return false;
            }
            return CurrentReactionCooldown <= 0;
        }
        
        public bool TryReact(GameObject triggerObject, Vector3 position)
        {
            triggerObject = GetOverriddenTrigger(triggerObject);
            if (!CanReact(triggerObject, position))
                return false;
            ReactBegin(triggerObject, position);
            return true;
        }
        public void ForceActivate()
        {
            if (IsActiveState) return;
            ReactBegin(CurrentTriggerObject, CurrentTriggerPosition);
        }
        public void ForceDeactivate()
        {
            if (!IsActiveState) return;
            ReactEnd(null, Vector3.zero);
        }
        public void ForceDeactivate(EventParameters parameters)
        {
            if (!IsActiveState) return;
            ReactEnd(parameters.TriggerObject, parameters.TriggerPosition);
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
        public void React(GameObject triggerObject, Vector3 position)
        {
            ReactBegin(triggerObject, position);
        }
        
        void ReactBegin(GameObject triggerObject, Vector3 position)
        {
            if (m_BeginReactionDepth > 100)
            {
                Debug.LogWarning($"[{Time.frameCount}] reaction state '{StateName}' begin on object '{gameObject.GetNameOrNull()}' is being triggered twice in the same reaction sequence. Look for infinite reaction loops. Triggered by '{triggerObject.GetNameOrNull()}' at position: {position}", gameObject);
                return;
            }
            ++m_BeginReactionDepth;
            CurrentIdleTime = 0;
            triggerObject = GetOverriddenTrigger(triggerObject);
            var thisObject = TargetObject;

            if (IsActiveState)
            {
                // if same trigger as current one
                // or do not reactivate on new trigger,
                // then do not execute begin
                if (triggerObject == CurrentTriggerObject || !ReactivateOnNewTrigger)
                {
                    --m_BeginReactionDepth;
                    return;
                }
            }
                
            // set to new state
            CurrentTriggerObject = triggerObject;
            CurrentTriggerPosition = position;
            IsActiveState = true;

            // deactivate previous active state.
            foreach (var state in gameObject.GetComponents<ReactionState>())
                if (state.IsActiveState && state != this && state.StateGroup == StateGroup)
                    state.ReactEnd(triggerObject, position);
            foreach (var sm in gameObject.GetComponents<ReactionStateMachine>())
                sm.DeactivateAllStateOfGroup(StateGroup, new EventParameters()
                    {
                        Self = gameObject,
                        TriggerObject = triggerObject,
                        PreviousTriggerObject = null,
                        TriggerPosition = position,
                        PreviousTriggerPosition = position,
                    });


            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactionState '{name}'.'{StateName}' ReactBegin (triggerObject: '{(triggerObject == null ? "<null>" : triggerObject.name)}', position: {position}");


            if (AttachTriggerObjectAt != null && triggerObject != null)
            {
                if (triggerObject.TryGetComponent<Grabbable>(out var grabbable))
                    grabbable.ReleaseIfGrabbed();
                //PreviousAttachedObject = triggerObject.transform.parent;
                triggerObject.transform.parent = AttachTriggerObjectAt.transform;
                if (MoveToParentOrigin)
                {
                    triggerObject.transform.localRotation = Quaternion.identity;
                    triggerObject.transform.localPosition = Vector3.zero;

                    if (triggerObject.TryGetComponent<Rigidbody>(out var rigidBody2))
                    {
                        rigidBody2.velocity = Vector3.zero;
                        rigidBody2.angularVelocity = Vector3.zero;
                    }
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
                PreviousKinematic = rigidBody.isKinematic;
                rigidBody.isKinematic = true;
            }
            else if (SetNonKinematic && thisObject.TryGetComponent<Rigidbody>(out var rigidBody2))
            {
                PreviousKinematic = rigidBody2.isKinematic;
                rigidBody2.isKinematic = false;
            }

            //if (!string.IsNullOrEmpty(OnBeginForceState) && m_TriggerObject != null)
            //    if (m_TriggerObject.TryGetReactionState(OnBeginForceState, out var state))
            //        state.ForceActivate();
            //    else
            //        Debug.LogWarning($"[{Time.frameCount}] ReactionState '{name}'.'{StateName}' cannot find ReactionState '{OnBeginForceState}' to force on trigger object '{(m_TriggerObject == null ? "<null>" : m_TriggerObject.name)}' at position: {m_TriggeredPosition}");

            TriggerOnBegin.TryReact(gameObject, triggerObject, position);

            foreach (var obs in m_Observers)
                obs.OnBegin(this, thisObject, triggerObject, position);

            if (Cooldown > 0)
            {
                CurrentReactionCooldown = Cooldown;
            }

            --m_BeginReactionDepth;
        }

        void ReactEnd(GameObject triggerObject, Vector3 position)
        {
            if (m_EndReactionDepth > 100)
            {
                Debug.LogWarning($"[{Time.frameCount}] reaction state '{StateName}' end on object '{gameObject.GetNameOrNull()}' is being triggered twice in the same reaction sequence. Look for infinite reaction loops. Triggered by '{triggerObject.GetNameOrNull()}' at position: {position}", gameObject);
                return;
            }
            ++m_EndReactionDepth;
            var thisObject = TargetObject;

            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactionState '{name}'.'{StateName}' ReactEnd (triggerObject: '{(CurrentTriggerObject == null ? "<null>" : CurrentTriggerObject.name)}', position: {CurrentTriggerPosition}");

            if ((SetKinematic || SetNonKinematic) && thisObject.TryGetComponent<Rigidbody>(out var rigidBody))
            {
                rigidBody.isKinematic = PreviousKinematic;
            }

            if (DetachOnEnd && AttachTriggerObjectAt != null && CurrentTriggerObject != null)
                CurrentTriggerObject.transform.parent = null;

            if(MoveToOnEnd != null)
            {
                CurrentTriggerObject.transform.position = MoveToOnEnd.transform.position;
                CurrentTriggerObject.transform.rotation = MoveToOnEnd.transform.rotation;
            }

            //if (!string.IsNullOrEmpty(OnEndForceState) && m_TriggerObject != null)
            //    if (m_TriggerObject.TryGetReactionState(OnEndForceState, out var state))
            //        state.ForceActivate();
            //    else //if(DebugLog)
            //        Debug.LogWarning($"[{Time.frameCount}] ReactionState '{name}'.'{StateName}' cannot find ReactionState '{OnEndForceState}' to force on trigger object '{(m_TriggerObject == null ? "<null>" : m_TriggerObject.name)}' at position: {m_TriggeredPosition}");

            TriggerOnEnd.TryReact(gameObject, triggerObject, position, CurrentTriggerObject);

            foreach (var obs in m_Observers)
                obs.OnEnd(this, thisObject, triggerObject, CurrentTriggerObject, position);

            IsActiveState = false;
            CurrentTriggerObject = null;
            CurrentTriggerPosition = Vector3.zero;
            --m_EndReactionDepth;
        }

    }
}