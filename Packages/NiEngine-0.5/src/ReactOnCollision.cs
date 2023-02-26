using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace Nie
{
    [AddComponentMenu("Nie/Object/ReactOnCollision")]
    public class ReactOnCollision : MonoBehaviour
    {
        #region Data
        [Header("Condition:")]

        [Tooltip("Time in second to delay the reaction.")]
        public float ReactionDelay = 0;

        [Tooltip("If check and reaction is delayed, the 2 objects must be touching for the full duration of the delay.")]
        public bool MustTouchDuringDelay;

        [Tooltip("Once Triggered, cannot triggered again within the cooldown period, in seconds.")]
        public float ReactionCooldown = 0;

        [Tooltip("If reaction is delayed, do not trigger new reactions during the delay.")]
        public bool SingleAtOnce = false;

        [Tooltip("Only react to collision with objects of these layers")]
        public LayerMask ObjectLayerMask = -1;

        public bool ReactToCollision = true;
        public bool ReactToTrigger = true;


        [Tooltip("Conditions to touch this touchable")]
        public ReactionConditions Conditions;

        [Tooltip("Reaction executed when this object starts colliding with another object.")]
        public ReactionList ReactionOnCollisionEnter;

        [Tooltip("Reaction executed when this object ends colliding with another object.")]
        public ReactionList ReactionOnCollisionExit;

        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;
        public bool CanReact(ReactOnCollision by, Vector3 position)
        {
            if (!enabled) return false;
            if (!Conditions.CanReact(by.gameObject, position)) return false;
            if (!ReactionOnCollisionEnter.CanReact(TargetObject, by.gameObject, position)) return false;
            return true;
        }

        public void Touch(TouchController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Touched By '{by.name}'");
            ReactionOnCollisionEnter.TryReact(TargetObject, by.gameObject, position);
        }

        public void Release(TouchController by, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Touchable '{name}' Released By '{by.name}'");
            ReactionOnCollisionExit.TryReact(TargetObject, by.gameObject, position);
        }



        [Tooltip("Print to console events caused by this ReactOnCollision")]
        public bool DebugLog = false;

        // Keep track of what ReactiveObject are currently touching
        List<GameObject> m_TouchingWith = new();


        float m_CooldownTimer;

        /// <summary>
        /// set only if SingleAtOnce is true
        /// </summary>
        GameObject m_CurrentSingleReaction;

        [System.Serializable]
        public class DelayedReaction
        {
            public GameObject Other;
            public Vector3 Position;
            public float TimerCountdown;
            public DelayedReaction(GameObject other, Vector3 position, float delay)
            {
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
        }

        // Keep track of all reactions currently on a delay
        List<DelayedReaction> m_DelayedReactions = new();
#endregion

        public void React(DelayedReaction delayedReaction)
        {

            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactOnCollision '{name}' reacts to '{delayedReaction.Other.name}'");

            ReactionOnCollisionEnter.React(TargetObject, delayedReaction.Other.gameObject, delayedReaction.Position);

            if (ReactionCooldown > 0)
                m_CooldownTimer = ReactionCooldown;
        }

        public bool RequestReaction(GameObject other, Vector3 position)
        {
            if (!enabled) return false;
            if (m_CooldownTimer > 0) return false;
            if (SingleAtOnce && m_CurrentSingleReaction != null) return false;
            if ((ObjectLayerMask.value & (1 << other.layer)) == 0) return false;
            if (!Conditions.CanReact(other, position)) return false;
            if (!ReactionOnCollisionEnter.CanReact(TargetObject, other, position)) return false;
            if (SingleAtOnce) m_CurrentSingleReaction = other;
            return true;
        }


        private void Update()
        {
            // Update all reaction on delay.
            m_DelayedReactions.RemoveAll(reaction =>
            {
                // abort the reaction if the other object was deleted
                if (reaction.Other == null)
                    return true;

                if (reaction.Tick())
                {
                    React(reaction);
                    return true;
                }
                return false;
            });



            if (m_CurrentSingleReaction != null && m_DelayedReactions.Count == 0)
                m_CurrentSingleReaction = null;
        }

        void LateUpdate()
        {
            // all GameObject in TouchingWith are still touching this frame
            foreach (var other in m_TouchingWith)
                Touching(other);
        }

        void OnDestroy()
        {
            foreach (var other in m_TouchingWith)
                EndTouch(other);
        }

#region Touching state
        void BeginTouch(GameObject other, Vector3 position)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactOnCollision '{name}' begins touching '{other.name}'");

            m_TouchingWith.Add(other);

            var reaction = new DelayedReaction(other, position, ReactionDelay);
            if (ReactionDelay == 0)
                React(reaction);
            else
                m_DelayedReactions.Add(reaction);

        }

        void Touching(GameObject other)
        {
        }

        void EndTouch(GameObject other)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactOnCollision '{name}' stopped touching '{other.name}'");

        }

        bool EndTouchIfTouching(GameObject other)
        {
            // if reactions require the objects to always touch during the delay, remove all current reaction with the other object.
            if (MustTouchDuringDelay)
                m_DelayedReactions.RemoveAll(reaction => reaction.Other == other);

            if (m_TouchingWith.Remove(other))
            {
                EndTouch(other);
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
            if (RequestReaction(collision.gameObject, position))
                BeginTouch(collision.gameObject, position);
        }

        public void OnCollisionExit(Collision collision)
        {
            if (!ReactToCollision) return;
            EndTouchIfTouching(collision.gameObject);
        }

        private void OnTriggerEnter(Collider otherCollider)
        {
            if (!ReactToTrigger) return;
            if (RequestReaction(otherCollider.gameObject, transform.position))
                BeginTouch(otherCollider.gameObject, transform.position);
        }
        private void OnTriggerEnterExit(Collider otherCollider)
        {
            if (!ReactToTrigger) return;
            EndTouchIfTouching(otherCollider.gameObject);
        }
#endregion
    }

}