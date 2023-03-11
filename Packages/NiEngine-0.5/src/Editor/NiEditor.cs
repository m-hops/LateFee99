using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

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

    // TODO: need to update when undo/redo/past
    public class EditorBase : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var veRoot = new VisualElement();
            var itor = serializedObject.GetIterator();
            bool first = true;
            while (itor.NextVisible(first))
            {
                first = false;
                var field = new PropertyField(itor.Copy(), itor.displayName);
                field.BindProperty(itor.Copy());
                veRoot.Add(field);
            }
            return veRoot;
        }
    }

    [CustomEditor(typeof(Grabbable))] public class GrabbableEditor : EditorBase { }
    [CustomEditor(typeof(ReactOnCollisionPair))] public class ReactOnCollisionPairEditor : EditorBase { }
    [CustomEditor(typeof(ReactOnInputKey))] public class ReactOnInputKeyEditor : EditorBase { }
    
    [CustomPropertyDrawer(typeof(DerivedClassPicker))]
    public class DerivedClassPickerDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Toggle Toggle;
            DropdownField DropdownField;
            VisualElement VeContent;
            //PropertyField PropertyField;


            var veRoot = new VisualElement();
            ReactionStateMachineEditor.ClassPickerAsset.CloneTree(veRoot);
            Toggle = veRoot.Query<Toggle>().First();
            Toggle.value = property.isExpanded;
            DropdownField = veRoot.Query<DropdownField>().First();
            VeContent = veRoot.Query<VisualElement>("veContent").First();

            VeContent.style.display = property.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            Toggle.RegisterCallback<ChangeEvent<bool>>(x =>
            {
                VeContent.style.display = x.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                property.isExpanded = x.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            var itor = property.Copy();
            int d = itor.depth;
            bool child = true;
            while (itor.Next(child))
            {
                if (itor.depth <= d)
                    break;
                child = false;

                var field = new PropertyField();
                field.BindProperty(itor.Copy());
                VeContent.Add(field);
            }

            //PropertyField = new PropertyField();
            //PropertyField.BindProperty(property);
            //PropertyField.RegisterCallback<ChangeEvent<string>>(evt =>
            //{
            //    Toggle t = PropertyField.Query<Toggle>().First();
            //    if (t != null)
            //    {
            //        t.style.display = DisplayStyle.None;
            //    }
            //});
            //VeContent.Add(PropertyField);


            Type baseType = fieldInfo.FieldType;
            if (attribute is DerivedClassPicker derivedClassPicker)
            {
                baseType = derivedClassPicker.BaseType;
                //showPrefixLabel = derivedClassPicker.ShowPrefixLabel;
            }
            var choicesType = new List<System.Type>();
            var choices = new List<string>();
            var currentIndex = -1;
            var type = property.managedReferenceValue?.GetType();
            foreach (var (t, name) in DerivedClassOf(baseType))
            {
                if(type == t)
                {
                    currentIndex = choices.Count;
                }
                choices.Add(name);
                choicesType.Add(t);
            }
            DropdownField.choices = choices;
            DropdownField.index = currentIndex;
            //string typeName = NameOfDerivedClass(baseType, type);
            //DropdownField.RegisterCallback<ChangeEvent<int>>(x =>
            //{

            //    Debug.Log(x.newValue);
            //    var t = choicesType[x.newValue];
            //    property.isExpanded = true;
            //    property.managedReferenceValue = t.GetConstructor(Type.EmptyTypes).Invoke(null);
            //    property.serializedObject.ApplyModifiedProperties();
            //});
            DropdownField.RegisterValueChangedCallback(x =>
            {
                int index = choices.FindIndex(y => y == x.newValue);
                Debug.Log(index);
                var t = choicesType[index];
                property.isExpanded = true;
                
                property.managedReferenceValue = t.GetConstructor(Type.EmptyTypes).Invoke(null);
                property.serializedObject.ApplyModifiedProperties();

                VeContent.Clear();

                var itor = property.Copy();
                int d = itor.depth;
                bool child = true;
                while (itor.Next(child))
                {
                    if (itor.depth <= d)
                        break;
                    child = false;

                    var field = new PropertyField();
                    field.BindProperty(itor.Copy());
                    VeContent.Add(field);
                }
                //PropertyField = new PropertyField();
                //PropertyField.BindProperty(property);
                //VeContent.Add(PropertyField);
                //property.serializedObject.Update();
                //PropertyField.Unbind();
                //PropertyField.Bind(property.serializedObject);
                //PropertyField.BindProperty(property);

            });
            return veRoot;
        }

        //// TODO can this be removed?
        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    float h = 0;

        //    h += EditorGUIUtility.singleLineHeight;
        //    if (property.managedReferenceValue != null && property.isExpanded)
        //        h += EditorGUI.GetPropertyHeight(property);
        //    return h;
        //}

        //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //{
        //    bool showPrefixLabel=true;
        //    Type baseType = fieldInfo.FieldType;
        //    if (attribute is DerivedClassPicker derivedClassPicker)
        //    {
        //        baseType = derivedClassPicker.BaseType;
        //        showPrefixLabel = derivedClassPicker.ShowPrefixLabel;
        //    }
            
        //    Rect dropdownRect = position;
        //    if (showPrefixLabel)
        //    {
        //        EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));
        //        dropdownRect.x += EditorGUIUtility.labelWidth + 2;
        //        dropdownRect.width -= EditorGUIUtility.labelWidth + 2;
        //    }

        //    dropdownRect.height = EditorGUIUtility.singleLineHeight;
        //    RectLayout.DerivedClassPicker(dropdownRect, baseType, property);
            
        //    if (property.managedReferenceValue != null)
        //        EditorGUI.PropertyField(position, property, GUIContent.none, true);
        //}


        static Dictionary<Type, Dictionary<Type, string>> _DerivedClass;

        static void OnBeforeAssemblyReload()
        {
            _DerivedClass = null;
        }

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
                AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
                _DerivedClass = new();
            }
            if (!_DerivedClass.TryGetValue(baseType, out var derivedTypes))
            {
                derivedTypes = new();
                //ClassPickerName

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
        public static void DerivedClassPicker(Rect position, Type baseType, SerializedProperty property)
        {
            var type = property.managedReferenceValue?.GetType();
            string typeName = NameOfDerivedClass(baseType, type);
            //string typeName = property.managedReferenceValue?.GetType().Name ?? "Not set";
            if (EditorGUI.DropdownButton(position, new(typeName), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                // null
                menu.AddItem(new GUIContent("Null"), property.managedReferenceValue == null, () =>
                {
                    property.isExpanded = false;
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                });

                //string typeName = NameOfDerivedClass(baseType, type);
                // inherited types
                foreach (var (t, name) in DerivedClassOf(baseType))
                {
                    menu.AddItem(new GUIContent(name), typeName == t.Name, () =>
                    {
                        property.isExpanded = true;
                        property.managedReferenceValue = t.GetConstructor(Type.EmptyTypes).Invoke(null); ;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            }
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

    [CustomPropertyDrawer(typeof(ReactionStateMachine.StateGroup))]
    public class ReactionStateMachineStateGroupPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Name
            float h = EditorGUIUtility.singleLineHeight + 5;

            if (property.isExpanded)
            {

                // Name & Notes foldout
                h += EditorGUIUtility.singleLineHeight;

                var propNotes = property.FindPropertyRelative("Notes");
                if (propNotes.isExpanded)
                {
                    // Name
                    h += EditorGUIUtility.singleLineHeight;
                    // Notes field
                    h += EditorGUIUtility.singleLineHeight * 4;
                }

                //// Conditions
                //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("TestCondition"));

                //// Action
                //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("TestAction"));

                var propIsActiveState = property.FindPropertyRelative("HasActiveState");

                // Name & Notes foldout
                h += EditorGUIUtility.singleLineHeight;

                if (propIsActiveState.isExpanded)
                {
                    h += EditorGUI.GetPropertyHeight(propIsActiveState);

                }

                var propStates = property.FindPropertyRelative("States");
                for (int i = 0; i != propStates.arraySize; ++i)
                {
                    var elem = propStates.GetArrayElementAtIndex(i);
                    h += EditorGUI.GetPropertyHeight(elem);
                }
                h += EditorGUIUtility.singleLineHeight;

                // States 
                //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("States"));

                h += 7;
            }
            return h;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var layout = RectLayout.Vertical(position);

            var propGroupName = property.FindPropertyRelative("GroupName");
            var propName = propGroupName.FindPropertyRelative("Name");
            property.isExpanded = layout.Foldout(property.isExpanded, new GUIContent(propName.stringValue, Assets.IconReactionState));



            var propHasActiveState = property.FindPropertyRelative("HasActiveState");
            Color stateColor = propHasActiveState.boolValue ? Color.green : Color.black;// new Color(0.6f, 0.6f, 0.6f);

            Rect left = position;
            //left.yMin += EditorGUIUtility.singleLineHeight;
            left.xMin -= 26;
            left.width = 3;
            left.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.DrawRect(left, stateColor);

            if (property.isExpanded)
            {

                // Name

                // Notes dropdown
                var propNotes = property.FindPropertyRelative("Notes");

                propNotes.isExpanded = layout.Foldout(propNotes.isExpanded, new("Group Name & Notes"));

                // Notes
                if (propNotes.isExpanded)
                {
                    layout.PropertyField(propName, new GUIContent("Name"));
                    propNotes.stringValue = EditorGUI.TextField(layout.AcquireHeight(EditorGUIUtility.singleLineHeight * 4), propNotes.stringValue);
                }
                //layout.Label("Run-Time Values");
                propHasActiveState.isExpanded = layout.Foldout(propHasActiveState.isExpanded, new("Run-Time Values"));
                if (propHasActiveState.isExpanded)
                {
                    layout.PropertyField(propHasActiveState, new GUIContent("Has Active State"));
                    //layout.PropertyField(property.FindPropertyRelative("LastBeginEvent"), new GUIContent("Last Begin Event:"), includeChildren: true);
                    //layout.PropertyField(property.FindPropertyRelative("LastEndEvent"), new GUIContent("Last End Event:"), includeChildren: true);

                }

                var propStates = property.FindPropertyRelative("States");
                for (int i = 0; i != propStates.arraySize; ++i)
                {
                    var group = propStates.GetArrayElementAtIndex(i);
                    layout.PropertyField(group);
                }
                if (layout.Button("Add State"))
                {
                    propStates.InsertArrayElementAtIndex(0);
                }
                //layout.PropertyField(property.FindPropertyRelative("States"), new GUIContent("States:", Assets.IconReactionState));
            }
            EditorGUI.EndProperty();
        }

    }

}