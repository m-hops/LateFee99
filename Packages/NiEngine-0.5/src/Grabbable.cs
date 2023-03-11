using UnityEngine;
using UnityEngine.Events;

namespace Nie
{
    /// <summary>
    /// Makes the owner Gameobject grabbable by a GrabberController
    /// </summary>
    [AddComponentMenu("Nie/Object/Grabbable")]
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        public bool DebugLog;

        [Tooltip("If true, will remove the grabbed object from its parent GameObject.")]
        public bool DetachWhenGrabbed = false;

        [Tooltip("If true, will remove the grabbed object from its parent GameObject.")]
        public bool ReattachWhenRelease = false;

        [UnityEngine.Serialization.FormerlySerializedAs("NewConditions")]
        public ConditionSet Conditions;

        [UnityEngine.Serialization.FormerlySerializedAs("NewOnGrab")]
        public StateActionSet OnGrab;

        [UnityEngine.Serialization.FormerlySerializedAs("NewOnRelease")]
        public ActionSet OnRelease;

        //[Tooltip("Conditions to be able to grab this object")]
        //public ReactionConditions Conditions;

        //[SerializeField]
        //[Tooltip("Reaction executed when this grabbable is grabbed by a GrabberController")]
        //ReactionList OnGrab;

        //[SerializeField]
        //[Tooltip("Reaction executed when this grabbable is release by a GrabberController")]
        //ReactionList OnRelease;

        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;

        public bool IsGrabbed => GrabbedBy != null;
        public GrabberController GrabbedBy { get; private set; }

        Transform m_PreviousParent;
        public void ReleaseIfGrabbed()
        {
            GrabbedBy?.ReleaseGrabbed();
        }

        public bool CanGrab(GrabberController by, Vector3 position)
        {
            if (!enabled) return false;
            var parameters = EventParameters.Trigger(gameObject, gameObject, by.gameObject, position);
            if (DebugLog)
            {
                Debug.Log($"[{Time.frameCount}] Grabbable.CanGrab '{name}' {parameters}");
                parameters = parameters.WithDebugTrace(new());
            }
            bool pass = Conditions.Pass(new Owner(this), parameters);
            if (DebugLog)
            {
                Debug.Log($"[{Time.frameCount}] Grabbable.CanGrab '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
            }
            return pass;
        }
        /// <summary>
        /// Call when a GrabberController grabs this grabbable
        /// </summary>
        /// <param name="by"></param>
        public void GrabBy(GrabberController by, Vector3 grabPosition)
        {
            var parameters = EventParameters.Trigger(gameObject, gameObject, by.gameObject, grabPosition);
            if (DebugLog)
            {
                Debug.Log($"[{Time.frameCount}] Grabbable.GrabBy '{name}' {parameters}");
                parameters = parameters.WithDebugTrace(new());
            }
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Grabbable '{name}' Grab By '{by.name}'");
            GrabbedBy = by;
            if(ReattachWhenRelease)
                m_PreviousParent = transform.parent;
            if (DetachWhenGrabbed)
                transform.parent = null;
            OnGrab.OnBegin(new Owner(this), parameters);
            if (DebugLog)
            {
                Debug.Log($"[{Time.frameCount}] Grabbable.GrabBy '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
            }
        }

        /// <summary>
        /// Call when a GrabberController release this grabbable
        /// </summary>
        /// <param name="by"></param>
        public void ReleaseBy(GrabberController by)
        {
            var parameters = EventParameters.Trigger(gameObject, gameObject, by.gameObject);
            if (DebugLog)
            {
                Debug.Log($"[{Time.frameCount}] Grabbable.GrabBy '{name}' {parameters}");
                parameters = parameters.WithDebugTrace(new());
            }
            if (ReattachWhenRelease)
                transform.parent = m_PreviousParent;
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Grabbable '{name}' Release By '{by.name}'");

            OnGrab.OnEnd(new Owner(this), parameters);
            OnRelease.Act(new Owner(this), parameters);
            GrabbedBy = null;
            if (DebugLog)
            {
                Debug.Log($"[{Time.frameCount}] Grabbable.GrabBy '{name}' {parameters} trace:\r\n{parameters.DebugTrace}");
            }
        }

    }
}