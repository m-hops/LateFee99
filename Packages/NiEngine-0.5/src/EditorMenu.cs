using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nie
{
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
    public class ReactOnDistance : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}