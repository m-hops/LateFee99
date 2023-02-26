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
            var propObject = property.FindPropertyRelative("TargetObject");
            if (propObject.objectReferenceValue is not null)
                return RectLayout.MinHeight * 2 + 4;
            return RectLayout.MinHeight + 4;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var layout = RectLayout.Horizontal(position);
            layout.PrefixLabel(label);
            layout = layout.SubVertical();
            var propObject = property.FindPropertyRelative("TargetObject");
            var obj = propObject.objectReferenceValue as GameObject;
            layout.PropertyField(propObject);
            if (obj != null)
            {
                layout = layout.SubHorizontal();
                layout.Label("State:");
                var propStateName = property.FindPropertyRelative("ReactionName");
                layout.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));
                string reactionName = propStateName.stringValue;

                if (ReactionReference.HasReaction(obj.gameObject, reactionName))
                {
                    layout.Label("Found");
                }
                else
                {
                    layout.Label("Missing");
                }
            }

            EditorGUI.EndProperty();
        }
    }
}