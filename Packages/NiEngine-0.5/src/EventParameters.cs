using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie
{

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

        public System.Text.StringBuilder DebugTrace;
        public bool HasTraces => DebugTrace != null && DebugTrace.Length > 0;

        public EventParameters WithDebugTrace(System.Text.StringBuilder stringBuilder)
        {
            var copy = this;
            copy.DebugTrace = stringBuilder;
            return copy;
        }
        public EventParameters WithSelf(GameObject self)
            => WithSelf(self: self, from: Self);
        public EventParameters WithSelf(GameObject self, GameObject from)
        {
            var copy = this;
            copy.Current.From = from;
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
            OnBegin = ParameterSet.Trigger(from, triggerObject, Vector3.zero),
        };
        public static EventParameters Trigger(GameObject self, GameObject from, GameObject triggerObject, Vector3 position) => new EventParameters
        {
            Self = self,
            Current = ParameterSet.Trigger(from, triggerObject, Vector3.zero),
            OnBegin = ParameterSet.Trigger(from, triggerObject, Vector3.zero),
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
}