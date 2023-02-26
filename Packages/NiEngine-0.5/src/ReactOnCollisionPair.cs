
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

        [Tooltip("Conditions to execute this reaction")]
        [FormerlySerializedAs("Conditions")]
        public ReactionConditions StateConditions;

        [Tooltip("Reaction executed when a valid pair of ReactOnConllisionPair collide.")]
        public ReactionList OnReact;

        //public AnimatorStateReference MustBeInAnimatorState;
        //public ReactionStateReference MustBeInReactionState;

        //public List<Reaction> Reactions;
        //public List<ReactionStateReference> ReactionStates;


        [Header("Debug:")]
        [Tooltip("Print to console events caused by this ReactOnCollisionPair")]
        public bool DebugLog = false;
        public bool DebugLogTouching = false;

        //[Header("Events:")]
        //[SerializeField]
        //[Tooltip("Event called when the reaction happens")]
        //UnityEvent<ReactOnCollisionPair, ReactOnCollisionPair> OnReact;
#if NIE_EXTRAEVENT

        [SerializeField]
        [Tooltip("Event called when this ReactiveItem starts touching another ReactiveItem with matching respective names. Parameters are (ReactiveItem this, ReactiveItem other)")]
        UnityEvent<ReactOnCollisionPair, ReactOnCollisionPair> OnTouchBegin;

        [SerializeField]
        [Tooltip("Event called when this ReactiveItem stops touching another ReactiveItem with matching respective names. Parameters are (ReactiveItem this, ReactiveItem other)")]
        UnityEvent<ReactOnCollisionPair, ReactOnCollisionPair> OnTouchEnd;

        [SerializeField]
        [Tooltip("Event called when this ReactiveItem is touching another ReactiveItem with matching respective names. Parameters are (ReactiveItem this, ReactiveItem other)")]
        UnityEvent<ReactOnCollisionPair, ReactOnCollisionPair> OnTouching;
#endif

        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;

        // Keep track of what ReactOnCollisionPair are currently touching
        List<ReactOnCollisionPair> m_TouchingWith = new();

        // Keep track of what Reaction is currently on cooldown
        List<DelayedReaction> m_CooldownWith = new();

        [System.Serializable]
        public class DelayedReaction
        {
            public ReactOnCollisionPair Other;
            public Vector3 Position;
            public float TimerCountdown;
            public DelayedReaction(ReactOnCollisionPair other, Vector3 position, float delay)
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
        // Keep track of all Reaction currently on a delay
        List<DelayedReaction> m_DelayReactions = new();
#endregion

        public void React(DelayedReaction delayedReaction)
        {
            
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] ReactOnCollisionPair '{ThisReactionName}' reacts to '{delayedReaction.Other.ThisReactionName}'. Triggering Object:'{(delayedReaction.Other == null ? "<null>" : delayedReaction.Other.gameObject.name)}' at:'{delayedReaction.Position}'");

            OnReact.React(TargetObject, delayedReaction.Other.gameObject, delayedReaction.Position);

            if (ReactionCooldown > 0)
            {
                delayedReaction.TimerCountdown = ReactionCooldown;
                m_CooldownWith.Add(delayedReaction);
            }
        }
        public bool CanReact(ReactOnCollisionPair other, Vector3 position)
        {

            if (!enabled) return false;
            if ((ObjectLayerMask.value & (1 << other.gameObject.layer)) == 0) return false;
            if (SingleAtOnce && m_CurrentSingleReaction != null) return false;
            if (!StateConditions.CanReact(other.gameObject, position)) return false;
            if (!OnReact.CanReact(TargetObject, other.gameObject, position)) return false;
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
                    React(reaction);
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
                Touching(other);
        }

        void OnDestroy()
        {
            foreach (var other in m_TouchingWith)
            {
                EndTouch(other);
                other.EndTouchIfTouching(this);
            }
        }

#region Touching state
        void BeginTouch(ReactOnCollisionPair other, Vector3 position)
        {
            if (DebugLogTouching)
                Debug.Log($"[{Time.frameCount}] ReactOnCollisionPair '{ThisReactionName}' begins touching '{other.ThisReactionName}'. Triggering Object:'{(other == null ? "<null>" : other.gameObject.name)}' at:'{position}'");
            m_TouchingWith.Add(other);

#if NIE_EXTRAEVENT
            OnTouchBegin?.Invoke(this, other);
#endif

            // React if not currently on cooldown
            if (m_CooldownWith.All(x => x.Other != other))
            {
                var reaction = new DelayedReaction(other, position, ReactionDelay);
                if (ReactionDelay == 0)
                    React(reaction);
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

        void EndTouch(ReactOnCollisionPair other)
        {
            if (DebugLogTouching)
                Debug.Log($"[{Time.frameCount}] ReactOnCollisionPair '{ThisReactionName}' begins touching '{other.ThisReactionName}'. Triggering Object:'{(other == null ? "<null>" : other.gameObject.name)}'");

#if NIE_EXTRAEVENT
            OnTouchEnd?.Invoke(this, other);
#endif

        }

        bool EndTouchIfTouching(ReactOnCollisionPair other)
        {
            // if reactions require the objects to always touch during the delay, remove all current reaction with the other object.
            if (MustTouchDuringDelay)
                m_DelayReactions.RemoveAll(reaction => reaction.Other == other);

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
        private void OnTriggerEnterExit(Collider otherCollider)
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