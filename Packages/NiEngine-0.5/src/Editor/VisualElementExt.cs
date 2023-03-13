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
    public static class VisualElementExt
    {
        public static void FixSize(this VisualElement ve, float w, float h)
        {
            ve.style.width = w;
            ve.style.minWidth = w;
            ve.style.maxWidth = w;
            ve.style.height = h;
            ve.style.minHeight = h;
            ve.style.maxHeight = h;
            ve.style.flexGrow = 0;
            ve.style.flexShrink = 0;
        }
        public static void FixSizeByLines(this VisualElement ve, float w, int lines)
        {
            ve.style.width = w;
            ve.style.minWidth = w;
            ve.style.maxWidth = w;
            ve.style.height = lines * 18;
            ve.style.minHeight = lines * 18;
            ve.style.maxHeight = lines * 18;
            ve.style.flexGrow = 0;
            ve.style.flexShrink = 0;
        }
        public static void FixSizeByCharLines(this VisualElement ve, int charCount, int lines)
        {
            ve.style.width = charCount * 8;
            ve.style.minWidth = charCount * 8;
            ve.style.maxWidth = charCount * 8;
            ve.style.height = lines * 18;
            ve.style.minHeight = lines * 18;
            ve.style.maxHeight = lines * 18;
            ve.style.flexGrow = 0;
            ve.style.flexShrink = 0;
        }
        public static void FixHeightFlexWidth(this VisualElement ve, int lines)
        {
            ve.style.height = lines * 18;
            ve.style.minHeight = lines * 18;
            ve.style.maxHeight = lines * 18;
            ve.style.flexGrow = 1;
            ve.style.flexShrink = 1;
        }
        public static void FixHeight(this VisualElement ve, int lines)
        {
            ve.style.height = lines * 18;
            ve.style.minHeight = lines * 18;
            ve.style.maxHeight = lines * 18;
        }
    }
}
