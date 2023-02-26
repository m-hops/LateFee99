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

        [Tooltip("Conditions to be able to grab this object")]
        public ReactionConditions Conditions;

        [SerializeField]
        [Tooltip("Reaction executed when this grabbable is grabbed by a GrabberController")]
        ReactionList OnGrab;

        [SerializeField]
        [Tooltip("Reaction executed when this grabbable is release by a GrabberController")]
        ReactionList OnRelease;

        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;

        public bool IsGrabbed => GrabbedBy != null;
        public GrabberController GrabbedBy { get; private set; }

        public void ReleaseIfGrabbed()
        {
            GrabbedBy?.ReleaseGrabbed();
        }

        public bool CanGrab(GrabberController by, Vector3 position)
        {
            if (!enabled) return false;
            if (!Conditions.CanReact(by.gameObject, position)) return false;
            if (!OnGrab.CanReact(TargetObject, by.gameObject, position)) return false;
            return true;
        }
        /// <summary>
        /// Call when a GrabberController grabs this grabbable
        /// </summary>
        /// <param name="by"></param>
        public void GrabBy(GrabberController by, Vector3 grabPosition)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Grabbable '{name}' Grab By '{by.name}'");
            GrabbedBy = by;
            OnGrab.React(TargetObject, by.gameObject, grabPosition);
        }

        /// <summary>
        /// Call when a GrabberController release this grabbable
        /// </summary>
        /// <param name="by"></param>
        public void ReleaseBy(GrabberController by)
        {
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] Grabbable '{name}' Release By '{by.name}'");
            OnRelease.React(TargetObject, by.gameObject, transform.position);
            GrabbedBy = null;
        }

    }
}