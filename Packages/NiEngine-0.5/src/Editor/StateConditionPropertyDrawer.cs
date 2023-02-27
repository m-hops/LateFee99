using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nie.Editor
{
    [CustomPropertyDrawer(typeof(StateCondition))]
    public class StateConditionPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propObject = property.FindPropertyRelative("ReactionState");
            if (propObject.objectReferenceValue is not null)
                return RectLayout.MinHeight * 2 + 4;
            return RectLayout.MinHeight + 4;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var layout = RectLayout.Horizontal(position);
            layout.PrefixLabel(label);

            
            layout = layout.SubVertical();
            var layoutLine0 = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);
            var propType = property.FindPropertyRelative("Type");
            var type = layoutLine0.EnumPopup(RectLayout.WidthOf("ReactionState") + 18, (StateCondition.StateType)propType.enumValueIndex);
            propType.enumValueIndex = (int)type;
            switch (type)
            {
                case StateCondition.StateType.ReactionState:
                    var propObject = property.FindPropertyRelative("ReactionState");
                    if(propObject.objectReferenceValue == null
                        && property.serializedObject.targetObject is MonoBehaviour mb
                        && mb.gameObject.TryGetComponent<ReactionState>(out var reactionState))
                    {
                        propObject.objectReferenceValue = reactionState;
                    }
                    var obj = propObject.objectReferenceValue as ReactionState;
                    layoutLine0.PropertyField(propObject);
                    if (obj != null)
                    {
                        layout = layout.SubHorizontal();
                        layout.Label("State:");
                        var propStateName = property.FindPropertyRelative("State");
                        layout.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));
                        string reactionName = propStateName.stringValue;

                        if (ReactionReference.HasReaction(obj.gameObject, reactionName))
                        {
                            layout.Label("Found");
                        }
                        else
                        {
                            layout.Label("Missing");
                        }
                    }
                    break;
                case StateCondition.StateType.Animator:
                    var propAnimator = property.FindPropertyRelative("Animator");
                    if (propAnimator.objectReferenceValue == null
                        && property.serializedObject.targetObject is MonoBehaviour mb2
                        && mb2.TryGetComponent<Animator>(out var animator2))
                    {
                        propAnimator.objectReferenceValue = animator2;
                    }
                    var animator = propAnimator.objectReferenceValue as Animator;
                    layoutLine0.PropertyField(propAnimator);
                    if (animator != null)
                    {
                        layout = layout.SubHorizontal();
                        layout.Label("State:");
                        var propStateName = property.FindPropertyRelative("State");
                        layout.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));

                        var stateHash = Animator.StringToHash(propStateName.stringValue);
                        if (animator.HasState(0, stateHash))
                        {
                            property.FindPropertyRelative("StateHash").intValue = stateHash;
                            layout.Label("Found");
                        }
                        else
                        {
                            layout.Label("Missing");
                        }
                    }
                    break;
            }


            EditorGUI.EndProperty();
        }
    }
}