using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nie.Editor
{
    [CustomPropertyDrawer(typeof(ReactionList))]
    public class ReactionListPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight + 4;
            if (property.isExpanded)
            {
                h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("ReactionOnTriggeringObject"));
                h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("ReactionReferences"));
                //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("SetReactionStates"));
                h += EditorGUIUtility.singleLineHeight + 4;
                
                var propEvent = property.FindPropertyRelative("Events");
                if(propEvent.isExpanded)
                    h += EditorGUI.GetPropertyHeight(propEvent);
            }
            return h;
        }
        public static Texture2D IconReactionReference = (Texture2D) AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/IconReactionReference.png", typeof(Texture2D));
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var layout = RectLayout.Vertical(position);
            property.isExpanded = layout.Foldout(property.isExpanded, new GUIContent(property.name, IconReactionReference));
            if (property.isExpanded)
            {
                layout = layout.SubHorizontal();
                layout.Acquire(16);
                layout = layout.SubVertical();
                layout.PropertyField(property.FindPropertyRelative("ReactionReferences"), new GUIContent("Trigger Reaction"));
                //layout.PropertyField(property.FindPropertyRelative("SetReactionStates"), new GUIContent("Set Reaction State"));

                var propEvent = property.FindPropertyRelative("Events");
                propEvent.isExpanded = layout.Foldout(propEvent.isExpanded, new GUIContent(propEvent.name));
                if (propEvent.isExpanded)
                    layout.PropertyField(propEvent);
                layout.PropertyField(property.FindPropertyRelative("ReactionOnTriggeringObject"), new GUIContent("Reaction On Triggering Object:"));
            }

            EditorGUI.EndProperty();
        }
    }
}