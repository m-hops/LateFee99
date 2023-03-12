
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;
using System.Linq;

namespace Nie
{
    /// <summary>
    /// A reactive object will trigger a reaction when it collide with a matching ReactOnCollisionPair
    /// </summary>
    [AddComponentMenu("Nie/Object/ReactOnCollisionPair")]
    public class ReactOnCollisionPair : MonoBehaviour
    {
        #region Data
        [Header("Reaction Condition:")]

        [Tooltip("Name for this object.")]
        public string ThisReactionName;

        [Tooltip("Name for the other object. \n\rFor the reaction to happen between 2 ReactOnCollisionPair: \n\r* this.ThisReactionName must be equal to other.OtherReactionName \n\r AND \n\r\t* this.OtherReactionName must be equal to other.ThisReactionName.")]
        public string OtherReactionName;

        [Tooltip("Time in second to delay the reaction.")]
        public float ReactionDelay = 0;

        [Tooltip("If check and reaction is delayed, the 2 objects must be touching for the full duration of the delay.")]
        public bool MustTouchDuringDelay;

        [Tooltip("Once this ReactOnCollisionPair reacts with another ReactOnCollisionPair, the same reaction cannot be triggered again within the cooldown period, in seconds.")]
        public float ReactionCooldown = 0;

        [Tooltip("If reaction is delayed, do not trigger new reactions during the delay.")]
        public bool SingleAtOnce = false;

        [Tooltip("Only react with objects of these layers")]
        public LayerMask ObjectLayerMask = -1;

        /// <summary>
        /// set only if SingleAtOnce is true
        /// </summary>
        private ReactOnCollisionPair m_CurrentSingleReaction;

        public bool ReactToCollision = true;
        public bool ReactToTrigger = true;


        public ConditionSet Conditions;

        public StateActionSet OnBegin;

        public ActionSet OnEnd;


        [Header("Debug:")]
        [Tooltip("Print to console events caused by this ReactOnCollisionPair")]
        public bool DebugLog = false;
        public bool DebugLogTouching = false;

        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;

        // Keep track of what ReactOnCollisionPair are currently touching
        List<ReactionTrigger> m_TouchingWith = new();

        // Keep track of what Reaction is currently on cooldown
        List<ReactionTrigger> m_CooldownWith = new();

        [System.Serializable]
        public class ReactionTrigger
        {
            public ReactOnCollisionPair Self;
            public ReactOnCollisionPair Other;
            public Vector3 Position;
            public float TimerCountdown;
            public bool HasBegun;
            public ReactionTrigger(ReactOnCollisionPair self, ReactOnCollisionPair other, Vector3 position, float delay)
            {
                Self = self;
                Other = other;
                Position = position;
                TimerCountdown = delay;
            }
            public bool Tick()
            {
                if (TimerCountdown >= 0)
                {
                    TimerCountdown -= Time.deltaTime;
                    return TimerCountdown < 0;
                }
                return false;
            }
            public void Begin()
            {

                if (Self.DebugLog)
                    Debug.Log($"[{Time.frameCount}] ReactOnCollisionPair '{Self.name}.{Self.ThisReactionName}' collision begin with '{Other.name}.{Other.ThisReactionName}'.", Self);

                Self.OnBegin.OnBegin(new Owner(Self), EventParameters.Trigger(Self.gameObject, Self.gameObject, Other.gameObject, Position));


                if (Self.ReactionCooldown > 0)
                {
                    TimerCountdown = Self.ReactionCooldown;
                    Self.m_CooldownWith.Add(this);
                }
                HasBegun = true;
            }
            public void End()
            {
                if (Self.DebugLog)
                    Debug.Log($"[{Time.frameCount}] ReactOnCollisionPair '{Self.name}.{Self.ThisReactionName}' collision end with '{Other.name}.{Other.ThisReactionName}'.", Self);

                Self.OnBegin.OnEnd(new Owner(Self), EventParameters.Trigger(Self.gameObject, Self.gameObject, Other.gameObject, Position));
                Self.OnEnd.Act(new Owner(Self), EventParameters.Trigger(Self.gameObject, Self.gameObject, Other.gameObject, Position));
            }
        }
        // Keep track of all Reaction currently on a delay
        List<ReactionTrigger> m_DelayReactions = new();
#endregion

        public bool CanReact(ReactOnCollisionPair other, Vector3 position)
        {

            if (!enabled) return false;
            if ((ObjectLayerMask.value & (1 << other.gameObject.layer)) == 0) return false;
            if (SingleAtOnce && m_CurrentSingleReaction != null) return false;

            if (!Conditions.Pass(new Owner(this), EventParameters.Trigger(gameObject, gameObject, other.gameObject, position))) return false;

            return true;
        }
        public bool RequestReaction(ReactOnCollisionPair other, Vector3 position)
        {
            if (!CanReact(other, position)) return false;
            if (!other.CanReact(this, position)) return false;
            if (other.ThisReactionName != OtherReactionName || other.OtherReactionName != ThisReactionName) return false;
            if (SingleAtOnce) m_CurrentSingleReaction = other;
            return true;
        }


        private void Update()
        {
            // Update all reaction on delay.
            m_DelayReactions.RemoveAll(reaction =>
            {
                // abort the reaction if the other object was deleted
                if (reaction.Other == null)
                    return true;

                if (reaction.Tick())
                {
                    reaction.Begin();
                    return true;
                }
                return false;
            });


            // Update all reactions on cooldown
            m_CooldownWith.RemoveAll(reaction =>
            {
                if (reaction.Other == null)
                    return true;

                if (reaction.Tick())
                {
                    return true;
                }
                return false;
            });

            if (m_CurrentSingleReaction != null && m_DelayReactions.Count == 0 && m_CooldownWith.Count == 0)
                m_CurrentSingleReaction = null;
        }

        void LateUpdate()
        {
            // all ReactOnCollisionPair in TouchingWith are still touching this frame
            foreach (var other in m_TouchingWith)
                Touching(other.Other);
        }

        void OnDestroy()
        {
            foreach (var other in m_TouchingWith)
            {
                other.End();
                other.Other.EndTouchIfTouching(this);
            }
        }

#region Touching state
        void BeginTouch(ReactOnCollisionPair other, Vector3 position)
        {
            var reaction = new ReactionTrigger(this, other, position, ReactionDelay);
            m_TouchingWith.Add(reaction);

            // React if not currently on cooldown
            if (m_CooldownWith.All(x => x.Other != other))
            {
                if (ReactionDelay == 0)
                    reaction.Begin();
                else
                    m_DelayReactions.Add(reaction);
            }
        }

        void Touching(ReactOnCollisionPair other)
        {
#if NIE_EXTRAEVENT
            OnTouching?.Invoke(this, other);
#endif
        }

        bool EndTouchIfTouching(ReactOnCollisionPair other)
        {
            // if reactions require the objects to always touch during the delay, remove all current reaction with the other object.
            if (MustTouchDuringDelay)
                m_DelayReactions.RemoveAll(reaction => reaction.Other == other);
            
            var reaction = m_TouchingWith.FirstOrDefault(reaction => reaction.Other == other);
            if (reaction != null && m_TouchingWith.Remove(reaction))
            {
                reaction.End();
                return true;
            }
            return false;
        }
#endregion

#region Collision Callbacks
        public void OnCollisionEnter(Collision collision)
        {
            if (!enabled) return;
            if (!ReactToCollision) return;
            var position = collision.GetContact(0).point;
            foreach (var other in collision.gameObject.GetComponentsInChildren<ReactOnCollisionPair>().Where(other => RequestReaction(other, position)))
                BeginTouch(other, position);
        }

        public void OnCollisionExit(Collision collision)
        {
            if (!ReactToCollision) return;
            foreach (var other in collision.gameObject.GetComponentsInChildren<ReactOnCollisionPair>())
                EndTouchIfTouching(other);
        }

        private void OnTriggerEnter(Collider otherCollider)
        {
            if (!ReactToTrigger) return;
            foreach (var other in otherCollider.GetComponentsInChildren<ReactOnCollisionPair>().Where(other => other.ReactToTrigger && RequestReaction(other, transform.position)))
            {
                BeginTouch(other, transform.position);
                other.BeginTouch(this, transform.position);
            }
        }
        private void OnTriggerExit(Collider otherCollider)
        {
            if (!ReactToTrigger) return;
            foreach (var other in otherCollider.GetComponentsInChildren<ReactOnCollisionPair>().Where(other => other.ReactToTrigger))
            {
                EndTouchIfTouching(other);
                other.EndTouchIfTouching(this);
            }

        }
#endregion
    }

}