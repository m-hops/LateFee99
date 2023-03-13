using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nie
{

    public static class GlobalStates
    {

        public static bool DrawStatesGizmos;
        public static bool DrawStatesLabel;
        public static bool LogAllEvents;

        public static void LogReaction(string reactionName, EventParameters parameters)
        {
            if (LogAllEvents)
            {
                Debug.Log($"Reaction: \"{reactionName}\" : {parameters}");
            }
        }
        public static void LogReactionOnBegin(string reactionName, EventParameters parameters)
        {
            if (LogAllEvents)
            {
                Debug.Log($"Reaction OnBegin: \"{reactionName}\" : {parameters}");
            }
        }
        public static void LogReactionOnEnd(string reactionName, EventParameters parameters)
        {
            if (LogAllEvents)
            {
                Debug.Log($"Reaction OnEnd: \"{reactionName}\" : {parameters}");
            }
        }
    }

    //[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Class)]
    public class DerivedClassPicker : PropertyAttribute
    {
        public System.Type BaseType;
        public bool ShowPrefixLabel;
        public DerivedClassPicker(System.Type baseType, bool showPrefixLabel = true)
        {
            BaseType = baseType;
            ShowPrefixLabel = showPrefixLabel;
        }
    }

    //[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Class)]
    public class ClassPickerName : System.Attribute 
    {
        public string Name;
        public ClassPickerName(string name)
        {
            Name = name;
        }
    }
}