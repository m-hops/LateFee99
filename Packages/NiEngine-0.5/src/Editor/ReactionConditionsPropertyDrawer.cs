using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nie.Editor
{
    [CustomPropertyDrawer(typeof(ReactionConditions))]
    public class ReactionConditionsPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("MustBeInAnimatorState"));
                //h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("MustBeInReactionState"));
                h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("States"));
            }
            return h;
        }
        public static Texture2D IconCondition = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/NiEngine/src/Editor/Assets/IconCondition.png", typeof(Texture2D));
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var layout = RectLayout.Vertical(position);
            property.isExpanded = layout.Foldout(property.isExpanded, new GUIContent(property.name, IconCondition));
            if (property.isExpanded)
            {
                layout = layout.SubHorizontal();
                layout.AcquireWidth(16);
                layout = layout.SubVertical();
                //layout.PropertyField(property.FindPropertyRelative("MustBeInAnimatorState"), new GUIContent("Must Be In Animator State"));
                //layout.PropertyField(property.FindPropertyRelative("MustBeInReactionState"), new GUIContent("Must Be In Reaction State"));
                layout.PropertyField(property.FindPropertyRelative("States"), new GUIContent("Must Be In State"));

            }
            EditorGUI.EndProperty();
        }
    }
}