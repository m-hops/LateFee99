using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nie.Actions
{

    [Serializable, ClassPickerName("AttachTo")]
    public class AttachTo : StateAction
    {
        public GameObjectReference Attach;
        public GameObjectReference To;
        public bool MoveToParentOrigin = true;
        public bool DetachOnEnd;

        public GameObject AttachedObject;
        public Transform m_PreviousParent;
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            m_PreviousParent = null;
            AttachedObject = null;
            var a = Attach.GetTargetGameObject(parameters);
            if (a != null)
            {
                var t = To.GetTargetGameObject(parameters);
                if (t != null)
                {
                    AttachedObject = a;
                    m_PreviousParent = a.transform.parent;
#if UNITY_EDITOR

                    if (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(AttachedObject.scene))
                    {
                        Debug.LogWarning($"Could not attach object '{AttachedObject.name}' to '{t.name}' in prefab mode. You must do it manualy.", AttachedObject);
                        if (MoveToParentOrigin)
                        {
                            a.transform.position = t.transform.position;
                            a.transform.rotation = t.transform.rotation;
                        }
                    }
                    else
#endif
                    {
                        a.transform.parent = t.transform;
                        if (MoveToParentOrigin)
                        {
                            a.transform.localPosition = Vector3.zero;
                            a.transform.localRotation = Quaternion.identity;
                        }
                    }
                }
            }
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            if (DetachOnEnd && AttachedObject != null)
            {
                AttachedObject.transform.parent = m_PreviousParent;
            }
        }
    }
}