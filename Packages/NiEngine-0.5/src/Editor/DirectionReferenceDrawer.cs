using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nie.Editor
{

    [CustomPropertyDrawer(typeof(DirectionReference))]
    public class DirectionReferenceDrawer : PropertyDrawer
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
            //layout.FreeRect.xMin -= 16;
            var propType = property.FindPropertyRelative("Type");
            var value = (DirectionReference.TypeEnum)propType.enumValueIndex;
            var type = layout.EnumPopup(RectLayout.WidthOf(value.ToString()) + 42, value);
            propType.enumValueIndex = (int)type;
            switch (type)
            {
                case DirectionReference.TypeEnum.Axis:
                    layout.PropertyField(property.FindPropertyRelative("Axis"), 32);
                    layout.PropertyField(property.FindPropertyRelative("Object"));
                    break;
                case DirectionReference.TypeEnum.Vector:
                    layout.PropertyField(property.FindPropertyRelative("PositionFrom"));
                    break;
                case DirectionReference.TypeEnum.Toward:
                    layout = layout.SubVertical();
                    layout.PropertyField(property.FindPropertyRelative("PositionFrom"));
                    layout.PropertyField(property.FindPropertyRelative("PositionTo"));
                    break;
            }

            EditorGUI.EndProperty();
        }
    }

}