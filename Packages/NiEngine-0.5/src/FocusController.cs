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
        public LayerMask LayerMask = -1;

        [Tooltip("Focus only on object closer to this distance")]
        public float MaxDistance = 10;

        [Tooltip("Object to move to the currently focused 'ReactOnFocus' object.")]
        public GameObject Hand;

        [Tooltip("Output debug log when objects are focused or unfocused")]
        public bool DebugLog;

        [Header("Run-Time State")]
        public Vector3 FocusPosition;
        public Vector3 RayCastTarget => FocusTarget != null ? FocusTarget.position : transform.position + transform.forward;

        void UnfocusAll()
        {
            if(CurrentCollider == CurrentRigidBody)
                UnfocusOn(CurrentCollider);
            else
            {
                UnfocusOn(CurrentCollider);
                UnfocusOn(CurrentRigidBody);
            }

            CurrentCollider = null;
            CurrentRigidBody = null;
        }
        enum FocusType
        {
            N_N,    // No Collider, no RigidBody
            A_N,    // With Collider, no RigidBody
            A_A,    // With Collider and RigidBody on same GameObject
            A_B,    // With Collider and RigidBody on different GameObject
            N_A,    // No collider, with RigidBody
        }
        FocusType CurrentFocusType = FocusType.N_N;
        public GameObject CurrentCollider;
        public GameObject CurrentRigidBody;

        FocusType GetFocusType(ref GameObject collider, ref GameObject rigidBody, out bool sendToRigidBody)
        {
            sendToRigidBody = false;
            if (rigidBody == null)
            {
                // *_N
                if (collider == null || !CanFocusOn(collider, out sendToRigidBody))
                {
                    collider = null;
                    rigidBody = null;
                    return FocusType.N_N;
                }
                else
                {
                    rigidBody = null;
                    return FocusType.A_N;
                }
            }
            else
            {
                // *_A, *_B
                if (collider == null)
                {
                    // N_A
                    if (CanFocusOn(rigidBody, out var _))
                    {
                        collider = null;
                        return FocusType.N_A;
                    }
                    else
                    {
                        collider = null;
                        rigidBody = null;
                        return FocusType.N_N;
                    }
                }
                else if (collider == rigidBody)
                {
                    // A_A
                    if (CanFocusOn(collider, out sendToRigidBody))
                        return FocusType.A_A;
                    else
                    {
                        collider = null;
                        rigidBody = null;
                        return FocusType.N_N;
                    }
                }
                else
                {
                    // A_B
                    if (CanFocusOn(collider, out sendToRigidBody))
                    {
                        if (sendToRigidBody && CanFocusOn(rigidBody, out var _))
                            return FocusType.A_B;
                        else
                        {
                            rigidBody = null;
                            return FocusType.A_N;
                        }
                    }
                    else if (CanFocusOn(rigidBody, out var _))
                    {
                        collider = null;
                        return FocusType.N_A;
                    }
                    else
                    {
                        collider = null;
                        rigidBody = null;
                        return FocusType.N_N;
                    }
                }
            }
        }
        void TransitToN_N()
        {
            // -> N_N
            UnfocusAll();
            CurrentFocusType = FocusType.N_N;
            HideHand();

        }
        void Update()
        {
            CheckTransition();
        }
        void CheckTransition()
        {

            var ray = new Ray(transform.position, (RayCastTarget - transform.position).normalized);
            if (Physics.Raycast(ray, out var hit, MaxDistance, LayerMask.value))
            {
                var position = hit.point;
                GameObject nCollider = hit.collider.GetGameObjectOrNull();
                GameObject nRigidBody = hit.rigidbody.GetGameObjectOrNull();
                var targetType = GetFocusType(ref nCollider, ref nRigidBody, out var sentToRB);

                if (DebugLog)
                {
                    if(CurrentCollider != nCollider || CurrentRigidBody != nRigidBody)
                        Debug.Log($"[{Time.frameCount}] FocuserController '{name}' transit {CurrentFocusType} -> {targetType} : ('{CurrentCollider.GetNameOrNull()}', '{CurrentRigidBody.GetNameOrNull()}') -> ('{nCollider.GetNameOrNull()}', '{nRigidBody.GetNameOrNull()}')", this);
                }
                bool nShowHand = false;
                if (targetType == FocusType.N_N)
                {
                    TransitToN_N();
                    return;
                }
                // Transition notation: Current -> Next
                switch (CurrentFocusType)
                {
                    case FocusType.N_N:
                        switch (targetType)
                        {
                            case FocusType.A_N:
                                // N_N -> A_N
                                FocusOn(nCollider, ref nShowHand);
                                break;
                            case FocusType.A_A:
                                // N_N -> A_A
                                FocusOn(nCollider, ref nShowHand);
                                break;
                            case FocusType.A_B:
                                // N_N -> A_B
                                FocusOn(nCollider, ref nShowHand);
                                FocusOn(nRigidBody, ref nShowHand);
                                break;
                            case FocusType.N_A:
                                // N_N -> N_A
                                FocusOn(nRigidBody, ref nShowHand);
                                break;
                        }
                        break;
                    case FocusType.A_N:
                        switch (targetType)
                        {
                            case FocusType.A_N:
                                // A_N -> A_N
                                if (CurrentCollider != nCollider)
                                {
                                    UnfocusOn(CurrentCollider);
                                    FocusOn(nCollider, ref nShowHand);
                                }
                                break;
                            case FocusType.A_A:
                                // A_N -> A_A
                                if (CurrentCollider != nCollider)
                                {
                                    UnfocusOn(CurrentCollider);
                                    FocusOn(nCollider, ref nShowHand);
                                }
                                break;
                            case FocusType.A_B:
                                // A_N -> A_B
                                if(CurrentCollider == nCollider)
                                {
                                    // Same A, new B
                                    FocusOn(nRigidBody, ref nShowHand);

                                } else
                                {
                                    // new A, new B
                                    UnfocusOn(CurrentCollider);
                                    FocusOn(nCollider, ref nShowHand);
                                    FocusOn(nRigidBody, ref nShowHand);
                                }
                                break;
                            case FocusType.N_A:
                                // A_N -> N_A
                                if (CurrentCollider != nRigidBody)
                                {
                                    // new A
                                    UnfocusOn(CurrentCollider);
                                    FocusOn(nRigidBody, ref nShowHand);
                                }
                                break;
                        }
                        break;
                    case FocusType.A_A:
                        switch (targetType)
                        {
                            case FocusType.A_N:
                                // A_A -> A_N
                                if(CurrentCollider != nCollider)
                                {
                                    // new A
                                    UnfocusOn(CurrentCollider);
                                    FocusOn(nCollider, ref nShowHand);
                                }
                                break;
                            case FocusType.A_A:
                                // A_A -> A_A
                                if(CurrentCollider != nCollider)
                                {
                                    // new A
                                    UnfocusOn(CurrentCollider);
                                    FocusOn(nCollider, ref nShowHand);
                                }
                                break;
                            case FocusType.A_B:
                                // A_A -> A_B
                                if (CurrentCollider != nCollider)
                                {
                                    UnfocusOn(CurrentCollider);
                                    FocusOn(nCollider, ref nShowHand);
                                }
                                FocusOn(nRigidBody, ref nShowHand);
                                break;
                            case FocusType.N_A:
                                // A_A -> N_A
                                if (CurrentRigidBody != nRigidBody)
                                {
                                    // new A
                                    UnfocusOn(CurrentRigidBody);
                                    FocusOn(nRigidBody, ref nShowHand);
                                }
                                break;
                        }
                        break;
                    case FocusType.A_B:
                        switch (targetType)
                        {
                            case FocusType.A_N:
                                // A_B -> A_N
                                if(CurrentCollider != nCollider)
                                {
                                    if (CurrentRigidBody != nCollider)
                                    {
                                        // new A
                                        UnfocusOn(CurrentCollider);
                                        UnfocusOn(CurrentRigidBody);
                                        FocusOn(nCollider, ref nShowHand);
                                    }
                                    else
                                    {
                                        // cur B == next A
                                        UnfocusOn(CurrentCollider);
                                    }
                                }
                                break;
                            case FocusType.A_A:
                                // A_B -> A_A
                                if (CurrentCollider != nCollider)
                                {
                                    if (CurrentRigidBody != nCollider)
                                    {
                                        // new A
                                        UnfocusOn(CurrentCollider);
                                        UnfocusOn(CurrentRigidBody);
                                        FocusOn(nCollider, ref nShowHand);
                                    }
                                    else
                                    {
                                        // cur B == next A
                                        UnfocusOn(CurrentCollider);
                                    }
                                }
                                break;
                            case FocusType.A_B:
                                // A_B -> A_B
                                if (CurrentCollider != nCollider && CurrentRigidBody != nCollider)
                                {
                                    // new A
                                    UnfocusOn(CurrentCollider);
                                    FocusOn(nCollider, ref nShowHand);
                                }
                                if (CurrentCollider != nRigidBody && CurrentRigidBody != nRigidBody)
                                {
                                    // new B
                                    UnfocusOn(CurrentRigidBody);
                                    FocusOn(nRigidBody, ref nShowHand);
                                }
                                break;
                            case FocusType.N_A:
                                // A_B -> N_A
                                
                                if(CurrentCollider != nRigidBody)
                                {
                                    UnfocusOn(CurrentCollider);
                                }
                                if (CurrentRigidBody != nRigidBody)
                                {
                                    UnfocusOn(CurrentRigidBody);
                                    if (CurrentCollider != nRigidBody)
                                    {
                                        // new A
                                        FocusOn(nRigidBody, ref nShowHand);
                                    }
                                }
                                break;
                        }
                        break;
                    case FocusType.N_A:
                        switch (targetType)
                        {
                            case FocusType.A_N:
                                // N_A -> A_N
                                if (CurrentRigidBody != nCollider)
                                {
                                    // new A
                                    UnfocusOn(CurrentRigidBody);
                                    FocusOn(nCollider, ref nShowHand);
                                }
                                break;
                            case FocusType.A_A:
                                // N_A -> A_A
                                if (CurrentRigidBody != nCollider)
                                {
                                    // new A
                                    UnfocusOn(CurrentRigidBody);
                                    FocusOn(nCollider, ref nShowHand);
                                }
                                break;
                            case FocusType.A_B:
                                // N_A -> A_B

                                if (CurrentRigidBody != nCollider && CurrentRigidBody != nRigidBody)
                                {
                                    // old A
                                    UnfocusOn(CurrentRigidBody);
                                }
                                if (CurrentRigidBody != nCollider)
                                {
                                    // new A
                                    FocusOn(nCollider, ref nShowHand);

                                }
                                if (CurrentRigidBody != nRigidBody)
                                {
                                    // new B
                                    FocusOn(nRigidBody, ref nShowHand);
                                }
                                break;
                            case FocusType.N_A:
                                // N_A -> N_A
                                if(CurrentRigidBody != nRigidBody)
                                {
                                    // new A
                                    UnfocusOn(CurrentRigidBody);
                                    FocusOn(nRigidBody, ref nShowHand);

                                }
                                break;
                        }
                        break;
                }
                CurrentCollider = nCollider;
                CurrentRigidBody = nRigidBody;
                CurrentFocusType = targetType;
                if (nShowHand)
                {
                    ShowHand(position);
                } else
                {
                    MoveHand(position);
                }

            } else
            {
                TransitToN_N();
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
        void MoveHand(Vector3 position)
        {
            if (Hand != null)
            {
                Hand.transform.position = position;
            }
        }
        public void Unfocus()
        {
            if (CurrentCollider == null && CurrentRigidBody == null) return;

            CurrentCollider = UnfocusOn(CurrentCollider);
            CurrentRigidBody = UnfocusOn(CurrentRigidBody);
            HideHand();
        }


        bool CanFocusOn(GameObject gameObject, out bool sendToRigidBody)
        {
            if (gameObject == null)
            {
                sendToRigidBody = false;
                return false;
            }
            bool canFocus = false;
            sendToRigidBody = false;
            foreach (var f in gameObject.GetComponents<ReactOnFocus>())
            {
                if (f.CanFocus(this, FocusPosition))
                {
                    canFocus = true;
                }
                if(f.SendFocusToRigidBodyObject)
                    sendToRigidBody = true;
            }
            return canFocus;
        }
        // return the GameObject to focus on
        GameObject FocusOn(GameObject gameObject, ref bool showHand)
        {
            if (gameObject == null)
            {
                Debug.LogError($"[{Time.frameCount}] FocuserController '{name}' cannot focus on null gameObject", gameObject); 
                return gameObject;
            }
            foreach (var f in gameObject.GetComponents<ReactOnFocus>())
            {
                if (DebugLog)
                    Debug.Log($"[{Time.frameCount}] FocuserController '{name}' Focus on '{gameObject.GetNameOrNull()}'", gameObject);
                f.Focus(this, FocusPosition);
                if (f.ShowHand)
                    showHand = true;
            }
            return gameObject;
        }

        GameObject UnfocusOn(GameObject gameObject)
        {
            if(gameObject == null) return null;
            foreach(var f in gameObject.GetComponents<ReactOnFocus>())
            {
                if (DebugLog)
                    Debug.Log($"[{Time.frameCount}] FocuserController '{name}' Unfocus on '{gameObject.GetNameOrNull()}'", gameObject);
                f.Unfocus(this, FocusPosition);
            }
            return null;
        }
    }
}