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
    }

    [Serializable]
    public struct EventParameters
    {
        public GameObject TriggerObject;
        public GameObject PreviousTriggerObject;
        public Vector3 Position;
    }
    [Serializable]
    public struct GameObjectReference
    {
        public enum TypeEnum
        {
            Self,
            Object,
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

        public GameObject GetTargetGameObject(GameObject self, EventParameters eventParams)
        {
            if(!TryGetTargetGameObject(self, eventParams, out var go))
            {
                switch (Type)
                {
                    case GameObjectReference.TypeEnum.Self:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference unable to find Self");
                        break;
                    case GameObjectReference.TypeEnum.Object:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{self.name}' unable to find Object", self);
                        break;
                    case GameObjectReference.TypeEnum.TriggerObject:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{self.name}' could not find Trigger Object", self);
                        break;
                    case GameObjectReference.TypeEnum.PreviousTriggerObject:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{self.name}' could not find Previous Trigger Object", self);
                        break;
                    case GameObjectReference.TypeEnum.FirstOfType:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{self.name}' unable to find First Object Of Type '{String}'", self);
                        break;
                    case GameObjectReference.TypeEnum.FirstWithTag:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{self.name}' unable to find First Object With Tag '{String}'", self);
                        break;
                    case GameObjectReference.TypeEnum.FirstWithName:
                        Debug.LogWarning($"[{Time.frameCount}] GameObjectReference on '{self.name}' unable to find First Object With Name '{String}'", self);
                        break;
                }
            }
            return go;
        }
        public bool TryGetTargetGameObject(GameObject self, EventParameters eventParams, out GameObject go)
        {
            switch (Type)
            {
                case GameObjectReference.TypeEnum.Self:
                    go = self;
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
    }
    [Serializable]
    public struct PositionReference
    {

        public enum TypeEnum
        {
            Self,
            AtPosition,
            AtTriggerPosition,
            AtGameObject,
        }
        public TypeEnum Type;
        public Vector3 AtPosition;
        public GameObjectReference AtTransform;


        public Vector3 GetPosition(GameObject self, EventParameters eventParams)
        {
            if (!TryGetPosition(self, eventParams, out var go))
            {
                switch (Type)
                {
                    case PositionReference.TypeEnum.Self:
                        Debug.LogWarning($"[{Time.frameCount}] PositionReference unable to find Self");
                        break;
                    case PositionReference.TypeEnum.AtPosition:
                        break;
                    case PositionReference.TypeEnum.AtTriggerPosition:
                        Debug.LogWarning($"[{Time.frameCount}] PositionReference on '{self.name}' could not find Trigger Object", self);
                        break;
                }
            }
            return go;
        }
        public bool TryGetPosition(GameObject self, EventParameters eventParams, out Vector3 position)
        {
            switch (Type)
            {
                case PositionReference.TypeEnum.Self:
                    position = self.transform.position;
                    return true;
                case PositionReference.TypeEnum.AtPosition:
                    position = AtPosition;
                    return true;
                case PositionReference.TypeEnum.AtGameObject:
                    if (AtTransform.TryGetTargetGameObject(self, eventParams, out var obj))
                    {
                        position = obj.transform.position;
                        return true;
                    }
                    else
                    {
                        position = self.transform.position;
                    }
                    return false;
                case PositionReference.TypeEnum.AtTriggerPosition:
                    position = eventParams.Position;
                    break;
            }
            position = default;
            return false;
        }
    }

    public interface IUpdate
    {
        void Update();
    }
    [Serializable]
    public abstract class Action
    {
        public abstract void OnBegin(EventParameters parameters);
        public abstract void OnEnd(EventParameters parameters);
    }
    namespace Actions
    {
        [Serializable, ClassPickerName("Spawn")]
        public class ActionSpawn : Action
        {
            public GameObject ObjectToSpawn;
            public PositionReference SpawnPosition;
            public override void OnBegin(EventParameters parameters)
            {
                var spawned = GameObject.Instantiate(ObjectToSpawn);
                spawned.transform.position = parameters.Position;
            }
            public override void OnEnd(EventParameters parameters)
            {

            }
        }

    }

    [Serializable]
    public abstract class Condition
    {
        public abstract bool Pass(EventParameters parameters);
    }
    [Serializable]
    public class Condition2
    {
        public bool Pass(EventParameters parameters) => false;
    }
    [Serializable, ClassPickerName("Cooldown")]
    public class ConditionCooldown : Condition, IUpdate
    {
        float TimeLeft;
        public override bool Pass(EventParameters parameters)
        {
            return TimeLeft <= 0;
        }
        void IUpdate.Update()
        {
            TimeLeft -= Time.deltaTime;
        }
    }



    public class ReactionStateMachine : MonoBehaviour
    {
        [Serializable]
        public struct State
        {
            public string Name;
            public string Notes;

            ReactionStateMachine StateMachine;

            [SerializeReference, DerivedClassPicker]
            public Condition TestCondition;

            [SerializeReference, DerivedClassPicker]
            public Action TestAction;

            [SerializeReference, DerivedClassPicker(showPrefixLabel: false)]
            public List<Condition> Conditions;// = new();

            [SerializeReference, DerivedClassPicker(showPrefixLabel: false)]
            public List<Action> Actions;


        }
        [Tooltip("Name of the group of states to be exclusive with")]
        public string StateGroup;

        public List<State> States = new();
        public List<IUpdate> Updates = new();

        [SerializeReference, DerivedClassPicker]
        public Condition TestCondition;

        [SerializeReference, DerivedClassPicker]
        public Action TestAction;

        void AddUpdate(IUpdate update)
        {
            Updates.Add(update);
        }
        void RemoveUpdate(IUpdate update)
        {
            Updates.Remove(update);
        }
        void Update()
        {
            foreach(var u in Updates)
                u.Update();
        }
    }
}