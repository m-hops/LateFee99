using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Nie.Editor
{
    public class ReactionStateMachineVE : VisualElement
    {
        //public Nie.ReactionStateMachine StateMachine;
        public List<StateGroupVE> StateGroupVEs = new();


        VisualElement RootGroup;
        public SerializedObject SerializedObject;

        SerializedProperty PropGroups;
        public ReactionStateMachineVE(ReactionStateMachine stateMachine, SerializedObject serializedObject)
        {
            //StateMachine = stateMachine;

            SerializedObject = serializedObject;
            PropGroups = SerializedObject.FindProperty("Groups");
            Build();
        }
        
        public void OnUndo()
        {
            Debug.Log("OnUndo");
            Clear();
            Build();
        }
        void Build()
        {
            ReactionStateMachineEditor.StateMachineAsset.CloneTree(this);

            RootGroup = this.Query<VisualElement>("Groups").First();
            for(int i = 0; i != PropGroups.arraySize; i++)
                AddGroupToEnd(PropGroups.GetArrayElementAtIndex(i));


            VisualElement btAddGroup = this.Query<VisualElement>("btAddGroup").First();
            btAddGroup.RegisterCallback<ClickEvent>(x =>
            {
                Debug.Log("Add Group");
                PropGroups.InsertArrayElementAtIndex(PropGroups.arraySize);
                var newPropGroup = PropGroups.GetArrayElementAtIndex(PropGroups.arraySize - 1);
                // add to ui
                AddGroupToEnd(newPropGroup);
                //SerializedObject.Update();
                SerializedObject.ApplyModifiedProperties();
                
            });


        }
        
        void AddGroupToEnd(SerializedProperty prop)//, SerializedObject serializedObject)
        {
            // create new group element
            var newGroup = new StateGroupVE(prop);
            StateGroupVEs.Add(newGroup);
            
            // add to ui
            RootGroup.Add(newGroup);
        }
        public class StateGroupVE : VisualElement
        {
            //public Nie.ReactionStateMachine.StateGroup Group;

            public Foldout Foldout;
            public TextField GroupName;
            public VisualElement States;
            //public SerializedObject SerializedObject;
            SerializedProperty Property;
            SerializedProperty PropName;
            SerializedProperty PropNotes;
            SerializedProperty PropHasActiveState;
            SerializedProperty PropStates;
            public StateGroupVE(SerializedProperty prop)//, SerializedObject serializedObject)
            {
                Property = prop;
                PropName = Property.FindPropertyRelative("GroupName").FindPropertyRelative("Name");
                PropNotes = Property.FindPropertyRelative("Notes");
                PropHasActiveState = Property.FindPropertyRelative("HasActiveState");
                PropStates = Property.FindPropertyRelative("States");

                //Group = group;
                //SerializedObject = serializedObject;
                Build();
            }
            void Build()
            {
                Foldout = new Foldout();
                Add(Foldout);
                Foldout.text = PropName.stringValue;
                Foldout.value = true;
                var root = Foldout.contentContainer;


                ReactionStateMachineEditor.GroupAsset.CloneTree(root);
                GroupName = root.Query<TextField>("Name").First();
                GroupName.value = PropName.stringValue;

                States = root.Query<VisualElement>("States").First();
                
                GroupName.RegisterCallback<ChangeEvent<string>>(x => 
                {
                    Foldout.text = x.newValue; // GroupName.value;

                    PropName.stringValue = x.newValue;
                    PropName.serializedObject.ApplyModifiedProperties();
                    
                });

                VisualElement btAddGroup = this.Query<VisualElement>("btAddState").First();
                btAddGroup.RegisterCallback<ClickEvent>(x =>
                {
                    PropStates.InsertArrayElementAtIndex(PropStates.arraySize);
                    var newPropState = PropStates.GetArrayElementAtIndex(PropStates.arraySize - 1);
                    // add to ui
                    AddStateToEnd(newPropState);
                    Property.serializedObject.ApplyModifiedProperties();

                });
            }
            void AddStateToEnd(SerializedProperty prop)//, SerializedObject serializedObject)
            {
                // create new group element
                var newState = new StateVE(prop);
                // add to ui
                States.Add(newState);
            }
            public void Open()
            {

            }
        }

        public class StateVE : VisualElement
        {
            //public Nie.ReactionStateMachine.StateGroup Group;


            //public SerializedObject SerializedObject;
            SerializedProperty Property;
            SerializedProperty PropName;
            SerializedProperty PropNotes;
            SerializedProperty PropIsActiveState;

            Foldout Foldout;
            TextField Name;
            Foldout Conditions;
            Foldout OnBegin;
            Foldout OnUpdate;
            Foldout OnEnd;
            Label ConditionsCount;
            Label OnBeginCount;
            Label OnUpdateCount;
            Label OnEndCount;

            public StateVE(SerializedProperty prop)//, SerializedObject serializedObject)
            {
                Property = prop;
                PropName = Property.FindPropertyRelative("StateName").FindPropertyRelative("Name");
                PropNotes = Property.FindPropertyRelative("Notes");
                PropIsActiveState = Property.FindPropertyRelative("IsActiveState");

                //Group = group;
                //SerializedObject = serializedObject;
                Build();
            }
            void Build()
            {

                Foldout = new Foldout();
                Add(Foldout);
                Foldout.text = PropName.stringValue;
                Foldout.value = true;
                var root = Foldout.contentContainer;


                ReactionStateMachineEditor.StateAsset.CloneTree(root);
                Name = this.Query<TextField>("tfName").First();
                Conditions = this.Query<Foldout>("foConditions").First();
                OnBegin = this.Query<Foldout>("foOnBegin").First();
                OnUpdate = this.Query<Foldout>("foOnUpdate").First();
                OnEnd = this.Query<Foldout>("foOnEnd").First();

                ConditionsCount = this.Query<Label>("lbConditionsCount").First();
                OnBeginCount = this.Query<Label>("lbOnBeginCount").First();
                OnUpdateCount = this.Query<Label>("lbOnUpdateCount").First();
                OnEndCount = this.Query<Label>("lbOnEndCount").First();



                Name.value = PropName.stringValue;

                Name.RegisterCallback<ChangeEvent<string>>(x =>
                {
                    Foldout.text = x.newValue; // GroupName.value;

                    PropName.stringValue = x.newValue;
                    PropName.serializedObject.ApplyModifiedProperties();

                });

            }
            public void Open()
            {

            }
        }
    }
    [CustomEditor(typeof(ReactionStateMachine))]
    public class ReactionStateMachineEditor : UnityEditor.Editor
    {
        public static VisualTreeAsset StateMachineAsset;
        public static VisualTreeAsset GroupAsset;
        public static VisualTreeAsset StateAsset;

        Nie.ReactionStateMachine StateMachine;
        ReactionStateMachineVE Root;

        public void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndo;
            StateMachine = (Nie.ReactionStateMachine)target;
            StateMachineAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/StateMachine.uxml");
            GroupAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/StateGroup.uxml");
            StateAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/State.uxml");

            //StyleSheet stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Star System Editor/StarSystemEditor.uss");
            //rootElement.styleSheets.Add(stylesheet);
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
            Root = null;
        }
        void OnUndo()
        {
            serializedObject.Update();
            Debug.Log("OnUndo");
            Root.OnUndo();
        }
        //private void HandleUndo()
        //{
        //    serializedObject.Update();

        //    // TODO recreate the whole ui here
        //    //var parentElement = uiRoot.parent;
        //    //var index = parentElement.IndexOf(uiRoot);
        //    //parentElement.RemoveAt(index);

        //    //uiRoot = CreateInspectorGUI();
        //    //parentElement.Insert(index, uiRoot);
        //}
        public override VisualElement CreateInspectorGUI()
        {

            Root = new ReactionStateMachineVE(StateMachine, serializedObject);
            return Root;

            //VisualElement btAddGroup = Root.Query<VisualElement>("btAddGroup").First();
            //btAddGroup.RegisterCallback<ClickEvent>(x =>
            //{
            //    Debug.Log("Add Group");
            //    AddGroupToEnd(new ReactionStateMachine.StateGroup());
            //    EditorUtility.SetDirty(StateMachine);
                
            //});


            //VisualElement veGroups = Root.Query<VisualElement>("Groups").First();
            //return Root;
        }

        //public class StateMachineVisual : VisualElement
        //{
        //    ReactionStateMachine StateMachine;
        //    //ReactionStateMachine.State State;
        //    //public Action<VisualElement> OnChange;
        //    //VisualElement Root;
        //    public StateMachineVisual(ReactionStateMachine sm)
        //    {
        //        StateMachine = sm;
        //        //Root = new VisualElement();
        //        Update();
        //    }
        //    void Update()
        //    {
        //        if (StateMachine != null)
        //        {

        //            foreach (var group in StateMachine.Groups)
        //            {
        //                var groupBox = new UnityEngine.UIElements.GroupBox("Group");

        //                var groupName = new UnityEngine.UIElements.TextField("Group Name:");
        //                groupBox.Add(groupName);
        //                Add(groupBox);
        //                //groupName.RegisterValueChangedCallback()
        //            }

        //        }
        //        var buttonAddGroup = new UnityEngine.UIElements.Button(() =>
        //        {
        //            StateMachine.Groups.Add(new ReactionStateMachine.StateGroup());
        //            Clear();
        //            Update();
        //            //OnChange(Root);
        //        });
        //        Add(buttonAddGroup);
        //    }
        //}
        //SerializedProperty Groups;
        //void OnEnable()
        //{
        //    Groups = serializedObject.FindProperty("Groups");
        //}
        ////void OnNewGroup(VisualElement ve)
        ////{
        ////    Root.Remove(StateMachineVE);
        ////    Root.Add(StateMachineVE);
        ////}
        //VisualElement Root;
        //StateMachineVisual StateMachineVE;
        //public override VisualElement CreateInspectorGUI()
        //{
        //    ReactionStateMachine sm = target as ReactionStateMachine;
        //    //sm.Groups;
        //    //SerializedObject serializedObject = new SerializedObject(sm);

        //    VisualElement root = new VisualElement();
        //    StateMachineVE = new StateMachineVisual(sm);
        //    //StateMachineVE.OnChange += OnNewGroup;

        //    root.Add(StateMachineVE);
        //    Root = root;
        //    return root;


        //    //foreach (var group in sm.Groups)
        //    ////for (int i = 0; i != Groups.arraySize; ++i)
        //    //{

        //    //    //EditorGUILayout.BeginHorizontal();
        //    //    //GUILayout.Space(16);
        //    //    //EditorGUILayout.BeginVertical();
        //    //    foreach (var state in group.States)
        //    //    //for (int iState = 0; iState != propStates.arraySize; ++iState)
        //    //    {

        //    //        var listConditions = new UnityEngine.UIElements.ListView(state.Conditions);
        //    //        root.Add(listConditions);
        //    //        //EditorGUILayout.BeginHorizontal();
        //    //        //GUILayout.Space(16);
        //    //        //EditorGUILayout.BeginVertical();
        //    //        //var propConditions = state.FindPropertyRelative("Conditions");
        //    //        //foreach(var condition in state.Conditions)
        //    //        //for (int iCondition = 0; iCondition != propConditions.arraySize; ++iCondition)
        //    //        //{
        //    //        //    var condition = propConditions.GetArrayElementAtIndex(iCondition);
        //    //        //    EditorGUILayout.PropertyField(condition);
        //    //        //}
        //    //        //if (GUILayout.Button("Add Condition"))
        //    //        //{
        //    //        //    propConditions.InsertArrayElementAtIndex(propConditions.arraySize);
        //    //        //}
        //    //        //EditorGUILayout.EndVertical();
        //    //        //EditorGUILayout.EndHorizontal();
        //    //        //layout.PropertyField(group);
        //    //    }

        //    //    var buttonAddState = new UnityEngine.UIElements.Button(() =>
        //    //        {
        //    //            var s = new ReactionStateMachine.State();
        //    //            group.States.Add(s);
        //    //            //AssetDatabase.AddObjectToAsset(s, sm);
        //    //            EditorUtility.SetDirty(sm);
        //    //            AssetDatabase.SaveAssets();
        //    //            AssetDatabase.Refresh();
        //    //        });
        //    //    buttonAddState.Add(new UnityEngine.UIElements.Label("Add State"));
        //    //    root.Add(buttonAddState);
        //    //}
        //    //var buttonAddGroup = new UnityEngine.UIElements.Button(() =>
        //    //{
        //    //    sm.Groups.Add(new ReactionStateMachine.StateGroup());
        //    //    EditorUtility.SetDirty(sm);
        //    //});
        //    //buttonAddGroup.Add(new UnityEngine.UIElements.Label("Add Group"));
        //    //root.Add(buttonAddGroup);
        //    ////UnityEditor.UIElements.
        //    ////var o = new UnityEditor.UIElements.PropertyField(serializedObject.FindProperty("Groups"));
        //    ////root.Add(o);
        //    ////position.BindProperty(posProp);
        //    //return root;
        //}

        //public override void OnInspectorGUI()
        //{
            
        //    EditorGUILayout.BeginVertical();
        //    serializedObject.Update();
        //    //EditorGUILayout.PropertyField(Groups);
        //    serializedObject.ApplyModifiedProperties();
        //    for (int i = 0; i != Groups.arraySize; ++i)
        //    {
        //        var group = Groups.GetArrayElementAtIndex(i);

        //        EditorGUILayout.BeginHorizontal();
        //        GUILayout.Space(16);
        //        EditorGUILayout.BeginVertical();
        //        var propStates = group.FindPropertyRelative("States");
        //        for (int iState = 0; iState != propStates.arraySize; ++iState)
        //        {
        //            var state = propStates.GetArrayElementAtIndex(iState);

        //            EditorGUILayout.BeginHorizontal();
        //            GUILayout.Space(16);
        //            EditorGUILayout.BeginVertical();
        //            var propConditions = state.FindPropertyRelative("Conditions");
        //            for (int iCondition = 0; iCondition != propConditions.arraySize; ++iCondition)
        //            {
        //                var condition = propConditions.GetArrayElementAtIndex(iCondition);
        //                EditorGUILayout.PropertyField(condition);
        //            }
        //            if (GUILayout.Button("Add Condition"))
        //            {
        //                propConditions.InsertArrayElementAtIndex(propConditions.arraySize);
        //            }
        //            EditorGUILayout.EndVertical();
        //            EditorGUILayout.EndHorizontal();
        //            //layout.PropertyField(group);
        //        }
        //        if (GUILayout.Button("Add State"))
        //        {
        //            propStates.InsertArrayElementAtIndex(propStates.arraySize);
        //        }
        //        EditorGUILayout.EndVertical();
        //        EditorGUILayout.EndHorizontal();

        //        //EditorGUILayout.PropertyField(group);
        //    }
        //    if (GUILayout.Button("Add Group"))
        //    {
        //        Groups.InsertArrayElementAtIndex(Groups.arraySize);
        //    }
        //    serializedObject.ApplyModifiedProperties();
        //    EditorGUILayout.EndVertical();
        //}

    }

    //[CustomPropertyDrawer(typeof(ReactionStateMachine))]
    //public class ReactionStateMachinePropertyDrawer : PropertyDrawer
    //{
    //    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //    {
    //        // StateGroup
    //        float h = EditorGUIUtility.singleLineHeight;


    //        var propGroup = property.FindPropertyRelative("Groups");
    //        //propGroup.isExpanded = layout.Foldout(propGroup, new GUIContent("Groups"))
    //        //layout.Label(Groups);
    //        for (int i = 0; i != propGroup.arraySize; ++i)
    //        {
    //            var group = propGroup.GetArrayElementAtIndex(i);
    //            h += EditorGUI.GetPropertyHeight(group);
    //        }
    //        h += EditorGUIUtility.singleLineHeight;
    //        //// Groups
    //        //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Groups"));

    //        //// States
    //        //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("States"));

    //        return h;
    //    }

    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //    {
    //        EditorGUI.BeginProperty(position, label, property);

    //        var layout = RectLayout.Vertical(position);

    //        var propGroup = property.FindPropertyRelative("Groups");
    //        //propGroup.isExpanded = layout.Foldout(propGroup, new GUIContent("Groups"))
    //        //layout.Label(Groups);
    //        for (int i = 0; i != propGroup.arraySize; ++i)
    //        {
    //            var group = propGroup.GetArrayElementAtIndex(i);
    //            layout.PropertyField(group);
    //        }
    //        if (layout.Button("Add Group"))
    //        {
    //            propGroup.InsertArrayElementAtIndex(propGroup.arraySize);
    //        }
    //        // Groups
    //        //layout.PropertyField();

    //        // GroupName
    //        //layout.PropertyField(property.FindPropertyRelative("GroupName"));

    //        //var propStates = property.FindPropertyRelative("States");

    //        // States
    //        //layout.PropertyField(property.FindPropertyRelative("States"), new GUIContent("SomeStates", Assets.IconCondition));
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