//using System;
//using System.Reflection;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEditor;

//namespace Nie.Editor
//{

//    [CustomPropertyDrawer(typeof(ReactionStateMachine.State))]
//    public class ReactionStateMachineStatePropertyDrawer : PropertyDrawer
//    {
//        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//        {
//            // Name
//            float h = EditorGUIUtility.singleLineHeight + 5;

//            if (property.isExpanded)
//            {

//                // Name & Notes foldout
//                h += EditorGUIUtility.singleLineHeight;

//                var propNotes = property.FindPropertyRelative("Notes");
//                if (propNotes.isExpanded)
//                {
//                    // Name
//                    h += EditorGUIUtility.singleLineHeight;
//                    // Notes field
//                    h += EditorGUIUtility.singleLineHeight * 4;
//                }

//                //// Conditions
//                //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("TestCondition"));

//                //// Action
//                //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("TestAction"));

//                h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("IsActiveState"));
                

//                // Name & Notes foldout
//                h += EditorGUIUtility.singleLineHeight;
//                //var propIsActiveState = property.FindPropertyRelative("IsActiveState");
//                //if (propIsActiveState.isExpanded)
//                //{
//                //    h += EditorGUI.GetPropertyHeight(propIsActiveState);
//                //    //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("LastBeginEvent"), includeChildren: true);
//                //    //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("LastEndEvent"), includeChildren: true);
//                //
//                //}
//                // Conditions 
//                h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Conditions"));

//                // OnBeginActions
//                h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OnBeginActions"));

//                h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OnUpdate"));

//                // OnEndActions
//                h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OnEndActions"));

//                h += 7;
//            }
//            return h;
//        }
//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//        {
//            EditorGUI.BeginProperty(position, label, property);
//            var layout = RectLayout.Vertical(position);

//            var propStateName = property.FindPropertyRelative("StateName");
//            var propName = propStateName.FindPropertyRelative("Name");
//            property.isExpanded = layout.Foldout(property.isExpanded, new GUIContent(propName.stringValue, Assets.IconReactionState));



//            var propIsActiveState = property.FindPropertyRelative("IsActiveState");
//            Color stateColor = propIsActiveState.boolValue ? Color.green : Color.black;// new Color(0.6f, 0.6f, 0.6f);
            
//            Rect left = position;

//            //left.yMin += EditorGUIUtility.singleLineHeight;
//            //left.xMin -= 26;
//            left.width = 3;
//            EditorGUI.DrawRect(left, stateColor);
//            if (property.isExpanded)
//            {

//                // Name

//                // Notes dropdown
//                var propNotes = property.FindPropertyRelative("Notes");

//                propNotes.isExpanded = layout.Foldout(propNotes.isExpanded, new("Name & Notes"));

//                // Notes
//                if (propNotes.isExpanded)
//                {
//                    layout.PropertyField(propName, new GUIContent("Name"));
//                    propNotes.stringValue = EditorGUI.TextField(layout.AcquireHeight(EditorGUIUtility.singleLineHeight * 4), propNotes.stringValue);
//                }
//                if (string.IsNullOrEmpty(propName.stringValue))
//                {
//                    propNotes.isExpanded = true;
//                }

//                layout.PropertyField(propIsActiveState, new GUIContent("Active State"));

//                ////layout.Label("Run-Time Values");
//                //propIsActiveState.isExpanded = layout.Foldout(propIsActiveState.isExpanded, new("Run-Time Values"));
//                //if (propIsActiveState.isExpanded)
//                //{
//                //    //layout.PropertyField(property.FindPropertyRelative("LastBeginEvent"), new GUIContent("Last Begin Event:"), includeChildren:true);
//                //    //layout.PropertyField(property.FindPropertyRelative("LastEndEvent"), new GUIContent("Last End Event:"), includeChildren: true);
//                //
//                //}
//                //// Conditions 
//                //layout.PropertyField(property.FindPropertyRelative("TestCondition"));

//                //// Actions 
//                //layout.PropertyField(property.FindPropertyRelative("TestAction"));
//                left.x += 3;
//                left.width = 9;
//                Rect usedRect;
//                // Conditions 
//                usedRect = layout.PropertyField(property.FindPropertyRelative("Conditions"), new GUIContent("Conditions:", Assets.IconCondition));
//                left.yMin = usedRect.yMin;
//                left.yMax = usedRect.yMax;
//                EditorGUI.DrawRect(left, new Color(0.5f,0.5f,0));

//                // OnBeginActions 
//                usedRect = layout.PropertyField(property.FindPropertyRelative("OnBeginActions"), new GUIContent("On Begin:", Assets.IconAction));
//                left.yMin = usedRect.yMin;
//                left.yMax = usedRect.yMax;
//                EditorGUI.DrawRect(left, new Color(0, 0.5f, 0));

//                // OnBeginActions 
//                usedRect = layout.PropertyField(property.FindPropertyRelative("OnUpdate"), new GUIContent("On Update:", Assets.IconAction));
//                left.yMin = usedRect.yMin;
//                left.yMax = usedRect.yMax;
//                EditorGUI.DrawRect(left, new Color(0, 0.8f, 0.8f));

//                // OnEndActions
//                usedRect = layout.PropertyField(property.FindPropertyRelative("OnEndActions"), new GUIContent("On End:", Assets.IconAction));
//                left.yMin = usedRect.yMin;
//                left.yMax = usedRect.yMax;
//                EditorGUI.DrawRect(left, new Color(0, 0.25f, 0.75f));

//                Rect bottom = left;
//                bottom.yMin = position.yMax - 3;
//                bottom.height = 3;
//                bottom.width = position.width + 24;
//                EditorGUI.DrawRect(bottom, stateColor);
//                //GUI.Box(layout.Acquire(2), )
//            }
//            EditorGUI.EndProperty();
//        }

//    }


//}