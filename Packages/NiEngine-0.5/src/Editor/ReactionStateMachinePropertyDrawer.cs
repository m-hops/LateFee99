using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nie.Editor
{

    [CustomPropertyDrawer(typeof(ReactionStateMachine))]
    public class ReactionStateMachinePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // StateGroup
            float h = EditorGUIUtility.singleLineHeight;

            // States
            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("States"));

            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var layout = RectLayout.Vertical(position);

            // StateGroup
            layout.PropertyField(property.FindPropertyRelative("StateGroup"));

            //var propStates = property.FindPropertyRelative("States");

            // States
            layout.PropertyField(property.FindPropertyRelative("States"), new GUIContent("SomeStates", Assets.IconCondition));
            //propStates.isExpanded = layout.Foldout(propStates.isExpanded);
            //if (propStates.isExpanded)
            //{
            //    layout = layout.SubHorizontal();
            //    layout.AcquireWidth(16);
            //    layout = layout.SubVertical();
            //    //layout.PropertyField(property.FindPropertyRelative("MustBeInAnimatorState"), new GUIContent("Must Be In Animator State"));
            //    //layout.PropertyField(property.FindPropertyRelative("MustBeInReactionState"), new GUIContent("Must Be In Reaction State"));
            //    layout.PropertyField(property.FindPropertyRelative("States:"), new GUIContent("States:", Assets.IconReactionState));

            //}
            EditorGUI.EndProperty();
        }
    }
}