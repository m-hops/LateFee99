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
        public static EventParameters Default => new EventParameters
        {
            Self = null,
            TriggerObject = null,
            PreviousTriggerObject = null,
            TriggerPosition = Vector3.zero,
            PreviousTriggerPosition = Vector3.zero,
        };
        public static EventParameters Trigger(GameObject self, GameObject triggerObject) => new EventParameters
        {
            Self = self,
            TriggerObject = triggerObject,
            PreviousTriggerObject = null,
            TriggerPosition = triggerObject.transform.position,
            PreviousTriggerPosition = Vector3.zero,
        };
        public static EventParameters WithoutTrigger(GameObject self) => new EventParameters
        {
            Self = self,
            TriggerObject = null,
            PreviousTriggerObject = null,
            TriggerPosition = Vector3.zero,
            PreviousTriggerPosition = Vector3.zero,
        };
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
        public static implicit operator StateName(string value) => new StateName { Name = value };
        public static implicit operator string(StateName value) => value.Name;
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

    [Serializable]
    public abstract class Condition
    {
        public abstract bool Pass(Owner owner, EventParameters parameters);
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
            public EventParameters LastBeginEvent;
            //public EventParameters LastEndEvent;

            [NonSerialized]
            public ReactionStateMachine StateMachine;
            [NonSerialized]
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


                //LastEndEvent = parameters;
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

        [NonSerialized]
        List<IInitialize> Initializers = new();

        public bool TryGetGroup(StateName name, out StateGroup group)
        {
            foreach (var g in Groups)
            {
                if (g.GroupName == name)
                {
                    group = g;
                    return true;
                }
            }

            group = default;
            return false;
        }
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

        #region Unity Callback
        private void Start()
        {
            foreach (var group in Groups)
                group.Handshake(this);

            var owner = new Owner(this);
            foreach (var i in Initializers)
                i.Initialize(owner);
        }

        void Update()
        {
            foreach (var group in Groups)
                group.Update();
        }

        #endregion
    }
}