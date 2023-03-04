using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nie.Editor
{

    [CustomPropertyDrawer(typeof(PositionReference))]
    public class PositionReferenceDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var layout = RectLayout.Horizontal(position);
            if (label != GUIContent.none)
                layout.PrefixLabel(label);
            layout.FreeRect.xMin -= 16;
            var propType = property.FindPropertyRelative("Type");
            var value = (PositionReference.TypeEnum)propType.enumValueIndex;
            var type = layout.EnumPopup(RectLayout.WidthOf(value.ToString()) + 42, value);
            propType.enumValueIndex = (int)type;
            switch (type)
            {
                case PositionReference.TypeEnum.Self:
                    break;
                case PositionReference.TypeEnum.AtPosition:
                    layout.PropertyField(property.FindPropertyRelative("AtPosition"));
                    break;
                case PositionReference.TypeEnum.AtGameObject:
                    layout.PropertyField(property.FindPropertyRelative("AtTransform"));
                    break;
                case PositionReference.TypeEnum.AtTriggerPosition:
                    break;
            }

            EditorGUI.EndProperty();
        }
    }

}