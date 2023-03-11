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
        [Serializable]
        public struct ParameterSet
        {
            /// <summary>
            /// Object sending the event
            /// </summary>
            public GameObject From;
            /// <summary>
            /// Object that triggered the event.
            /// may be different if an event is passed around between objects
            /// </summary>
            public GameObject TriggerObject;
            public Vector3 TriggerPosition;

            public override string ToString()
            {
                return $"(From: '{From.GetNameOrNull()}', TriggerObject: '{TriggerObject.GetNameOrNull()}', TriggerPosition: {TriggerPosition}";
            }
            public static ParameterSet Default => new ParameterSet
            {
                From = null,
                TriggerObject = null,
                TriggerPosition = Vector3.zero,
            };
            public static ParameterSet Trigger(GameObject from, GameObject triggerObject, Vector3 position) => new ParameterSet
            {
                From = from,
                TriggerObject = triggerObject,
                TriggerPosition = position,
            };
            public static ParameterSet WithoutTrigger(GameObject from) => new ParameterSet
            {
                From = from,
                TriggerObject = null,
                TriggerPosition = Vector3.zero,
            };
        }
        /// <summary>
        /// The object on which the event happens
        /// </summary>
        public GameObject Self;


        public ParameterSet Current;
        public ParameterSet OnBegin;

        ///// <summary>
        ///// Object sending the event
        ///// </summary>
        //public GameObject From => Current.From;

        ///// <summary>
        ///// Object that triggered the event.
        ///// may be different if an event is passed around between objects
        ///// </summary>
        //public GameObject TriggerObject => Current.TriggerObject;
        ///// <summary>
        ///// Previous trigger object, useful in StateAction.OnEnd for getting the object that triggered its StateAction.OnBegin
        ///// </summary>
        //public Vector3 TriggerPosition => Current.TriggerPosition;


        //public GameObject FromOnBegin;
        //public Vector3 PreviousTriggerPosition;
        //public GameObject PreviousTriggerObject;

        public System.Text.StringBuilder DebugTrace;


        public EventParameters WithDebugTrace(System.Text.StringBuilder stringBuilder)
        {
            var copy = this;
            copy.DebugTrace = stringBuilder;
            return copy;
        }
        public EventParameters WithSelf(GameObject self)
        {
            var copy = this;
            copy.Current.From = Self;
            copy.Self = self;
            return copy;
        }
        public EventParameters WithBegin(EventParameters begin)
        {
            var copy = this;
            copy.OnBegin.From = begin.Current.From;
            copy.OnBegin.TriggerObject = begin.Current.TriggerObject;
            copy.OnBegin.TriggerPosition = begin.Current.TriggerPosition;
            return copy;
        }
        public EventParameters WithOnBeginTrigger()
        {
            var copy = this;
            copy.Current.From = OnBegin.From;
            copy.Current.TriggerPosition = OnBegin.TriggerPosition;
            copy.Current.TriggerObject = OnBegin.TriggerObject;
            return copy;
        }
        public override string ToString()
        {
            return $"(Self: '{Self.GetNameOrNull()}', Current:{Current}, OnBegin:{OnBegin}";
        }
        public static EventParameters Default => new EventParameters
        {
            Self = null,
            Current = ParameterSet.Default,
            OnBegin = ParameterSet.Default,
        };
        public static EventParameters Trigger(GameObject self, GameObject from, GameObject triggerObject) => new EventParameters
        {
            Self = self,
            Current = ParameterSet.Trigger(from, triggerObject, Vector3.zero),
            OnBegin = ParameterSet.Default,
        };
        public static EventParameters Trigger(GameObject self, GameObject from, GameObject triggerObject, Vector3 position) => new EventParameters
        {
            Self = self,
            Current = ParameterSet.Trigger(from, triggerObject, Vector3.zero),
            OnBegin = ParameterSet.Default,
        };
        public static EventParameters WithoutTrigger(GameObject self) => new EventParameters
        {
            Self = self,
            Current = ParameterSet.Default,
            OnBegin = ParameterSet.Default,
        };
    }
    public struct Owner
    {
        public Owner(MonoBehaviour monoBehaviour)
        {
            MonoBehaviour = monoBehaviour;
            StateMachine = null;
            State = null;
        }
        public Owner(ReactionStateMachine sm)
        {
            MonoBehaviour = null;
            StateMachine = sm;
            State = null;
        }
        public Owner(ReactionStateMachine.State state)
        {
            MonoBehaviour = null;
            StateMachine = state.StateMachine;
            State = state;
        }
        public MonoBehaviour MonoBehaviour;
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
        public override string ToString()
        {
            return Name;
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
        void Update(Owner owner, EventParameters parameters);
    }
    public interface IInitialize
    {
        void Initialize(Owner owner);
    }

    [Serializable]
    public abstract class Action : StateAction, IAction
    {
        public abstract void Act(Owner owner, EventParameters parameters);
        public override void OnBegin(Owner owner, EventParameters parameters) => Act(owner, parameters);
        public override void OnEnd(Owner owner, EventParameters parameters) { }
    }
    [Serializable]
    public abstract class StateAction  : IStateAction
    {
        public abstract void OnBegin(Owner owner, EventParameters parameters);
        public abstract void OnEnd(Owner owner, EventParameters parameters);
    }


    [Serializable]
    public abstract class Condition : ICondition
    {
        public abstract bool Pass(Owner owner, EventParameters parameters);
    }

    public interface IAction : IStateAction
    {
        void Act(Owner owner, EventParameters parameters);
    }
    public interface IStateAction
    {
        void OnBegin(Owner owner, EventParameters parameters);
        void OnEnd(Owner owner, EventParameters parameters);
    }
    public interface ICondition
    {
        bool Pass(Owner owner, EventParameters parameters);
    }

    [Serializable]
    public class ConditionSet : ICondition
    {
        [SerializeReference, DerivedClassPicker(typeof(ICondition), showPrefixLabel: false)]
        public List<ICondition> Conditions;
        public bool Pass(Owner owner, EventParameters parameters)
        {

            if (parameters.DebugTrace != null)
            {
                foreach (var c in Conditions)
                {
                    var result = c.Pass(owner, parameters);
                    parameters.DebugTrace.AppendLine($"Condition:{result} => {c.GetType().Name}.Pass{parameters}");
                    if (!result) return false;
                }
            }
            else
            {
                foreach (var c in Conditions)
                    if (!c.Pass(owner, parameters)) return false;
            }
            return true;
        }
    }
    [Serializable]
    public struct ActionSet : IAction
    {
        [SerializeReference, DerivedClassPicker(typeof(IAction), showPrefixLabel: false)]
        public List<IAction> Actions;
        public void Act(Owner owner, EventParameters parameters)
        {
            foreach (var a in Actions)
            {
                if (parameters.DebugTrace != null)
                    parameters.DebugTrace.AppendLine($"{a.GetType().Name}.Act{parameters}");
                a.Act(owner, parameters);
            }
        }
        public void OnBegin(Owner owner, EventParameters parameters)
            => Act(owner, parameters);
        public void OnEnd(Owner owner, EventParameters parameters)
        {
        }
    }
    [Serializable]
    public struct StateActionSet : IStateAction
    {
        [SerializeReference, DerivedClassPicker(typeof(IStateAction), showPrefixLabel: false)]
        public List<IStateAction> Actions;

        public void OnBegin(Owner owner, EventParameters parameters)
        {
            foreach (var a in Actions)
            {
                if (parameters.DebugTrace != null)
                    parameters.DebugTrace.AppendLine($"{a.GetType().Name}.OnBegin{parameters}");
                a.OnBegin(owner, parameters);
            }
        }
        public void OnEnd(Owner owner, EventParameters parameters)
        {
            foreach (var a in Actions)
            {
                if (parameters.DebugTrace != null)
                    parameters.DebugTrace.AppendLine($"{a.GetType().Name}.OnEnd{parameters}");
                a.OnEnd(owner, parameters);
            }
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
            public bool DebugLog;
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

            public ConditionSet NewConditions = new();
            public StateActionSet NewOnBegin = new();
            public ActionSet NewOnUpdate = new();
            public ActionSet NewOnEnd = new();

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
                if(!NewConditions.Pass(owner, parameters)) return false;
                //foreach (var condition in Conditions)
                //    if (!condition.Pass(owner, parameters)) return false;
                return true;
            }

            public void Handshake(ReactionStateMachine owner, StateGroup group = null)
            {
                StateMachine = owner;
                StateGroup = group;
                //foreach (var action in Conditions)
                //    Handshake(action);
                //foreach (var action in OnBeginActions)
                //    Handshake(action);
                //foreach (var action in OnEndActions)
                //    Handshake(action);
                foreach (var action in NewConditions.Conditions)
                    Handshake(action);
                foreach (var action in NewOnBegin.Actions)
                    Handshake(action);
                foreach (var action in NewOnEnd.Actions)
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
                var parameters = LastBeginEvent;
                if (DebugLog)
                {
                    parameters = parameters.WithDebugTrace(new());
                }

                var owner = new Owner(this);

                foreach (var o in Updates)
                    o.Update(owner, parameters);

                NewOnUpdate.Act(owner, parameters);
                //foreach (var action in OnUpdate)
                //    action.Act(owner, parameters);

                if (DebugLog)
                {
                    if(parameters.DebugTrace.Length > 0)
                    {
                        Debug.Log($"[{Time.frameCount}] ReactionStateMachine.OnUpdate [\"{StateMachine.name}\".{StateGroup.GroupName}.{StateName}] {parameters}\r\n{parameters.DebugTrace}", StateMachine.gameObject);
                    }
                }
            }

            public void OnBegin(EventParameters parameters)
            {
                parameters.WithBegin(parameters);
                var parametersCopy = parameters;
                if (DebugLog)
                {
                    parameters = parameters.WithDebugTrace(new());
                    parameters.DebugTrace.AppendLine($"[{Time.frameCount}]ReactionStateMachine.OnBegin [\"{StateMachine.name}\".{StateGroup.GroupName}.{StateName}] {parameters}");
                    //Debug.Log($"[{Time.frameCount}] ReactionStateMachine '{StateMachine.name}'.'{StateGroup.GroupName}'.'{StateName}' On Begin {parameters}");
                }


                IsActiveState = true;
                var owner = new Owner(this);

                foreach (var observer in StateObservers)
                    observer.OnBegin(owner, parameters);


                NewOnBegin.OnBegin(owner, parameters);
                //foreach (var action in OnBeginActions)
                //    action.OnBegin(owner, parameters);

                if (DebugLog)
                {
                    Debug.Log(parameters.DebugTrace.ToString(), StateMachine.gameObject);
                }
                LastBeginEvent = parametersCopy;
            }

            public void OnEnd(EventParameters parameters)
            {
                if (DebugLog)
                {
                    parameters = parameters.WithDebugTrace(new());
                    parameters.DebugTrace.AppendLine($"[{Time.frameCount}] ReactionStateMachine.OnEnd [\"{StateMachine.name}\".{StateGroup.GroupName}.{StateName}] {parameters}");
                }
                parameters = parameters.WithBegin(LastBeginEvent);

                var owner = new Owner(this);
                NewOnBegin.OnEnd(owner, parameters);

                foreach (var observer in StateObservers)
                    observer.OnEnd(owner, parameters);

                NewOnEnd.Act(owner, parameters);
                if (DebugLog)
                    Debug.Log(parameters.DebugTrace.ToString(), StateMachine.gameObject);
                IsActiveState = false;
            }
        }

        [Serializable]
        public class StateGroup
        {
            [NonSerialized]
            public ReactionStateMachine StateMachine;
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
                Debug.Assert(parameters.Self == StateMachine.gameObject);
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
                Debug.Assert(parameters.Self == StateMachine.gameObject);
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
                StateMachine = owner;
                foreach (var state in States)
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
            Debug.Assert(parameters.Self == gameObject);
            parameters = parameters.WithSelf(gameObject);
            foreach (var group in Groups)
                if (group.TryGetState(new StateName(reactionOrStateName), out var state))
                    if (state.CanReact(parameters))
                        return true;
            return false;
        }

        public void ForceActivateState(string stateName)
        {
            ReactionReference.React(stateName, EventParameters.Default.WithSelf(gameObject));
        }
        public bool React(string reactionOrStateName, EventParameters parameters)
        {
            Debug.Assert(parameters.Self == gameObject);
            foreach (var group in Groups)
            {
                if (group.TryGetState(new StateName(reactionOrStateName), out var state))
                {
                    group.SetActiveState(this, state, parameters);
                    return true;
                }

            }
            return false;
        }

        public void DeactivateAllStateOfGroup(string groupName, EventParameters parameters)
        {
            Debug.Assert(parameters.Self == gameObject);
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