using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nie.Editor
{
    [CustomPropertyDrawer(typeof(ReactionReference))]
    public class ReactionReferencePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = 8;
            var propType = property.FindPropertyRelative("ReactionType");
            h += EditorGUI.GetPropertyHeight(propType);
            var type = (ReactionReference.Type)propType.enumValueIndex;
            propType.enumValueIndex = (int)type;
            switch (type)
            {
                case ReactionReference.Type.Reaction:
                    // part of the Type line; h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("TargetObjectReference"));
                    h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("ReactionName"));
                    break;
                case ReactionReference.Type.Event:
                    h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Event"));
                    break;
            }
            return h;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var layout = RectLayout.Horizontal(position);
            //layout.PrefixLabel(label);
            bool fixIndent = false;
            if (EditorGUI.indentLevel > 0)
            {
                EditorGUI.indentLevel--;
                fixIndent = true;
            }

            layout = layout.SubVertical();
            var layoutLine0 = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);
            var propType = property.FindPropertyRelative("ReactionType");
            var type = layoutLine0.EnumPopup(RectLayout.WidthOf("ReactionOO") + 18, (ReactionReference.Type)propType.enumValueIndex);
            propType.enumValueIndex = (int)type;
            
            switch (type)
            {
                case ReactionReference.Type.Reaction:
                    layoutLine0.Label("on");
                    var propTargetReference = property.FindPropertyRelative("TargetObjectReference");

                    layoutLine0.PropertyField(propTargetReference);
                    
                    //var t = (GameObjectReference.TypeEnum)propTargetReference_Type.intValue;
                    //bool isVirtualCall = (t != GameObjectReference.TypeEnum.Object && t != GameObjectReference.TypeEnum.Self);
                    var gameObjectReference = GameObjectReference.FromProperty(propTargetReference);//(GameObjectReference)propTargetReference.value;
                    bool isVirtualCall = (gameObjectReference.Type != GameObjectReference.TypeEnum.Object && gameObjectReference.Type != GameObjectReference.TypeEnum.Self);

                    GameObject self = null;
                    if (property.serializedObject.targetObject is MonoBehaviour mb2)
                        self = mb2.gameObject;
                    gameObjectReference.TryGetTargetGameObject(new EventParameters()
                    {
                        Self = self,
                        TriggerObject = null,
                        PreviousTriggerObject = null,
                        TriggerPosition = Vector3.zero,
                        PreviousTriggerPosition = Vector3.zero,
                    }, out var targetObject);



                    //// if old values
                    //var propTarget = property.FindPropertyRelative("ObjectTargetType");
                    //if (propTarget != null)
                    //{
                    //    var propTargetReference_Type = propTargetReference.FindPropertyRelative("Type");
                    //    var propTargetReference_ThisGameObject = propTargetReference.FindPropertyRelative("ThisGameObject");
                    //    var propTargetReference_ObjectType = propTargetReference.FindPropertyRelative("ObjectType");

                    //    var target = (ReactionReference.TargetType)propTarget.enumValueIndex;
                    //    //target = layoutLine1.EnumPopup(RectLayout.WidthOf(target.ToString()) + 18, target);
                    //    //propTarget.enumValueIndex = (int)target;
                    //    //GameObject targetObject = null;
                    //    //bool isVirtualCall = false;
                    //    switch (target)
                    //    {
                    //        case ReactionReference.TargetType.Self:

                    //            if (property.serializedObject.targetObject is MonoBehaviour mb)
                    //                targetObject = mb.gameObject;


                    //            propTargetReference_Type.intValue = (int)GameObjectReference.TypeEnum.Self;
                    //            break;
                    //        case ReactionReference.TargetType.Other:

                    //            var propObject = property.FindPropertyRelative("TargetObject");
                    //            targetObject = propObject.objectReferenceValue as GameObject;
                    //            //layoutLine1.PropertyField(propObject);


                    //            propTargetReference_Type.intValue = (int)GameObjectReference.TypeEnum.Object;
                    //            propTargetReference_ThisGameObject.objectReferenceValue = propObject.objectReferenceValue;
                    //            break;
                    //        case ReactionReference.TargetType.TriggerObject:
                    //            isVirtualCall = true;
                    //            propTargetReference_Type.intValue = (int)GameObjectReference.TypeEnum.TriggerObject;
                    //            break;
                    //        case ReactionReference.TargetType.PreviousTriggerObject:

                    //            isVirtualCall = true;
                    //            propTargetReference_Type.intValue = (int)GameObjectReference.TypeEnum.PreviousTriggerObject;
                    //            break;
                    //    }


                    //}
                    if (targetObject != null || isVirtualCall)
                    {
                        var layoutLine1 = layout.SubHorizontal(EditorGUIUtility.singleLineHeight);
                        layoutLine1.Label("Name:");
                        var propStateName = property.FindPropertyRelative("ReactionName");
                        layoutLine1.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));
                        string reactionName = propStateName.stringValue;

                        if (isVirtualCall)
                        {
                            layoutLine1.Label("");
                        }
                        else if (ReactionReference.HasReaction(targetObject, reactionName))
                        {
                            layoutLine1.Label("Found");
                        }
                        else
                        {
                            layoutLine1.Label("Missing");
                        }

                        //// if has old values
                        //if (propTarget != null)
                        //{
                        //    var propTargetReference_String = propTargetReference.FindPropertyRelative("String");
                        //    propTargetReference_String.stringValue = propStateName.stringValue;
                        //}

                    }

                    break;
                case ReactionReference.Type.Event:
                    layout.PropertyField(property.FindPropertyRelative("Event"));
                    break;
            }

            if (fixIndent)
            {
                EditorGUI.indentLevel++;
            }

            EditorGUI.EndProperty();
        }
    }
}