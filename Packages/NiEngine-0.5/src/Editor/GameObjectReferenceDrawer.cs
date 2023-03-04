using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nie.Editor
{

    [CustomPropertyDrawer(typeof(GameObjectReference))]
    public class GameObjectReferenceDrawer : PropertyDrawer
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
            var value = (GameObjectReference.TypeEnum)propType.enumValueIndex;
            var type = layout.EnumPopup(RectLayout.WidthOf(value.ToString()) + 42, value);
            propType.enumValueIndex = (int)type;
            switch (type)
            {
                case GameObjectReference.TypeEnum.Self:
                    break;
                case GameObjectReference.TypeEnum.Object:
                    layout.PropertyField(property.FindPropertyRelative("ThisGameObject"));
                    break;
                case GameObjectReference.TypeEnum.TriggerObject:
                    break;
                case GameObjectReference.TypeEnum.PreviousTriggerObject:
                    break;
                case GameObjectReference.TypeEnum.FirstOfType:
                    layout.PropertyField(property.FindPropertyRelative("ObjectType"));
                    break;
                case GameObjectReference.TypeEnum.FirstWithTag:
                    layout.PropertyField(property.FindPropertyRelative("String"));
                    break;
                case GameObjectReference.TypeEnum.FirstWithName:
                    layout.PropertyField(property.FindPropertyRelative("String"));
                    break;
            }

            EditorGUI.EndProperty();
        }
    }
}