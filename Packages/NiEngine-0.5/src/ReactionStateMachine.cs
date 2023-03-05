using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{

    [System.Serializable]
    public class SerializableType<T> : ISerializationCallbackReceiver
    {

        [SerializeField] string TypeName;

        [NonSerialized]
        System.Type StoredType;

#if UNITY_EDITOR
        // HACK: I wasn't able to find the base type from the SerializedProperty,
        // so I'm smuggling it in via an extra string stored only in-editor.
        [SerializeField] string baseTypeName;
        public SerializableType(string typeName, string baseTypeName)
        {
            TypeName = typeName;
            this.baseTypeName = baseTypeName;
        }
#endif


        public SerializableType(System.Type typeToStore)
        {
            StoredType = typeToStore;
        }

        public override string ToString()
        {
            if (StoredType == null) return string.Empty;
            return StoredType.Name;
        }

        public void OnBeforeSerialize()
        {
            if(StoredType != null)
            {
                TypeName = StoredType?.AssemblyQualifiedName;
            }

#if UNITY_EDITOR
            baseTypeName = typeof(T).AssemblyQualifiedName;
#endif
        }

        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(TypeName) || TypeName == "null")
            {
                StoredType = null;
                return;
            }
            StoredType = System.Type.GetType(TypeName);
        }
        public Type Type => StoredType;

        public static implicit operator System.Type(SerializableType<T> t) => t.StoredType;

        // TODO: Validate that t is a subtype of T?
        public static implicit operator SerializableType<T>(System.Type t) => new SerializableType<T>(t);

#if UNITY_EDITOR
        public static SerializableType<T> FromProperty(UnityEditor.SerializedProperty property)
        {
            var o = new SerializableType<T>(
                property.FindPropertyRelative("TypeName").stringValue,
                property.FindPropertyRelative("baseTypeName").stringValue);
            return o;
        }
#endif
    }

    [Serializable]
    public struct EventParameters
    {
        public GameObject Self;
        public GameObject TriggerObject;
        public GameObject PreviousTriggerObject;
        public Vector3 TriggerPosition;
        public Vector3 PreviousTriggerPosition;
    }
    [Serializable]
    public struct GameObjectReference
    {
        public enum TypeEnum
        {
            Object,
            Self,
            TriggerObject,
            PreviousTriggerObject,
            FirstOfType,
            FirstWithTag,
            FirstWithName,
        }
        public TypeEnum Type;
        [SerializeField]
        GameObject ThisGameObject;
        [SerializeField]
        SerializableType<Component> ObjectType;
        [SerializeField]
        string String;

        public GameObject GetTargetGameObject(EventParameters eventParams)
        {
            if(!TryGetTargetGameObject(eventParams, out var go))
            {
                switch (Type)
                {
                    case GameObjectReference.TypeEnum.Self:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference unable to find Self");
                        break;
                    case GameObjectReference.TypeEnum.Object:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' unable to find Object", eventParams.Self);
                        break;
                    case GameObjectReference.TypeEnum.TriggerObject:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' could not find Trigger Object", eventParams.Self);
                        break;
                    case GameObjectReference.TypeEnum.PreviousTriggerObject:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' could not find Previous Trigger Object", eventParams.Self);
                        break;
                    case GameObjectReference.TypeEnum.FirstOfType:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' unable to find First Object Of Type '{String}'", eventParams.Self);
                        break;
                    case GameObjectReference.TypeEnum.FirstWithTag:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' unable to find First Object With Tag '{String}'", eventParams.Self);
                        break;
                    case GameObjectReference.TypeEnum.FirstWithName:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{eventParams.Self.name}' unable to find First Object With Name '{String}'", eventParams.Self);
                        break;
                }
            }
            return go;
        }
        public bool TryGetTargetGameObject(EventParameters eventParams, out GameObject go)
        {
            switch (Type)
            {
                case GameObjectReference.TypeEnum.Self:
                    go = eventParams.Self;
                    return true;
                case GameObjectReference.TypeEnum.Object:
                    go = ThisGameObject;
                    return true;
                case GameObjectReference.TypeEnum.TriggerObject:
                    go = eventParams.TriggerObject;
                    return true;
                case GameObjectReference.TypeEnum.PreviousTriggerObject:
                    go = eventParams.PreviousTriggerObject;
                    return true;
                case GameObjectReference.TypeEnum.FirstOfType:
                    var obj = GameObject.FindObjectOfType(ObjectType);
                    go = default;
                    switch (obj)
                    {
                        case Component comp:
                            go = comp.gameObject;
                            break;
                        case GameObject goo:
                            go = goo;
                            break;
                    }
                    return go != null;
                case GameObjectReference.TypeEnum.FirstWithTag:
                    go = GameObject.FindGameObjectWithTag(String);
                    return go != null;
                case GameObjectReference.TypeEnum.FirstWithName:
                    go = GameObject.Find(String);
                    return go != null;
            }
            go = default;
            return false;
        }

#if UNITY_EDITOR
        public static GameObjectReference FromProperty(UnityEditor.SerializedProperty property)
        {
            return new GameObjectReference
            {
                Type = (TypeEnum)property.FindPropertyRelative("Type").intValue,
                ThisGameObject = (GameObject)property.FindPropertyRelative("ThisGameObject").objectReferenceValue,
                ObjectType = SerializableType<Component>.FromProperty(property.FindPropertyRelative("ObjectType")),
                String = property.FindPropertyRelative("String").stringValue,
            };

        }
#endif
    }
    [Serializable]
    public struct PositionReference
    {

        public enum TypeEnum
        {
            Self,
            AtPosition,
            AtTriggerPosition,
            AtPreviousTriggerPosition,
            AtGameObject,
        }
        public TypeEnum Type;
        public Vector3 AtPosition;
        public GameObjectReference AtTransform;


        public Vector3 GetPosition(EventParameters eventParams)
        {
            if (!TryGetPosition(eventParams, out var go))
            {
                switch (Type)
                {
                    case PositionReference.TypeEnum.Self:
                        Debug.LogWarning($"[{Time.frameCount}] PositionReference unable to find Self");
                        break;
                    case PositionReference.TypeEnum.AtPosition:
                        break;
                    case PositionReference.TypeEnum.AtTriggerPosition:
                        break;
                    case PositionReference.TypeEnum.AtPreviousTriggerPosition:
                        break;
                }
            }
            return go;
        }
        public bool TryGetPosition(EventParameters eventParams, out Vector3 position)
        {
            switch (Type)
            {
                case PositionReference.TypeEnum.Self:
                    position = eventParams.Self.transform.position;
                    return true;
                case PositionReference.TypeEnum.AtPosition:
                    position = AtPosition;
                    return true;
                case PositionReference.TypeEnum.AtGameObject:
                    if (AtTransform.TryGetTargetGameObject(eventParams, out var obj))
                    {
                        position = obj.transform.position;
                        return true;
                    }
                    else
                    {
                        position = eventParams.Self.transform.position;
                    }
                    return false;
                case PositionReference.TypeEnum.AtTriggerPosition:
                    position = eventParams.TriggerPosition;
                    break;
                case PositionReference.TypeEnum.AtPreviousTriggerPosition:
                    position = eventParams.PreviousTriggerPosition;
                    break;
            }
            position = default;
            return false;
        }
    }
    public struct Owner
    {
        public Owner(ReactionStateMachine sm)
        {
            StateMachine = sm;
            State = null;
        }
        public Owner(ReactionStateMachine.State state)
        {
            StateMachine = state.StateMachine;
            State = state;
        }
        public ReactionStateMachine StateMachine;
        public ReactionStateMachine.State State;
    }

    [Serializable]
    public struct StateName : IComparable, IComparable<StateName>, IEquatable<StateName>
    {
        public string Name;
        public StateName(string name)
        {
            Name = name;
        }
        public static bool operator ==(StateName a, StateName b)
        {
            return a.Name == b.Name;
        }
        public static bool operator !=(StateName a, StateName b)
        {
            return a.Name == b.Name;
        }
        int IComparable.CompareTo(object obj)
        {
            if(!(obj is StateName sn)) return -1;
            return Name.CompareTo(sn.Name);
        }
        int IComparable<StateName>.CompareTo(StateName sn)
        {
            return Name.CompareTo(sn.Name);
        }
        bool IEquatable<StateName>.Equals(StateName other)
        {
            return Name.Equals(other.Name);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is StateName sn)) return false;
            return Name.Equals(sn.Name);
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
    public interface IStateObserver
    {
        void OnBegin(Owner owner, EventParameters parameters);
        void OnEnd(Owner owner, EventParameters parameters);
    }
    public interface IUpdate
    {
        void Update(Owner owner);
    }
    public interface IInitialize
    {
        void Initialize(Owner owner);
    }

    [Serializable]
    public abstract class Action : StateAction
    {
        public abstract void Act(Owner owner, EventParameters parameters);
        public override void OnBegin(Owner owner, EventParameters parameters) => Act(owner, parameters);
        public override void OnEnd(Owner owner, EventParameters parameters) { }
    }
    [Serializable]
    public abstract class StateAction 
    {
        public abstract void OnBegin(Owner owner, EventParameters parameters);
        public abstract void OnEnd(Owner owner, EventParameters parameters);
    }
    namespace Actions
    {
        [Serializable, ClassPickerName("Reaction")]
        public class Reaction : Action
        {
            public ReactionReference Reference;
            public override void Act(Owner owner, EventParameters parameters)
            {
                Reference.React(parameters);
            }
        }
        [Serializable, ClassPickerName("Delayed Reaction")]
        public class DelayedReaction : StateAction, IUpdate
        {
            public float Seconds;
            public bool Repeat;

            public ReactionList Reaction;
            
            [Tooltip("Will be set to Seconds when the state begin")]
            public float CurrentlyRemaining;

            [NonSerialized]
            EventParameters Parameters;
            public void Update(Owner owner)
            {
                var nextTime = CurrentlyRemaining - Time.deltaTime;

                if (CurrentlyRemaining >= 0 && nextTime < 0)
                {
                    Reaction.React(Parameters);
                    if (Repeat)
                        CurrentlyRemaining = Seconds;
                } 
                else 
                {
                    CurrentlyRemaining = nextTime;
                }
            }
            public override void OnBegin(Owner owner, EventParameters parameters)
            {
                CurrentlyRemaining = Seconds;
            }
            public override void OnEnd(Owner owner, EventParameters parameters)
            {
            }
        }
        [Serializable, ClassPickerName("Enable")]
        public class EnableDisable : StateAction
        {
            public GameObjectReference Target;
            public bool Enable;
            public bool RevertAtEnd;

            [NonSerialized]
            GameObject TargetObject;
            [NonSerialized]
            bool WasActive;
            public override void OnBegin(Owner owner, EventParameters parameters)
            {
                var target = Target.GetTargetGameObject(parameters);
                if (target != null)
                {
                    TargetObject = target;
                    if (RevertAtEnd)
                        WasActive = target.activeSelf;
                    target.SetActive(Enable);
                    
                }
            }
            public override void OnEnd(Owner owner, EventParameters parameters)
            {
                if (RevertAtEnd)
                    TargetObject.SetActive(WasActive);
            }
        }
        [Serializable, ClassPickerName("Spawn")]
        public class Spawn : Action
        {
            public GameObjectReference ObjectToSpawn;
            public PositionReference SpawnPosition;
            public override void Act(Owner owner, EventParameters parameters)
            {
                var obj = ObjectToSpawn.GetTargetGameObject(parameters);
                if (obj)
                {
                    var spawned = GameObject.Instantiate(obj);
                    spawned.transform.position = parameters.TriggerPosition;
                }
            }
        }

        [Serializable, ClassPickerName("AttachTo")]
        public class AttachTo : StateAction
        {
            public GameObjectReference Attach;
            public GameObjectReference To;
            public bool DetachOnEnd;

            GameObject AttachedObject;
            Transform m_PreviousParent;
            public override void OnBegin(Owner owner, EventParameters parameters)
            {
                m_PreviousParent = null;
                AttachedObject = null;
                var a = Attach.GetTargetGameObject(parameters);
                if (a != null)
                {
                    var t = To.GetTargetGameObject(parameters);
                    if (t != null)
                    {
                        AttachedObject = a;
                        m_PreviousParent = a.transform.parent;
                        a.transform.parent = t.transform;
                    }
                }
            }
            public override void OnEnd(Owner owner, EventParameters parameters)
            {
                if (DetachOnEnd && AttachedObject != null)
                {
                    AttachedObject.transform.parent = m_PreviousParent;
                }
            }
        }
        [Serializable, ClassPickerName("Event")]
        public class Event : Action
        {
            public UnityEvent UnityEvent;
            public override void Act(Owner owner, EventParameters parameters)
            {
                UnityEvent?.Invoke();
            }
        }
    }

    [Serializable]
    public abstract class Condition
    {
        public abstract bool Pass(Owner owner, EventParameters parameters);
    }

    [Serializable, ClassPickerName("Cooldown")]
    public class ConditionCooldown : Condition, IStateObserver, IUpdate, IInitialize
    {
        [Tooltip("In Seconds")]
        public float TimeInSeconds;
        float TimeLeft;
        public override bool Pass(Owner owner, EventParameters parameters)
        {
            return TimeLeft <= 0;
        }
        void IUpdate.Update(Owner owner)
        {
            TimeLeft -= Time.deltaTime;
        }
        void IInitialize.Initialize(Owner owner)
        {
            TimeLeft = TimeInSeconds;
        }
        void IStateObserver.OnBegin(Owner owner, EventParameters parameters)
        {

        }
        void IStateObserver.OnEnd(Owner owner, EventParameters parameters)
        {
            TimeLeft = TimeInSeconds;
        }
    }



    [Serializable]
    public class ReactionStateMachine : MonoBehaviour
    {
        [Serializable]
        public class State
        {
            public StateName StateName;
            public bool IsActiveState;
            public string Notes;
            [NonSerialized]
            public EventParameters LastBeginEvent;
            [NonSerialized]
            public EventParameters LastEndEvent;

            [HideInInspector]
            public ReactionStateMachine StateMachine;
            [HideInInspector]
            public StateGroup StateGroup;

            //[SerializeReference, DerivedClassPicker]
            //public Condition TestCondition;

            //[SerializeReference, DerivedClassPicker]
            //public Action TestAction;

            [SerializeReference, DerivedClassPicker(typeof(Condition), showPrefixLabel: false)]
            public List<Condition> Conditions;// = new();

            [SerializeReference, DerivedClassPicker(typeof(StateAction), showPrefixLabel: false)]
            public List<StateAction> OnBeginActions;

            [SerializeReference, DerivedClassPicker(typeof(Action), showPrefixLabel: false)]
            public List<Action> OnUpdate;

            [SerializeReference, DerivedClassPicker(typeof(Action), showPrefixLabel: false)]
            public List<Action> OnEndActions;

            [HideInInspector, NonSerialized]
            public List<IUpdate> Updates = new();

            [HideInInspector, NonSerialized]
            public List<IStateObserver> StateObservers = new();

            public bool CanReact(EventParameters parameters)
            {
                var owner = new Owner(this);
                foreach (var condition in Conditions)
                    if (!condition.Pass(owner, parameters)) return false;
                return true;
            }

            public void Handshake(ReactionStateMachine owner, StateGroup group = null)
            {
                StateMachine = owner;
                StateGroup = group;
                foreach (var action in Conditions)
                    Handshake(action);
                foreach (var action in OnBeginActions)
                    Handshake(action);
                foreach (var action in OnEndActions)
                    Handshake(action);
                if (IsActiveState)
                {
                    group.HasActiveState = true;
                    group.CurrentState = this;
                    //owner.InitialState = this;
                }
            }

            void Handshake(object obj)
            {
                if (obj is IUpdate update)
                    Updates.Add(update);
                if (obj is IStateObserver observer)
                    StateObservers.Add(observer);
                if (obj is IInitialize initialize)
                    StateMachine.Initializers.Add(initialize);
            }

            public void Update()
            {
                var owner = new Owner(this);

                foreach (var o in Updates)
                    o.Update(owner);

                foreach (var action in OnUpdate)
                    action.Act(owner, LastBeginEvent);
            }

            public void OnBegin(EventParameters parameters)
            {
                IsActiveState = true;
                var owner = new Owner(this);

                foreach (var observer in StateObservers)
                    observer.OnBegin(owner, parameters);

                foreach (var action in OnBeginActions)
                    action.OnBegin(owner, parameters);


                LastBeginEvent = parameters;
            }

            public void OnEnd(EventParameters parameters)
            {
                var owner = new Owner(this);
                foreach (var action in OnBeginActions)
                    action.OnEnd(owner, parameters);


                parameters.PreviousTriggerObject = LastBeginEvent.TriggerObject;
                parameters.PreviousTriggerPosition = LastBeginEvent.TriggerPosition;


                foreach (var observer in StateObservers)
                    observer.OnEnd(owner, parameters);

                foreach (var action in OnEndActions)
                    action.Act(owner, parameters);


                LastEndEvent = parameters;
                IsActiveState = false;
            }
        }

        [Serializable]
        public class StateGroup
        {
            public StateName GroupName;
            public string Notes;
            public bool HasActiveState;
            public List<State> States = new();

            [NonSerialized]
            public State CurrentState;
            public bool HasState(StateName name)
            {
                foreach (var s in States)
                    if (s.StateName == name)
                        return true;
                return false;
            }
            public bool TryGetState(StateName name, out State state)
            {

                foreach (var s in States)
                    if (s.StateName == name)
                    {
                        state = s;
                        return true;
                    }
                state = default;
                return false;
            }

            public void DeactivateAllState(EventParameters parameters)
            {
                foreach (var s in States)
                {
                    if (s.IsActiveState)
                    {
                        s.IsActiveState = false;
                        s.OnEnd(parameters);
                    }
                }
                HasActiveState = false;
            }
            public void SetActiveState(ReactionStateMachine component, State state, EventParameters parameters)
            {
#if UNITY_EDITOR
                if (state.StateGroup != this || state.StateMachine != component)
                    Debug.LogError($"ReactionStateMachine '{component.name}' switches to an unknown state '{state.StateName.Name}'", component);
#endif
                if (CurrentState != null)
                {
                    CurrentState.OnEnd(parameters);
                }
                CurrentState = state;
                if (CurrentState != null)
                {
                    CurrentState.OnBegin(parameters);
                    HasActiveState = true;
                }

            }
            public void Handshake(ReactionStateMachine owner)
            {
                foreach(var state in States)
                    state.Handshake(owner, this);
            }
            public void Update()
            {
                if (CurrentState != null)
                {
                    CurrentState.Update();

                }
            }

        }

        public List<StateGroup> Groups = new();

        //[Tooltip("Name of the group of states to be exclusive with")]
        //public string GroupName;

        //public List<State> States = new();
        [NonSerialized]
        List<IInitialize> Initializers = new();
        //State CurrentState;
        //State InitialState;

        public bool HasState(StateName name)
        {
            foreach (var group in Groups)
                if (group.HasState(name))
                    return true;
            return false;
        }
        public bool CanReact(string reactionOrStateName, EventParameters parameters)
        {
            foreach (var group in Groups)
                if (group.TryGetState(new StateName(reactionOrStateName), out var state))
                    if (state.CanReact(parameters))
                        return true;
            return false;
        }
        public void React(string reactionOrStateName, EventParameters parameters)
        {
            foreach(var group in Groups)
            {
                if (group.TryGetState(new StateName(reactionOrStateName), out var state))
                {
                    group.SetActiveState(this, state, parameters);
                }

            }
        }
        public void DeactivateAllStateOfGroup(string groupName, EventParameters parameters)
        {
            foreach (var group in Groups)
                if (group.GroupName == new StateName(groupName))
                    group.DeactivateAllState(parameters);
        }
        //        public bool HasState(StateName name)
        //        {
        //            foreach (var s in States)
        //                if (s.StateName == name)
        //                    return true;
        //            return false;
        //        }

        //        public bool TryGetState(StateName name, out State state)
        //        {

        //            foreach (var s in States)
        //                if (s.StateName == name)
        //                {
        //                    state = s;
        //                    return true;
        //                }
        //            state = default;
        //            return false;
        //        }

        //        public void DeactivateAllState(EventParameters parameters)
        //        {
        //            foreach(var s in States)
        //            {
        //                if (s.IsActiveState)
        //                {
        //                    s.IsActiveState = false;
        //                    s.OnEnd(parameters);
        //                }
        //            }
        //        }
        //        public void SetActiveState(State state, EventParameters parameters)
        //        {
        //            // Deactivate other state of the same group
        //            foreach (var s in gameObject.GetComponents<ReactionState>())
        //                if (s.IsActiveState && s.StateGroup == GroupName)
        //                    s.ForceDeactivate(parameters);
        //            foreach (var sm in gameObject.GetComponents<ReactionStateMachine>())
        //                if (sm.GroupName == GroupName)
        //                    sm.DeactivateAllState(parameters);

        //#if UNITY_EDITOR
        //            if (state.StateMachine != this)
        //                Debug.LogError($"ReactionStateMachine '{name}' switches to an unknown state '{state.StateName.Name}'", this);
        //#endif
        //            if(CurrentState != null)
        //            {
        //                CurrentState.OnEnd(parameters);
        //            }
        //            CurrentState = state;
        //            if (CurrentState != null)
        //            {
        //                CurrentState.OnBegin(parameters);
        //            }

        //        }
        #region Unity Callback
        private void Start()
        {
            foreach (var group in Groups)
                group.Handshake(this);

            //foreach (var state in States)
            //    state.Handshake(this);

            //CurrentState = InitialState;

            var owner = new Owner(this);
            foreach (var i in Initializers)
                i.Initialize(owner);
        }

        void Update()
        {
            foreach (var group in Groups)
                group.Update();

            //if (CurrentState != null)
            //{
            //    CurrentState.Update();

            //}
        }

        #endregion
    }
}