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
    [CustomPropertyDrawer(typeof(ConditionSet))]
    public class ConditionSetDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ListView2 listView = new ListView2();
            listView.SetText(property.displayName);
            listView.SetIcon(Assets.IconCondition);
            listView.SetColor(new Color(0.75f, 0.75f, 0));
            listView.style.marginBottom = 2;
            listView.BindProperty(property.FindPropertyRelative("Conditions"));
            return listView;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Conditions"));
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Conditions"), new GUIContent(property.name, Assets.IconCondition), true);
        }
    }

    [CustomPropertyDrawer(typeof(ActionSet))]
    public class ActionSetDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ListView2 listView = new ListView2();
            listView.SetText(property.displayName);
            listView.style.marginBottom = 2;
            listView.SetIcon(Assets.IconAction);
            if (property.name.Contains("OnBegin"))
            {
                listView.SetColor(new Color(0.0f, 0.75f, 0.75f));
            }
            else if (property.name.Contains("OnUpdate"))
            {
                listView.SetColor(new Color(0.2f, 0.2f, 0.75f));
            }
            else if (property.name.Contains("OnEnd"))
            {
                listView.SetColor(new Color(0.75f, 0, 0.75f));
            }
            listView.BindProperty(property.FindPropertyRelative("Actions"));
            return listView;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Actions"));
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Actions"), new GUIContent(property.displayName, Assets.IconAction), true);
        }
    }

    [CustomPropertyDrawer(typeof(StateActionSet))]
    public class StateActionSetDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ListView2 listView = new ListView2();
            listView.SetText(property.displayName);
            listView.style.marginBottom = 2;
            listView.SetIcon(Assets.IconAction);
            if (property.name.Contains("OnBegin"))
            {
                listView.SetColor(new Color(0.0f, 0.75f, 0.75f));
            }
            else if (property.name.Contains("OnUpdate"))
            {
                listView.SetColor(new Color(0.2f, 0.2f, 0.75f));
            }
            else if (property.name.Contains("OnEnd"))
            {
                listView.SetColor(new Color(0.75f, 0, 0.75f));
            }
            listView.BindProperty(property.FindPropertyRelative("Actions"));
            return listView;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Actions"));
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Actions"), new GUIContent(property.displayName, Assets.IconAction), true);
        }
    }

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
                AddGroupToEnd(PropGroups.GetArrayElementAtIndex(i), i);


            VisualElement btAddGroup = this.Query<VisualElement>("btAddGroup").First();
            btAddGroup.RegisterCallback<ClickEvent>(x =>
            {
                PropGroups.InsertArrayElementAtIndex(PropGroups.arraySize);
                var newPropGroup = PropGroups.GetArrayElementAtIndex(PropGroups.arraySize - 1);
                // add to ui
                AddGroupToEnd(newPropGroup, PropGroups.arraySize - 1);
                //SerializedObject.Update();
                SerializedObject.ApplyModifiedProperties();
                
            });


        }
        
        void AddGroupToEnd(SerializedProperty prop, int index)//, SerializedObject serializedObject)
        {
            // create new group element
            var newGroup = new StateGroupVE(prop);
            newGroup.BtDelete.RegisterCallback<ClickEvent>(x =>
            {
                PropGroups.DeleteArrayElementAtIndex(index);
                PropGroups.serializedObject.ApplyModifiedProperties();
                RootGroup.Remove(newGroup);

            });

            StateGroupVEs.Add(newGroup);
            
            // add to ui
            RootGroup.Add(newGroup);
        }
    }

    public class StateGroupVE : VisualElement
    {
        //public Nie.ReactionStateMachine.StateGroup Group;

        public Foldout Foldout;
        public TextField GroupName;
        public VisualElement States;
        //public ListView StatesList;
        public Button BtDelete;

        //public SerializedObject SerializedObject;
        public SerializedProperty Property;
        public SerializedProperty PropName;
        public SerializedProperty PropNotes;
        public SerializedProperty PropHasActiveState;
        public SerializedProperty PropStates;
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
            //var root = Foldout.contentContainer;
            var root = new VisualElement();
            Foldout.RegisterCallback<ChangeEvent<bool>>(x =>
            {
                root.style.display = x.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            this.Add(root);
            ReactionStateMachineEditor.GroupAsset.CloneTree(root);
            GroupName = root.Query<TextField>("Name").First();
            GroupName.value = PropName.stringValue;

            States = root.Query<VisualElement>("States").First();
            BtDelete = root.Query<Button>("btDelete").First();

            //StatesList.BindProperty(PropStates);
            //



            GroupName.RegisterCallback<ChangeEvent<string>>(x =>
            {
                Foldout.text = x.newValue; // GroupName.value;

                PropName.stringValue = x.newValue;
                PropName.serializedObject.ApplyModifiedProperties();

            });

            VisualElement btAddState = this.Query<VisualElement>("btAddState").First();
            btAddState.RegisterCallback<ClickEvent>(x =>
            {
                PropStates.InsertArrayElementAtIndex(PropStates.arraySize);
                var newPropState = PropStates.GetArrayElementAtIndex(PropStates.arraySize - 1);
                PropStates.serializedObject.ApplyModifiedProperties();
                RefreshAfter(PropStates.arraySize - 1);
            });
            RefreshAfter(0);
        }

        public void RefreshAfter(int index)
        {

            int i = index;
            for (; i != States.childCount; ++i)
            {
                var item = States[i] as StateVE;
                if (i < PropStates.arraySize)
                {
                    item.BindProperty(this, PropStates.GetArrayElementAtIndex(i), i);
                }
                else
                {
                    // delete the remaining of items
                    while (States.childCount > i)
                        States.RemoveAt(i);
                    return;
                }
            }
            for (; i < PropStates.arraySize; ++i)
            {
                var e = PropStates.GetArrayElementAtIndex(i);
                var item = new StateVE(this, e, i);
                States.Add(item);
            }
        }
    }

    public class StateVE : VisualElement
    {
        //public Nie.ReactionStateMachine.StateGroup Group;
        public StateGroupVE Parent;

        //public SerializedObject SerializedObject;
        SerializedProperty Property;
        SerializedProperty PropName;
        SerializedProperty PropDebug;
        SerializedProperty PropNotes;
        SerializedProperty PropIsActiveState;

        SerializedProperty PropNewConditions;
        SerializedProperty PropNewOnBegin;
        SerializedProperty PropNewOnUpdate;
        SerializedProperty PropNewOnEnd;

        Foldout Foldout;
        TextField Name;
        Toggle TgDebug;
        PropertyField PfLastBeginEvent;
        VisualElement VeContentParent;
        VisualElement VeContent;
        List<VisualElement> VeIsActive;
        //Foldout FoInternal;

        PropertyField PfNewConditions;
        PropertyField PfNewOnBegin;
        PropertyField PfNewOnUpdate;
        PropertyField PfNewOnEnd;
        public int Index;
        public StateVE()
        {
            Build();
        }
        public StateVE(StateGroupVE parent, SerializedProperty prop, int index)//, SerializedObject serializedObject)
        {
            Build();
            BindProperty(parent, prop, index);
        }
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }
        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }
        void Build()
        {
            // create state foldout
            var foldoutRoot = new VisualElement();
            ReactionStateMachineEditor.StateFoldoutAsset.CloneTree(foldoutRoot);
            Foldout = foldoutRoot.Query<Foldout>("Foldout").First();
            VeContent = foldoutRoot.Query<VisualElement>("veContent").First();
            VeContentParent = VeContent.parent;
            m_ContentVisible = true;

            Foldout.RegisterCallback<ChangeEvent<bool>>(x =>
            {
                SetContentVisible(x.newValue);
                Property.isExpanded = x.newValue;
                Property.serializedObject.ApplyModifiedProperties();
            });
            foldoutRoot.Query<Button>("btDelete").First().RegisterCallback<ClickEvent>(x =>
            {
                Parent.PropStates.DeleteArrayElementAtIndex(Index);
                Parent.PropStates.serializedObject.ApplyModifiedProperties();
                Parent.RefreshAfter(Index);
            });
            var veIcon = foldoutRoot.Query<VisualElement>("veIcon").First();
            veIcon.RegisterCallback<ClickEvent>(x =>
            {
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    if (Property.serializedObject.targetObject is ReactionStateMachine rsm)
                    {
                        if (rsm.TryGetGroup(Parent.PropName.stringValue, out var group)
                            && group.TryGetState(PropName.stringValue, out var state))
                        {
                            var parameters = EventParameters.Default;
                            if (state.IsActiveState)
                            {
                                Debug.Log($"Deactivating State '{state.StateName.Name}' on gameObject '{rsm.gameObject.name}' without trigger object.");
                                group.DeactivateAllState(parameters);
                            }
                            else
                            {
                                Debug.Log($"Activating State '{state.StateName.Name}' on gameObject '{rsm.gameObject.name}' without trigger object.");
                                group.DeactivateAllState(parameters);
                                group.SetActiveState(rsm, state, parameters);
                            }
                            //PropIsActiveState.serializedObject.Update();
                        }

                    }
                }
                else
                {
                    PropIsActiveState.boolValue = !PropIsActiveState.boolValue;
                    Parent.PropStates.serializedObject.ApplyModifiedProperties();
                    Debug.Log($"Set state to {PropIsActiveState.boolValue}");
                }
            });

            veIcon.RegisterCallback<DragEnterEvent>(x =>
            {
                if (DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences[0] is GameObject triggerObject)
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                else
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

            });
            //veIcon.RegisterCallback<DragUpdatedEvent>(x =>
            //{
            //    if (DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences[0] is GameObject triggerObject)
            //        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            //    else
            //        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

            //});
            veIcon.RegisterCallback<DragPerformEvent>(x =>
            {
                if (DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences[0] is GameObject triggerObject)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    if(Property.serializedObject.targetObject is ReactionStateMachine rsm)
                    {
                        if(rsm.TryGetGroup(Parent.PropName.stringValue, out var group)
                            && group.TryGetState(PropName.stringValue, out var state))
                        {
                            if (!UnityEditor.EditorApplication.isPlaying)
                            {
                                group.Handshake(rsm);
                                state.Handshake(rsm, group);
                            }
                            var parameters = EventParameters.Trigger(rsm.gameObject, rsm.gameObject, triggerObject);
                            if (state.IsActiveState)
                            {
                                Debug.Log($"Deactivating State '{state.StateName.Name}' on gameObject '{rsm.gameObject.name}' with trigger object '{triggerObject.name}'.");
                                group.DeactivateAllState(parameters);
                            }
                            else
                            {
                                Debug.Log($"Activating State '{state.StateName.Name}' on gameObject '{rsm.gameObject.name}' with trigger object '{triggerObject.name}'.");
                                group.DeactivateAllState(parameters);
                                group.SetActiveState(rsm, state, parameters);
                            }
                            if (!UnityEditor.EditorApplication.isPlaying)
                                Property.serializedObject.Update();
                        }
                        
                    }
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }

            });

            Add(foldoutRoot);

            // create state ui
            var veStateRoot = new VisualElement();
            ReactionStateMachineEditor.StateAsset.CloneTree(veStateRoot);

            Name = veStateRoot.Query<TextField>("tfName").First();
            Name.RegisterCallback<ChangeEvent<string>>(x =>
            {
                Foldout.text = x.newValue; // GroupName.value;
                PropName.stringValue = x.newValue;
                PropName.serializedObject.ApplyModifiedProperties();

            });

            TgDebug = veStateRoot.Query<Toggle>("tgDebug").First();
            TgDebug.RegisterCallback<ChangeEvent<bool>>(x =>
            {
                PropDebug.boolValue = x.newValue;
                PropDebug.serializedObject.ApplyModifiedProperties();

            });
            //FoInternal = veStateRoot.Query<Foldout>("foInternal").First();
            //var veInternalContent = FoInternal.contentContainer;
            //PfLastBeginEvent = new PropertyField();
            //veInternalContent.Add(PfLastBeginEvent);//LastBeginEvent

            var veStateContent = veStateRoot.Query<VisualElement>("veStateContent").First();
            VeContent.Add(veStateRoot);

            PfNewConditions = new PropertyField();
            PfNewOnBegin    = new PropertyField();
            PfNewOnUpdate   = new PropertyField();
            PfNewOnEnd = new PropertyField();

            veStateContent.Add(PfNewConditions);
            veStateContent.Add(PfNewOnBegin);
            veStateContent.Add(PfNewOnUpdate);
            veStateContent.Add(PfNewOnEnd);

            PfLastBeginEvent = new PropertyField();
            PfLastBeginEvent.style.marginLeft = 16;
            veStateContent.Add(PfLastBeginEvent);
            VeIsActive = foldoutRoot.Query<VisualElement>("veIsActive").ToList();


        }
        public void BindProperty(StateGroupVE parent, SerializedProperty property, int index)
        {
            if (property == Property) return;
            Parent = parent;
            Index = index;
            Property = property;
            PropName = Property.FindPropertyRelative("StateName").FindPropertyRelative("Name");
            PropNotes = Property.FindPropertyRelative("Notes");
            PropIsActiveState = Property.FindPropertyRelative("IsActiveState");

            PropNewConditions = property.FindPropertyRelative("NewConditions");
            PropNewOnBegin = property.FindPropertyRelative("NewOnBegin");
            PropNewOnUpdate = property.FindPropertyRelative("NewOnUpdate");
            PropNewOnEnd = property.FindPropertyRelative("NewOnEnd");

            PropDebug = property.FindPropertyRelative("DebugLog");
            TgDebug.value = PropDebug.boolValue;

            SetContentVisible(Property.isExpanded);
            PfLastBeginEvent.BindProperty(property.FindPropertyRelative("LastBeginEvent"));
            PfNewConditions.BindProperty(PropNewConditions);
            PfNewOnBegin.BindProperty(PropNewOnBegin);
            PfNewOnUpdate.BindProperty(PropNewOnUpdate);
            PfNewOnEnd.BindProperty(PropNewOnEnd);


            if (VeIsActive != null && VeIsActive.Count > 0)
            {
                var element = VeIsActive[0];
                element.Unbind();
                element.TrackPropertyValue(PropIsActiveState, x =>
                {
                    foreach (var v in VeIsActive)
                        v.style.backgroundColor = x.boolValue ? Color.green : Color.black;
                });
            }

            //VeIsActive.Bind(PropIsActiveState);
            Refresh();
        }
        bool m_ContentVisible;
        void SetContentVisible(bool visible)
        {
            if (visible && !m_ContentVisible)
                VeContentParent.Add(VeContent);
            else if(!visible && m_ContentVisible)
                VeContent.parent.Remove(VeContent);
            m_ContentVisible = visible;
        }
        public void Refresh()
        {

            Name.value = PropName.stringValue;
            Foldout.value = Property.isExpanded;
            Foldout.text = PropName.stringValue;
            foreach (var v in VeIsActive)
                v.style.backgroundColor = PropIsActiveState.boolValue ? Color.green : Color.black;
        }
    }

    [CustomEditor(typeof(ReactionStateMachine))]
    public class ReactionStateMachineEditor : UnityEditor.Editor
    {
        public static VisualTreeAsset _StateMachineAsset;
        public static VisualTreeAsset _GroupAsset;
        public static VisualTreeAsset _StateAsset;
        public static VisualTreeAsset _StateFoldoutAsset;
        public static VisualTreeAsset _ListAsset;
        public static VisualTreeAsset _ListItemAsset;
        public static VisualTreeAsset _ClassPickerAsset;

        public static VisualTreeAsset StateMachineAsset => _StateMachineAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/StateMachine.uxml");
        public static VisualTreeAsset GroupAsset => _GroupAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/StateGroup.uxml");
        public static VisualTreeAsset StateAsset => _StateAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/State.uxml");
        public static VisualTreeAsset StateFoldoutAsset => _StateFoldoutAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/StateFoldout.uxml");
        public static VisualTreeAsset ListAsset => _ListAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/List.uxml");
        public static VisualTreeAsset ListItemAsset => _ClassPickerAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/ListItem.uxml");
        public static VisualTreeAsset ClassPickerAsset => _ListItemAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/ClassPicker.uxml");
        Nie.ReactionStateMachine StateMachine;
        ReactionStateMachineVE Root;

        public void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndo;
            StateMachine = (Nie.ReactionStateMachine)target;
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
        public override VisualElement CreateInspectorGUI()
        {

            Root = new ReactionStateMachineVE(StateMachine, serializedObject);
            return Root;

        }


    }

}