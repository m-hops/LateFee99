using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nie
{
    [AddComponentMenu("Nie/Player/FocusController")]
    public class FocusController : MonoBehaviour
    {
        [Tooltip("Where the ray cast to detect any object with a 'ReactOnFocus' component will be directed toward. If left null, will ray cast in the middle of the screen")]
        public Transform FocusTarget;
        
        [Tooltip("Focus only on object of these layers")]
        public LayerMask LayerMask;

        [Tooltip("Focus only on object closer to this distance")]
        public float MaxDistance = 10;

        [Tooltip("Object to move to the currently focused 'ReactOnFocus' object.")]
        public GameObject Hand;

        [Tooltip("Output debug log when objects are focused or unfocused")]
        public bool DebugLog;

        ReactOnFocus m_Focus;

        public Vector3 FocusPosition;
        public Vector3 RayCastTarget => FocusTarget != null ? FocusTarget.position : transform.position + transform.forward;
        public ReactOnFocus CurrentFocus => m_Focus;
        
        void Update()
        {
            var ray = new Ray(transform.position, (RayCastTarget - transform.position).normalized);
            if (Physics.Raycast(ray, out var hit, MaxDistance, LayerMask.value) && hit.collider.gameObject.TryGetComponent<ReactOnFocus>(out var focusable) && focusable.CanFocus(this, hit.point))
            {
                ShowHand(hit.point);
                if (m_Focus != focusable)
                    Focus(focusable, hit.point);
            }
            else
            {
                Unfocus();
            }


        }
        void HideHand()
        {
            if (Hand != null && Hand.TryGetComponent<MeshRenderer>(out var rendererHand))
                rendererHand.enabled = false;
            if (FocusTarget.TryGetComponent<MeshRenderer>(out var rendererGrabPosition))
                rendererGrabPosition.enabled = true;
        }
        void ShowHand(Vector3 position)
        {
            if (Hand != null)
            {
                Hand.transform.position = position;
                if (Hand.TryGetComponent<MeshRenderer>(out var rendererHand))
                    rendererHand.enabled = true;
            }
            if (FocusTarget.TryGetComponent<MeshRenderer>(out var rendererGrabPosition))
                rendererGrabPosition.enabled = false;
        }
        public void Focus(ReactOnFocus focusable, Vector3 position)
        {
            Unfocus();
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] FocuserController '{name}' Focus on '{focusable.name}'");
            m_Focus = focusable;
            m_Focus.Focus(this, position);
        }
        public void Unfocus()
        {
            if (m_Focus == null) return;
            if (DebugLog)
                Debug.Log($"[{Time.frameCount}] FocuserController '{name}' Unfocus on '{m_Focus.name}'");
            m_Focus.Unfocus(this, FocusPosition);
            m_Focus = null;
            HideHand();
        }
    }
}