using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Nie.Editor
{

    [CustomPropertyDrawer(typeof(GameObjectReference))]
    public class GameObjectReferenceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            var veRoot = new VisualElement();
            veRoot.style.flexDirection = FlexDirection.Row;

            var lbType = new Label(property.displayName);
            lbType.FixSizeByCharLines(property.displayName.Length, 1);
            veRoot.Add(lbType);

            var propType = property.FindPropertyRelative("Type");
            var type = (GameObjectReference.TypeEnum)propType.enumValueIndex;
            var efType = new EnumField(property.name, type);
            efType.label = "";
            efType.FixSizeByCharLines(16, 1);
            //efType.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            veRoot.Add(efType);

            var pfThisGameObject = new PropertyField();
            var pfObjectType = new PropertyField();
            var pfString = new PropertyField();
            pfThisGameObject.label = "";
            pfObjectType.label = "";
            pfString.label = "";
            pfThisGameObject.FixSizeByCharLines(16, 1);
            pfObjectType.FixSizeByCharLines(16, 1);
            pfString.FixSizeByCharLines(16, 1);

            efType.RegisterCallback<ChangeEvent<Enum>>(x =>
            {
                propType.enumValueIndex = (int)(GameObjectReference.TypeEnum)x.newValue;
                propType.serializedObject.ApplyModifiedProperties();
                var oldType = (GameObjectReference.TypeEnum)x.previousValue;
                switch (oldType)
                {
                    case GameObjectReference.TypeEnum.Object:
                        veRoot.Remove(pfThisGameObject);
                        break;
                    case GameObjectReference.TypeEnum.FirstOfType:
                        veRoot.Remove(pfObjectType);
                        break;
                    case GameObjectReference.TypeEnum.FirstWithTag:
                        veRoot.Remove(pfString);
                        break;
                    case GameObjectReference.TypeEnum.FirstWithName:
                        veRoot.Remove(pfString);
                        break;

                }
                var type = (GameObjectReference.TypeEnum)x.newValue;
                switch (type)
                {
                    case GameObjectReference.TypeEnum.Self:
                        break;
                    case GameObjectReference.TypeEnum.Object:
                        pfThisGameObject.BindProperty(property.FindPropertyRelative("ThisGameObject"));
                        veRoot.Add(pfThisGameObject);
                        break;
                    case GameObjectReference.TypeEnum.TriggerObject:
                        break;
                    case GameObjectReference.TypeEnum.TriggerObjectOnBegin:
                        break;
                    case GameObjectReference.TypeEnum.FirstOfType:
                        pfObjectType.BindProperty(property.FindPropertyRelative("ObjectType"));
                        veRoot.Add(pfObjectType);
                        break;
                    case GameObjectReference.TypeEnum.FirstWithTag:
                        pfString.BindProperty(property.FindPropertyRelative("String"));
                        veRoot.Add(pfString);
                        break;
                    case GameObjectReference.TypeEnum.FirstWithName:
                        pfString.BindProperty(property.FindPropertyRelative("String"));
                        veRoot.Add(pfString);
                        break;
                }
            });


            switch (type)
            {
                case GameObjectReference.TypeEnum.Self:
                    break;
                case GameObjectReference.TypeEnum.Object:
                    pfThisGameObject.BindProperty(property.FindPropertyRelative("ThisGameObject"));
                    veRoot.Add(pfThisGameObject);
                    break;
                case GameObjectReference.TypeEnum.TriggerObject:
                    break;
                case GameObjectReference.TypeEnum.TriggerObjectOnBegin:
                    break;
                case GameObjectReference.TypeEnum.FirstOfType:
                    pfObjectType.BindProperty(property.FindPropertyRelative("ObjectType"));
                    veRoot.Add(pfObjectType);
                    break;
                case GameObjectReference.TypeEnum.FirstWithTag:
                    pfString.BindProperty(property.FindPropertyRelative("String"));
                    veRoot.Add(pfString);
                    break;
                case GameObjectReference.TypeEnum.FirstWithName:
                    pfString.BindProperty(property.FindPropertyRelative("String"));
                    veRoot.Add(pfString);
                    break;
            }
            return veRoot;
        }

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
                case GameObjectReference.TypeEnum.TriggerObjectOnBegin:
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