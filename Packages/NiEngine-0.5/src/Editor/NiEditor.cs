using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Nie
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    public static class EditorMenu
    {

        private const string DrawStatesGizmosName = "Tools/NiEngine/Draw States/Gizmos";
        private const string DrawStatesLabelName = "Tools/NiEngine/Draw States/Label";
        public static bool DrawStatesGizmos;
        public static bool DrawStatesLabel;

        static EditorMenu()
        {
            DrawStatesGizmos = UnityEditor.EditorPrefs.GetBool(DrawStatesGizmosName, true);
            DrawStatesLabel = UnityEditor.EditorPrefs.GetBool(DrawStatesLabelName, false);
            UnityEditor.EditorApplication.delayCall += () => SetDrawStates(DrawStatesGizmos, DrawStatesLabel);
        }


        [UnityEditor.MenuItem("Tools/NiEngine/Draw States/Off")]
        private static void SetOff() => SetDrawStates(false, false);

        [UnityEditor.MenuItem(DrawStatesGizmosName)]
        private static void SetGizmo() => SetDrawStates(!DrawStatesGizmos, DrawStatesLabel);

        [UnityEditor.MenuItem(DrawStatesLabelName)]
        private static void SetLabel() => SetDrawStates(DrawStatesGizmos, !DrawStatesLabel);
        public static void SetDrawStates(bool gizmos, bool label)
        {
            DrawStatesGizmos = gizmos;
            DrawStatesLabel = label;
            UnityEditor.Menu.SetChecked(DrawStatesGizmosName, DrawStatesGizmos);
            UnityEditor.Menu.SetChecked(DrawStatesLabelName, DrawStatesLabel);
            UnityEditor.EditorPrefs.SetBool(DrawStatesGizmosName, DrawStatesGizmos);
            UnityEditor.EditorPrefs.SetBool(DrawStatesLabelName, DrawStatesLabel);


        }

        public static GameObject DebugLabelAsset = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/label.prefab", typeof(GameObject));
    }

#endif
}

namespace Nie.Editor
{
    public class Assets
    {
        public static Texture2D IconReactionState = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/IconReactionState@16.png", typeof(Texture2D));
        public static Texture2D IconReactionReference = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/IconReactionReference.png", typeof(Texture2D));
        public static Texture2D IconCondition = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/IconCondition.png", typeof(Texture2D));
        public static Texture2D IconAction = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/IconReactionReference.png", typeof(Texture2D));
    }

    [CustomPropertyDrawer(typeof(DerivedClassPicker))]
    public class DerivedClassPickerDrawer : PropertyDrawer
    {
        // TODO can this be removed?
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = 0;

            h += EditorGUIUtility.singleLineHeight;
            if (property.managedReferenceValue != null && property.isExpanded)
                h += EditorGUI.GetPropertyHeight(property);
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool showPrefixLabel=true;
            if(attribute is DerivedClassPicker derivedClassPicker)
            {
                showPrefixLabel = derivedClassPicker.ShowPrefixLabel;
            }

            // Get the property type, handle cases where the field is a list
            Type baseType = fieldInfo.FieldType;
            if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                baseType = fieldInfo.FieldType.GenericTypeArguments[0];
            
            Rect dropdownRect = position;
            if (showPrefixLabel)
            {
                EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));
                dropdownRect.x += EditorGUIUtility.labelWidth + 2;
                dropdownRect.width -= EditorGUIUtility.labelWidth + 2;
            }

            dropdownRect.height = EditorGUIUtility.singleLineHeight;
            RectLayout.DerivedClassPicker(dropdownRect, baseType, property);
            
            if (property.managedReferenceValue != null)
                EditorGUI.PropertyField(position, property, GUIContent.none, true);
        }
    }

    [CustomPropertyDrawer(typeof(SerializableType<>), true)]
    public class InspectableTypeDrawer : PropertyDrawer
    {

        static Dictionary<Type, Dictionary<Type, string>> _DerivedClass;
        static string NameOfDerivedClass(Type baseType, Type derivedType)
        {
            if (derivedType is null) return "<Null>";
            var types = DerivedClassOf(baseType);
            if (types.TryGetValue(derivedType, out var name))
                return name;
            return derivedType.FullName;
        }

        static Dictionary<Type, string> DerivedClassOf(Type baseType)
        {

            if (_DerivedClass == null)
            {
                _DerivedClass = new();
            }
            if (!_DerivedClass.TryGetValue(baseType, out var derivedTypes))
            {
                derivedTypes = new();
                var ll = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                    x => x.GetTypes().Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t)));
                foreach (var type in System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))))
                {
                    string name = type.FullName;
                    foreach (var attribute in type.GetCustomAttributes(inherit: false))
                        if (attribute is ClassPickerName classPickerName)
                        {
                            name = classPickerName.Name;
                            break;
                        }
                    derivedTypes.Add(type, name);
                }
                _DerivedClass.Add(baseType, derivedTypes);

            }
            return derivedTypes;
        }

        //GUIContent[] _optionLabels;
        //int _selectedIndex;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var baseTypeProperty = property.FindPropertyRelative("baseTypeName");
            var baseType = Type.GetType(baseTypeProperty.stringValue);


            var propTypeName = property.FindPropertyRelative("TypeName");
            string typeName = propTypeName.stringValue;
            //string typeName = NameOfClass(property.managedReferenceValue?.GetType());
            var layout = RectLayout.Horizontal(position);
            var str = EditorGUI.TextField(layout.AcquireWidth(-16), propTypeName.stringValue);
            Debug.Log(str);
            if (EditorGUI.DropdownButton(layout.AcquireWidth(16), label, FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                // null
                menu.AddItem(new GUIContent("Null"), propTypeName.stringValue == null, () =>
                {
                    propTypeName.stringValue = null;
                });

                // inherited types
                foreach (var (t, name) in DerivedClassOf(baseType))
                {
                    if (string.IsNullOrEmpty(str) || name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        menu.AddItem(new GUIContent(name), typeName == t.FullName, () =>
                        {
                            propTypeName.stringValue = t.FullName;
                        });
                    }
                }
                menu.ShowAsContext();
            } else
            {

                propTypeName.stringValue = str;
            }
            

            EditorGUI.EndProperty();

        }

    }





    //[CustomPropertyDrawer(typeof(StateName))]
    //public class StateNamePropertyDrawer : PropertyDrawer
    //{
    //    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //    {
    //        // StateGroup
    //        float h = EditorGUIUtility.singleLineHeight;

    //        // States
    //        h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("States"));

    //        return h;
    //    }

    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //    {
    //        EditorGUI.BeginProperty(position, label, property);

    //        var layout = RectLayout.Vertical(position);

    //        // StateGroup
    //        layout.PropertyField(property.FindPropertyRelative("StateGroup"));

    //        //var propStates = property.FindPropertyRelative("States");

    //        // States
    //        layout.PropertyField(property.FindPropertyRelative("States"), new GUIContent("SomeStates", Assets.IconCondition));
    //        //propStates.isExpanded = layout.Foldout(propStates.isExpanded);
    //        //if (propStates.isExpanded)
    //        //{
    //        //    layout = layout.SubHorizontal();
    //        //    layout.AcquireWidth(16);
    //        //    layout = layout.SubVertical();
    //        //    //layout.PropertyField(property.FindPropertyRelative("MustBeInAnimatorState"), new GUIContent("Must Be In Animator State"));
    //        //    //layout.PropertyField(property.FindPropertyRelative("MustBeInReactionState"), new GUIContent("Must Be In Reaction State"));
    //        //    layout.PropertyField(property.FindPropertyRelative("States:"), new GUIContent("States:", Assets.IconReactionState));

    //        //}
    //        EditorGUI.EndProperty();
    //    }
    //}


}