using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nie.Editor
{
    [CustomPropertyDrawer(typeof(ReactionStateReference))]
    public class ReactionStateReferencePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propObject = property.FindPropertyRelative("Object");
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
            var propObject = property.FindPropertyRelative("Object");
            var obj = propObject.objectReferenceValue as ReactionState;
            layout.PropertyField(propObject);
            if (obj != null)
            {
                layout = layout.SubHorizontal();
                layout.Label("State:");
                var propStateName = property.FindPropertyRelative("StateName");
                layout.PropertyField(propStateName, -RectLayout.WidthOf("Missing"));

                if (obj.gameObject.GetComponents<ReactionState>().Any(x => x.StateName == propStateName.stringValue))
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