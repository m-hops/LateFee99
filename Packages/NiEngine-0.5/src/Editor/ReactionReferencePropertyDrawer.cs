using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nie.Editor
{
    [CustomPropertyDrawer(typeof(ReactionReference))]
    public class ReactionReferencePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = 8;
            var propType = property.FindPropertyRelative("ReactionType");
            h += EditorGUI.GetPropertyHeight(propType);
            var type = (ReactionReference.Type)propType.enumValueIndex;
            propType.enumValueIndex = (int)type;
            switch (type)
            {
                case ReactionReference.Type.Reaction:
                    //var propObject = property.FindPropertyRelative("TargetObject");
                    //var obj = propObject.objectReferenceValue as GameObject;
                    h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("ReactionName"));
                    //if (obj != null)
                    //{
                    //    h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("ReactionName"));
                    //}
                    break;
                case ReactionReference.Type.Event:
                    h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Event"));
                    break;
            }
            return h;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var layout = RectLayout.Horizontal(position);
            //layout.PrefixLabel(label);


            layout = layout.SubVertical();
            var layoutLine0 = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);
            var propType = property.FindPropertyRelative("ReactionType");
            var type = layoutLine0.EnumPopup(RectLayout.WidthOf("Reaction") + 18, (ReactionReference.Type)propType.enumValueIndex);
            propType.enumValueIndex = (int)type;
            switch (type)
            {
                case ReactionReference.Type.Reaction:
                    layoutLine0.Label("on");
                    var propTarget = property.FindPropertyRelative("ObjectTargetType");
                    var target = (ReactionReference.TargetType)propTarget.enumValueIndex;
                    target = layoutLine0.EnumPopup(RectLayout.WidthOf(target.ToString()) + 18, target);
                    propTarget.enumValueIndex = (int)target;
                    GameObject targetObject = null;
                    bool isVirtualCall = false;
                    switch (target)
                    {
                        case ReactionReference.TargetType.Self:
                            if(property.serializedObject.targetObject is MonoBehaviour mb)
                                targetObject = mb.gameObject;
                            break;
                        case ReactionReference.TargetType.Other:
                            var propObject = property.FindPropertyRelative("TargetObject");
                            //if (propObject.objectReferenceValue == null
                            //    && property.serializedObject.targetObject is MonoBehaviour mb
                            //    && mb.gameObject.TryGetComponent<ReactionState>(out var reactionState))
                            //{
                            //    propObject.objectReferenceValue = mb.gameObject;
                            //}
                            targetObject = propObject.objectReferenceValue as GameObject;
                            layoutLine0.PropertyField(propObject);
                            break;
                        case ReactionReference.TargetType.TriggeringObject:
                        case ReactionReference.TargetType.PreviousTriggeringObject:
                            isVirtualCall = true;
                            break;
                    }

                    
                    if (targetObject != null || isVirtualCall)
                    {
                        layout = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);
                        layout.Label("Name:");
                        var propStateName = property.FindPropertyRelative("ReactionName");
                        layout.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));
                        string reactionName = propStateName.stringValue;

                        if (isVirtualCall)
                        {
                            layout.Label("");
                        }
                        else if (ReactionReference.HasReaction(targetObject, reactionName))
                        {
                            layout.Label("Found");
                        }
                        else
                        {
                            layout.Label("Missing");
                        }
                    }
                    break;
                case ReactionReference.Type.Event:
                    layout.PropertyField(property.FindPropertyRelative("Event"));

                    //var propAnimator = property.FindPropertyRelative("Animator");
                    //if (propAnimator.objectReferenceValue == null
                    //    && property.serializedObject.targetObject is MonoBehaviour mb2
                    //    && mb2.TryGetComponent<Animator>(out var animator2))
                    //{
                    //    propAnimator.objectReferenceValue = animator2;
                    //}
                    //var animator = propAnimator.objectReferenceValue as Animator;
                    //layoutLine0.PropertyField(propAnimator);
                    //if (animator != null)
                    //{
                    //    layout = layout.SubHorizontal();
                    //    layout.Label("State:");
                    //    var propStateName = property.FindPropertyRelative("State");
                    //    layout.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));

                    //    var stateHash = Animator.StringToHash(propStateName.stringValue);
                    //    if (animator.HasState(0, stateHash))
                    //    {
                    //        property.FindPropertyRelative("StateHash").intValue = stateHash;
                    //        layout.Label("Found");
                    //    }
                    //    else
                    //    {
                    //        layout.Label("Missing");
                    //    }
                    //}
                    break;
            }


            EditorGUI.EndProperty();
        }
    }
}