//using System.Linq;
//using UnityEngine;
//using UnityEditor;

//namespace Nie.Editor
//{
//    [CustomPropertyDrawer(typeof(StateCondition))]
//    public class StateConditionPropertyDrawer : PropertyDrawer
//    {
//        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//        {
//            float h = 0;
//            var propType = property.FindPropertyRelative("Type");
//            var type = (StateCondition.StateType)propType.enumValueIndex;
//            switch (type)
//            {
//                case StateCondition.StateType.ReactionState:
//                    h += RectLayout.MinHeight * 2;
//                    break;
//                case StateCondition.StateType.Animator:
//                    var propAnimator = property.FindPropertyRelative("Animator");
//                    h += EditorGUI.GetPropertyHeight(propAnimator);
//                    break;
//            }
//            return h;
//            //        var propObject = property.FindPropertyRelative("ReactionState");
//            //if (propObject.objectReferenceValue is not null)
//            //    return RectLayout.MinHeight * 2 + 4;
//            //return RectLayout.MinHeight + 4;
//        }
//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//        {
//            EditorGUI.BeginProperty(position, label, property);
//            var layout = RectLayout.Horizontal(position);
//            //layout.PrefixLabel(label);

//            {
//                //temp

//                var propReactionState = property.FindPropertyRelative("ReactionState");
//                if (propReactionState.objectReferenceValue is MonoBehaviour mbOther)
//                {
//                    var propObject = property.FindPropertyRelative("Other");
//                    propObject.objectReferenceValue = mbOther.gameObject;
//                    propReactionState.objectReferenceValue = null;

//                    var propTarget = property.FindPropertyRelative("ObjectTargetType");
//                    propTarget.intValue = (int)StateCondition.TargetType.Other;
//                }
//            }

//            layout = layout.SubVertical();
//            var layoutLine0 = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);
//            var propType = property.FindPropertyRelative("Type");
//            var type = layoutLine0.EnumPopup(RectLayout.WidthOf("ReactionState") + 18, (StateCondition.StateType)propType.enumValueIndex);
//            propType.enumValueIndex = (int)type;
//            switch (type)
//            {
//                case StateCondition.StateType.ReactionState:
//                    var propTarget = property.FindPropertyRelative("ObjectTargetType");
//                    var target = (StateCondition.TargetType)propTarget.enumValueIndex;
//                    target = layoutLine0.EnumPopup(RectLayout.WidthOf(target.ToString()) + 18, target);
//                    propTarget.enumValueIndex = (int)target;
//                    GameObject targetObject = null;
//                    bool isVirtualCall = false;

//                    switch (target)
//                    {
//                        case StateCondition.TargetType.Self:
//                            if (property.serializedObject.targetObject is MonoBehaviour mbSelf)
//                                targetObject = mbSelf.gameObject;
//                            break;
//                        case StateCondition.TargetType.Other:
//                            var propObject = property.FindPropertyRelative("Other");
//                            if(propObject.objectReferenceValue is GameObject goOther)
//                                targetObject = goOther;
//                            layoutLine0.PropertyField(propObject);
//                            break;
//                        case StateCondition.TargetType.TriggerObject:
//                        case StateCondition.TargetType.PreviousTriggerObject:
//                            isVirtualCall = true;
//                            break;
//                    }


//                    if (targetObject != null || isVirtualCall)
//                    {
//                        layout = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);
//                        layout.Label("Name:");
//                        var propStateName = property.FindPropertyRelative("State");
//                        layout.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));
//                        string reactionName = propStateName.stringValue;

//                        if (isVirtualCall)
//                        {
//                            layout.Label("");
//                        }
//                        else if (ReactionReference.HasReaction(targetObject, reactionName))
//                        {
//                            layout.Label("Found");
//                        }
//                        else
//                        {
//                            layout.Label("Missing");
//                        }
//                    }


//                    //var propObject = property.FindPropertyRelative("ReactionState");
//                    //if(propObject.objectReferenceValue == null
//                    //    && property.serializedObject.targetObject is MonoBehaviour mb
//                    //    && mb.gameObject.TryGetComponent<ReactionState>(out var reactionState))
//                    //{
//                    //    propObject.objectReferenceValue = reactionState;
//                    //}
//                    //var obj = propObject.objectReferenceValue as ReactionState;
//                    //layoutLine0.PropertyField(propObject);
//                    //if (obj != null)
//                    //{
//                    //    layout = layout.SubHorizontal();
//                    //    layout.Label("State:");
//                    //    var propStateName = property.FindPropertyRelative("State");
//                    //    layout.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));
//                    //    string reactionName = propStateName.stringValue;

//                    //    if (ReactionReference.HasReaction(obj.gameObject, reactionName))
//                    //    {
//                    //        layout.Label("Found");
//                    //    }
//                    //    else
//                    //    {
//                    //        layout.Label("Missing");
//                    //    }
//                    //}
//                    break;
//                case StateCondition.StateType.Animator:
//                    var propAnimator = property.FindPropertyRelative("Animator");
//                    if (propAnimator.objectReferenceValue == null
//                        && property.serializedObject.targetObject is MonoBehaviour mb2
//                        && mb2.TryGetComponent<Animator>(out var animator2))
//                    {
//                        propAnimator.objectReferenceValue = animator2;
//                    }
//                    var animator = propAnimator.objectReferenceValue as Animator;
//                    layoutLine0.PropertyField(propAnimator);
//                    if (animator != null)
//                    {
//                        layout = layout.SubHorizontal();
//                        layout.Label("State:");
//                        var propStateName = property.FindPropertyRelative("State");
//                        layout.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));

//                        var stateHash = Animator.StringToHash(propStateName.stringValue);
//                        if (animator.HasState(0, stateHash))
//                        {
//                            property.FindPropertyRelative("StateHash").intValue = stateHash;
//                            layout.Label("Found");
//                        }
//                        else
//                        {
//                            layout.Label("Missing");
//                        }
//                    }
//                    break;
//            }


//            EditorGUI.EndProperty();
//        }
//    }
//}