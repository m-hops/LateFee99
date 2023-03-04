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
    public abstract class Action
    {
        public abstract void OnBegin(Owner owner, EventParameters parameters);
        public abstract void OnEnd(Owner owner, EventParameters parameters);
    }
    namespace Actions
    {
        [Serializable, ClassPickerName("Spawn")]
        public class ActionSpawn : Action
        {
            public GameObjectReference ObjectToSpawn;
            public PositionReference SpawnPosition;
            public override void OnBegin(Owner owner, EventParameters parameters)
            {
                var obj = ObjectToSpawn.GetTargetGameObject(parameters);
                if (obj)
                {
                    var spawned = GameObject.Instantiate(obj);
                    spawned.transform.position = parameters.TriggerPosition;
                }
            }
            public override void OnEnd(Owner owner, EventParameters parameters)
            {

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



    public class ReactionStateMachine : MonoBehaviour
    {
        [Serializable]
        public class State
        {
            public StateName StateName;
            public bool IsActiveState;
            public EventParameters LastBeginEvent;
            public EventParameters LastEndEvent;
            public string Notes;

            [HideInInspector]
            public ReactionStateMachine StateMachine;

            [SerializeReference, DerivedClassPicker]
            public Condition TestCondition;

            [SerializeReference, DerivedClassPicker]
            public Action TestAction;

            [SerializeReference, DerivedClassPicker(showPrefixLabel: false)]
            public List<Condition> Conditions;// = new();

            [SerializeReference, DerivedClassPicker(showPrefixLabel: false)]
            public List<Action> OnBeginActions;

            [SerializeReference, DerivedClassPicker(showPrefixLabel: false)]
            public List<Action> OnUpdate;

            [SerializeReference, DerivedClassPicker(showPrefixLabel: false)]
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

            public void Handshake(ReactionStateMachine owner)
            {
                StateMachine = owner;
                foreach (var action in Conditions)
                    Handshake(action);
                foreach (var action in OnBeginActions)
                    Handshake(action);
                foreach (var action in OnEndActions)
                    Handshake(action);
                if (IsActiveState)
                {
                    owner.InitialState = this;
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

                foreach (var update in Updates)
                    update.Update(owner);
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

                foreach (var observer in StateObservers)
                    observer.OnEnd(owner, parameters);

                foreach (var action in OnBeginActions)
                    action.OnEnd(owner, parameters);


                LastEndEvent = parameters;
                IsActiveState = false;
            }
        }
        [Tooltip("Name of the group of states to be exclusive with")]
        public string StateGroup;

        public List<State> States = new();
        List<IInitialize> Initializers = new();
        State CurrentState;
        State InitialState;

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
                if(s.StateName == name)
                {
                    state = s;
                    return true;
                }
            state = default;
            return false;
        }
        public void SetActiveState(State state, EventParameters parameters)
        {
#if UNITY_EDITOR
            if (state.StateMachine != this)
                Debug.LogError($"ReactionStateMachine '{name}' switches to an unknown state '{state.StateName.Name}'", this);
#endif
            if(CurrentState != null)
            {
                CurrentState.OnEnd(parameters);
            }
            CurrentState = state;
            if (CurrentState != null)
            {
                CurrentState.OnBegin(parameters);
            }
        }
        #region Unity Callback
        private void Start()
        {
            foreach (var state in States)
                state.Handshake(this);

            CurrentState = InitialState;

            var owner = new Owner(this);
            foreach (var i in Initializers)
                i.Initialize(owner);
        }

        void Update()
        {
            if(CurrentState != null)
            {
                CurrentState.Update();

            }
        }

        #endregion
    }
}